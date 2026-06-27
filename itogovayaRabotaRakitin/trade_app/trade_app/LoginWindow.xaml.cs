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
using TradeApp.Models;

namespace TradeApp
{

    /// <summary>
    /// Окно авторизации для проверки учетных записей сотрудников и клиентов.
    /// </summary
    public partial class LoginWindow : Window
    {

        /// <summary>
        /// Хранит данные успешно авторизованного пользователя. Принимает null, если вход не выполнен.
        /// </summary>
        public User? AuthenticatedUser { get; private set; }

        /// <summary>
        /// Инициализирует окно авторизации.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля ввода!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var user = db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user != null)
                {
                    AuthenticatedUser = user;
                    MessageBox.Show($"Добро пожаловать, {user.FIO}!", "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Properties["LastSavedLogin"] = user.Login;
                    Application.Current.Properties["LastSavedRoleId"] = user.RoleId;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль! Попробуйте снова.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
