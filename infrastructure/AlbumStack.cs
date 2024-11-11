using Pulumi;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;

public class AlbumStack : Stack
{
    public AlbumStack()
    {
        var resourceGroup = new ResourceGroup("rg-album-pulumi",
            new ResourceGroupArgs
            {
                ResourceGroupName = "rg-album-pulumi"
            });

        CreateStorage();

        var containerHelper = new ContainerHelper();
        var registry = containerHelper.CreateRegistry("acralbum",resourceGroup.Name);

        var ratingDockerImage = containerHelper.CreateDockerImage("rating-api", "latest",
            "../ratingapi//Dockerfile", "..//ratingapi", registry);

        var albumDockerImage = containerHelper.CreateDockerImage("album-api", "latest",
            "../albumapi//Dockerfile", "..//albumapi", registry);

        var uiDockerImage = containerHelper.CreateDockerImage("album-ui", "latest",
            "../albumui/Dockerfile", "..//albumui", registry);

        var containerAppsHelper = new ContainerAppsHelper();
        var containerAppEnvironment = containerAppsHelper.CreateEnvironment("cae-album", resourceGroup.Name);
        
        var ratingApi =containerAppsHelper.CreateContainerApp("ca-rating-api", ratingDockerImage, registry, containerAppEnvironment, resourceGroup.Name,
            containerHelper.AdminUsername, containerHelper.AdminPassword, 8080);
        
        var ratingApiFqdn = ratingApi.Configuration.Apply(i => i.Ingress.Fqdn);
        var ratingApiUrl = Output.Format($"https://{ratingApiFqdn}");

        var albumApi =containerAppsHelper.CreateContainerApp("ca-album-api", albumDockerImage, registry, containerAppEnvironment, resourceGroup.Name,
            containerHelper.AdminUsername, containerHelper.AdminPassword,
            8080,
            new EnvironmentVarArgs {
                Name = "RATINGAPIBASEURL",
                //Value = "http://ca-rating-api",
                Value = ratingApiUrl,
            });

        var albumApiFqdn = albumApi.Configuration.Apply(i => i.Ingress.Fqdn);
        var albumApiUrl = Output.Format($"https://{albumApiFqdn}");

        var ui =containerAppsHelper.CreateContainerApp("ca-album-ui", uiDockerImage, registry, containerAppEnvironment, resourceGroup.Name,
            containerHelper.AdminUsername, containerHelper.AdminPassword, 3000,
            new EnvironmentVarArgs {
                Name = "API_BASE_URL",
                Value = Output.Format($"https://{albumApi.Configuration.Apply(i => i.Ingress.Fqdn)}")
            });
        
        AlbumApiUrl = albumApiUrl;
        RatingApiUrl = ratingApiUrl;
        UiUrl=ui.Configuration.Apply(i => i.Ingress.Fqdn);
    }

    private static void CreateStorage()
    {
        //var storageAccountHelper = new StorageAccountHelper();
        //var storageAccount = storageAccountHelper.Create("stalbum", resourceGroup.Name);
        //var storageAccountKey = storageAccountHelper.Key;
        //var storageAccountConnectionString = storageAccountHelper.ConnectionString;

        //var fileShare= storageAccountHelper.CreateFileShare("file-share-1", resourceGroup.Name, storageAccount.Name);
        
        //var managedEnvironmentsStorage = containerAppsHelper.CreateEnvironmentsStorage("stalbum9163cf6a", containerAppEnvironment.Name,
        //    storageAccount.Name, storageAccountKey, fileShare.Name, resourceGroup.Name);
    }

    [Output]
    public Output<string> UiUrl { get; set; }
    [Output]
    public Output<string> AlbumApiUrl { get; set; }
    [Output]
    public Output<string> RatingApiUrl { get; set; }
}