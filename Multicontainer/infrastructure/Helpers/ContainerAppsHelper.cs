using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.Docker;
using ContainerArgs = Pulumi.AzureNative.App.Inputs.ContainerArgs;
using SecretArgs = Pulumi.AzureNative.App.Inputs.SecretArgs;

internal class ContainerAppsHelper
{
    public ManagedEnvironment CreateEnvironment(string name, Output<string> resourceGroupName)
    {
        var containerAppEnvironment = new ManagedEnvironment(name, new ManagedEnvironmentArgs
        {
            ResourceGroupName = resourceGroupName,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = ""
            },
            ZoneRedundant = false, //Se necesita VNET para activar esta propiedad
        });
        return containerAppEnvironment;
    }

    public ManagedEnvironmentsStorage CreateEnvironmentsStorage(string name, Output<string> environmentName,
        Output<string> storageAccountName, Output<string> storageAccountKey, Output<string> fileShareName,
        Output<string> resourceGroupName)
    {
        var managedEnvironmentsStorage = new ManagedEnvironmentsStorage(name,
            new ManagedEnvironmentsStorageArgs
            {
                EnvironmentName = environmentName,
                Properties = new ManagedEnvironmentStoragePropertiesArgs
                {
                    AzureFile = new AzureFilePropertiesArgs
                    {
                        AccessMode = AccessMode.ReadWrite,
                        AccountKey = storageAccountKey,
                        AccountName = storageAccountName,
                        ShareName = fileShareName,
                    },
                },
                ResourceGroupName = resourceGroupName,
                StorageName = storageAccountName
            });
        return managedEnvironmentsStorage;
    }

    public ContainerApp CreateContainerApp(string name, Image image, Registry registry,
        ManagedEnvironment managedEnvironment,
        Output<string> resourceGroupName, Output<string> registryAdminUsername, Output<string> registryPassword,
        int targetPort,
        EnvironmentVarArgs environment= null)
    {
        var registryPasswordSecret = new SecretArgs
        {
            Name = "registry-pwd",
            Value = registryPassword
        };

        return new ContainerApp(name, new ContainerAppArgs
        {
            ContainerAppName = name,
            EnvironmentId = managedEnvironment.Id,
            ResourceGroupName = resourceGroupName,
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = targetPort
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
                        Username = registryAdminUsername!,
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
                        Name = name,
                        Image = image.ImageName,
                        Env = {environment },
                        Resources =
                            new ContainerResourcesArgs()
                            {
                                Cpu = 0.25,
                                Memory = "0.5Gi"
                            }
                    }
                }
            }
        });
    }

}