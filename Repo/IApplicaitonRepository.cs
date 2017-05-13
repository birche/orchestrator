using process_tracker.Kernel;

namespace process_tracker.Repo
{
  
    public interface IApplicationRepository
    {
        ApplicationDescriptor[] GetAllApplications();
    }


}