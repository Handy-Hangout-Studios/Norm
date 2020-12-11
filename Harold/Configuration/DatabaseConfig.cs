using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Configuration
{
    public class DatabaseConfig
    {
        public static readonly string Section = "DatabaseConfig";
        // For database login
        public string Host { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Pooling { get; set; }
    }

    public static class DatabaseExtensionMethods
    {
        public static string AsNpgsqlConnectionString(this DatabaseConfig config)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder
            {
                Host = config.Host,
                Port = config.Port,
                Database = config.Name,
                Username = config.Username,
                Password = config.Password,
                Pooling = config.Pooling,
            };

            return builder.ConnectionString;
        }
    }
}
