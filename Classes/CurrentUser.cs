using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oraton.Classes
{
    internal class CurrentUser
    {
        public static User User { get; set; }

        public static void SetUser(User user)
        {
            User = user;
        }

        public static bool IsAuthenticated()
        {
            return User != null;
        }
    }
}
