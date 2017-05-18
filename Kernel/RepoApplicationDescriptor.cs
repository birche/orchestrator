using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchestrator.Kernel;

namespace Orchestrator.Kernel
{
    public class RepoApplicationDescriptor
    {
        public ApplicationManifest Manifest { get; set; }
        public string WorkingDirectory { get; set; }

        public string ManifestPath { get; set; }

    }
}
