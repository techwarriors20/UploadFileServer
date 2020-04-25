using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadFilesServer.Models
{
    public class Email
    {
        public string from { get; set; }
        public string name { get; set; }
        public string subject { get; set; }
        public string to { get; set; }
        public string url { get; set; }
        public string bodycontent { get; set; }
    }
}
