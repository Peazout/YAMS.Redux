using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace YAMS.Redux.Data
{
    public class YAMSDatabase : DbContext
    {

        protected static string CONNECTIONNAME;

        public YAMSDatabase(string ConnectionName = "YAMS")
        {
            CONNECTIONNAME = ConnectionName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings[CONNECTIONNAME].ConnectionString,
            providerOptions => providerOptions.EnableRetryOnFailure());
        }

        public virtual DbSet<MinecraftServer> Servers { get; set; }

    }

}
