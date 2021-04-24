using System;
using System.IO;
using System.Windows.Forms;

namespace DocAuth
{
    public partial class DocScan : Form
    {
        public DocScan()
        {
            InitializeComponent();
        }



        private void uploadButton_Click(object sender, EventArgs e)
        {
            //To where your opendialog box get starting location. My initial directory location is desktop.
            openFileDialog.InitialDirectory = "C://Desktop";
            //Your opendialog box title name.
            openFileDialog.Title = "Select file to be upload.";
            //which type file format you want to upload in database. just add them.
            openFileDialog.Filter = "Select Valid Document(*.pdf)|*.pdf;";
            //FilterIndex property represents the index of the filter currently selected in the file dialog box.
            openFileDialog.FilterIndex = 1;
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.CheckFileExists)
                    {
                        UploadForm docForm = new UploadForm
                        {
                            FilePath = Path.GetFullPath(openFileDialog.FileName)
                        };
                        Hide();
                        docForm.Show();
                    }
                }
                else { MessageBox.Show("Please Upload document."); }
            }
            catch (Exception) { }
        }

        private void DocScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }
    }
}
