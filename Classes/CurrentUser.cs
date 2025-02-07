using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oraton.Classes
{
    internal class CurrentUser
    {// Статическое свойство для хранения текущего пользователя
        public static User User { get; set; }

        // Метод для установки текущего пользователя
        public static void SetUser(User user)
        {
            User = user;
        }

        // Метод для проверки, авторизован ли пользователь
        public static bool IsAuthenticated()
        {
            return User != null;
        }
    }
}
