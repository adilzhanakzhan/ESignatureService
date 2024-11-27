using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESignatureService.Models
{
    public class SignerInformationPayload
    {
        public string SignerInfo { get; set; }
        public string DonwloadUrl { get; set; }
        public string FileName { get; set; }
    }
}
