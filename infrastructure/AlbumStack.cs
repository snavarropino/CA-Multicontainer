using System;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.Docker;
using ContainerArgs = Pulumi.AzureNative.App.Inputs.ContainerArgs;
using SecretArgs = Pulumi.AzureNative.App.Inputs.SecretArgs;

public class AlbumStack : Stack
{
    public AlbumStack()
    {
        var resourceGroup = new ResourceGroup("rg-album-pulumi");

        var registry = new Pulumi.AzureNative.ContainerRegistry.Registry("acralbum", new()
        {
            AdminUserEnabled = true,
            ResourceGroupName = resourceGroup.Name,
            Sku = new Pulumi.AzureNative.ContainerRegistry.Inputs.SkuArgs
            {
                Name = Pulumi.AzureNative.ContainerRegistry.SkuName.Basic,
            }
        });
        var credentials = Output
            .Tuple(resourceGroup.Name, registry.Name)
            .Apply(items =>
                Pulumi.AzureNative.ContainerRegistry.ListRegistryCredentials.InvokeAsync(new Pulumi.AzureNative.ContainerRegistry.ListRegistryCredentialsArgs
                {
                    ResourceGroupName = items.Item1,
                    RegistryName = items.Item2
                }));

        var adminUsername = credentials.Apply(credentials => Output.CreateSecret(credentials.Username));
        var adminPassword = credentials.Apply(credentials => Output.CreateSecret(credentials.Passwords[0].Value));

        var registryPasswordSecret = new SecretArgs
        {
            Name = "registry-pwd",
            Value = adminPassword!
        };

        var containerAppEnvironment = new ManagedEnvironment("cae-album", new ManagedEnvironmentArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = ""
            },
        });

        var ratingDockerImage = CreateRatingDockerImage(registry, adminUsername, adminPassword);
        var ratingApi = new ContainerApp("ca-rating-api", new ContainerAppArgs
        {
            EnvironmentId = containerAppEnvironment.Id,
            ResourceGroupName = resourceGroup.Name,
            ContainerAppName = "ca-rating-api",
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    //External = true,
                    External = false,
                    TargetPort = 8080
                },
                Secrets =
                {
                    registryPasswordSecret
                },
                Registries =
                {
                    new RegistryCredentialsArgs
                    {
                        Server = registry.LoginServer,
                        Username = adminUsername!,
                        PasswordSecretRef = registryPasswordSecret.Name,
                    }
                }
            },
            Template = new TemplateArgs
            {
                Scale = new ScaleArgs
                {
                    MinReplicas = 0,
                    MaxReplicas = 1,
                },
                Containers =
                {
                    new ContainerArgs
                    {
                        Name = "rating-api",
                        Image = ratingDockerImage.ImageName,
                    }
                },
            }
        });

        var ratingApiFqdn = ratingApi.Configuration.Apply(i => i.Ingress.Fqdn);
        var ratingApiUrl = Output.Format($"https://{ratingApiFqdn}");

        var albumDockerImage = CreateAlbumApiDockerImage(registry, adminUsername, adminPassword);
        var albumApi = new ContainerApp("ca-album-api", new ContainerAppArgs
        {
            EnvironmentId = containerAppEnvironment.Id,
            ResourceGroupName = resourceGroup.Name,
            ContainerAppName = "ca-album-api",
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = 8080
                },
                Secrets =
                {
                    registryPasswordSecret
                },
                Registries =
                {
                    new RegistryCredentialsArgs
                    {
                        Server = registry.LoginServer,
                        Username = adminUsername!,
                        PasswordSecretRef = registryPasswordSecret.Name,
                    }
                }
            },
            Template = new TemplateArgs
            {
                Scale = new ScaleArgs
                {
                    MinReplicas = 0,
                    MaxReplicas = 1,
                },
                Containers =
                {
                    new ContainerArgs
                    {
                        Name = "album-api",
                        Image = albumDockerImage.ImageName,
                        Env = {
                            new EnvironmentVarArgs {
                                Name = "RATINGAPIBASEURL",
                                Value = "http://ca-rating-api",
                                //Value = ratingApiUrl,
                            }
                        },
                    }
                },
            }
        });

        var albumApiFqdn = albumApi.Configuration.Apply(i => i.Ingress.Fqdn);
        var albumApiUrl = Output.Format($"https://{albumApiFqdn}");

        var uiDockerImage = CreateUiDockerImage(registry, adminUsername, adminPassword);
        var ui = new ContainerApp("ca-album-ui", new ContainerAppArgs
        {
            EnvironmentId = containerAppEnvironment.Id,
            ResourceGroupName = resourceGroup.Name,
            ContainerAppName = "ca-album-ui",
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = 3000
                },
                Secrets =
                {
                    registryPasswordSecret
                },
                Registries =
                {
                    new RegistryCredentialsArgs
                    {
                        Server = registry.LoginServer,
                        Username = adminUsername!,
                        PasswordSecretRef = registryPasswordSecret.Name,
                    }
                }
            },
            Template = new TemplateArgs
            {
                Scale = new ScaleArgs
                {
                    MinReplicas = 0,
                    MaxReplicas = 1,
                },
                Containers =
                {
                    new ContainerArgs
                    {
                        Name = "album-ui",
                        Image = uiDockerImage.ImageName,
                        Env = {
                            new EnvironmentVarArgs {
                                Name = "API_BASE_URL",
                                Value = albumApiUrl,
                            }
                        }
                    }
                }
            }
        });
        AlbumApiUrl = albumApiUrl;
        RatingApiUrl = ratingApiUrl;
    }

    [Output]
    public Output<string> UiUrl { get; set; }
    [Output]
    public Output<string> AlbumApiUrl { get; set; }
    [Output]
    public Output<string> RatingApiUrl { get; set; }


    private static Image CreateRatingDockerImage(Pulumi.AzureNative.ContainerRegistry.Registry registry, Output<string?> output, Output<string?> adminPassword1)
    {
        var image = new Image("rating-api", new()
        {
            ImageName = Output.Format($"{registry.LoginServer}/rating-api:latest"),
            Build = new Pulumi.Docker.Inputs.DockerBuildArgs
            {
                Context = "..//ratingapi",
                Platform = "linux/amd64",
                Dockerfile = "../ratingapi//Dockerfile"
            },
            Registry = new Pulumi.Docker.Inputs.RegistryArgs
            {
                Server = registry.LoginServer,
                Username = output!,
                Password = adminPassword1!,
            },
        });
        return image;
    }

    private static Image CreateAlbumApiDockerImage(Pulumi.AzureNative.ContainerRegistry.Registry registry, Output<string?> output, Output<string?> adminPassword1)
    {
        var image = new Image("album-api", new()
        {
            ImageName = Output.Format($"{registry.LoginServer}/album-api:latest"),
            Build = new Pulumi.Docker.Inputs.DockerBuildArgs
            {
                Context = "..//albumapi",
                Platform = "linux/amd64",
                Dockerfile = "../albumapi//Dockerfile"
            },
            Registry = new Pulumi.Docker.Inputs.RegistryArgs
            {
                Server = registry.LoginServer,
                Username = output!,
                Password = adminPassword1!,
            },
        });
        return image;
    }

    private static Image CreateUiDockerImage(Pulumi.AzureNative.ContainerRegistry.Registry registry, Output<string?> output, Output<string?> adminPassword1)
    {
        var image = new Image("album-ui", new()
        {
            ImageName = Output.Format($"{registry.LoginServer}/album-ui:latest"),
            Registry = new Pulumi.Docker.Inputs.RegistryArgs
            {
                Server = registry.LoginServer,
                Username = output!,
                Password = adminPassword1!,
            },
            Build = new Pulumi.Docker.Inputs.DockerBuildArgs
            {
                Context = "..//albumui",
                Platform = "linux/amd64",
                Dockerfile = "../albumui/Dockerfile"
            },
        });
        return image;
    }
}