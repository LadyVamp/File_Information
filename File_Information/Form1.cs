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
using System.Data.SqlClient;

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

            // Initialize the OpenFileDialog to look for DOC files.
            f.DefaultExt = "*.doc";
            f.Filter = "DOC Files|*.doc";

            // Determine whether the user selected a file from the OpenFileDialog.
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
               f.FileName.Length > 0)
            {
                // Load the contents of the file into the RichTextBox.
                richTextBox1.LoadFile(f.FileName);
                GetFileInformation(f.FileName);
            }
        }

        public void LoadMyRtf()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.DefaultExt = "*.rtf";
            f.Filter = "RTF Files|*.rtf";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK && f.FileName.Length > 0)
            {
                richTextBox1.LoadFile(f.FileName);
                GetFileInformation(f.FileName);
            }
        }

        public void GetFileInformation(string FileName)
        {
            FileInfo f = new FileInfo(FileName);

            label1.Text = "Title - " + f.Name.ToString();
            label2.Text = "Type - " + f.Extension;
            label3.Text = "Date Create - " + f.CreationTime.ToString();
            label4.Text = "Date Change - " + f.LastWriteTime.ToString();
            label5.Text = "Path - " + f.DirectoryName;
            label6.Text = "Full Path - " + f.FullName;
            label7.Text = "Size - " + (f.Length / 1024).ToString() + "KB";
        }

        public void button2_Click(object sender, EventArgs e)
        {
            SaveFileToDatabase();
        }

        public void SaveFileToDatabase()
        {
            string connectionString = @"Data Source=DESKTOP-O9H5H8N;Initial Catalog=RepositoryDB4;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"INSERT INTO TFile VALUES (@Title, @Type, @DateCreate,  @DateChange, @Size, @Keywords, @Filecontent, @CatalogId)";
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 130);
                command.Parameters.Add("@Type", SqlDbType.NVarChar, 10);
                command.Parameters.Add("@DateCreate", SqlDbType.DateTime);
                command.Parameters.Add("@DateChange", SqlDbType.DateTime);
                command.Parameters.Add("@Size", SqlDbType.Int);
                command.Parameters.Add("@Keywords", SqlDbType.NVarChar, 100);
                command.Parameters.Add("@Filecontent", SqlDbType.NVarChar, 10000);
                command.Parameters.Add("@CatalogId", SqlDbType.Int);

                string path = label6.Text;  //путь к файлу для загрузки
                string title = label1.Text;  // заголовок файла
                string type = label2.Text; //расширение

                //размер
                string size = label7.Text;
                //string size = Int32.Parse(label7.Text);
                //string size = int.Parse(label7.Text); 

                string fileContent = richTextBox1.Text; //содержимое

                // передаем данные в команду через параметры
                command.Parameters["@Title"].Value = title;
                command.Parameters["@Type"].Value = type;
                command.Parameters["@DateCreate"].Value = dateTimePicker1.Value.Date;
                command.Parameters["@DateChange"].Value = dateTimePicker2.Value.Date;
                command.Parameters["@Size"].Value = size;
                //command.Parameters["@Size"].Value = Int32.Parse(size); 
                //command.Parameters["@Size"].Value = 123;
                command.Parameters["@Keywords"].Value = "test123";
                command.Parameters["@Filecontent"].Value = fileContent;
                command.Parameters["@CatalogId"].Value = 1;

                command.ExecuteNonQuery();
            }
        }

        
    }
}
