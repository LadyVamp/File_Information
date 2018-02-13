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
using HtmlAgilityPack;
using System.Xml;
using System.Net;

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
            else if ((rbHtml.Checked == true) && (rbRtf.Checked == false) && (rbDoc.Checked == false) && (rbDocx.Checked == false))
                LoadMyHtml();
        }

        public void ClearRtb()
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            richTextBox3.Clear();
        }

        public void ClearTxt()
        {
            txtKeywords.Text = "";
            rtbDescription.Text = "";
        }

        public void LoadMyDoc()
        {
            // Create an OpenFileDialog to request a file to open.
            OpenFileDialog f = new OpenFileDialog();

            // Initialize the OpenFileDialog to look for DOC files.
            f.DefaultExt = "*.doc";
            f.Filter = "DOC Files|*.doc";
            ClearRtb();
            ClearTxt();
            try
            {
                // Determine whether the user selected a file from the OpenFileDialog.
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                   f.FileName.Length > 0)
                {
                    // Load the contents of the file into the RichTextBox.
                    richTextBox1.LoadFile(f.FileName);
                    GetFileInformation(f.FileName);
                    //удалить стоп-слова из rtb1 и вставить результат в невидимый rtb3
                    //в rbt3 текст без стилей и почищенный от стоп-слов
                    richTextBox3.AppendText(StopwordTool.RemoveStopwords(richTextBox1.Text));
                }
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show("В словаре стоп-слов есть повтор!!!", ex.Message,  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        public void LoadMyRtf()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.DefaultExt = "*.rtf";
            f.Filter = "RTF Files|*.rtf";
            ClearRtb();
            ClearTxt();
            try
            {
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK && f.FileName.Length > 0)
                {
                    richTextBox1.LoadFile(f.FileName);
                    GetFileInformation(f.FileName);
                    //удалить стоп-слова из rtb1 и вставить результат в невидимый rtb3
                    richTextBox3.AppendText(StopwordTool.RemoveStopwords(richTextBox1.Text));
                }
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show("В словаре стоп-слов есть повтор!!!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadMyHtml()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.DefaultExt = "*.html";
            f.Filter = "HTML Files|*.html";
            ClearRtb();
            ClearTxt();
            try
            {
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK && f.FileName.Length > 0)
                {
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.Load(f.FileName, true); //true чтобы русские символы были не иероглифами
                    GetFileInformation(f.FileName);
                    ////удалить стоп-слова из rtb1 и вставить результат в невидимый rtb3
                    //richTextBox3.AppendText(StopwordTool.RemoveStopwords(richTextBox1.Text));

                    //body в rtb1
                    var node = htmlDoc.DocumentNode.SelectSingleNode("//body");
                    richTextBox1.AppendText(node.OuterHtml);

                    //title 
                    var title = htmlDoc.DocumentNode.SelectSingleNode("//title");
                    label1.Text = title.InnerText;

                    //keywords
                    HtmlNode mdnode1 = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
                    if (mdnode1 != null)
                    {
                        HtmlAttribute key;
                        key = mdnode1.Attributes["content"];
                        string keywords = key.Value;
                        txtKeywords.Text = keywords;
                    }

                    //description
                    HtmlNode mdnode2 = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
                    if (mdnode2 != null)
                    {
                        HtmlAttribute descr;
                        descr = mdnode2.Attributes["content"];
                        string descrip = descr.Value;
                        rtbDescription.Text = descrip;
                    }

                    //post
                    try
                    {
                        HtmlNode[] post = htmlDoc.DocumentNode.SelectNodes("//div[@class='post__body post__body_full']").Where(x => x.InnerHtml.Contains("post__text")).ToArray();
                        //удалить теги с содержимым
                        htmlDoc.DocumentNode.Descendants()
                                        .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "img" || n.Name == "br" || n.Name == "a")
                                        .ToList()
                                        .ForEach(n => n.Remove());
                        foreach (HtmlNode item in post)
                        {
                            richTextBox3.AppendText(item.InnerText); //InnerText для удаления тегов без удаления содержимого
                        }
                    }
                    catch (ArgumentNullException ex)
                    {
                        MessageBox.Show("Отсутствует класс post__body post__body_full", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                   
                }
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show("В словаре стоп-слов есть повтор!!!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

        //      TEST!!! откроется ли этот формат
        public void LoadMyDocx()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.DefaultExt = "*.docx";
            f.Filter = "DOCX Files|*.docx";
            ClearRtb();
            ClearTxt();
            try
            {
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK && f.FileName.Length > 0)
                {
                    richTextBox1.LoadFile(f.FileName);
                    GetFileInformation(f.FileName);
                    //удалить стоп-слова из rtb1 и вставить результат в невидимый rtb3
                    richTextBox3.AppendText(StopwordTool.RemoveStopwords(richTextBox1.Text));
                }
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show("В словаре стоп-слов есть повтор!!!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //Заполнение лейблов
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
                string keyword = txtKeywords.Text; // ключевые слова
                string fileContent = richTextBox3.Text; // содержимое

                // передаем данные в команду через параметры
                command.Parameters["@Title"].Value = title;
                command.Parameters["@Type"].Value = type;
                command.Parameters["@DateCreate"].Value = dateCreate;
                command.Parameters["@DateChange"].Value = dateChange;
                command.Parameters["@Size"].Value = size;
                command.Parameters["@Keywords"].Value = keyword;
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

        // Удалить стоп-слова
        static class StopwordTool
        {
           //Словарь стоп-слов
           static Dictionary<string, bool> _stops = new Dictionary<string, bool>
          {
               //в словаре не должно быть повторов!!!
               { "1", true }, { "2", true }, { "3", true }, { "4", true }, { "5", true }, { "6", true }, { "7", true }, { "8", true }, { "9", true }, { "0", true },
               { "в", true }, { "на", true }, { "из", true }, { "к", true }, { "у", true }, { "по", true }, { "из-за", true }, { "по-над", true }, { "под", true }, { "около", true },
               { "вокруг", true }, { "перед", true }, { "возле", true }, { "до", true }, { "через", true }, { "с", true },
               { "в течение", true }, { "накануне", true }, { "в ходе", true }, { "от", true }, { "со зла", true }, { "за", true }, { "в силу", true }, { "по случаю", true }, { "благодаря", true }, { "ввиду", true },
               { "вследствие", true }, { "по причине", true }, { "для", true }, { "ради", true }, { "без", true }, { "о", true }, { "об", true }, { "про", true },
               { "насчет", true }, { "относительно", true }, { "во", true }, { "однако", true }, { "тип", true }, { "давайте", true },
               {"а", true }, { "абы", true }, { "аж", true }, { "ан", true }, { "благо", true }, { "буде", true }, { "будто", true }, { "вроде", true }, { "да", true },
               { "дабы", true }, { "даже", true }, { "едва", true }, { "ежели", true }, { "если", true }, { "же", true }, { "затем", true }, { "зато", true }, { "и", true },
               { "ибо", true }, { "или", true }, { "итак", true }, { "кабы", true }, { "как", true }, { "когда", true }, { "коли", true }, { "коль", true }, { "ли", true },
               { "либо", true }, { "лишь", true }, { "нежели", true }, { "но", true }, { "пока", true }, { "покамест", true }, { "покуда", true }, { "поскольку", true },
               { "притом", true }, { "причем", true }, { "пускай", true }, { "пусть", true }, { "раз", true }, { "разве", true }, { "ровно", true }, { "сиречь", true },
               { "словно", true }, { "так", true }, { "также", true }, { "то", true }, { "тоже", true }, { "только", true }, { "точно", true }, { "хоть", true },
               { "хотя", true }, { "чем", true }, { "чисто", true }, { "что", true }, { "чтоб", true }, { "чтобы", true }, { "чуть", true }, { "якобы", true },
               { "неужели", true }, { "что за", true }, { "ну и", true }, { "ишь", true }, { "еще бы", true }, { "ну", true }, { "ещё", true },
               { "ведь", true }, { "ни", true }, { "уж", true }, { "уже", true }, { " то", true }, { "всё-таки", true }, { "все же", true },
               { "не", true }, { "нет", true }, { "вовсе не", true }, { "отнюдь не", true }, { "ага", true }, { "угу", true }, { "вот", true }, { "вон", true }, { "вот и", true }, { "всего лишь", true },
               { "единственно", true }, { "почти", true }, { "исключительно", true }, { "именно", true }, { "прямо", true }, { "всего", true }, { " в точности", true }, { "едва ли", true }, { "вряд ли", true },
               { "навряд ли", true }, { "авось", true }, { "я", true }, { " я", true }, { "я ", true },{ " я ", true },  { "мы", true }, { "ты", true }, { "вы", true }, { "он", true }, { "она", true }, { "оно", true }, { "они", true },
               { "себя", true }, { "мой", true }, { "моя", true }, { "мое", true }, { "мои", true }, { "наш", true }, { "наша", true }, { "наше", true },
               { "наши", true }, { "твой", true }, { "твоя", true }, { "твое", true }, { "твои", true }, { "ваш", true }, { "ваша", true }, { "ваше", true },
               { "ваши", true }, { "его", true }, { "ему", true }, { "ее", true }, { "их", true }, { "	кто", true }, { "какой", true }, { "каков", true },
               { "чей", true }, { "который", true }, { "сколько", true }, { "где", true }, { "куда", true }, { "зачем", true },
               { "столько", true }, { "этот", true }, { "тот", true }, { "такой", true }, { "таков", true }, { "тут", true }, { "здесь", true },
               { "сюда", true }, { "туда", true }, { "оттуда", true }, { "отсюда", true }, { "тогда", true }, { "поэтому", true },
               { "весь", true }, { "всякий", true }, { "все", true }, { "сам", true }, { "самый", true }, { "каждый", true }, { "любой", true },
               { "другой", true }, { "иной", true }, { "всяческий", true }, { "всюду", true }, { "везде", true }, { "всегда", true }, { "никто", true },
               { "ничто", true }, { "некого", true }, { "нечего", true }, { "никакой", true }, { "ничей", true }, { "некто", true }, { "нечто", true },
               { "некий", true }, { "некоторый", true }, { "несколько", true }, { "кое-кто", true }, { "кое-где", true }, { "кое-что", true },
               { "кое-куда", true }, { "какой-либо", true }, { "сколько-нибудь", true }, { "куда-нибудь", true }, { "зачем-нибудь", true },
               { "чей-либо", true }, { "это", true }, { "т", true }, { "public", true }, { "можно", true }, { "i", true }, { "иногда", true }, { "желательно", true }, { "p", true },
               { "при", true }, { "почему", true }, { "еще", true }, { "теперь", true }, { "самое", true }, { "во-первых", true }, { "во-вторых", true }, { "в-третьих", true }, { "вообще", true },
               { "of", true }, { "the", true }, { "некоторое", true }, { "после", true }, { "тем", true }
            };

                /// <summary>
                /// Chars that separate words.
                /// </summary>
                static char[] _delimiters = new char[]
                {
                    '.', ' ', ',', ':', ';',
                    '"', '-', '=', '·', '%', '<', '>', '!', '@', '#', '$', '%', '^', '&', '+', ')', '(', '{', '}', '«', '»', '?','/', '|', '\'',
                };

                /// <summary>
                /// Remove stopwords from string.
                /// </summary>
                public static string RemoveStopwords(string input)
                {
                    // 1 Split parameter into words
                    var words = input.Split(_delimiters,
                        StringSplitOptions.RemoveEmptyEntries);
                    // 2 Allocate new dictionary to store found words
                    var found = new Dictionary<string, bool>();
                    // 3 Store results in this StringBuilder
                    StringBuilder builder = new StringBuilder();
                    // 4 Loop through all words
                    foreach (string currentWord in words)
                    {
                        // 5 Convert to lowercase
                        string lowerWord = currentWord.ToLower();
                        // 6 If this is a usable word, add it
                        if (!_stops.ContainsKey(lowerWord) &&
                            !found.ContainsKey(lowerWord))
                        {
                            builder.Append(currentWord).Append(' ');
                            found.Add(lowerWord, true);
                        }
                    }
                    // 7 Return string with words removed
                    return builder.ToString().Trim();
                }
        } /*end static class StopwordTool*/

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

        //Анализ текста. Текст берется из richTextBox3 (visible=false)
        private void button3_Click(object sender, EventArgs e)
        {
            int cntWord = 0;
            int cntSymbol = 0;
            float average = 0;

            string[] slova = richTextBox3.Text.Split(new char[] { ' ', ',', '.', '!', ':', '?', ';', }, StringSplitOptions.RemoveEmptyEntries); //разбить текст на слова
            cntWord = slova.Length; //Кол-во слов в тексте
            cntSymbol = richTextBox3.Text.Length; //Кол-во символов в тексте

                try
                {
                    average = cntSymbol / cntWord;
                    richTextBox2.Text = " Кол-во слов в тексте: " + cntWord.ToString() + "\n" + " Кол-во символов в тексте: " + cntSymbol.ToString() + "\n" + " Среднее кол-во символов в слове: " + average.ToString() + "\n" + " Наиболее повторяющиеся слова в тексте:\n";
                    string[] splits = { ".", " ", ",", ":", ";", "<", ">", "!", "@", "#", "$", "%", "^", "&", "+", ")", "(", "{", "}", "[", "]", "\n", "\r", "<i>", "*", "\t", "\v", "\f", "|", "_", "`", Environment.NewLine };

                    List<string> list = new List<string>();
                    list = richTextBox3.Text.Split(splits, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<Word> words = new List<Word>();

                    var duplicate_words = list.GroupBy(x => x.ToLower()).Where(x => x.Count() > 1).Select(x => x.Key.ToLower());

                        foreach (string value in duplicate_words.ToList())
                        {
                            Regex reg = new Regex(value + " ", RegexOptions.IgnoreCase);
                            int n = 0;
                            foreach (Match match in reg.Matches(richTextBox3.Text))
                            {
                                richTextBox3.Select(match.Index, match.Length);
                                n++;
                            }
                            words.Add(new Word(value, n));
                        }                 

                    words.Sort(comparase);
                        if (words.Count >= 15) words.RemoveRange(15, words.Count - 15);

                    foreach (Word item in words)
                    {
                        if (item.word.Length > 2) //длина слова >2, чтобы убрать "я", "с" и цифры
                        {
                            richTextBox2.AppendText("\n" + " " + item.word + " (" + item.count.ToString() + ")");
                            txtKeywords.AppendText(item.word + ", ");                    
                        }
                    }
                    txtKeywords.Text = txtKeywords.Text.Remove((txtKeywords.Text.Length - 2)); //удалить запятую и пробел в конце

                }
                catch (DivideByZeroException ex) //файл не выбран => cntWord = 0 => деление на ноль
                {
                    MessageBox.Show("Сначала выберите файл!!!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            //для description, первые 500 символов
            string original = richTextBox1.Text;
            int maxLength = 500;
            string first500 = original.Substring(0, Math.Min(original.Length, maxLength));
            rtbDescription.AppendText(first500);

        }

        //TODO
        //добавить description (аннотация) в БД
        //добавить этот модуль к основной проге
    }
}
