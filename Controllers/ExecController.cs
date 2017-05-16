
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Kernel;

namespace Orchestrator.Controllers
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

        [HttpPost("deploy")]
        [AllowAnonymous]
        public Task<string> FileUploadAction(IFormFileCollection files)
        {
            var sb = new StringBuilder().AppendLine("** File upload **").AppendLine($"File count: {files.Count}");
            foreach (var file in files)
            {
                if (file.Length > 0 && file.FileName.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase))
                {
                    using (Stream zipStream = file.OpenReadStream())
                    {
                        //todo:fix this
                        zipStream.Dispose();
                        //store in /var/storage/repo unzipped

                        //m_ProjectPackage.LoadProjectZipToRepository(zipStream);
                    }
                    sb.AppendLine($"Did unzip file {file.FileName}");
                }
            }
            return Task.FromResult(sb.ToString());
        }


        [HttpGet("status")]
        public ApplicationStatus[] GetAllStatus() => m_Exec.GetStatus();

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
            if(!m_Exec.SupportsIsReadyUri(applicationId))
                return true;
            Uri uri = m_Exec.GetIsReadyUri(applicationId);
            Console.WriteLine("isReady:" + applicationId + " using " + uri.ToString());
            HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
            var response = (HttpWebResponse) webRequest.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }

    }
}
