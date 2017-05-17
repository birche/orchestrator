using Orchestrator.Kernel;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Orchestrator.Repo
{
    class ApplicationRepository : IApplicationRepository
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(ApplicationDescriptor));
        private readonly RepoSettings m_Settings; 
        public ApplicationRepository(IOptions<RepoSettings> settings)
        {
            m_Settings = settings.Value;
        }

        public string RootPath => m_Settings.RootPath;

        public ApplicationDescriptor[] GetAllApplications()
        {
            string[] applicationDescriptorFiles = Directory.GetFiles(RootPath, $"*{m_Settings.AppExtension}", SearchOption.AllDirectories);

            List<ApplicationDescriptor> apps =  applicationDescriptorFiles 
                .Select(ReadOutDescriptor)
                .ToList();

            apps.ForEach(item => item.CommandLine = item.CommandLine.Replace("%dotnet%", m_Settings.DotnetCliPath, StringComparison.InvariantCultureIgnoreCase));

            return apps.ToArray();
        }


        private ApplicationDescriptor ReadOutDescriptor(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                return ParseDescriptor(stream);
            }
        }

        public ApplicationDescriptor ParseDescriptor(Stream stream) => (ApplicationDescriptor) serializer.Deserialize(stream);

    }
}