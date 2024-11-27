using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.Pkcs;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using ESignatureService.Models;

namespace ESignatureService.Controllers
{
    [ApiController]
    [Route("/script")]
    public class ScriptController : Controller
    {
        private readonly ILogger<ScriptController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScriptController(ILogger<ScriptController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        //[HttpPost("document")]
        //public IActionResult Document([FromBody]DocumentContentPayload input)
        //{
        //    if(input == null)
        //    {
        //        return BadRequest();
        //}
        //var downloadUrl = input.DownloadUrl;
        //    using (WebClient webclient = new WebClient())
        //    {
        //webclient.DownloadFileAsync(
        // Param1 = Link of file
        //new System.Uri(downloadUrl),
        // Param2 = Path to save
        //        "D:\\Images\\front_view.jpg"
        //        );
        //}
        //var fileExtesion = downloadUrl.Split('.').Last();

        //  return Ok("dddd");
        //}

        //[HttpPost("certificate")]
        //public IActionResult Certificate([FromBody] EncodedCertificatePayload input)
        //{
        //    if (input == null)
        //    {
        //        return BadRequest();
        //    }
        //    var response = new Functions.Functions().DecodeSignerInformation(input);
        //    return Ok(response);
        //}

        //[HttpPost("createQr")]
        //public IActionResult CreateQR([FromBody] SignerInformationPayload input)
        //{
        //    if (input == null)
        //    {
        //        return BadRequest();
        //    }
        //    var base64string = new Functions.Functions().CreateQRFromRequest(input);
        //    var response = new QrResponse();
        //    response.Base64StringQR = base64string;
        //    //var folderPath = Directory.GetCurrentDirectory() + "\\TempFolder\\Barcodes\\";
        //    //DirectoryInfo barcodes = new DirectoryInfo(folderPath);
        //    //foreach (FileInfo file in barcodes.EnumerateFiles())
        //    //{
        //    //    file.Delete();
        //    //}
        //    return Ok(response);
        //}
        //[HttpPost("insertToPdf")]
        //public IActionResult InsertQrToPdf([FromBody] SignerInformationPayload input)
        //{
        //    if(input == null)
        //    {
        //        return BadRequest();
        //    }
        //    var base64string = new Functions.Functions().CreateDocWithQR(input);
        //    var response = new QrResponse();
        //    response.Base64StringQR = base64string;
        //    //var folderPath = Directory.GetCurrentDirectory() + "\\TempFolder\\Barcodes\\";
        //    //DirectoryInfo barcodes = new DirectoryInfo(folderPath);
        //    //foreach (FileInfo file in barcodes.EnumerateFiles())
        //    //{
        //    //    file.Delete();
        //    //}
        //    return Ok(response);
        //}

        [HttpGet]
        public string Index()
        {
            return "Service is working!";
        }
    }
}
