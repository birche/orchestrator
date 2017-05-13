
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using process_tracker.Kernel;

namespace process_tracker.Controllers
{
    [Route("api/v1/exec")]
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


        [HttpGet("installed")]
        public ApplicationDescriptor[] GetAllDescriptors()
        {
            return m_Exec.GetAllDescriptors();
        }

        [HttpGet("status")]
        public ApplicationStatus[] GetAllStatus()
        {
            return m_Exec.GetStatus();
        }


        [HttpGet(nameof(Start))]
        public void Start(string applicationId)
        {
            m_Exec.Start(applicationId);
        }


        [HttpGet(nameof(Stop))]
        public void Stop(string applicationId)
        {
            m_Exec.Stop(applicationId);
        }

        [HttpGet(nameof(IsReady))]
        public bool IsReady(string applicationId)
        {
            Console.WriteLine("isReady:" + applicationId);
            Uri uri = m_Exec.GetIsAliveUri(applicationId);
            HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
            var response = (HttpWebResponse) webRequest.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }

    }
}
