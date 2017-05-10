using System;
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
            Process pInfo = Process.Start("notepad");
            Process pInfo2 = Process.Start("notepad");

            List<Task> list = new List<Task>();

            Process []processes ={pInfo, pInfo2};
            foreach (var process in processes) {
                Task t = Task.Run( async () => {
                    string moduleName = process.MainModule.ModuleName;
                    int id = process.Id;

                    while(true)
                    {
                        Console.WriteLine(moduleName + " is alive ");   
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
