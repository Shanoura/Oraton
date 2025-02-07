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
using Oraton.DataBaseConnectors;

namespace Oraton.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameField.Text.Trim();
            string email = emailField.Text.Trim();
            string password = passwordField.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заповніть всі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DatabaseReader dbReader = new DatabaseReader();

            // Проверяем, есть ли уже пользователь с таким username или email
            if (dbReader.IsUsernameTaken(username))
            {
                MessageBox.Show("Це ім'я користувача все зайнято!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dbReader.IsEmailTaken(email))
            {
                MessageBox.Show("Цей email вже зареєстрований!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Validator.IsValidEmail(email))
            {
                MessageBox.Show("Не вказано дійсний email!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); 
                return;
            }
            // Если имя пользователя и email уникальны, создаём нового пользователя
            User newUser = new User
            {
                Username = username,
                Email = email,
                Password = password // Можно дополнительно хешировать
            };

            DatabaseWriter dbWriter = new DatabaseWriter();
            dbWriter.InsertUser(newUser);

            MessageBox.Show("Реєстрація успішна!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            this.NavigationService.Navigate(new LoginPage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService navService = this.NavigationService;
            if (navService != null)
            {
                if(Application.Current.MainWindow is MainWindow mainWindow)
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
