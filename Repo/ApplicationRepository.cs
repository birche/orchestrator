using process_tracker;

namespace process_tracker.Repo
{
    class ApplicationRepository : IApplicationRepository
    {
        public ApplicationDescriptor[] GetAllApplications()
        {
            var applicationDescriptor = new ApplicationDescriptor
            {
                ApplicationId = "AutoUpdateServer",
                CommandLine = "/var/storage/repo/dotnet/dotnet",
                CommandLineParams = new[] { "AutoTransfer.dll" },
                RelativeDirectory = "autoupdateserver/netcoreapp2.0/linux-arm/publish"
            };
            return new [] { applicationDescriptor };
        }
    }
}