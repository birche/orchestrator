﻿using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Orchestrator;

namespace Orchestrator
{
    partial class Program
    {
        static void Main(string[] args)
        {
            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:5001")
                .Build();

            host.Run();
        }
    }
}
