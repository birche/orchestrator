using Orchestrator.Kernel;

namespace Orchestrator.Repo
{
    public interface IApplicationRepository
    {
        string RootPath {get;}
        ApplicationDescriptor[] GetAllApplications();
    }
}