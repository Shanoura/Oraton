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
using System.Xml;
using Oraton.Classes;
using Oraton.DataBase;
using Oraton.Pages;

namespace Oraton
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoginButton.Visibility = Visibility.Hidden;
            RegButton.Visibility = Visibility.Hidden;
            namelabel.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new LoginPage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            LoginButton.Visibility = Visibility.Hidden;
            RegButton.Visibility = Visibility.Hidden;
            namelabel.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new RegistrationPage());
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Автор: Кузнецов Є.Р.\nГрупа: КІУКІ-21-6\nВерсія 1.0", "Про програму", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

