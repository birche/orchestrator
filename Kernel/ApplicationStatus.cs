namespace process_tracker.Kernel
{
    public class ApplicationStatus
    {
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public bool IsRunning { get; set; }
    }
}