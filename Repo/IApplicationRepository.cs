using System.IO.Compression;
using System.IO;
using Orchestrator.Kernel;

namespace Orchestrator.Repo
{
    public interface IApplicationRepository
    {
        string RootPath {get;}
        RepoApplicationDescriptor[] GetAllApplications();

        RepoApplicationDescriptor AddApplication(Stream stream, ZipArchive archive);
    }
}