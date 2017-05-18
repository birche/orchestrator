using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orchestrator.Repo;
using Orchestrator;
using System.IO.Compression;
using System.Xml.Serialization;

namespace Orchestrator.Kernel
{
    public sealed class Exec
    {
        private static readonly ConcurrentDictionary<string, RepoApplicationDescriptor> m_InstalledApplications = new ConcurrentDictionary<string, RepoApplicationDescriptor>();
        private static readonly ConcurrentDictionary<string, ApplicationInfo> m_RunningApplications = new ConcurrentDictionary<string, ApplicationInfo>();

        private readonly IApplicationRepository m_ApplicationRepository;
     

        public Exec(IApplicationRepository repo)
        {
            m_ApplicationRepository = repo;
            RepoApplicationDescriptor[] installedApplications = m_ApplicationRepository.GetAllApplications();

            foreach (RepoApplicationDescriptor application in installedApplications)
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
            m_InstalledApplications.TryRemove(applicationId, out RepoApplicationDescriptor descriptor);
        }

        public void DeployAndInstall(ZipArchive archive)
        {
            ZipArchiveEntry archiveEntry = archive.Entries.FirstOrDefault(item => Path.GetExtension(item.Name).Equals(m_ApplicationRepository.ManifestExtension, StringComparison.InvariantCultureIgnoreCase));
            if (archiveEntry == null)
                throw new Exception("No app descriptor");
            
            using (Stream stream = archiveEntry.Open())
            {
                RepoApplicationDescriptor appDescriptor = m_ApplicationRepository.AddApplication(stream, archive);
                if (appDescriptor == null)
                    throw new Exception("Could not evaluate " + nameof(ApplicationManifest));
                Install(appDescriptor);
            }
        }

        void Install(RepoApplicationDescriptor repoApplicationDescriptor)
        {
            m_InstalledApplications.AddOrUpdate(repoApplicationDescriptor.Manifest.ApplicationId, repoApplicationDescriptor, (_, __) => repoApplicationDescriptor);
        }

        private ProcessStartInfo GenerateProcessStartInfo(RepoApplicationDescriptor repoApplicationDescriptor)
        {
            var processStartInfo = new ProcessStartInfo(repoApplicationDescriptor.Manifest.CommandLine, string.Join(" ", repoApplicationDescriptor.Manifest.CommandLineParams));
            processStartInfo.WorkingDirectory = repoApplicationDescriptor.WorkingDirectory;
            processStartInfo.UseShellExecute = true;
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
            GetRunningApplicationInfo(applicationId).Descriptor.Manifest.StartOnReboot = startOnReboot;
        }

        public RepoApplicationDescriptor[] GetAllDescriptors() => m_InstalledApplications.Select(item => item.Value).ToArray();

        private RepoApplicationDescriptor GetApplicationDescriptor(string applicationId)
        {
            RepoApplicationDescriptor descriptor;
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
            RepoApplicationDescriptor descriptor = GetApplicationDescriptor(applicationId);
            Console.WriteLine($"Generate process start info for {applicationId}...");
            ProcessStartInfo pInfo = GenerateProcessStartInfo(descriptor);
            Console.WriteLine("Executing: WorkingDirectory: " + pInfo.WorkingDirectory+ ", process: " + pInfo.FileName + " " + pInfo.Arguments);
            try
            {
                var (task, process) = ProcessHandler.StartProcess(pInfo);
                if (process.HasExited)
                {
                    Console.WriteLine("process exited with error code " + process.ExitCode);
                }
                process.EnableRaisingEvents = true;
                string moduleName = process.MainModule.ModuleName;
                var aInfo = new ApplicationInfo {Descriptor = descriptor, Process = process, Task = task};
                m_RunningApplications.TryAdd(descriptor.Manifest.ApplicationId, aInfo);

                process.Exited += (_, __) =>
                {
                    ApplicationInfo app;
                    Console.WriteLine(moduleName + " exited at " + process.ExitTime);
                    m_RunningApplications.TryRemove(applicationId, out app);
                    if (!app.RequestStop && descriptor.Manifest.RestartOnUnexpectedDeath)
                    {
                        Start(applicationId);
                    }
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name);
                Console.WriteLine(e.Message);
            }

        }


        internal ApplicationInfo Stop(string applicationId)
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
            m_InstalledApplications.Select(item => new ApplicationStatus {ApplicationManifest = item.Value.Manifest, IsRunning = IsAlive(item.Key), WorkingDirectory = item.Value.WorkingDirectory}).ToArray();

        public ApplicationStatus GetStatus(string applicationId)
        {
            RepoApplicationDescriptor item = m_InstalledApplications.GetValueOrDefault(applicationId);
            if (item == null)
                throw new Exception("No such applicationid: " + applicationId);
            return new ApplicationStatus
            {
                ApplicationManifest = item.Manifest,
                IsRunning = IsAlive(applicationId),
                WorkingDirectory = item.WorkingDirectory
            };
        }

        public bool SupportsIsReadyUri(string applicationId) => GetApplicationDescriptor(applicationId)?.Manifest.SupportsIsReadyUri ?? false;

        public Uri GetIsReadyUri(string applicationId) => new Uri(GetApplicationDescriptor(applicationId)?.Manifest.IsReadyUri);

        public string GetIconPath(string applicationId)
        {
            RepoApplicationDescriptor applicationDescriptor = GetApplicationDescriptor(applicationId);
            string iconPath = Path.Combine(applicationDescriptor.ManifestPath, applicationDescriptor.Manifest.IconPath);
            return iconPath;
        }

    }

    
}
