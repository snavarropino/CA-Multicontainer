using Pulumi;
using Pulumi.AzureNative.ContainerRegistry;

internal class ContainerHelper
{
    public Output<string?> AdminUsername { get; private set; }
    public Output<string?> AdminPassword { get; private set; }

    private const string ImagePlatform = "linux/amd64";
    public Registry CreateRegistry(string name, Output<string> resourceGroupName)
    {
        return CreateRegistry(name, resourceGroupName, SkuName.Basic);
    }

    public Registry CreateRegistry(string name, Output<string> resourceGroupName, SkuName skuName)
    {
        var registry = new Registry(name, new RegistryArgs
        {
            AdminUserEnabled = true,
            ResourceGroupName = resourceGroupName,
            Sku = new Pulumi.AzureNative.ContainerRegistry.Inputs.SkuArgs
            {
                Name = skuName,
            }
        });
        LoadCredentials(resourceGroupName,registry.Name);
        return registry;
    }

    public Pulumi.Docker.Image CreateDockerImage(string name, string tag, string dockerfile, string context, Registry registry)
    {
        var image = new Pulumi.Docker.Image(name, new()
        {
            ImageName = Output.Format($"{registry.LoginServer}/{name}:{tag}"),
            Build = new Pulumi.Docker.Inputs.DockerBuildArgs
            {
                Context = context,
                Platform = ImagePlatform,
                Dockerfile = dockerfile
            },
            Registry = new Pulumi.Docker.Inputs.RegistryArgs
            {
                Server = registry.LoginServer,
                Username = AdminUsername!,
                Password = AdminPassword!,
            },
        });
        return image;
    }

    private void LoadCredentials(Output<string> resourceGroupName, Output<string> registryName)
    {
        var credentials = Output
            .Tuple(resourceGroupName, registryName)
            .Apply(items =>
                ListRegistryCredentials.Invoke(new Pulumi.AzureNative.ContainerRegistry.ListRegistryCredentialsInvokeArgs()
                {
                    ResourceGroupName = items.Item1,
                    RegistryName = items.Item2
                }));

       AdminUsername = credentials.Apply(credentials => Output.CreateSecret(credentials.Username));
       AdminPassword = credentials.Apply(credentials => Output.CreateSecret(credentials.Passwords[0].Value));
    }
}