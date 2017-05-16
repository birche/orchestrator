using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Orchestrator.Repo;
using Orchestrator;

namespace Orchestrator.Kernel
{

    internal class ApplicationInfo
    {
        public ApplicationDescriptor Descriptor { get; set; }
        public Process Process { get; set; }
        public Task Task { get; set; }
        public DateTime StartTime { get; } = DateTime.Now;
        public bool RequestStop { get; set; }


        public bool IsRunning() => !Process.HasExited;
    }

    internal class Exec
    {

        private static readonly ConcurrentDictionary<string, ApplicationDescriptor> m_InstalledApplications = new ConcurrentDictionary<string, ApplicationDescriptor>();
        private static readonly ConcurrentDictionary<string, ApplicationInfo> m_RunningApplications = new ConcurrentDictionary<string, ApplicationInfo>();

        private readonly IApplicationRepository m_ApplicationRepository;

     

        public Exec(IApplicationRepository repo)
        {

            m_ApplicationRepository = repo;
            ApplicationDescriptor[] installedApplications = m_ApplicationRepository.GetAllApplications();

            foreach (ApplicationDescriptor application in installedApplications)
            {
                try
                {
                    Install(application);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void UnInstall(string applicationId)
        {
            ApplicationInfo aInfo = Stop(applicationId);
            aInfo?.Process?.WaitForExit();
            m_InstalledApplications.TryRemove(applicationId, out ApplicationDescriptor descriptor);
        }

        public void Install(ApplicationDescriptor descriptor)
        {
            m_InstalledApplications.AddOrUpdate(descriptor.ApplicationId, descriptor, (_, __) => descriptor);
        }

        private ProcessStartInfo GenerateProcessStartInfo(ApplicationDescriptor applicationDescriptor)
        {
            var processStartInfo = new ProcessStartInfo(applicationDescriptor.CommandLine, string.Join(" ", applicationDescriptor.CommandLineParams));
            processStartInfo.WorkingDirectory = Path.Combine(m_ApplicationRepository.RootPath, applicationDescriptor.RelativeWorkingDirectory);
            return processStartInfo;
        }

        private static ApplicationInfo GetRunningApplicationInfo(string applicationId)
        {
            ApplicationInfo result;
            m_RunningApplications.TryGetValue(applicationId, out result);
            return result;
        }

        public void ConfigureRunOnStartup(string applicationId, bool startOnReboot)
        {
            GetRunningApplicationInfo(applicationId).Descriptor.StartOnReboot = startOnReboot;
        }

        public ApplicationDescriptor[] GetAllDescriptors() => m_InstalledApplications.Select(item => item.Value).ToArray();

        private ApplicationDescriptor GetApplicationDescriptor(string applicationId)
        {
            ApplicationDescriptor descriptor;
            m_InstalledApplications.TryGetValue(applicationId, out descriptor);
            return descriptor;
        }


        public void Start(string applicationId)
        {
            ApplicationInfo info = GetRunningApplicationInfo(applicationId);
            if (info?.IsRunning() ?? false)
            {
                Console.WriteLine($"applicationid {applicationId} is already running!");
                return;
            }
            Console.WriteLine($"Starting {applicationId}..");
            ApplicationDescriptor descriptor = GetApplicationDescriptor(applicationId);
            ProcessStartInfo pInfo = GenerateProcessStartInfo(descriptor);
            var (task, process) = ProcessHandler.StartProcess(pInfo);
            process.EnableRaisingEvents = true;
            string moduleName = process.MainModule.ModuleName;
            var aInfo = new ApplicationInfo {Descriptor = descriptor, Process = process, Task = task};
            m_RunningApplications.TryAdd(descriptor.ApplicationId, aInfo);

            process.Exited += (_, __) =>
            {
                ApplicationInfo app;
                Console.WriteLine(moduleName + " exited at " + process.ExitTime);
                m_RunningApplications.TryRemove(applicationId, out app);
                if (!app.RequestStop && descriptor.RestartOnUnexpectedDeath)
                {
                    Start(applicationId);
                }
            };

        }

        
        public ApplicationInfo Stop(string applicationId)
        {
            ApplicationInfo aInfo = GetRunningApplicationInfo(applicationId);
            aInfo.RequestStop = true;
            
            if (!aInfo?.IsRunning()??false)
                return null;
            aInfo.Process.Kill();
            return aInfo;
        }

        public static bool IsStarting(string applicationId)
        {
            ApplicationInfo info = GetRunningApplicationInfo(applicationId);
            return (DateTime.Now - info?.StartTime) > TimeSpan.FromSeconds(5);
        }


        public static bool IsAlive(string applicationId)
        {
            Console.WriteLine($"IsAlive({applicationId})");
            return !GetRunningApplicationInfo(applicationId)?.Process.HasExited ?? false;
        }

        public ApplicationStatus[] GetStatus() => 
            m_InstalledApplications.Select(item => new ApplicationStatus {ApplicationDescriptor = item.Value, IsRunning = IsAlive(item.Key)}).ToArray();

        public bool SupportsIsReadyUri(string applicationId) => GetApplicationDescriptor(applicationId)?.SupportsIsReadyUri ?? false;

        public Uri GetIsReadyUri(string applicationId) => new Uri(GetApplicationDescriptor(applicationId)?.IsReadyUri);

    }

    
}
