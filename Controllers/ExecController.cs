
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace process_tracker.Controllers
{
    [Route("api/v1/account")]
    public class ExecController : Controller
    {
        private readonly Exec m_Exec;

        public ExecController(Exec exec)
        {
            m_Exec = exec;
        }

        public void ConfigureRunOnStartup(string applicationId, bool startOnReboot)
        {
            m_Exec.ConfigureRunOnStartup(applicationId, startOnReboot);
        }


        [HttpGet("All")]
        public ApplicationDescriptor[] GetAllDescriptors()
        {
            return m_Exec.GetAllDescriptors();
        }


        [HttpGet("Start")]
        public void Start(string applicationId)
        {
            m_Exec.Start(applicationId);
        }


        [HttpGet("Stop")]
        public void Stop(string applicationId)
        {
            m_Exec.Stop(applicationId);
        }

    }
}
