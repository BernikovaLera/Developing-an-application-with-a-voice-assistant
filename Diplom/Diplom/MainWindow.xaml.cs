using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Speech.Synthesis;
using NAudio;
using NAudio.Wave;
using System.Net;
using System.IO;
using System.Speech.Recognition;
using System.Speech.AudioFormat;
using System.Data.Entity;
using System.Xml.Linq;

namespace Diplom
{
    public partial class MainWindow : Window
    {
        DiplomEntities db = new DiplomEntities();
        DiplomEntities1 dba = new DiplomEntities1();
        WaveIn waveIn;                                                                                                          // WaveIn - поток для записи
        WaveFileWriter writer;                                                                                                  //Класс для записи в файл
        string outputFilename = "demo.wav";                                                                                     //Имя файла для записи
        bool ON = false;
        public MainWindow()
        {
            InitializeComponent();
            Closing += Window_Closing;
            StateChanged += Window_StateChanged;
            DataContext = this;
        }
        private void Question_Click(object sender, RoutedEventArgs e)
        {
            if (ON == false)
            {
                waveIn = new WaveIn();                                                                                           //Дефолтное устройство для записи (если оно имеется)
                waveIn.DeviceNumber = 0;                                                                                         //встроенный микрофон ноутбука имеет номер 0
                waveIn.DataAvailable += waveIn_DataAvailable;                                                                    //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                waveIn.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(waveIn_RecordingStopped);              //Прикрепляем обработчик завершения записи
                waveIn.WaveFormat = new WaveFormat(16000, 1);                                                                    //Формат wav-файла - принимает параметры - частоту дискретизации и количество каналов(здесь mono)
                writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);                                                  //Инициализируем объект WaveFileWriter
                label2.Content = "Идет запись...";
                //Question_button.Content = "Стоп";
                waveIn.StartRecording();                                                                                        //Начало записи
                ON = true;
            }
            else
            {
                waveIn.StopRecording();                                                                                         //Завершаем запись
                label2.Content = "Датчик записи";
                ON = false;
                //Question_button.Content = "Вопрос";
            }
        }
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.WriteData(e.Buffer, 0, e.BytesRecorded);                                                                     //Записываем данные из буфера в файл
        }
        void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            waveIn.Dispose();                                                                                                   //Окончание записи
            waveIn = null;
            writer.Close();
            writer = null;
        }
        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            SpeechSynthesizer ss = new SpeechSynthesizer();
            ss.Volume = 80;                                                                                                     // от 0 до 100
            ss.Rate = 0;                                                                                                        //от -10 до 10

            WebRequest request = WebRequest.Create("https://www.google.com/speech-api/v2/recognize?output=json&lang=ru-RU&key=AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw");
            request.Method = "POST";
            byte[] byteArray = File.ReadAllBytes(outputFilename);
            request.ContentType = "audio/l16; rate=16000";                                                                      //"16000";
            request.ContentLength = byteArray.Length;
            request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();                                                 //Получите ответ. //Откройте поток с помощью программы чтения потоков для легкого доступа.
            StreamReader reader = new StreamReader(response.GetResponseStream());                                              //Прочитайте содержание.

            string strtrs = reader.ReadToEnd();
            var rg = new Regex(@"transcript" + '"' + ":" + '"' + "([A-Z, А-Я, a-z,а-я, ,0-9]*)");
            var result = rg.Match(strtrs).Groups[1].Value;                                                                    //распознанный текст
            Users_question_TextBlock.Text = result;

            //Ошибка на пустой запрос
            if (Users_question_TextBlock.Text.Length == 0)
            {
                Users_question_TextBlock.Text = "Запрос не распознан";
                Bots_response_TextBlock.Text = "Повторите запрос";
                ss.SpeakAsync("Повторите запрос");
            }
            //Приветствие
            if (result.Contains("Привет") || result.Contains("привет"))                                         
            {
                Bots_response_TextBlock.Text = "Хомяк Валера слушает Вас";
                ss.SpeakAsync("Хомяк Валера слушает Вас");
            }
            //Как твои дела
            if (result.Contains("Как твои дела"))                                                                             
            {
                Bots_response_TextBlock.Text = "Спасибо, всё хорошо. А как ваши дела?";
                ss.SpeakAsync("Спасибо, всё хорошо. А как ваши дела");
            }
            //Хорошо
            if (result.Contains("хорошо") || result.Contains("Хорошо") || result.Contains("Отлично") || result.Contains("отлично"))      
            {
                Bots_response_TextBlock.Text = "Замечательно";
                ss.SpeakAsync("Замечательно");
            }
            //Плохо
            if (result.Contains("Плохо") || result.Contains("плохо") || result.Contains("Так себе") || result.Contains("так себе"))      
            {
                Bots_response_TextBlock.Text = "Надеюсь ваши дела станут лучше";
                ss.SpeakAsync("Надеюсь ваши дела станут лучше");
            }
            //Спасибо
            if (result.Contains("Спасибо"))                                                                                             
            {
                Bots_response_TextBlock.Text = "Ну, что вы, мне только в радость";
                ss.SpeakAsync("Ну, что вы, мне только в радость");
            }
            //Умение
            if (result.Contains("Что ты умеешь"))                                                                                       
            {
                Bots_response_TextBlock.Text = "Могу ответить на не сложные вопросы \nНайти в интернете интересующий вас вопрос";
                ss.SpeakAsync("Могу ответить на не сложные вопросы \nНайти в интернете интересующий вас вопрос");
            }
            //Шутка
            if (result.Contains("Расскажи шутку"))
            {
                Random rnd = new Random();
                string[] str = { "Никому нельзя верить: ни начальству, ни женщинам, а с 2012 года ещё и индейцам майя.",
                    "Идеальная работа – это где можно полежать, вкусные печеньки и начальник добрый как бабушка. А в конце месяца еще и денежки дают.",
                    "Бывают такие секунды, когда все решают минуты. И длится это часами.",
                    "Если у тебя появилась умная мысль, будь умным — держи эту мысль при себе.", 
                    "Если свет на кухне включать-выключать каждые 5 секунд, то тараканы умруг от челночного бега туда-обратно.",
                    "Когда дует ветер перемен, многие оказываются в пролете…",
                    "Такой я человек: зла не помню — приходится записывать.",
                    "Чтобы найти с человеком общий язык, иногда надо уметь помолчать.",
                    "Компьютер — это почти человек. Единственное серьезное различие — ему не свойственно сваливать свои ошибки на другой компьютер.",
                    "Россияне собираются осваивать Луну и Марс. Потому что проще все построить заново на другой планете, чем что-то изменить в России."};

                Bots_response_TextBlock.Text = (str[rnd.Next(0,10)]);
                ss.SpeakAsync(Bots_response_TextBlock.Text);
            }

            //Youtube
            if (result.Contains("Youtube") || result.Contains("youtube") || result.Contains("YouTube"))
            {
                Bots_response_TextBlock.Text = "Открываю Youtube";
                ss.SpeakAsync("Открываю Youtube");

                System.Diagnostics.Process.Start("https://www.youtube.com/");
            }
            //ВКонтакте
            if (result.Contains("ВКонтакте") || result.Contains("вконтакте") || result.Contains("ВК") || result.Contains("вк"))
            {
                Bots_response_TextBlock.Text = "Открываю ВКонтакте";
                ss.SpeakAsync("Открываю ВКонтакте");

                System.Diagnostics.Process.Start("https://vk.com/");
            }
            //Wildberries
            if (result.Contains("Wildberries") || result.Contains("wildberries"))
            {
                Bots_response_TextBlock.Text = "Открываю Wildberries";
                ss.SpeakAsync("Открываю Wildberries");

                System.Diagnostics.Process.Start("https://www.wildberries.ru");
            }
            //Госуслуги
            if (result.Contains("Госуслуги") || result.Contains("госуслуги"))
            {
                Bots_response_TextBlock.Text = "Открываю Госуслуги";
                ss.SpeakAsync("Открываю Госуслуги");

                System.Diagnostics.Process.Start("https://www.gosuslugi.ru");
            }
            //Ozon
            if (result.Contains("Ozon") || result.Contains("ozon"))
            {
                Bots_response_TextBlock.Text = "Открываю Ozon";
                ss.SpeakAsync("Открываю Ozon");

                System.Diagnostics.Process.Start("https://www.ozon.ru");
            }

            //Поиск
            if (result.Contains("Найди картинку") || result.Contains("Найди картину") || result.Contains("Найди картинки")
                || result.Contains("Найди картины") || result.Contains("Найди фото") || result.Contains("Найди фотографию")
                || result.Contains("Найди открытку") || result.Contains("Найди открытки") || result.Contains("Найди схему")
                || result.Contains("Найди схемы"))
            {
                Bots_response_TextBlock.Text = "Вот что я с могла найти по вашему запросу";
                ss.SpeakAsync("Вот что я с могла найти по вашему запросу");

                string searchquery = result;
                searchquery.Replace("найди", "");
                searchquery.Replace("Найди", "");
                string a = "Найди";
                System.Diagnostics.Process.Start("https://yandex.ru/images/search?from=tabbar&text= " + searchquery.Replace(a, ""));
            }
            //Поиск
            if (result.Contains("Найди видео") || result.Contains("Найди ролик") || result.Contains("Найди клип"))
            {
                Bots_response_TextBlock.Text = "Вот что я с могла найти по вашему запросу";
                ss.SpeakAsync("Вот что я с могла найти по вашему запросу");

                string searchquery = result;
                searchquery.Replace("найди", "");
                searchquery.Replace("Найди", "");
                string a = "Найди";
                System.Diagnostics.Process.Start("https://www.youtube.com/results?search_query= " + searchquery.Replace(a, ""));
            }
            //Поиск
            else if (result.Contains("Найди") || result.Contains("найди"))
            {

                Bots_response_TextBlock.Text = "Вот что я с могла найти по вашему запросу";
                ss.SpeakAsync("Вот что я с могла найти по вашему запросу");

                string searchquery = result;
                searchquery.Replace("найди", "");
                searchquery.Replace("Найди", "");
                string a = "Найди";
                System.Diagnostics.Process.Start("https://yandex.ru/search/?text=" + searchquery.Replace(a, ""));
            }
            //Поиск
            if (result.Contains("Хомяк Валера") || result.Contains("хомяк Валера"))
            {
                Bots_response_TextBlock.Text = "Вот что я с могла найти по вашему запросу";
                ss.SpeakAsync("Вот что я с могла найти по вашему запросу");

                string searchquery = result;
                searchquery.Replace("Хомяк Валера", "");
                searchquery.Replace("хомяк Валера", "");
                string b = "хомяк Валера";
                System.Diagnostics.Process.Start("https://yandex.ru/search/?text=" + searchquery.Replace(b, ""));
            }

            //Дзен
            if (result.Contains("Дзен") || result.Contains("дзен"))
            {
                Bots_response_TextBlock.Text = "Хотите почитать интересные статьи";
                ss.SpeakAsync("Хотите почитать интересные статьи");

                System.Diagnostics.Process.Start("https://zen.yandex.ru/?utm_source=main_stripe_big");
            }
            //Карты
            if (result.Contains("построить маршрут") || result.Contains("Построить маршрут") || result.Contains("Карты") || result.Contains("карты") || result.Contains("Яндекс карты") || result.Contains("яндекс карты"))
            {
                Bots_response_TextBlock.Text = "Вы куда-то собираетесь?";
                ss.SpeakAsync("Вы куда-то собираетесь?");

                System.Diagnostics.Process.Start("https://yandex.ru/maps/213/moscow/?ll=37.622504%2C55.753215&utm_source=main_stripe_big&z=10");
            }
            //Кинопоиск
            if (result.Contains("Кинопоиск") || result.Contains("кинопоиск") || result.Contains("КиноПоиск"))
            {
                Bots_response_TextBlock.Text = "Хорошее кино можно смотреть всегда";
                ss.SpeakAsync("Хорошее кино можно смотреть всегда");

                System.Diagnostics.Process.Start("https://www.kinopoisk.ru/?utm_source=main_stripe_big");
            }
            //Метро
            if (result.Contains("Метро") || result.Contains("метро"))
            {
                Bots_response_TextBlock.Text = "Открываю карты метро";
                ss.SpeakAsync("Открываю карты метро");

                System.Diagnostics.Process.Start("https://yandex.ru/metro/moscow?scheme_id=sc34974011");
            }
            //Музыка
            if (result.Contains("Музыка") || result.Contains("музыка"))
            {
                Bots_response_TextBlock.Text = "Время для отличной музыки всегда есть";
                ss.SpeakAsync("Время для отличной музыки всегда есть");

                System.Diagnostics.Process.Start("https://music.yandex.ru/home?utm_source=main_stripe_big");
            }
            //Новости
            if (result.Contains("Новости") || result.Contains("новости"))
            {
                Bots_response_TextBlock.Text = "Новости сегодня необычные";
                ss.SpeakAsync("Новости сегодня необычные");

                System.Diagnostics.Process.Start("https://yandex.ru/news/?utm_source=main_stripe_big");
            }
            //Переводчик
            if (result.Contains("Переводчик") || result.Contains("переводчик"))
            {
                Bots_response_TextBlock.Text = "Открываю переводчик";
                ss.SpeakAsync("Открываю переводчик");

                System.Diagnostics.Process.Start("https://translate.yandex.ru/?utm_source=main_stripe_big");
            }
            //Погода
            if (result.Contains("Погода") || result.Contains("погода"))
            {
                Bots_response_TextBlock.Text = "Вот что я с могла найти по вашему запросу";
                ss.SpeakAsync("Вот что я с могла найти по вашему запросу");

                System.Diagnostics.Process.Start("https://yandex.ru/pogoda/213?utm_source=serp&utm_campaign=wizard&utm_medium=desktop&utm_content=wizard_desktop_main&utm_term=title");
            }
            //Расписание электричек
            if (result.Contains("Расписание электричек") || result.Contains("расписание электричек"))
            {
                Bots_response_TextBlock.Text = "Открываю расписание электричек";
                ss.SpeakAsync("Открываю расписание электричек");

                System.Diagnostics.Process.Start("https://rasp.yandex.ru/?utm_source=yamain&utm_medium=allservices&utm_campaign=general_ru_desktop_no_all");
            }
            //Телепрограмма
            if (result.Contains("Телепрограмма") || result.Contains("телепрограмма"))
            {
                Bots_response_TextBlock.Text = "Сегодня есть интересное кино";
                ss.SpeakAsync("Сегодня есть интересное кино");

                System.Diagnostics.Process.Start("https://tv.yandex.ru/?utm_source=main_stripe_big");
            }

            //Гугл почта
            if (result.Contains("Гугл почта") || result.Contains("гугл почта") || result.Contains("google почта") || result.Contains("Google почта") || result.Contains("Gmail почта") || result.Contains("gmail почта"))
            {
                Bots_response_TextBlock.Text = "Открываю Гугл почту";
                ss.SpeakAsync("Открываю Гугл почту");

                System.Diagnostics.Process.Start("https://www.gmail.com/mail/help/intl/ru/about.html?de.");
            }
            //mail почта
            if (result.Contains("mail почта") || result.Contains("майл почта") || result.Contains("Майл почта"))
            {
                Bots_response_TextBlock.Text = "Открываю mail почту";
                ss.SpeakAsync("Открываю mail почта");

                System.Diagnostics.Process.Start("https://mail.ru/?from=logout&ref=main");
            }

            //Австралийский доллар
            if (result.Contains("Курс австралийский доллар") || result.Contains("курс австралийский доллар"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string AUD = el.Where(x => x.Attribute("ID").Value == "R01010").Select(x => x.Element("Value").Value).FirstOrDefault();
                double AU = Convert.ToDouble(AUD);

                Bots_response_TextBlock.Text = $"1 Австралийский доллар: { Math.Round(AU, 2)} Российских рублей";
                ss.SpeakAsync($"1 Австралийский доллар: { Math.Round(AU, 2)} Российских рублей");
            }
            //Азербайджанский манат
            if (result.Contains("Курс азербайджанский манат") || result.Contains("курс азербайджанский манат"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string AZN = el.Where(x => x.Attribute("ID").Value == "R01020A").Select(x => x.Element("Value").Value).FirstOrDefault();
                double AZ = Convert.ToDouble(AZN);

                Bots_response_TextBlock.Text = $"1 Азербайджанский манат: { Math.Round(AZ, 2)} Российских рублей";
                ss.SpeakAsync($"1 Азербайджанский манат: { Math.Round(AZ, 2)} Российских рублей");
            }
            //Фунт стерлингов
            if (result.Contains("Курс фунт стерлингов") || result.Contains("курс фунт стерлингов"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string GBP = el.Where(x => x.Attribute("ID").Value == "R01035").Select(x => x.Element("Value").Value).FirstOrDefault();
                double GB = Convert.ToDouble(GBP);

                Bots_response_TextBlock.Text = $"1 Фунт стерлингов: { Math.Round(GB, 2)} Российских рублей";
                ss.SpeakAsync($"1 Фунт стерлингов: { Math.Round(GB, 2)} Российских рублей");
            }
            //Белорусский рубль
            if (result.Contains("Курс белорусский рубль") || result.Contains("курс белорусский рубль"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string BYN = el.Where(x => x.Attribute("ID").Value == "R01090B").Select(x => x.Element("Value").Value).FirstOrDefault();
                double BY = Convert.ToDouble(BYN);

                Bots_response_TextBlock.Text = $"1 Белорусский рубль: { Math.Round(BY, 2)} Российских рублей";
                ss.SpeakAsync($"1 Белорусский рубль: { Math.Round(BY, 2)} Российских рублей");
            }
            //Болгарский лев
            if (result.Contains("Курс болгарский лев") || result.Contains("курс болгарский лев"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string BGN = el.Where(x => x.Attribute("ID").Value == "R01100").Select(x => x.Element("Value").Value).FirstOrDefault();
                double BG = Convert.ToDouble(BGN);

                Bots_response_TextBlock.Text = $"1 Болгарский лев: { Math.Round(BG, 2)} Российских рублей";
                ss.SpeakAsync($"1 Болгарский лев: { Math.Round(BG, 2)} Российских рублей");
            }
            //Бразильский реал
            if (result.Contains("Курс бразильский реал") || result.Contains("курс бразильский реал"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string BRL = el.Where(x => x.Attribute("ID").Value == "R01115").Select(x => x.Element("Value").Value).FirstOrDefault();
                double BR = Convert.ToDouble(BRL);

                Bots_response_TextBlock.Text = $"1 Бразильский реал: { Math.Round(BR, 2)} Российских рублей";
                ss.SpeakAsync($"1 Бразильский реал: { Math.Round(BR, 2)} Российских рублей");
            }
            //Гонконгских долларов
            if (result.Contains("Курс гонконский доллар") || result.Contains("курс гонконский доллар"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string HKD = el.Where(x => x.Attribute("ID").Value == "R01200").Select(x => x.Element("Value").Value).FirstOrDefault();
                double HK = Convert.ToDouble(HKD);

                Bots_response_TextBlock.Text = $"10 Гонконгских долларов: { Math.Round(HK, 2)} Российских рублей";
                ss.SpeakAsync($"10 Гонконгских долларов: { Math.Round(HK, 2)} Российских рублей");
            }
            //Датская крона
            if (result.Contains("Курс датская крона") || result.Contains("курс датская крона"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string DKK = el.Where(x => x.Attribute("ID").Value == "R01215").Select(x => x.Element("Value").Value).FirstOrDefault();
                double DK = Convert.ToDouble(DKK);

                Bots_response_TextBlock.Text = $"1 Датская крона: { Math.Round(DK, 2)} Российских рублей";
                ss.SpeakAsync($"1 Датская крона: { Math.Round(DK, 2)} Российских рублей");
            }
            //Курс доллара
            if (result.Contains("Курс доллара") || result.Contains("курс доллара"))                                                         
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string dollar = el.Where(x => x.Attribute("ID").Value == "R01235").Select(x => x.Element("Value").Value).FirstOrDefault();               
                double d = Convert.ToDouble(dollar);
               
                Bots_response_TextBlock.Text = $"1 Доллар США: { Math.Round(d, 2)} Российских рублей";
                ss.SpeakAsync($"1 Доллар США: { Math.Round(d, 2)} Российских рублей");
            }
            //Курс Евро
            if (result.Contains("Курс евро") || result.Contains("курс евро"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string EUR = el.Where(x => x.Attribute("ID").Value == "R01239").Select(x => x.Element("Value").Value).FirstOrDefault();
                double EU = Convert.ToDouble(EUR);

                Bots_response_TextBlock.Text = $"1 Евро: { Math.Round(EU, 2)} Российских рублей";
                ss.SpeakAsync($"1 Евро: { Math.Round(EU, 2)} Российских рублей");
            }
            //Индийских рупий
            if (result.Contains("Курс индийский рупий") || result.Contains("курс индийский рупий"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string INR = el.Where(x => x.Attribute("ID").Value == "R01270").Select(x => x.Element("Value").Value).FirstOrDefault();
                double IN = Convert.ToDouble(INR);

                Bots_response_TextBlock.Text = $"100 Индийских рупий: { Math.Round(IN, 2)} Российских рублей";
                ss.SpeakAsync($"100 Индийских рупий: { Math.Round(IN, 2)} Российских рублей");
            }
            //Казахстанских тенге
            if (result.Contains("Курс казахстанский тенге") || result.Contains("курс казахстанский тенге"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string KZT = el.Where(x => x.Attribute("ID").Value == "R01335").Select(x => x.Element("Value").Value).FirstOrDefault();
                double KZ = Convert.ToDouble(KZT);

                Bots_response_TextBlock.Text = $"100 Казахстанских тенге: { Math.Round(KZ, 2)} Российских рублей";
                ss.SpeakAsync($"100 Казахстанских тенге: { Math.Round(KZ, 2)} Российских рублей");
            }
            //Канадский доллар
            if (result.Contains("Курс канадский доллар") || result.Contains("курс канадский доллар"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string CAD = el.Where(x => x.Attribute("ID").Value == "R01350").Select(x => x.Element("Value").Value).FirstOrDefault();
                double CA = Convert.ToDouble(CAD);

                Bots_response_TextBlock.Text = $"1 Канадский доллар: { Math.Round(CA, 2)} Российских рублей";
                ss.SpeakAsync($"1 Канадский доллар: { Math.Round(CA, 2)} Российских рублей");
            }
            //Китайский юань
            if (result.Contains("Курс китайский юань") || result.Contains("курс китайский юань"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string CNY = el.Where(x => x.Attribute("ID").Value == "R01375").Select(x => x.Element("Value").Value).FirstOrDefault();
                double CN = Convert.ToDouble(CNY);

                Bots_response_TextBlock.Text = $"1 Китайский юань: { Math.Round(CN, 2)} Российских рублей";
                ss.SpeakAsync($"1 Китайский юань: { Math.Round(CN, 2)} Российских рублей");
            }
            //Норвежских крон
            if (result.Contains("Курс норвежский крон") || result.Contains("курс норвежский крон"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string NOK = el.Where(x => x.Attribute("ID").Value == "R01535").Select(x => x.Element("Value").Value).FirstOrDefault();
                double NO = Convert.ToDouble(NOK);

                Bots_response_TextBlock.Text = $"10 Норвежских крон: { Math.Round(NO, 2)} Российских рублей";
                ss.SpeakAsync($"10 Норвежских крон: { Math.Round(NO, 2)} Российских рублей");
            }
            //Сингапурский доллар
            if (result.Contains("Курс сингапурский доллар") || result.Contains("курс сингапурский доллар"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string SGD = el.Where(x => x.Attribute("ID").Value == "R01625").Select(x => x.Element("Value").Value).FirstOrDefault();
                double SG = Convert.ToDouble(SGD);

                Bots_response_TextBlock.Text = $"1 Сингапурский доллар: { Math.Round(SG, 2)} Российских рублей";
                ss.SpeakAsync($"1 Сингапурский доллар: { Math.Round(SG, 2)} Российских рублей");
            }
            //Турецких лир
            if (result.Contains("Курс турецкий лир") || result.Contains("курс турецкий лир"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string TRY = el.Where(x => x.Attribute("ID").Value == "R01700J").Select(x => x.Element("Value").Value).FirstOrDefault();
                double TR = Convert.ToDouble(TRY);

                Bots_response_TextBlock.Text = $"10 Турецких лир: { Math.Round(TR, 2)} Российских рублей";
                ss.SpeakAsync($"10 Турецких лир: { Math.Round(TR, 2)} Российских рублей");
            }
            //Украинских гривен
            if (result.Contains("Курс украинских гривен") || result.Contains("курс украинских гривен"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string UAH = el.Where(x => x.Attribute("ID").Value == "R01720").Select(x => x.Element("Value").Value).FirstOrDefault();
                double UA = Convert.ToDouble(UAH);

                Bots_response_TextBlock.Text = $"10 Украинских гривен: { Math.Round(UA, 2)} Российских рублей";
                ss.SpeakAsync($"10 Украинских гривен: { Math.Round(UA, 2)} Российских рублей");
            }
            //Чешских крон
            if (result.Contains("Курс чешский крон") || result.Contains("курс чешский крон"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string CZK = el.Where(x => x.Attribute("ID").Value == "R01760").Select(x => x.Element("Value").Value).FirstOrDefault();
                double CZ = Convert.ToDouble(CZK);

                Bots_response_TextBlock.Text = $"10 Чешских крон: { Math.Round(CZ, 2)} Российских рублей";
                ss.SpeakAsync($"10 Чешских крон: { Math.Round(CZ, 2)} Российских рублей");
            }
            //Шведских крон
            if (result.Contains("Курс шведский крон") || result.Contains("курс шведский крон"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string SEK = el.Where(x => x.Attribute("ID").Value == "R01770").Select(x => x.Element("Value").Value).FirstOrDefault();
                double SE = Convert.ToDouble(SEK);

                Bots_response_TextBlock.Text = $"10 Шведских крон: { Math.Round(SE, 2)} Российских рублей";
                ss.SpeakAsync($"10 Шведских крон: { Math.Round(SE, 2)} Российских рублей");
            }
            //Швейцарский франк
            if (result.Contains("Курс швейцарский франк") || result.Contains("курс швейцарский франк"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string CHF = el.Where(x => x.Attribute("ID").Value == "R01775").Select(x => x.Element("Value").Value).FirstOrDefault();
                double CH = Convert.ToDouble(CHF);

                Bots_response_TextBlock.Text = $"1 Швейцарский франк: { Math.Round(CH, 2)} Российских рублей";
                ss.SpeakAsync($"1 Швейцарский франк: { Math.Round(CH, 2)} Российских рублей");
            }
            //Вон Республики Корея
            if (result.Contains("Курс вон республики корея") || result.Contains("курс вон республики корея"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string KRW = el.Where(x => x.Attribute("ID").Value == "R01815").Select(x => x.Element("Value").Value).FirstOrDefault();
                double KR = Convert.ToDouble(KRW);

                Bots_response_TextBlock.Text = $"1000 Вон Республики Корея: { Math.Round(KR, 2)} Российских рублей";
                ss.SpeakAsync($"1000 Вон Республики Корея: { Math.Round(KR, 2)} Российских рублей");
            }
            //Японских иен
            if (result.Contains("Курс японских иен") || result.Contains("курс японских иен"))
            {
                WebClient client = new WebClient();
                var xml = client.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp");
                XDocument xdoc = XDocument.Parse(xml);
                var el = xdoc.Element("ValCurs").Elements("Valute");
                string JPY = el.Where(x => x.Attribute("ID").Value == "R01820").Select(x => x.Element("Value").Value).FirstOrDefault();
                double JP = Convert.ToDouble(JPY);

                Bots_response_TextBlock.Text = $"100 Японских иен: { Math.Round(JP, 2)} Российских рублей";
                ss.SpeakAsync($"100 Японских иен: { Math.Round(JP, 2)} Российских рублей");
            }

            //Word
            if (result.Contains("Word") || result.Contains("word"))
            {
                Bots_response_TextBlock.Text = "Открываю Word";
                ss.SpeakAsync("Открываю Word");

                System.Diagnostics.Process.Start("C:/ProgramData/Microsoft/Windows/Start Menu/Programs/Microsoft Office 2013/Word 2013");
            }
            //Excel
            if (result.Contains("Excel") || result.Contains("excel"))
            {
                Bots_response_TextBlock.Text = "Открываю Excel";
                ss.SpeakAsync("Открываю Excel");

                System.Diagnostics.Process.Start("C:/ProgramData/Microsoft/Windows/Start Menu/Programs/Microsoft Office 2013/Excel 2013");
            }
            //PowerPoint
            if (result.Contains("PowerPoint") || result.Contains("powerpoint"))
            {
                Bots_response_TextBlock.Text = "Открываю PowerPoint";
                ss.SpeakAsync("Открываю PowerPoint");

                System.Diagnostics.Process.Start("C:/ProgramData/Microsoft/Windows/Start Menu/Programs/Microsoft Office 2013/PowerPoint 2013");
            }
            //Access 
            if (result.Contains("Access") || result.Contains("access"))
            {
                Bots_response_TextBlock.Text = "Открываю Access";
                ss.SpeakAsync("Открываю Access");

                System.Diagnostics.Process.Start("C:/ProgramData/Microsoft/Windows/Start Menu/Programs/Microsoft Office 2013/Access 2013");
            }
            //Project 
            if (result.Contains("Project") || result.Contains("project"))
            {
                Bots_response_TextBlock.Text = "Открываю Project";
                ss.SpeakAsync("Открываю Project");

                System.Diagnostics.Process.Start("C:/ProgramData/Microsoft/Windows/Start Menu/Programs/Microsoft Office 2013/Project 2013");
            }

            //Калькулятор
            if (result.Contains("Калькулятор") || result.Contains("калькулятор"))
            {
                Bots_response_TextBlock.Text = "Открываю Калькулятор";
                ss.SpeakAsync("Открываю Калькулятор");

                System.Diagnostics.Process.Start("calc.exe");
            }
            //Параметры
            if (result.Contains("Параметры") || result.Contains("параметры"))
            {
                Bots_response_TextBlock.Text = "Открываю Параметры";
                ss.SpeakAsync("Открываю Параметры");

                System.Diagnostics.Process.Start("Control.exe");
            }
            //Браузер
            if (result.Contains("Браузер") || result.Contains("браузер"))
            {
                Bots_response_TextBlock.Text = "Открываю Браузер";
                ss.SpeakAsync("Открываю Браузер");

                System.Diagnostics.Process.Start("msedge.exe");
            }

            // Овен
            if (result.Contains("Гороскоп Овен") || result.Contains("гороскоп Овен") || result.Contains("гороскоп овен") || result.Contains("гороскоп Овны") || result.Contains("гороскоп овны"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/aries/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Тельц
            if (result.Contains("Гороскоп Телец")  || result.Contains("гороскоп Телец") || result.Contains("Гороскоп тельцы") || result.Contains("Гороскоп Тельцы") || result.Contains("гороскоп Тельцы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/taurus/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Близницы
            if (result.Contains("Гороскоп Близнецы") || result.Contains("гороскоп Близнецы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/gemini/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Рак
            if (result.Contains("Гороскоп Рак") || result.Contains("гороскоп Рак") || result.Contains("гороскоп Раки") || result.Contains("гороскоп раки"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/cancer/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Лев
            if (result.Contains("Гороскоп Лев") || result.Contains("гороскоп Лев") || result.Contains("гороскоп Львы") || result.Contains("гороскоп львы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/leo/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Дева
            if (result.Contains("Гороскоп Дева") || result.Contains("гороскоп Дева") || result.Contains("гороскоп Девы") || result.Contains("гороскоп девы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/virgo/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Весы
            if (result.Contains("Гороскоп Весы") || result.Contains("гороскоп Весы") || result.Contains("гороскоп весы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/libra/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            //Скорпион
            if (result.Contains("Гороскоп Скорпион") || result.Contains("гороскоп Скорпион") || result.Contains("гороскоп Скорпионы") || result.Contains("гороскоп скорпионы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/scorpio/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            //Стрелец
            if (result.Contains("Гороскоп Стрелец") || result.Contains("гороскоп Стрелец") || result.Contains("гороскоп Стрельцы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/sagittarius/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            // Козерог
            if (result.Contains("Гороскоп Козерог") || result.Contains("гороскоп Козерог") || result.Contains("гороскоп Козероги"))
            {
                string[] k = { "<p>", "</p>" }; 
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/capricorn/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            //Водолей
            if (result.Contains("Гороскоп Водолей") || result.Contains("гороскоп Водолей"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/aquarius/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
            //Рыба
            if (result.Contains("Гороскоп Рыбы") || result.Contains("гороскоп Рыбы"))
            {
                string[] k = { "<p>", "</p>" };
                string l = "<div class=\"article__item article__item_alignment_left article__item_html\">";
                WebRequest req = WebRequest.Create(@"https://horo.mail.ru/prediction/pisces/today/");
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader st = new StreamReader(stream);
                string tex = st.ReadToEnd();
                tex = tex.Substring(tex.IndexOf(l) + l.Length);
                tex = tex.Remove(tex.IndexOf("</p>"));
                foreach (string tx in k)
                    tex = tex.Replace(tx, "");
                Bots_response_TextBlock.Text = tex;
                ss.SpeakAsync(tex);
            }
           
            //Время
            if (result.Contains("Сколько сейчас время?") || result.Contains("Сколько сейчас времени?") || result.Contains("Время") || result.Contains("время") || result.Contains("Который час"))
            {
                int h = DateTime.Now.Hour;
                int m = DateTime.Now.Minute;
                string time = "";
                if (h < 10)
                {
                    time += "0" + h;
                }
                else
                {
                    time += h;
                }
                time += ":";
                if (m < 10)
                {
                    time += "0" + m;
                }
                else
                {
                    time += m;
                }
                Bots_response_TextBlock.Text = time;
                ss.SpeakAsync("Сейчас " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ".");
            }

            reader.Close();
            response.Close();

            var question_ = Users_question_TextBlock.Text;
            var answer_ = Bots_response_TextBlock.Text;
            var date_ = DateTime.Now;
            var QuestionAnswer_ = new TableQuestionAnswer();
            QuestionAnswer_.Question = question_;
            QuestionAnswer_.Answer = answer_;
            QuestionAnswer_.Date = date_;
            db.TableQuestionAnswer.Add(QuestionAnswer_);
            db.SaveChanges();


            using (var dba = new DiplomEntities1())
            {
                var pol = dba.TableCommand.AsNoTracking().FirstOrDefault(p => p.Command == Users_question_TextBlock.Text);
                if (pol == null)
                {
                    return;
                }

                var url = dba.TableUrl.AsNoTracking().FirstOrDefault(u => u.IdUrl == pol.IdUrl).URL;

                Bots_response_TextBlock.Text = "Открываю";
                ss.SpeakAsync("Открываю"); 

                System.Diagnostics.Process.Start(url);
            }

        }
        private void Reference_Click(object sender, RoutedEventArgs e)
        {
            Reference reference = new Reference();
            reference.Show();

            //string commandText = "Справочная система.chm";
            //var proc = new System.Diagnostics.Process();
            //proc.StartInfo.FileName = commandText;
            //proc.StartInfo.UseShellExecute = true;
            //proc.Start();
        }
        private void Dialog_Click(object sender, RoutedEventArgs e)
        {
            Dialog dialog = new Dialog();
            dialog.Show();
        }
        private void About_the_program_Click(object sender, RoutedEventArgs e)
        {
            About_the_program AboutTheProgram = new About_the_program();
            AboutTheProgram.Show();
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = MessageBox.Show("Хотите закрыть окно? (Если закроете окно окончательно, то он не появится на панели быстрого доступа)", "Вопрос на засыпку", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No;
        }
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            this.Show();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            Statistica statistics = new Statistica();
            statistics.Show();
        }
        private void Favorites_Click(object sender, RoutedEventArgs e)
        {
            Favorites favorites = new Favorites();
            favorites.Show();
        }
    }   
}