using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESignatureService
{
    public class DecodedCertificateResponse
    {
        public string SignSubjSurname { get; set; }
        public string Issuer { get; set; }
        public string Thumbprint { get; set; }
        public string SignSerialNumber { get; set; }
        public string SignNotBefore { get; set; }
        public string SignNotAfter { get; set; }
        public string SignSubjOrgItem { get; set; }
        public string SignSubjName { get; set; }
    }
}
