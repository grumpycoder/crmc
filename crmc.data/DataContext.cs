using crmc.domain;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Diagnostics;

namespace crmc.data
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        public DataContext() : base("DefaultConnection")
        {
            Database.Log = msg => Debug.WriteLine(msg);
        }

        public static DataContext Create()
        {
            return new DataContext();
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<WallConfiguration> WallConfigurations { get; set; }
        public DbSet<Censor> Censors { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Properties<string>().Configure(c => c.HasColumnType("varchar"));
            builder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

            builder.Entity<ApplicationUser>().ToTable("Users", "Security");
            builder.Entity<IdentityUserRole>().ToTable("UserRoles", "Security");
            builder.Entity<IdentityUserClaim>().ToTable("UserClaims", "Security");
            builder.Entity<IdentityUserLogin>().ToTable("UserLogins", "Security");
            builder.Entity<IdentityRole>().ToTable("Roles", "Security");

            builder.Entity<Person>().ToTable("Persons");
            builder.Entity<WallConfiguration>().ToTable("WallConfigurations");
        }
    }
}