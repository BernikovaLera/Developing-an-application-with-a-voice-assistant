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
using System.Windows.Shapes;
using System.Data.Entity.Core.Objects;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows.Navigation;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Threading;

namespace Diplom
{
    public partial class Statistica : Window, INotifyPropertyChanged
    {
        DiplomEntities db = new DiplomEntities();
        public Statistica()
        {
            InitializeComponent();
            DataContext = this;
        }
        public List<MyDataType1> prelist = new List<MyDataType1>();
        public class MyDataType
        {
            public string Date { get; set; }
            public int kolvo { get; set; }
        }
        public class MyDataType1
        {
            public string Date { get; set; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private List<MyDataType> tablee { get; set; }
        public List<MyDataType> Tablee
        {
            get { return tablee; }
            set
            {
                tablee = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tablee)));
            }
        }
        private List<TableQuestionAnswer> alltable { get; set; }
        public List<TableQuestionAnswer> AllTable
        {
            get { return alltable; }
            set
            {
                alltable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllTable)));
            }
        }      
        private void Stat(object sender, RoutedEventArgs e)
        {
            AllTable = db.TableQuestionAnswer.ToList();
            foreach (var val in AllTable)
            {
                MyDataType1 mdt = new MyDataType1();
                mdt.Date = Convert.ToDateTime(val.Date).ToString("dd.MM.yyyy");
                prelist.Add(mdt);
            }
            var listdate = prelist.GroupBy(g => g.Date);
            List<MyDataType> vivodlist = new List<MyDataType>();
            foreach (var date in listdate)
            {
                MyDataType mdt = new MyDataType();
                mdt.Date = date.Key;
                mdt.kolvo = date.Count();
                vivodlist.Add(mdt);
            }
            Tablee = vivodlist;
        }
    }
}
