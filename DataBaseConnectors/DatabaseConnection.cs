using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Oraton.DataBase
{
    internal class DatabaseConnection
    {
        private static readonly string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=postgres";

        public static NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
