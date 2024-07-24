using System;
using System.Threading.Tasks;
using Deployment = Pulumi.Deployment;

class Program
{
    private static Task<int> Main(string[] args)
    {
        return Deployment.RunAsync<AlbumStack>();
    }
}