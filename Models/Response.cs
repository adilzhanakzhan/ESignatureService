using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESignatureService.Models
{
    public class Response
    {
        public string FinalFileBase64 { get; set; }
        public float[] Scalies { get; set; }
    }
}
