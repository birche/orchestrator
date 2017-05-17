using Orchestrator.Kernel;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;

namespace Orchestrator.Repo
{
    class ApplicationRepository : IApplicationRepository
    {
        private RepoSettings m_Settings; 
        public ApplicationRepository(IOptions<RepoSettings> settings)
        {
            m_Settings = settings.Value;
        }

        public string RootPath => m_Settings.RootPath;

        public ApplicationDescriptor[] GetAllApplications()
        {
            string[] applicationDescriptorFiles = Directory.GetFiles(RootPath, $"*{m_Settings.AppExtension}", SearchOption.AllDirectories);

            var serializer = new XmlSerializer(typeof(ApplicationDescriptor));

            ApplicationDescriptor[]apps =  applicationDescriptorFiles 
                .Select(file => (ApplicationDescriptor) serializer.Deserialize(File.OpenRead(file)))
                .ToArray();

            return apps;
            /*

            var applicationDescriptor = new ApplicationDescriptor
            {
                ApplicationId = "AutoUpdateServer",
                StartOnReboot = true,
                RestartOnUnexpectedDeath = true,
                RelativeWorkingDirectory = "publish",//autoupdateserver/netcoreapp2.0/linux-arm/publish",
                CommandLine = "notepad",
                CommandLineParams = new string[0],// = new[] { "AutoTransfer.dll" },
                
                IconPath = "icon.svg",
                StartPageUri = "http://localhost:5000/",
                IsReadyUri = "http://localhost:5000/api/v1/account/IsAlive",
            };
            return new [] { applicationDescriptor };*/
        }
    }
}