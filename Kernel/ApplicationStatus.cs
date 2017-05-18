namespace Orchestrator.Kernel
{
    public class ApplicationStatus
    {
        public ApplicationManifest ApplicationManifest { get; set; }
        public bool IsRunning { get; set; }

        public string WorkingDirectory { get; set; }
    }
}