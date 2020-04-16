using System;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace UploadFilesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UploadController(ILogger<UploadController> logger,
                              IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                string folderName = Path.Combine(Directory.GetCurrentDirectory(), "Images");
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                    _logger.LogInformation("New folder created:" + folderName);
                }
                               
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(folderName, fileName);
                    _logger.LogInformation("Temp file path before creation:" + fullPath);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    string schema = Request.Scheme;
                    string imageUrl = schema+"://"+_httpContextAccessor.HttpContext.Request.Host.Value + "/Images/"+ fileName;
                    _logger.LogInformation("File upload success, created image url:" + imageUrl);
                    return Ok(new { imageUrl });
                }
                else
                {
                    _logger.LogInformation("Bad request");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error in creating file:" + ex);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("UploadAzure"), DisableRequestSizeLimit]
        public IActionResult UploadAzure()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("StaticFiles", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    
                    // Get a reference to a share and then create it
                    ShareClient share = new ShareClient("DefaultEndpointsProtocol=https;AccountName=faceidentity;AccountKey=N/gO9IA0exmgv4yH+ZISlxc0wxv2haDMWi2fZoL2/ErfqM/KHAePM4qkwMjnWEXesDvDidDVYJqKAtv3FvaXnQ==;EndpointSuffix=core.windows.net", "employeesimages");
                   // share.Create();

                    // Get a reference to a directory and create it
                    ShareDirectoryClient directory = share.GetDirectoryClient("auth");
                   // directory.Create();

                    // Get a reference to a file and upload it
                    ShareFileClient fileAzure = directory.GetFileClient(fileName);
                    using (FileStream stream = System.IO.File.OpenRead(fullPath))
                    {
                        fileAzure.Create(stream.Length);
                        fileAzure.UploadRange(
                            new HttpRange(0, stream.Length),
                            stream);
                    }

                    string fileAzurePath = fileAzure.Uri.ToString();
                    //System.IO.File.Delete(fullPath);

                    return Ok(new { fileAzurePath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}