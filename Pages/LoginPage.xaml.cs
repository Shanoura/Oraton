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
using Oraton.Classes;
using Oraton.DataBase;
using Oraton.Pages;

namespace Oraton
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            DatabaseReader dbReader = new DatabaseReader();
            User user = dbReader.GetUserByCredentials(username, password);

            if (user != null)
            {
                // Сохраняем текущего пользователя в CurrentUser
                CurrentUser.SetUser(user);
                this.NavigationService.Navigate(new MainPage());
            }
            else
            {
                lblMessage.Text = "Невірний логін або пароль!";
                lblMessage.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService navService = this.NavigationService;
            if (navService != null)
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LoginButton.Visibility = Visibility.Visible;
                    mainWindow.RegButton.Visibility = Visibility.Visible;
                    mainWindow.namelabel.Visibility = Visibility.Visible;
                }
                navService.Content = null; // Очищаем содержимое Frame
            }
        }
    }
}
