using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comandante.TestsWeb.Services
{
    public class Db
    {
        public Db()
        {
            Provider = "MySQL";
        }

        public string ConnectionString
        {
            get
            {
                if (string.Equals("Sqlite", Provider, StringComparison.CurrentCultureIgnoreCase))
                    return $"Data Source={Name};";
                if (string.Equals("MySQL", Provider, StringComparison.CurrentCultureIgnoreCase))
                    return $"Server={Host};Database={Name};User={User};Password={Password};";
                throw new ApplicationException($"Provider is not supported {Provider}");
            }
        }

        public string Provider { get; set; }
        public string Host { get; set; }
        public string Name { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
