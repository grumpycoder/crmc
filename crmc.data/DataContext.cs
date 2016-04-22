using crmc.domain;
using System;
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
        public DbSet<WallConfiguration> WallConfigurations { get; set; }
        public DbSet<Censor> Censors { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Properties<string>().Configure(c => c.HasColumnType("varchar"));
            builder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
            builder.Entity<Person>().ToTable("Persons");
            builder.Entity<WallConfiguration>().ToTable("WallConfigurations");

            base.OnModelCreating(builder);
        }
    }
}