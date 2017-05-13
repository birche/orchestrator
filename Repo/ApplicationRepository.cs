using process_tracker.Kernel;

namespace process_tracker.Repo
{
    class ApplicationRepository : IApplicationRepository
    {
        public string RootPath => "/var/storage/repo";

        public ApplicationDescriptor[] GetAllApplications()
        {
            var applicationDescriptor = new ApplicationDescriptor
            {
                ApplicationId = "AutoUpdateServer",
                CommandLine = "/var/storage/repo/dotnet/dotnet",
                CommandLineParams = new[] { "AutoTransfer.dll" },
                RelativeDirectory = "autoupdateserver/netcoreapp2.0/linux-arm/publish",
                IsReadyUri = "http://localhost:5000/api/v1/account/IsAlive"
            };
            return new [] { applicationDescriptor };
        }
    }
}