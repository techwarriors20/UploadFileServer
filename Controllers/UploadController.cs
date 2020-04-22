using System;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs.Models;

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
                          
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    BlobClient blobShare = new BlobClient("DefaultEndpointsProtocol=https;AccountName=imagefaceprocess;AccountKey=JMNqY9DyKRnPZHSd6r7eUnBzoxHDsHWrweE8eG9z/x1E/NqoSihrv5VgmJJbNIjapylEvsqFcKDXnUkv4lEmnQ==;EndpointSuffix=core.windows.net", "imagesprocess", fileName);
                    string imageUri = blobShare.Uri.AbsoluteUri;
                    var contentInfo = blobShare.Upload(file.OpenReadStream());
                  
                    return Ok(new { imageUri });
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