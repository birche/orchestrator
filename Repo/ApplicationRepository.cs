﻿using Orchestrator.Kernel;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Orchestrator.Repo
{
    class ApplicationRepository : IApplicationRepository
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(ApplicationManifest));
        private readonly RepoSettings m_Settings; 
        public ApplicationRepository(IOptions<RepoSettings> settings)
        {
            m_Settings = settings.Value;
        }

        public string RootPath => m_Settings.RootPath;

        public RepoApplicationDescriptor[] GetAllApplications()
        {
            Console.WriteLine("Searching for applications...");
            string[] applicationDescriptorFiles = Directory.GetFiles(RootPath, $"*{m_Settings.AppExtension}", SearchOption.AllDirectories);

            RepoApplicationDescriptor[] apps =
                applicationDescriptorFiles.Select(ParseDescriptor).ToArray();

            Console.WriteLine($"Found and installed {apps.Count()} applications...");

            return apps;
        }

        public RepoApplicationDescriptor AddApplication(Stream stream, ZipArchive archive)
        {
            ApplicationManifest descriptor = ParseDescriptor(stream);
            //store in /var/storage/repo unzipped
            string installDirectory = Path.Combine(m_Settings.RootPath, descriptor.ApplicationId);
            Directory.CreateDirectory(installDirectory);
            archive.ExtractToDirectory(installDirectory);
            string workingDirectory = Path.Combine(installDirectory, descriptor.RelativeWorkingDirectory);
            return new RepoApplicationDescriptor{ Manifest = descriptor, WorkingDirectory = workingDirectory};
        }
        private RepoApplicationDescriptor ParseDescriptor(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                ApplicationManifest descriptor = ParseDescriptor(stream);
                return new RepoApplicationDescriptor
                {
                    Manifest = descriptor,
                    WorkingDirectory = Path.Combine(Path.GetDirectoryName(path), descriptor.RelativeWorkingDirectory)
                };
            }
        }

        private ApplicationManifest ParseDescriptor(Stream stream)
        {
            ApplicationManifest descriptor = (ApplicationManifest)serializer.Deserialize(stream);
            descriptor.CommandLine = descriptor.CommandLine.Replace("%dotnet%", m_Settings.DotnetCliPath, StringComparison.InvariantCultureIgnoreCase);
            return descriptor;
        } 

     

    }


 }