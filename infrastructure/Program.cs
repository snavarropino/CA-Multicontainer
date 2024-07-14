using System;
using System.Threading.Tasks;
using Deployment = Pulumi.Deployment;

class Program
{
    private static Task<int> Main(string[] args)
    {
        Console.WriteLine("Serg starting");
        return Deployment.RunAsync<AlbumStack>();
    }
}