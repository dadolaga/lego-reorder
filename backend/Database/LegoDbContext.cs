using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database {
    public class LegoDbContext : DbContext {
        internal static string ConnectionString { set; private get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseMySql(connectionString: ConnectionString, ServerVersion.AutoDetect(ConnectionString));
        }
    }
}
