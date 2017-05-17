using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Orchestrator.Repo;

namespace Orchestrator.Kernel
{
   internal class ApplicationInfo
    {
        public RepoApplicationDescriptor Descriptor { get; set; }
        public Process Process { get; set; }
        public Task Task { get; set; }
        public DateTime StartTime { get; } = DateTime.Now;
        public bool RequestStop { get; set; }


        public bool IsRunning() => !Process?.HasExited ?? false;
    }
}