using iTextSharp.text.pdf;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Image = iTextSharp.text.Image;
using MessageBox = System.Windows.MessageBox;

namespace DocAuth
{
    public partial class UploadForm : Form
    {
        public UploadForm()
        {
            InitializeComponent();
            uploadButton.Enabled = false;
        }

        public string FilePath = "";
        private string uploadedFile = "";
        private string fileName = "";
        private Guid _documentLastGuid = Guid.Empty;
        private void modifyPDF(string path)
        {
            string output = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DocAuth";
            //string output = Directory.GetCurrentDirectory() + "\\pdfcache";
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            #region clean Existing cache files
            System.IO.DirectoryInfo di = new DirectoryInfo(output);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            #endregion

            _documentLastGuid = Guid.NewGuid();
            string uId = _documentLastGuid.ToString();
            using (Stream inputPdfStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(output + uId + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(uId, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                string QRSize = System.Configuration.ConfigurationManager.AppSettings["QRSize"];
                int QRCodeSize = 1;
                if (!string.IsNullOrWhiteSpace(QRSize) && int.TryParse(QRSize, out QRCodeSize))
                {
                    QRCodeSize = Convert.ToInt32(QRSize);
                }
                Bitmap qrCodeImage = qrCode.GetGraphic(1);

                string storeQRCode = System.Configuration.ConfigurationManager.AppSettings["storeQRCode"];
                bool storeQRCodeBoolean = false;
                if (!string.IsNullOrWhiteSpace(storeQRCode) && bool.TryParse(storeQRCode, out storeQRCodeBoolean))
                {
                    storeQRCodeBoolean = Convert.ToBoolean(storeQRCode);
                }
                if (storeQRCodeBoolean)
                {
                    Bitmap qrCodeImageSave = qrCode.GetGraphic(20);
                    qrCodeImageSave.Save(output + uId + ".png");
                }

                PdfReader reader = new PdfReader(inputPdfStream);
                PdfStamper stamper = new PdfStamper(reader, outputPdfStream);

                Image image = Image.GetInstance(qrCodeImage, ImageFormat.Bmp);
                float qrWidth = image.Width;
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfContentByte pdfContentByte = stamper.GetOverContent(i);
                    image.SetAbsolutePosition(pdfContentByte.PdfDocument.PageSize.Width - qrWidth, 0);
                    pdfContentByte.AddImage(image);

                    var image2 = Image.GetInstance(Properties.Resources.docscanstamp, ImageFormat.Png);
                    image2.ScaleAbsolute(130f, 20f);
                    image2.SetAbsolutePosition((pdfContentByte.PdfDocument.PageSize.Width - (qrWidth + 140)), 5);
                    pdfContentByte.AddImage(image2);
                }

                stamper.Close();

                PdfiumViewer.PdfDocument pdfDocs = PdfiumViewer.PdfDocument.Load(this, output + uId + ".pdf");
                uploadedFile = output + uId + ".pdf";
                fileName = Path.GetFileName(path);
                pdfviewer.Document = pdfDocs;
            }
        }

        public DocAuth.Model.Document getFileById(Guid id)
        {
            DocAuth.Model.Document doc = null;
            using (DocsDBContext db = new DocsDBContext())
            {
                doc = db.Documents.FirstOrDefault(x => x.Id == id);
            }
            return doc;
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] file;
                using (FileStream stream = new FileStream(uploadedFile, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        file = reader.ReadBytes((int)stream.Length);
                    }
                }
                DocAuth.Model.Document doc = new Model.Document
                {
                    Content = file,
                    createdDate = DateTime.Now,
                    FileName = fileName,
                    Id = _documentLastGuid
                };
                using (DocsDBContext db = new DocsDBContext())
                {
                    db.Documents.Add(doc);
                    db.SaveChanges();
                }
                MessageBox.Show("Document uploaded successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void generateQRButton_Click(object sender, EventArgs e)
        {
            try
            {
                modifyPDF(FilePath);
                uploadButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DocForm_Load(object sender, EventArgs e)
        {
            try
            {
                PdfiumViewer.PdfDocument pdfDocs = PdfiumViewer.PdfDocument.Load(this, FilePath);
                pdfviewer.Document = pdfDocs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DocForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Hide();
            DocScan docForm = new DocScan();
            docForm.Show();
        }
    }
}
