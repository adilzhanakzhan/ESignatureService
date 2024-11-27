using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.Pkcs;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using ESignatureService.Models;
using System.IO;
using System.Threading.Tasks;

namespace ESignatureService.Controllers
{
    [ApiController]
    [Route("api/converter")]
    public class ConverterController: Controller
    {
        private readonly ILogger<ConverterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Functions.Functions _functions;
        private string barcodesFolder = Directory.GetCurrentDirectory() + "/barcodes/";
        private string storageFolder = Directory.GetCurrentDirectory() + "/elmaDownloads/";
        private string outputDocxFolder = Directory.GetCurrentDirectory() + "/outputDocx/";
        private string outputPdfFolder = Directory.GetCurrentDirectory() + "/outputPdf/";
        private string logoFolder = Directory.GetCurrentDirectory() + "/logo/";
        private string fontFolder = Directory.GetCurrentDirectory() + "/font/";
        public ConverterController(ILogger<ConverterController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _functions = new Functions.Functions(logger);
        }
        [HttpGet("/healthcheck")]
        async public Task<IActionResult> HealthCheck()
        {
            _logger.LogInformation(string.Format("{0} Got request HealthCheck, service is working.", DateTime.Now.ToString("F")));
            return Ok("Service is working!");
        } 

        [HttpPost("/decodeSignature")]
        async public Task<IActionResult> DecodeSignature([FromBody] InputPayload input)
        {
            var answer = String.Empty;
            if (input == null)
            {
                _logger.LogError(string.Format("{0} Got request ProcessFile, but input is empty!", DateTime.Now.ToString("F")));
                return BadRequest();
            }
            var information = _functions.DecodeSignerInformation(input);
            return Ok(information);
        }

        [HttpPost("/processFile")] // First&second request
        async public Task<IActionResult> Generate([FromBody] InputPayload input)
        {
            Response answer;
            if (input == null)
            {
                _logger.LogError(string.Format("{0} Got request ProcessFile, but input is empty!", DateTime.Now.ToString("F")));
                return BadRequest();
            }
            _logger.LogInformation(string.Format("{0} Got request ProcessFile, for file: {1}.", DateTime.Now.ToString("F"), input.FileName));
            _logger.LogInformation(string.Format("{0} Creating folders for barcodes, downloads and output files.", DateTime.Now.ToString("F")));
            _functions.CreateDirectory(barcodesFolder);
            _functions.CreateDirectory(storageFolder);
            _functions.CreateDirectory(outputPdfFolder);
            _functions.CreateDirectory(outputDocxFolder);
            _functions.CreateDirectory(logoFolder);
            _functions.CreateDirectory(fontFolder);
            DecodedCertificateResponse info = _functions.DecodeSignerInformation(input);
            //название и номер документа, ФИО подписанта, дату подписания, ссылку на документ в Elma
            string infoForQrGeneration = String.Format
                ("Автор подписи: {0} {1}\nИздатель: {2}\nОтпечаток: {3}\nСерийный номер: {4}\nДействителен с: {5}\nДействителен по: {6}\n{7}",
                 info.SignSubjSurname,
                 info.SignSubjName,
                 info.Issuer,
                 info.Thumbprint,
                 info.SignSerialNumber,
                 info.SignNotBefore,
                 info.SignNotAfter,
                 input.InputText);
            _logger.LogInformation(string.Format("{0} Processed Signature information: {1}.", DateTime.Now.ToString("F"), infoForQrGeneration));
            string qrUniqueName = Guid.NewGuid().ToString();
            _logger.LogInformation(string.Format("{0} Creating QR with name: {1}.", DateTime.Now.ToString("F"), qrUniqueName));
            try
            {
                _functions.CreateQrFromGenerator(infoForQrGeneration, qrUniqueName, Directory.GetCurrentDirectory() + "/barcodes/", ".png");
                answer = _functions.ProcessDocument(input, String.Format("{0}{1}{2}", barcodesFolder, qrUniqueName, ".png"), storageFolder, outputDocxFolder, outputPdfFolder, barcodesFolder, logoFolder, fontFolder);
                _functions.ClearDirectory(barcodesFolder, qrUniqueName + ".png");
                return Ok(answer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: {0}", ex.ToString());
                return BadRequest(ex.ToString());
            }
        }
    }
}
