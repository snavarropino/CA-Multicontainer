using System.Threading.Tasks;

internal class Program
{
    private static Task<int> Main()
    {
        return Pulumi.Deployment.RunAsync<AlbumStack>();
    }
}