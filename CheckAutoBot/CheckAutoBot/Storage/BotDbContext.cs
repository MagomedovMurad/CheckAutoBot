using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class BotDbContext: DbContext
    {
        public DbSet<RequestObject> RequestObjects { get; set; }

        public DbSet<Auto> Autos { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Passport> Passports { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<RequestObjectCache> RequestObjectCache { get; set; }

        public BotDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename = CheckAutoBot.sqlite");
        }
    }
}
