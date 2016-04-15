using crmc.domain;
using System.Data.Entity;
using System.Diagnostics;

namespace crmc.data
{
    public class DataContext : DbContext
    {
        public DataContext() : base("DefaultConnection")
        {
            Database.Log = msg => Debug.WriteLine(msg);
        }

        public DbSet<Person> Persons { get; set; }
    }
}