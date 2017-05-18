using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Kernel;
using Orchestrator.Repo;

namespace Orchestrator.Controllers
{
    [Route("api/v1/exec")]
    public class ExecController : Controller
    {
        private readonly Exec m_Exec;

        public ExecController(Exec exec, IApplicationRepository repository)
        {
            m_Exec = exec;
        }

        public void ConfigureRunOnStartup(string applicationId, bool startOnReboot)
        {
            m_Exec.ConfigureRunOnStartup(applicationId, startOnReboot);
        }

        [HttpGet(nameof(Ping))]
        public void Ping(string arg)
        {
            Console.WriteLine("Ping " + arg);
        }

        [HttpPost("install")]
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
                        using (ZipArchive archive = new ZipArchive(zipStream))
                        {
                            m_Exec.DeployAndInstall(archive);
                        }
                    }
                    sb.AppendLine($"Did deploy {file.FileName}");
                }
            }
            return Task.FromResult(sb.ToString());
        }

        [HttpGet(nameof(UnInstall))]
        public void UnInstall(string applicationId)
        {
            m_Exec.UnInstall(applicationId);
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

        [HttpGet(nameof(Icon))]
        public FileStreamResult Icon([FromQuery]string applicationId, [FromQuery]string random)
        {
            try
            {
                Console.WriteLine("calculate icon fetch path for " + applicationId);
                string path = m_Exec.GetIconPath(applicationId);
                Console.WriteLine("Icon path: " + path);

                FileStream image = System.IO.File.OpenRead(path);
                Console.WriteLine("Icon path opened as stream");

                string mimeType;
                if (path.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase) ||
                    path.EndsWith("jpeg", StringComparison.InvariantCultureIgnoreCase))
                {
                    mimeType = "image/jpeg";
                }
                else if (path.EndsWith("svg", StringComparison.InvariantCultureIgnoreCase))
                {
                    mimeType = "image/svg+xml";
                }
                else
                {
                    mimeType = $"image/{Path.GetExtension(path).Replace(".", string.Empty)}";
                }

                Console.WriteLine("iconpath: " + path + ", mimetype:" + mimeType);

                return File(image, mimeType);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.GetType().Name);
                throw;
            }
        }
    }
}
