using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orchestrator.Kernel
{

    /// <summary>
    /// This should be serializable
    /// </summary>
    public class ApplicationDescriptor
    {
        public string ApplicationId { get; set; }

        public bool StartOnReboot { get; set; }
        public bool RestartOnUnexpectedDeath { get; set; }
        public string RelativeWorkingDirectory { get; set; }
        public string CommandLine { get; set; }
        public string[] CommandLineParams { get; set; }

        public string IconPath { get; set; } 
        public string StartPageUri { get; set; }
        public string IsReadyUri { get; set; }


        public bool SupportsStartPageUri => !string.IsNullOrEmpty(StartPageUri?.Trim());
        public bool SupportsIsReadyUri => !string.IsNullOrEmpty(IsReadyUri?.Trim()); 
    }
}
