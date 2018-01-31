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

        private void button3_Click(object sender, EventArgs e)
        {
            //// v1 находит точное соответствие, не учитывает окончания
            richTextBox1.SelectedText.ToLower();
            string[] textArray = richTextBox1.Text.Split(new char[] { ' ', ',', '.', '!', ':', '?', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var result = textArray.GroupBy(x => x)
                              .Where(x => x.Count() > 1)
                              .Select(x => new { Word = x.Key, Frequency = x.Count() });

            foreach (var item in result)
            {
                richTextBox2.Text = ("Слово: " + item.Word + "\nКоличество повторов: " + item.Frequency);
            }

            //v2 не работает
            ////var x = File.ReadAllText("Your path to file");
            //var x =  richTextBox1.SelectedText.ToLower();
            //var words = x.Split(' ', '-', ':', '.', '"', '\'', '!', '?').Where(q => !string.IsNullOrEmpty(q));
            //var uniqWrds = words.Select(q => q.ToLower().Trim()).Distinct();
            //var result = new Dictionary<string, int>();
            //foreach (var word in uniqWrds)
            //{
            //    result.Add(word, words.Count(q => q.ToLower().Equals(word)));
            //}
            //result = result.OrderByDescending(q => q.Value).ToList().Take(5).ToDictionary(key => key.Key, value => value.Value);
            //foreach (var word in result)
            //{
            //    //richTextBox2.Text=("Word: {word.Key}. Count: {word.Value}");
            //    richTextBox2.Text = ("Слово: " + word.Key + "\nКоличество повторов: " + word.Value);
            //}
            ////Console.ReadKey();

            ////v3
            //if (string.IsNullOrEmpty(richTextBox1.Text) || richTextBox1.Text.Length != 1) MessageBox.Show("Не все поля заполнены!");
            //else
            //{
            //    string text = richTextBox1.Text;
            //    char c = Char.Parse(richTextBox1.Text);
            //    int count = 0;

            //    for (int i = 0; i < text.Length; i++)
            //    {
            //        if (text[i] == c) count++;
            //    }
            //    richTextBox2.Text = count.ToString();
            //}


            //v4 не работает
            ////string text = "шел шел зашел зашел зашел пришел пришел пришел пришел ушел ушел ушел ушел ушел";
            //string text = richTextBox1.SelectedText;
            ////Приведем текст в массив слов
            //string[] ArString = text.Split(new char[] { ' ' });
            //Dictionary<string, int> dr = new Dictionary<string, int>();
            ////Размещаем слова в словаре и подсчитываем частотность
            //foreach (string s in ArString)
            //    if (dr.Keys.Contains(s)) dr[s]++;
            //    else dr.Add(s, 1);
            //string S = ""; int k = 0;
            ////Отбираем 10 наиболее частотных слов и в нашем случае отображем их в сообщении
            //foreach (KeyValuePair<string, int> kk in dr.OrderByDescending(x => x.Value))
            //{
            //    S += kk.Key + " " + kk.Value.ToString() + "\n";
            //    if (k == 10) break;
            //}
            //MessageBox.Show(S);


            //const string delimitr = " ,.!?";
            ////var words = File.ReadAllText("text.txt")
            //var words = richTextBox1.SelectedText
            //    .Split(delimitr.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            //    .GroupBy(s => s).ToDictionary(s => new KeyValuePair<string, int>(s.Key, s.Count()));
            //foreach (var i in words)
            //    //Console.WriteLine("{0}: {1}", i.Key, i.Value);
            //    richTextBox2.Text = ("Слово: " + i.Key + "\nКоличество повторов: " + i.Value);






        }
    }
}
