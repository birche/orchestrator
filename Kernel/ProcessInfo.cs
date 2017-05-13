using System;

namespace process_tracker.Kernel
{
    public class ProcessInfo
    {
        public int Pid { get; set; }
        public DateTime StartTime { get; set; }
    }
}