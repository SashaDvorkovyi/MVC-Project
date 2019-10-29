using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public class DataBaseContext :DbContext
    {
        public DataBaseContext() : base("Users")
        {
            if (!Database.Exists("Users"))
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<DataBaseContext>());
            }
        }

        public DbSet<User> Users { get; set; }
    }
}
