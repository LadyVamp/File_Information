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
using System.Text.RegularExpressions;

namespace File_Information
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            rbDoc.Checked = true;
        }
        /* LoadFile 
         * +поддерживает rtf, doc
         * -не поддерживает html, docx, pdf, txt, odt, xml ...
        */
        private void button1_Click(object sender, EventArgs e)
        {
            if ((rbDoc.Checked == true) && (rbRtf.Checked == false) && (rbDocx.Checked == false))
                LoadMyDoc();
            else if ((rbDocx.Checked == true) && (rbDoc.Checked == false) && (rbRtf.Checked == false))
                LoadMyDocx();
            else if ((rbRtf.Checked == true) && (rbDoc.Checked == false) && (rbDocx.Checked == false))
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

        //      TEST!!!
        public void LoadMyDocx()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.DefaultExt = "*.docx";
            f.Filter = "DOCX Files|*.docx";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK && f.FileName.Length > 0)
            {
                richTextBox1.LoadFile(f.FileName);
                GetFileInformation(f.FileName);
            }
        }



        public void GetFileInformation(string FileName)
        {
            FileInfo f = new FileInfo(FileName);

            label1.Text = f.Name.ToString();
            label2.Text = f.Extension;
            label3.Text = f.CreationTime.ToString();
            label4.Text = f.LastWriteTime.ToString();
            label5.Text = f.DirectoryName;
            label6.Text = f.FullName;
            label7.Text = (f.Length / 1024).ToString(); //верно, не пихать строку к int!
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

                string title = label1.Text; // заголовок файла
                string type = label2.Text; // расширение
                string dateCreate = label3.Text; // дата создания
                string dateChange = label4.Text; //  дата изменения
                string path = label6.Text;  //путь к файлу для загрузки
                string size = label7.Text; // размер
                string fileContent = richTextBox1.Text; // содержимое

                // передаем данные в команду через параметры
                command.Parameters["@Title"].Value = title;
                command.Parameters["@Type"].Value = type;
                command.Parameters["@DateCreate"].Value = dateCreate;
                command.Parameters["@DateChange"].Value = dateChange;
                command.Parameters["@Size"].Value = size;
                command.Parameters["@Keywords"].Value = "test123";
                command.Parameters["@Filecontent"].Value = fileContent;
                command.Parameters["@CatalogId"].Value = 1;

                try
                {
                    if (command.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Файл успешно добавлен в БД");
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    connection.Close();
                }
             
               
            }
        }

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    //// v1 находит точное соответствие, не учитывает окончания
        //    richTextBox1.SelectedText.ToLower();
        //    string[] textArray = richTextBox1.Text.Split(new char[] { ' ', ',', '.', '!', ':', '?', ';' }, StringSplitOptions.RemoveEmptyEntries);
        //    var result = textArray.GroupBy(x => x)
        //                      .Where(x => x.Count() > 1)
        //                      .Select(x => new { Word = x.Key, Frequency = x.Count() });

        //    foreach (var item in result)
        //    {
        //        richTextBox2.Text = ("Слово: " + item.Word + "\nКоличество повторов: " + item.Frequency);
        //    }
        //}


        // v2 кол-во слов и 20 наиболее повторяющихся слов в тексте
        //todo: убрать стоп-слова
        class Word
        {
            public int count;
            public string word;
            public Word(string word, int count)
            {
                this.count = count;
                this.word = word;
            }
        }

        private int comparase(Word a, Word b)
        {
            if (a.count == b.count) return 0;
            if (a.count > b.count) return -1;
            else return 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] slova = richTextBox1.Text.Split(new char[] { ' ', ',', '.', '!', ':', '?', ';', }, StringSplitOptions.RemoveEmptyEntries);
            int kol = slova.Length;
            int sim = richTextBox1.Text.Length;
            richTextBox2.Text = " Количество слов в тексте: " + kol.ToString() + "\n\n " + "Двадцать наиболее повторяющихся слов в тексте:\n";
            int pov = slova.Length;

            string[] splits = { ".", " ", ",", ":", ";", "<", ">", "!", "@", "#", "$", "%", "^", "&", "+", ")", "(", "{", "}", "[", "]", "\n", "\r", "<i>", Environment.NewLine };
            List<string> list = new List<string>();
            list = richTextBox1.Text.Split(splits, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<Word> words = new List<Word>();
            var duplicate_words = list.GroupBy(x => x.ToLower()).Where(x => x.Count() > 1).Select(x => x.Key.ToLower());

            foreach (string value in duplicate_words.ToList())
            {
                Regex reg = new Regex(value + " ", RegexOptions.IgnoreCase);
                int n = 0;
                foreach (Match match in reg.Matches(richTextBox1.Text))
                {
                    richTextBox1.Select(match.Index, match.Length);
                    //richTextBox1.SelectionColor = Color.Red;
                    //richTextBox1.DeselectAll();
                    n++;
                }

                //richTextBox1.SelectionStart = richTextBox1.TextLength;
                //richTextBox1.SelectionColor = Color.Black; 

                words.Add(new Word(value, n));
            }

            words.Sort(comparase);
            if (words.Count >= 20) words.RemoveRange(20, words.Count - 20);

            foreach (Word item in words)
                richTextBox2.AppendText("\n" + item.word + " - " + item.count.ToString());

        }






    }
}
