using System;

using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace process_tracker
{
   
    static class ProcessHandler
    {

  
        public static IEnumerable<(Task,Process)> StartProcesses(params ProcessStartInfo[] startInfos)
        {
            foreach (ProcessStartInfo processStartInfo in startInfos)
            {
                yield return StartProcess(processStartInfo);
            }
        }

        public static (Task, Process) StartProcess(ProcessStartInfo startInfo)
        {
            Process process = Process.Start(startInfo);

            Task t = Task.Run(async () =>
            {
                string moduleName = process.MainModule.ModuleName;
                int id = process.Id;

                while (true)
                {
                    const double MB = 1; //1024.0*1024.0;
                    Console.WriteLine(moduleName + " with pid " + id + " is alive. Private memory: " +
                                        process.PrivateMemorySize64 / MB + "MB, Peak paged memory: " +
                                        process.PeakPagedMemorySize64 / MB + "MB");
                    if (process.HasExited)
                    {
                        Console.WriteLine(moduleName + " exited at " + process.ExitTime);
                        break;
                    }

                    await Task.Delay(2000).ConfigureAwait(false);
                }
            });

            return (t, process);

        }


    }
}
