using process_tracker.Kernel;

namespace process_tracker.Repo
{
    public interface IApplicationRepository
    {
        string RootPath {get;}
        ApplicationDescriptor[] GetAllApplications();
    }
}