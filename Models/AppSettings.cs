using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadFilesServer.Models
{
    public class AppSettings    {
        public string FaceApiUrl { get; set; }
        public string StorageConnection { get; set; }
        public string StorageKey { get; set; }
        public string Container { get; set; }        
        public string PersonGroup { get; set; }
        public string Email { get; set; }        
    }
}
