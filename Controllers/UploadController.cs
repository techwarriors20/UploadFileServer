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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UploadFilesServer.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace UploadFilesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        string returnJson = string.Empty;
        private readonly AppSettings _appSettings;

        public UploadController(ILogger<UploadController> logger,
                              IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings.Value;

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
                    string imageUrl = schema + "://" + _httpContextAccessor.HttpContext.Request.Host.Value + "/Images/" + fileName;
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
        public async Task<IActionResult> UploadAzure()
        {
            try
            {
                var file = Request.Form.Files[0];

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    BlobClient blobShare = new BlobClient(_appSettings.StorageConnection, _appSettings.Container, fileName);
                    string imageUri = blobShare.Uri.AbsoluteUri;
                    var contentInfo = blobShare.Upload(file.OpenReadStream());
                    _logger.LogInformation("Blob upload success, image url:"+ imageUri);

                    var person = await MakeAnalysisRequest(imageUri);

                    _logger.LogInformation("Person identified & Email Send:" + person.Name);
                    return Ok(new { imageUri, person });
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

        // Gets the analysis of the specified image by using the Face REST API.
        private async Task<Person> MakeAnalysisRequest(string imageFilePath)
        {
            string subscriptionKey = _appSettings.StorageKey;
            string uriBase = _appSettings.FaceApiUrl;

            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);
            #region Detect Persons

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uriDetect = uriBase + "detect?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uriDetect, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                var faces = JsonConvert.DeserializeObject<List<Face>>(contentString);

                _logger.LogInformation("Face identified using detect api call"+ uriDetect);
                #endregion

            #region Identify Persons

                List<Guid> faceIds = new List<Guid>();

                foreach (Face face in faces)
                {
                    faceIds.Add(face.FaceId);
                }

                string uriIdentify = uriBase + "identify";

                IdentifyInput identifyInput = new IdentifyInput();
                identifyInput.FaceIds = faceIds;
                identifyInput.PersonGroupId = "apartment";
                identifyInput.ConfidenceThreshold = 0.4;

                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(identifyInput), Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Execute the REST API call.
                response = await client.PostAsync(uriIdentify, httpContent);

                // Get the JSON response.
                string contentIdentify = await response.Content.ReadAsStringAsync();
                var identifiedFaces = JsonConvert.DeserializeObject<List<IdentifyOutput>>(contentIdentify);
                _logger.LogInformation("Faces identified using identify api call" + uriIdentify);
                #endregion

            #region Person Details

                string uriPerson = uriBase + "persongroups/" + _appSettings.PersonGroup + "/persons/" + identifiedFaces[0].Candidates[0].PersonId;

                // Execute the REST API call.
                response = await client.GetAsync(uriPerson);

                // Get the JSON response.
                string contentPerson = await response.Content.ReadAsStringAsync();
                var person = JsonConvert.DeserializeObject<Person>(contentPerson);
                _logger.LogInformation("Person identified : " + person.Name);
                #endregion

            #region Email
                string body = string.Format("<div> This person identified during the surveillance in your premises, and his/her name : <h5 style='color: green'>{0} </h5> <img width='452' height='302' src={1}  /></div>", person.Name, imageFilePath);
                var mailMessage = new MailMessage(_appSettings.Email, _appSettings.Email, "Person Detected & Identified :"+ person.Name, body);
                mailMessage.IsBodyHtml = true;
                SendEmail(mailMessage);

                _logger.LogInformation("Email sent successfully to the address : " + _appSettings.Email);
                #endregion
                return person;

            }
        }

        // Returns the contents of the specified file as a byte array.
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (var webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(imageFilePath);
                return imageBytes;
            }
        }

        private void SendEmail(MailMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(_appSettings.Email, "India123@");
                    client.EnableSsl = true;
                    client.Send(mailMessage);                    
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                   // await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}