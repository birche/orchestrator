using Orchestrator.Kernel;

namespace Orchestrator.Repo
{
    class ApplicationRepository : IApplicationRepository
    {
        public string RootPath => "/var/storage/repo";

        public ApplicationDescriptor[] GetAllApplications()
        {
            var applicationDescriptor = new ApplicationDescriptor
            {
                ApplicationId = "AutoUpdateServer",
                StartOnReboot = true,
                RestartOnUnexpectedDeath = true,
                RelativeWorkingDirectory = "publish",//autoupdateserver/netcoreapp2.0/linux-arm/publish",
                CommandLine = "notepad",
                CommandLineParams = new string[0],// = new[] { "AutoTransfer.dll" },
                
                IconPath = "icon.svg",
                StartPageUri = "http://localhost:5000/",
                IsReadyUri = "http://localhost:5000/api/v1/account/IsAlive",
            };
            return new [] { applicationDescriptor };
        }
    }
}