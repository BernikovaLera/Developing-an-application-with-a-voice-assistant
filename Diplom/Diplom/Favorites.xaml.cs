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

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для Favorites.xaml
    /// </summary>
    public partial class Favorites : Window
    {
        DiplomEntities1 dba = new DiplomEntities1();
        public Favorites()
        {
            InitializeComponent();
            DataContext = this;
            dataGrid2.ItemsSource = DiplomEntities1.GetContext().TableCommand.ToList();
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            var command = command_.Text;
            var way = way_.Text;

            var Favorites = new TableCommand();
            Favorites.Command = command;

            TableUrl tableUrl = new TableUrl();
            tableUrl.URL = way;
            Favorites.TableUrl = tableUrl;

            dba.TableCommand.Add(Favorites);
            dba.SaveChanges();
            MessageBox.Show("Успешное сохранение");
        }
        private void Del_Click(object sender, RoutedEventArgs e)
        {
            var item = dataGrid2.SelectedItem as TableCommand;
            List<TableUrl> reg = DiplomEntities1.GetContext().TableUrl.Where(s => s.IdUrl == item.IdUrl).ToList();

            MessageBoxResult result = MessageBox.Show($"Удалить {item.Command}?", "Удаление", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    DiplomEntities1.GetContext().TableUrl.RemoveRange(reg);
                    DiplomEntities1.GetContext().TableCommand.Remove(item);
                    DiplomEntities1.GetContext().SaveChanges();
                    dataGrid2.ItemsSource = DiplomEntities1.GetContext().TableCommand.ToList();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }
    }
}
