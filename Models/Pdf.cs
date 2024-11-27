using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ESignatureService.Models
{
    public class Pdf
    {
        static public Pdf _instance = new Pdf();

        private PdfContentByte _pdfContentByte;
        private const float _scale = 65;
        private const int _fontSize = 7;
        private string _fontStyle = Directory.GetCurrentDirectory() + "/font/Helvetica.ttf";
        private const float _absoluteX = 180;
        private static float _absoluteY = 70;
        private static string _pathImage;
        private static string _pathImageLink;

        private bool _flagVertical = true;

        public Output AddFirstQR(string pathImage, string pathImageLink, string pathFile, string newFile, string link, string logoPath)
        {
            _pathImage = pathImage;
            _pathImageLink = pathImageLink;
            byte[] bytes = File.ReadAllBytes(pathFile);

            using (FileStream fileStream = File.Create(newFile)) fileStream.Write(bytes, 0, bytes.Length);
            float[] scalies;
            using (Stream inputLogotypeStream = new FileStream(logoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputPdfStream = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputImageStream = new FileStream(pathImage, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputImageLinkStream = new FileStream(pathImageLink, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(newFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfReader reader = new(inputPdfStream);

                PdfStamper stamper = new(reader, outputPdfStream);

                int rotate = reader.GetPageRotation(1);
                var rectangle = reader.GetPageSizeWithRotation(1);

                if (rectangle.Width > rectangle.Height || rotate > 0)
                {
                    _flagVertical = false;
                }

                float scaleLogotypeX = 80f;
                float scaleLogotypeY = scaleLogotypeX / 2.877f;
                float xPosition = 500;
                Image logotype = SetSettingsImage(inputLogotypeStream, xPosition, 17, scaleLogotypeX, scaleLogotypeY);

                EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
                Encoding.RegisterProvider(encodingProvider);

                BaseFont baseFont = BaseFont.CreateFont(_fontStyle, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                if (_flagVertical) scalies = AddContentsVertical(reader, stamper, logotype, baseFont, link);
                else scalies = AddContentsHorizontal(reader, stamper, logotype, baseFont, link);

                stamper.Close();
                reader.Close();
                inputPdfStream.Close();
                outputPdfStream.Close();
                inputImageStream.Close();
                inputImageLinkStream.Close();
            }

            // Вернуть новый файл + размеры картинок
            Output output = new() { Scalies = scalies, NewFile = newFile };
            return output;
        }

        private static Image SetSettingsImage(Stream imageStream, float xPosition, float yPosition,
            float xScale = _scale, float yScale = _scale)
        {
            Image image = Image.GetInstance(imageStream);
            image.SetAbsolutePosition(xPosition, yPosition);
            image.ScaleAbsolute(xScale, yScale);
            return image;
        }

        private float[] AddContentsVertical(PdfReader reader, PdfStamper stamper, Image logotype, BaseFont baseFont, string link)
        {
            List<float> scalies = new();
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                Stream inputImageStream = new FileStream(_pathImage, FileMode.Open, FileAccess.Read, FileShare.Read);
                Stream inputImageLinkStream = new FileStream(_pathImageLink, FileMode.Open, FileAccess.Read, FileShare.Read);
                Image image = SetSettingsImage(inputImageStream, 15, 15);
                Image imageLink = SetSettingsImage(inputImageLinkStream, 425, 15);
                scalies.Add(image.ScaledWidth);
                inputImageStream.Close();
                inputImageLinkStream.Close();
                DefualtContent(reader, stamper, image, imageLink, logotype, baseFont, link, i);
                SetRectangleLink(stamper, imageLink, i, link);
            }
            return scalies.ToArray();
        }

        private float[] AddContentsHorizontal(PdfReader reader, PdfStamper stamper, Image logotype,
            BaseFont baseFont, string link)
        {
            PdfReaderContentParser parser = new(reader);
            List<float> scalies = new();
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                MarginFinder finder = parser.ProcessContent(i, new MarginFinder());
                List<float> listX = finder.GetXList();
                listX.Sort();
                float x = listX[0];
                float yLogotype = 14;
                float scale;
                Image image;
                Image imageLink;
                
                Stream inputImageStream = new FileStream(_pathImage, FileMode.Open, FileAccess.Read, FileShare.Read);
                Stream inputImageLinkStream = new FileStream(_pathImageLink, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (x < _scale)
                {
                    float different = _scale - x;
                    scale = x - 1;
                    scalies.Add(scale);
                    _absoluteY -= different;
                    yLogotype -= different;
                    image = SetSettingsImage(inputImageStream, 15, 15, scale, scale);
                    imageLink = SetSettingsImage(inputImageLinkStream, 625, 15, scale, scale);
                }
                else
                {
                    image = SetSettingsImage(inputImageStream, 10, 15);
                    imageLink = SetSettingsImage(inputImageLinkStream, 625, 15);
                }
                inputImageStream.Close();
                inputImageLinkStream.Close();

                logotype.SetAbsolutePosition(700, yLogotype);

                DefualtContent(reader, stamper, image, imageLink, logotype, baseFont, link, i);
                SetRectangleLink(stamper, imageLink, i, link);
            }
            return scalies.ToArray();
        }

        private void DefualtContent(PdfReader reader, PdfStamper stamper, Image image, Image imageLink, Image logotype,
            BaseFont baseFont, string link, int index)
        {
            _pdfContentByte = stamper.GetOverContent(index);

            _pdfContentByte.AddImage(image);
            _pdfContentByte.AddImage(imageLink);
            _pdfContentByte.AddImage(logotype);
            _pdfContentByte.SetColorFill(BaseColor.BLACK);
            _pdfContentByte.SetFontAndSize(baseFont, _fontSize);

            AddText("Подписи проверены в НУЦ РК", _absoluteX, _absoluteY - (_fontSize * 2));
            AddText("Документ подписан в сервисе СЭД Sulpak", _absoluteX, _absoluteY - (_fontSize * 3));
            AddText("Вы можете проверить подлинность электронного документа по ссылке:", _absoluteX, _absoluteY - (_fontSize * 4));
            _pdfContentByte.SetLineWidth(0.1f);
            float endLine = imageLink.AbsoluteX - 3;
            string [] lineArray = GetLinesForLink(link, endLine);
            for (int i = 0; i < lineArray.Length; i++)
            {
                string first = lineArray[i];
                float lengthLine = LengthPixels(first);

                float absoluteY = _absoluteY - (_fontSize * (5 + i));

                AddText(first, _absoluteX, absoluteY);
                _pdfContentByte.MoveTo(_absoluteX, absoluteY - 1f);
                _pdfContentByte.LineTo(lengthLine + _absoluteX, absoluteY - 1f);
                _pdfContentByte.Stroke();
            }
        }

        private static void SetRectangleLink(PdfStamper stamper, Image imageLink, int index, string link)
        {
            PdfContentByte overContent = stamper.GetOverContent(index);
            Rectangle rectangle = new Rectangle(imageLink.AbsoluteX, imageLink.AbsoluteY, imageLink.AbsoluteX + imageLink.ScaledWidth, imageLink.AbsoluteY + imageLink.ScaledHeight, 0);
            overContent.Rectangle(rectangle);
            PdfAnnotation pdfAnnotationLink = PdfAnnotation.CreateLink(stamper.Writer, rectangle, PdfAnnotation.HIGHLIGHT_INVERT, new PdfAction(link));
            stamper.AddAnnotation(pdfAnnotationLink, index);
        }

        private float LengthPixels(string text)
        {
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new System.Drawing.Bitmap(1, 1)))
            {
                System.Drawing.SizeF size = graphics.MeasureString(text, new System.Drawing.Font(_fontStyle, _fontSize, System.Drawing.GraphicsUnit.Pixel));
                return size.Width - 10;
            }
        }

        private string[] GetLinesForLink(string textOriginal, float endLine)
        {
            List<string> lineList = new();
            string tempLine = textOriginal;
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new System.Drawing.Bitmap(1, 1)))
            {
                System.Drawing.SizeF size;
                while (tempLine.Length > 0)
                {
                    size = graphics.MeasureString(tempLine, new System.Drawing.Font(_fontStyle, _fontSize, System.Drawing.GraphicsUnit.Pixel));
                    if (size.Width + _absoluteX  <= endLine)
                    {
                        lineList.Add(tempLine);

                        if (textOriginal.Length - tempLine.Length != 0)
                        {
                            int index = tempLine.Length;
                            string remainLine = textOriginal[index..];
                            lineList.AddRange(GetLinesForLink(remainLine, endLine));
                        }
                        
                        break;
                    }
                    tempLine = tempLine.Remove(tempLine.Length - 1);
                }
                
                return lineList.ToArray();
            }

        }

        public void AddSecondQR(string pathImage, string pathFile, string newFile, float[] scalies)
        {
            byte[] bytes = File.ReadAllBytes(pathFile);

            using (FileStream fileStream = File.Create(newFile)) fileStream.Write(bytes, 0, bytes.Length);

            using (Stream inputPdfStream = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(newFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);

                PdfReaderContentParser parser = new(reader);
                PdfStamper stamper = new(reader, outputPdfStream);

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    Stream inputImageStream = new FileStream(pathImage, FileMode.Open, FileAccess.Read, FileShare.Read);
                    float scale = scalies[0];
                    Image image = SetSettingsImage(inputImageStream, 110, 15, scale, scale);

                    inputImageStream.Close();

                    _pdfContentByte = stamper.GetOverContent(i);
                    _pdfContentByte.AddImage(image);
                }

                stamper.Close();
                reader.Close();
                inputPdfStream.Close();
                outputPdfStream.Close();
            }

            //return newFile;
        }

        private void AddText(string text, float absoluteX, float absoluteY)
        {
            _pdfContentByte.BeginText();
            _pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text,
                absoluteX, absoluteY, 0);
            _pdfContentByte.EndText();
        }

        private static float SearchAddressPosition(MarginFinder finder, Image image)
        {
            List<float> list = finder.GetXList();

            list = list.Distinct().ToList();

            list.Sort();

            List<Address> addressList = new();

            for (int i = 0; i < list.Count - 1; i++)
            {
                var result = list[i + 1] - list[i];

                Address address = new()
                {
                    Different = result,

                    Row = list[i]
                };

                addressList.Add(address);
            }

            float margin = image.ScaledHeight / 4;
            float scaled = image.ScaledHeight + margin;

            Address returnAdrdress = addressList.Where(x => x.Different >= scaled).FirstOrDefault();
            returnAdrdress ??= addressList[0];

            return returnAdrdress.Row - margin;
        }

        public class Address
        {
            public float Different { get; set; }
            public float Row { get; set; }
        }
    }

    public class MarginFinder : IRenderListener
    {
        private RectangleJ textRectangle = null;
        private RectangleJ currentPathRectangle = null;

        private List<float> _xList = new();

        public void RenderText(TextRenderInfo renderInfo)
        {
            string currentString = renderInfo.GetText();
            float x = renderInfo.GetBaseline().GetBoundingRectange().X;
            if (currentString.Count(x => x != ' ') != 0)
            {
                _xList.Add(x);
            }

            renderInfo.GetText();

            if (textRectangle == null)
                textRectangle = renderInfo.GetDescentLine().GetBoundingRectange();
            else
                textRectangle.Add(renderInfo.GetDescentLine().GetBoundingRectange());

            textRectangle.Add(renderInfo.GetAscentLine().GetBoundingRectange());
        }

        public List<float> GetXList()
        {
            return _xList;
        }
        public float GetLlx()
        {
            return textRectangle.X;
        }

        public float GetLly()
        {
            return textRectangle.Y;
        }

        public void SetX()
        {
            textRectangle.X += 20;
        }

        public float GetUrx()
        {
            return textRectangle.X + textRectangle.Width;
        }

        public float GetUry()
        {
            return textRectangle.Y + textRectangle.Height;
        }

        public float GetWidth()
        {
            return textRectangle.Width;
        }

        public float GetHeight()
        {
            return textRectangle.Height;
        }

        public void BeginTextBlock()
        {
        }

        public void EndTextBlock()
        {
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            Matrix imageCtm = renderInfo.GetImageCTM();
            Vector a = new Vector(0, 0, 1).Cross(imageCtm);
            Vector b = new Vector(1, 0, 1).Cross(imageCtm);
            Vector c = new Vector(0, 1, 1).Cross(imageCtm);
            Vector d = new Vector(1, 1, 1).Cross(imageCtm);
            LineSegment bottom = new LineSegment(a, b);
            LineSegment top = new LineSegment(c, d);
            if (textRectangle == null)
                textRectangle = bottom.GetBoundingRectange();
            else
                textRectangle.Add(bottom.GetBoundingRectange());

            textRectangle.Add(top.GetBoundingRectange());
        }


        public void ModifyPath(PathConstructionRenderInfo renderInfo)
        {
            List<Vector> points = new List<Vector>();
            if (renderInfo.Operation == PathConstructionRenderInfo.RECT)
            {
                float x = renderInfo.SegmentData[0];
                float y = renderInfo.SegmentData[1];
                float w = renderInfo.SegmentData[2];
                float h = renderInfo.SegmentData[3];
                points.Add(new Vector(x, y, 1));
                points.Add(new Vector(x + w, y, 1));
                points.Add(new Vector(x, y + h, 1));
                points.Add(new Vector(x + w, y + h, 1));
            }
            else if (renderInfo.SegmentData != null)
            {
                for (int i = 0; i < renderInfo.SegmentData.Count - 2; i += 2)
                {
                    points.Add(new Vector(renderInfo.SegmentData[i], renderInfo.SegmentData[i + 1], 1));
                }
            }

            foreach (Vector point in points)
            {
                var point1 = point.Cross(renderInfo.Ctm);
                RectangleJ pointRectangle = new RectangleJ(point1[Vector.I1], point1[Vector.I2], 0, 0);
                if (currentPathRectangle == null)
                    currentPathRectangle = pointRectangle;
                else
                    currentPathRectangle.Add(pointRectangle);
            }
        }


        public iTextSharp.text.pdf.parser.Path RenderPath(PathPaintingRenderInfo renderInfo)
        {
            if (renderInfo.Operation != PathPaintingRenderInfo.NO_OP)
            {
                if (textRectangle == null)
                    textRectangle = currentPathRectangle;
                else
                    textRectangle.Add(currentPathRectangle);
            }
            currentPathRectangle = null;

            return null;
        }


        public void ClipPath(int rule)
        {
        }

        public RectangleJ GetTextRectangle()
        {
            return textRectangle;
        }
    }
}
