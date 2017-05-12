﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace process_tracker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ProcessStartInfo startInfor = new ProcessStartInfo("/var/storage/repo/dotnet/dotnet", "AutoTransfer.dll");
            startInfor.WorkingDirectory = "/var/storage/repo/autoupdateserver/netcoreapp2.0/linux-arm/publish";
            Process pInfo = Process.Start(startInfor);

            List<Task> list = new List<Task>();
            int count = 0;
            Process []processes ={pInfo};
            foreach (var process in processes) {
                Task t = Task.Run( async () => {
                    string moduleName = process.MainModule.ModuleName;
                    int id = process.Id;

                
                    while(true)
                    {
                        if (count++ == 20)
                        {
                            process.Kill();
                        }

                        Console.WriteLine("Count =" + count);

                        const double MB = 1; //1024.0*1024.0;
                        Console.WriteLine(moduleName + " with pid " + id + " is alive. Private memory: " + process.PrivateMemorySize64/MB + "MB, Peak paged memory: " + process.PeakPagedMemorySize64/MB+ "MB");   
                        if(process.HasExited)
                        {
                            Console.WriteLine(moduleName + " exited at " +  process.ExitTime);   
                            break;
                        }

                        await Task.Delay(2000);
                    }
                });
                list.Add(t);
            }

            Task.WaitAll(list.ToArray());
            
        }
    }
}
