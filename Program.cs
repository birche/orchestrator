using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace process_tracker
{
    partial class Program
    {
        static void Main(string[] args)
        {
            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:5000")
                .Build();

            host.Run();


            var processDescriptor = new ApplicationDescriptor
            {
                ApplicationId = "AutoUpdateServer",
                CommandLine = "/var/storage/repo/dotnet/dotnet",
                CommandLineParams = new[] { "AutoTransfer.dll" },
                RelativeDirectory = "autoupdateserver/netcoreapp2.0/linux-arm/publish"
            };

            Exec exec = new Exec();
            exec.Handle(processDescriptor );
            
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            tcs.Task.Wait();
        }
    }
}
