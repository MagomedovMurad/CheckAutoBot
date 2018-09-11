using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class BotDbContext: DbContext
    {
        public DbSet<RequestObject> RequestObjects { get; set; }

        public BotDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename = TestBd.sqlite");
        }
    }
}
