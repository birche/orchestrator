using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace process_tracker
{

    /// <summary>
    /// This should be serializable
    /// </summary>
    public class ApplicationDescriptor
    {
        public string ApplicationId { get; set; }

        public bool StartOnReboot { get; set; }
        public string RelativeDirectory { get; set; }

        public string CommandLine { get; set; }

        public string[] CommandLineParams { get; set; }

    }
}
