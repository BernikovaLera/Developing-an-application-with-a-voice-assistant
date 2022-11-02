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
    /// Логика взаимодействия для About_the_program.xaml
    /// </summary>
    public partial class About_the_program : Window
    {
        public About_the_program()
        {
            InitializeComponent();
        }

        private void Exit_about_the_program_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
