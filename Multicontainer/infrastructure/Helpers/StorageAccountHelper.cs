using Pulumi;
using Pulumi.AzureNative.Storage;
using SkuName = Pulumi.AzureNative.Storage.SkuName;

internal class StorageAccountHelper
{
    public Output<string> Key { get; private set;} = null!;
    public Output<string> ConnectionString { get; private set; } = null!;

    public StorageAccount Create(string name, Output<string> resourceGroupName, Kind kind, SkuName skuName)
    {
        var storageAccount = new StorageAccount(name, new()
        {
            AccountName = name,
            ResourceGroupName = resourceGroupName,
            Kind = kind,
            Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
            {
                Name = skuName
            }
        });
        LoadKeys(resourceGroupName, storageAccount.Name);
        return storageAccount;
    }

    public StorageAccount Create(string name, Output<string> resourceGroupName)
    {
        return Create(name, resourceGroupName, Kind.StorageV2, SkuName.Standard_LRS);
    }

    public FileShare CreateFileShare(string name, Output<string> resourceGroupName, Output<string> storageAccountName)
    {
        var fileShare = new FileShare(name, new()
        {
            AccountName = storageAccountName,
            EnabledProtocols = EnabledProtocols.SMB,
            ResourceGroupName = resourceGroupName
        });
        return fileShare;
    }

    private void LoadKeys(Output<string> resourceGroupName, Output<string> storageAccountName)
    {
        var storageAccountKeys = Output.Tuple(resourceGroupName, storageAccountName).Apply(names =>
            ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs()
            {
                ResourceGroupName = names.Item1,
                AccountName = names.Item2,
            }));

        Key = storageAccountKeys.Apply(keys => keys.Keys[0].Value);

        ConnectionString = storageAccountKeys.Apply(keys => 
            Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={keys.Keys[0].Value};EndpointSuffix=core.windows.net")
        );
    }
}