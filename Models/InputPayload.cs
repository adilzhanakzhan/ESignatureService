using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESignatureService.Models
{
    public class InputPayload
    {
        public string DownloadLink { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string Base64Signature { get; set; }
        public string InputText { get; set; }
        public string ElmaDocLink { get; set; }
        public string LogoLink { get; set; }
        public string FontLink { get; set; }
        public float[] Scalies { get; set; }
    }
}
