using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Net;
using Aspose.BarCode.Generation;
using Microsoft.AspNetCore.Hosting;
using ESignatureService.Models;
using System.Security.Cryptography.Pkcs;
using ESignatureService.Controllers;
using System.Security.Cryptography;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace ESignatureService.Functions
{
    public class Functions
    {
        private readonly ILogger<ConverterController> _logger;
        public Functions(ILogger<ConverterController> logger)
        {
            _logger = logger;
        }
        public Functions() { }
        public string ConvertBytesToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
        public byte[] GetBytesOfDocument(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] docBytes = System.IO.File.ReadAllBytes(filePath);
                fs.Read(docBytes, 0, System.Convert.ToInt32(fs.Length));
                fs.Close();
                return docBytes;
            }
        }
        public void ClearDirectory(string dirPath, string fileName)
        {
            _logger.LogInformation(string.Format("{0} Clearing directory: {1}.", DateTime.Now.ToString("F"), dirPath));
            DirectoryInfo targetDirectory = new DirectoryInfo(dirPath);
            foreach (FileInfo file in targetDirectory.EnumerateFiles())
            {
                Console.WriteLine(file.Name);
                if(fileName == file.Name)
                {
                    _logger.LogInformation(string.Format("{0} Deleting File: {1}.", DateTime.Now.ToString("F"), fileName));
                    file.Delete();
                }              
            }
        }
        public void CreateDirectory(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                    return;
                else
                {
                    DirectoryInfo dir = Directory.CreateDirectory(dirPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: {0}", ex.ToString());
            }
        }
        //public string CreateQRFromRequest(SignerInformationPayload input)
        //{
        //    string pathOfQrDirectory = Directory.GetCurrentDirectory() + "/barcodes/";
        //    CreateDirectory(pathOfQrDirectory);
        //    string formatQR = ".png";
        //    string uniqueQrName = Guid.NewGuid().ToString(); //to string 
        //    CreateQR(input.SignerInfo, uniqueQrName, pathOfQrDirectory, formatQR);
        //    byte[] bytes = GetBytesOfDocument(pathOfQrDirectory + uniqueQrName + formatQR);
        //    ClearDirectory(pathOfQrDirectory, uniqueQrName + formatQR);
        //    return ConvertBytesToBase64(bytes);
        //}
        //public string CreateDocWithQR(SignerInformationPayload input)
        //{
        //    string answer = "";
        //    Guid uniqueIdQr = Guid.NewGuid(); //unique id for image name
        //    Guid uniqueIdDoc = Guid.NewGuid(); // unique id for doc name
        //    string qr_name = uniqueIdQr.ToString(); //to string           
        //    string formatQR = ".png";
        //    string formatPDF = ".pdf";
        //    string doc_name = input.FileName + uniqueIdDoc.ToString(); //to string
        //    string pathOfDocs = Directory.GetCurrentDirectory() + "/docs/";
        //    CreateDirectory(pathOfDocs); // create directories for docs and barcodes
        //    string pathOfQr = Directory.GetCurrentDirectory() + "/barcodes/";
        //    CreateDirectory(pathOfQr);
        //    // download file from elma 365
        //    using (WebClient webClient = new WebClient())
        //    {
        //        webClient.DownloadFile(input.DonwloadUrl, pathOfDocs + doc_name + formatPDF);
        //    }
        //    // create QR in service
        //    CreateQR(input.SignerInfo, qr_name, pathOfQr, formatQR);
        //    string path_output = InsertImageToDocument(pathOfDocs, doc_name, formatPDF, pathOfQr, qr_name, formatQR);
        //    byte[] bytes = GetBytesOfDocument(pathOfDocs + path_output + "output.pdf");
        //    answer = ConvertBytesToBase64(bytes);
        //    ClearDirectory(pathOfDocs, doc_name + formatPDF);
        //    ClearDirectory(pathOfQr, qr_name + formatQR);
        //    ClearDirectory(pathOfDocs, path_output + "output.pdf");
        //    return answer;
        //}
        public void CreateQrFromGenerator(string info, string qr_name, string folderPath, string format)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(info, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            qrCodeImage.Save(folderPath + qr_name + format, ImageFormat.Png);
            qrGenerator.Dispose();
            qrCode.Dispose();
            qrCodeImage.Dispose();
        }
        public void CreateQR(string info, string qr_name, string folderPath, string format)
        {      
            string fullPath = folderPath + qr_name + format;
            _logger.LogInformation(string.Format("{0} Creating QR with info: {1}.", DateTime.Now.ToString("F"), info));
            BarcodeGenerator generator = new BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.QR, info);
            generator.Parameters.Barcode.CodeTextParameters.Location = CodeLocation.None;
            generator.Parameters.ImageHeight.Millimeters = 10;
            generator.Parameters.ImageWidth.Millimeters = 30;
            generator.Save(fullPath, BarCodeImageFormat.Png);
            generator.Dispose();
        }
        //public string InsertImageToDocument(string pathOfDocs, string doc_name, string doc_format, string pathOfQr, string qr_name, string format)
        //{
        //    string docFullPath = pathOfDocs + doc_name + doc_format;
        //    string qrFullPath = pathOfQr + qr_name + format;
        //    Document pdfDocument = new Document(docFullPath);
        //    var last_page = pdfDocument.Pages.Count;
        //    Page page = pdfDocument.Pages[last_page];
        //    Guid uniqueIdDoc = Guid.NewGuid(); // unique id for doc name
        //    string id_to_string = uniqueIdDoc.ToString();

        //    int lowerLeftX = 50;
        //    int lowerLeftY = 50;
        //    int upperRightX = 100;
        //    int upperRightY = 100;

        //    FileStream imgStream = new FileStream(qrFullPath, FileMode.Open);
        //    page.Resources.Images.Add(imgStream);
        //    page.Contents.Add(new Aspose.Pdf.Operators.GSave());
        //    Aspose.Pdf.Rectangle rectangle = new Aspose.Pdf.Rectangle(lowerLeftX, lowerLeftY, upperRightX, upperRightY);
        //    Matrix matrix = new Matrix(new double[] { rectangle.URX - rectangle.LLX, 0, 0, rectangle.URY - rectangle.LLY, rectangle.LLX, rectangle.LLY });
        //    page.Contents.Add(new Aspose.Pdf.Operators.ConcatenateMatrix(matrix));
        //    XImage ximage = page.Resources.Images[page.Resources.Images.Count];
        //    page.Contents.Add(new Aspose.Pdf.Operators.Do(ximage.Name));
        //    page.Contents.Add(new Aspose.Pdf.Operators.GRestore());
        //    pdfDocument.Save(pathOfDocs + id_to_string + "output.pdf", Aspose.Pdf.SaveFormat.Pdf);
        //    imgStream.Close();
        //    return id_to_string;
        //}
        public DecodedCertificateResponse DecodeSignerInformation(InputPayload input)
        {
            var certificateBase64 = input.Base64Signature;
            byte[] bytes = Convert.FromBase64String(certificateBase64);
            SignedCms sms = new SignedCms();
            sms.Decode(bytes);
            Console.WriteLine(sms.Certificates);
            var collection = sms.Certificates;
            //var certificate1 = new X509Certificate2(bytes);
            //Console.WriteLine(certificate1.Subject);
            //var collection = new X509Certificate2Collection();
            //collection.Import(bytes);
            var certificate = collection[0];
            foreach (X509Extension extension in certificate.Extensions)
            {
                // Create an AsnEncodedData object using the extensions information.
                AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
                Console.WriteLine("Extension type: {0}", extension.Oid.FriendlyName);
                Console.WriteLine("Oid value: {0}", asndata.Oid.Value);
                Console.WriteLine("Raw data length: {0} {1}", asndata.RawData.Length, Environment.NewLine);
                Console.WriteLine(asndata.Format(true));
            }
            var response = new DecodedCertificateResponse();
            var dn = certificate.Subject;
            string pattern = string.Empty;
            string signSubjOrgItem = string.Empty;
            pattern = @"T=([^,]*)";
            Regex re = new Regex(pattern);
            Match m = re.Match(dn);
            if (m.Success)
            {
                signSubjOrgItem = Regex.Replace(m.Groups[1].Value, @"(\"")\1+", @"""").TrimStart('"').ToUpper();
                // Парсинг. Элементс с индексом 1 содержит первое совпадение из группы
            }

            string signSubjSurname = string.Empty;
            // Фамилия владельца ЭЦП
            pattern = @"CN=([^,]*)";
            re = new Regex(pattern);
            m = re.Match(dn);
            if (m.Success)
            {
                signSubjSurname = Regex.Replace(m.Groups[1].Value, @"(\"")\1+", @"""").TrimStart('"').ToUpper();
                // Парсинг. Элементс с индексом 1 содержит первое совпадение из группы
            }
            string signSubjName = string.Empty;
            // ИО владельца ЭЦП
            pattern = @"G=([^,]*)";
            dn = certificate.Subject;
            re = new Regex(pattern);
            m = re.Match(dn);
            if (m.Success)
            {
                signSubjName = Regex.Replace(m.Groups[1].Value, @"(\"")\1+", @"""").TrimStart('"').ToUpper();
                // Парсинг. Элементс с индексом 1 содержит первое совпадение из группы
            }

            response.SignSubjSurname = signSubjSurname; //автор подписи
            response.Issuer = certificate.Issuer; //издатель
            response.Thumbprint = certificate.Thumbprint; //отпечаток
            response.SignSerialNumber = certificate.SerialNumber; //серийный номер
            response.SignNotBefore = certificate.NotBefore.ToShortDateString(); //действителен с
            response.SignNotAfter = certificate.NotAfter.ToShortDateString(); //действителен по                                    
            response.SignSubjName = signSubjName;
            response.SignSubjOrgItem = signSubjOrgItem;
            collection.Clear();
            return response;
        }

        public void DownloadExtra(string link, string logoFolder, string fileName)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(link, $"{logoFolder}{fileName}");
                _logger.LogInformation(string.Format("{0} Downloaded logo from elma.", DateTime.Now.ToString("F")));
                webClient.Dispose();
            }
        }

        public Response ProcessDocument(InputPayload input, string qrPath, string downloadPath, string docPath, string pdfPath, string barcodesFolderPath, string logoFolder, string fontFolder)
        {
            if (!File.Exists(logoFolder + "logo.png"))
            {
                if (!Directory.Exists(logoFolder))
                {
                    CreateDirectory(logoFolder);
                }

                DownloadExtra(input.LogoLink, logoFolder, "logo.png");
            } 
            if (!File.Exists(fontFolder + "Helvetica.ttf"))
            {
                if (!Directory.Exists(logoFolder))
                {
                    CreateDirectory(logoFolder);
                }

                DownloadExtra(input.FontLink, fontFolder, "Helvetica.ttf");
            }
            Response r = new();
            bool docType = (input.FileExtension.Equals("docx") || input.FileExtension.Equals("doc")) ? false : true; // if false docx else pdf           
            string documentName = String.Format("{0}{1}.{2}",input.FileName, Guid.NewGuid().ToString(), input.FileExtension);
            _logger.LogInformation(string.Format("{0} Processing QR insertion to document: {1}.", DateTime.Now.ToString("F"), documentName));
            string outputPath = docType ? pdfPath : docPath;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(input.DownloadLink, downloadPath + documentName);
                    _logger.LogInformation(string.Format("{0} Downloaded file from elma.", DateTime.Now.ToString("F")));
                }
                Output finalVersion = new();
                if (docType == true)
                {
                    if (input.Scalies.Length == 0)
                    {
                        string qrUniqueName = Guid.NewGuid().ToString();
                        CreateQrFromGenerator(input.ElmaDocLink, qrUniqueName, Directory.GetCurrentDirectory() + "/barcodes/", ".png");
                        finalVersion = Pdf._instance.AddFirstQR(qrPath, string.Format($"{barcodesFolderPath}{qrUniqueName}.png")  , downloadPath + documentName, outputPath + documentName, input.ElmaDocLink, $"{logoFolder}logo.png");
                        ClearDirectory(barcodesFolderPath, qrUniqueName + ".png");
                        r.Scalies = finalVersion.Scalies;
                    }
                    else
                    {
                        Pdf._instance.AddSecondQR(qrPath, downloadPath + documentName, outputPath + documentName, input.Scalies);
                    }                    
                }
                else
                {
                    Word._instance.ExportAsDocx(qrPath, downloadPath + documentName, outputPath + documentName);
                }
                _logger.LogInformation(string.Format("{0} Processed file is stored here: {1}.", DateTime.Now.ToString("F"), outputPath + documentName));
                var stringRepresentation = ConvertBytesToBase64(GetBytesOfDocument(outputPath + documentName));
                r.FinalFileBase64 = stringRepresentation;
                ClearDirectory(outputPath, documentName);
                ClearDirectory(downloadPath, documentName);
                return r;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                r.FinalFileBase64 = "Error - " + e.Message;
                r.Scalies = Array.Empty<float>();
                ClearDirectory(outputPath, documentName);
                ClearDirectory(downloadPath, documentName);
                return r;
            }
        }
    }
}
