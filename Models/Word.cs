using Xceed.Words.NET;
using Xceed.Document.NET;

namespace ESignatureService.Models
{
    public class Word
    {
        static public Word _instance = new();

        public void ExportAsDocx(string pathImage, string pathFile, string newFile)
        {
            DocX document = DocX.Load(pathFile);

            Image image = document.AddImage(pathImage);

            Picture picture = image.CreatePicture();

            Paragraph title = document.InsertParagraph();
            title.Alignment = Alignment.center;

            title.AppendPicture(picture);

            document.SaveAs(newFile);

            //return newFile;
        }
    }
}
