using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using process_tracker;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace process_tracker
{
    public class Exec
    {

        private static readonly ConcurrentDictionary<string, ApplicationInfo> m_ApplicationInfo = new ConcurrentDictionary<string, ApplicationInfo>();

        internal class ApplicationInfo
        {
            public ApplicationDescriptor Descriptor { get; set; }
            public Process Process { get; set; }
            public Task Task { get; set; }
            public DateTime StartTime { get; } = DateTime.Now;


            public bool IsRunning() => !Process.HasExited;
        }



        public Exec()
        {
        }

        public void Handle(ApplicationDescriptor descriptor)
        {
            ApplicationInfo info = GetApplicationInfo(descriptor.ApplicationId);
            if (info?.IsRunning() ?? false)
            {
                return;
            }
            ProcessStartInfo pInfo = GenerateProcessStartInfo(descriptor);
            var (task, process) = ProcessHandler.StartProcess(pInfo);
            m_ApplicationInfo.TryAdd(descriptor.ApplicationId, new ApplicationInfo { Descriptor = descriptor, Process = process, Task = task});
        }

        private ProcessStartInfo GenerateProcessStartInfo(ApplicationDescriptor applicationDescriptor)
        {
            var processStartInfo = new ProcessStartInfo(applicationDescriptor.CommandLine, string.Join(" ", applicationDescriptor.CommandLineParams));
            processStartInfo.WorkingDirectory = Path.Combine("/var/storage/repo", applicationDescriptor.RelativeDirectory);
            return processStartInfo;
        }

      

        private static ApplicationInfo GetApplicationInfo(string applicationId)
        {
            ApplicationInfo result;
            m_ApplicationInfo.TryGetValue(applicationId, out result);
            return result;
        }


        public void ConfigureRunOnStartup(string applicationId, bool startOnReboot)
        {
            GetApplicationInfo(applicationId).Descriptor.StartOnReboot = startOnReboot;
        }


        public ApplicationDescriptor[] GetAllDescriptors() => m_ApplicationInfo.Select(item => item.Value.Descriptor).ToArray();


        public void Start(string applicationId)
        {
            
        }

        public void Stop(string applicationId)
        {
            ApplicationInfo aInfo = GetApplicationInfo(applicationId);
            if (!aInfo?.IsRunning()??false)
                return;
            aInfo.Process.Kill();
        }

        public static bool IsStarting(string applicationId)
        {
            ApplicationInfo info = GetApplicationInfo(applicationId);
            return (DateTime.Now - info?.StartTime) > TimeSpan.FromSeconds(5);
        }


        public static bool IsAlive(string applicationId)
        {
            return GetApplicationInfo(applicationId)?.Process.HasExited ?? false;
        }

    }
}
