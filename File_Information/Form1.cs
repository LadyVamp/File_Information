using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Information
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            rbDoc.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((rbDoc.Checked == true) && (rbRtf.Checked == false))
                LoadMyDoc();
            else if ((rbRtf.Checked == true) && (rbDoc.Checked == false))
                LoadMyRtf();
        }


        public void LoadMyDoc()
        {
            // Create an OpenFileDialog to request a file to open.
            OpenFileDialog f = new OpenFileDialog();

            // Initialize the OpenFileDialog to look for RTF files.
            f.DefaultExt = "*.doc";
            f.Filter = "DOC Files|*.doc";

            // Determine whether the user selected a file from the OpenFileDialog.
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
               f.FileName.Length > 0)
            {
                // Load the contents of the file into the RichTextBox.
                richTextBox1.LoadFile(f.FileName);
            }

            //if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
                GetFileInformation(f.FileName);
            //}
        }

        public void LoadMyRtf()
        {
            OpenFileDialog f = new OpenFileDialog();

            f.DefaultExt = "*.rtf";
            f.Filter = "RTF Files|*.rtf";

            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
               f.FileName.Length > 0)
            {
                richTextBox1.LoadFile(f.FileName);
            }

            //if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
                GetFileInformation(f.FileName);
            //}
        }

        private void GetFileInformation(string fileName)
        {
            FileInfo f = new FileInfo(fileName);

            label1.Text = "Name - " + f.Name.ToString();
            label2.Text = "File Extension - " + f.Extension;
            label3.Text = "Creation Time - " + f.CreationTime.ToString();
            label4.Text = "Last Access Time - " + f.LastAccessTime.ToString();
            label5.Text = "Last Write Time - " + f.LastWriteTime.ToString();
            label6.Text = "Directory Name - " + f.DirectoryName;
            label7.Text = "Full Name - " + f.FullName;
            label8.Text = "File Size - " + (f.Length / 1024).ToString() + "KB";
        }
    }
}
