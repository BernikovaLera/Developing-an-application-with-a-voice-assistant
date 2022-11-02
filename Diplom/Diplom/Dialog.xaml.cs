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
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.Data.Entity.Core.Objects;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows.Navigation;
using System.ComponentModel;
using System.IO;

namespace Diplom
{
    public partial class Dialog : System.Windows.Window, INotifyPropertyChanged
    {
        DiplomEntities db = new DiplomEntities();
        public Dialog()
        {
            InitializeComponent();
            DataContext = this;
            AllTable = db.TableQuestionAnswer.ToList();         
        }
        private List<TableQuestionAnswer> alltable;
        public event PropertyChangedEventHandler PropertyChanged;
        public List<TableQuestionAnswer> AllTable
        {
            get { return alltable; }
            set
            {
                alltable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllTable)));
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var str = SearchBox.Text;
            List<TableQuestionAnswer> result = DiplomEntities.GetContext().TableQuestionAnswer
                .Where(r => r.Question.StartsWith(str))
                .ToList();
            dataGrid1.ItemsSource = result;
        }
        private void Upload_to_excel(object sender, RoutedEventArgs e)
        {
            Excel.Application ExApp = new Excel.Application();
            Excel.Workbook Book;
            Excel.Worksheet workSheet;
            Book = ExApp.Workbooks.Add();
            workSheet = (Excel.Worksheet)Book.Worksheets.get_Item(1);
            workSheet.Cells[1, 1] = "Дата и время";
            workSheet.Cells[1, 2] = "Вопрос";
            workSheet.Cells[1, 3] = "Ответ";
            for (int i = 0; i < AllTable.Count; i++)
            {
                workSheet.Cells[i + 2, 1] = AllTable[i].Date;
                workSheet.Cells[i + 2, 2] = AllTable[i].Question;
                workSheet.Cells[i + 2, 3] = AllTable[i].Answer;
            }
            workSheet.Columns.AutoFit();
            workSheet.Rows.AutoFit();
            ExApp.Visible = true;
            ExApp.UserControl = true;
        }
        private void Del_Click(object sender, RoutedEventArgs e)
        {
            var item = dataGrid1.SelectedItem as TableQuestionAnswer;
            List<TableQuestionAnswer> reg = DiplomEntities.GetContext().TableQuestionAnswer
                .Where(s => s.ID == item.ID).ToList();

            MessageBoxResult result = MessageBox.Show($"Удалить {item.Question}?", "Удаление", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    DiplomEntities.GetContext().TableQuestionAnswer.RemoveRange(reg);
                    //DiplomEntities.GetContext().TableFavorites.Remove(item);
                    DiplomEntities.GetContext().SaveChanges();
                    dataGrid1.ItemsSource = DiplomEntities.GetContext().TableQuestionAnswer.ToList();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }
    }
}
