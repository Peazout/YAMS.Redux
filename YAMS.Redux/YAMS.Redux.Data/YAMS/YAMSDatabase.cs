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
        public virtual DbSet<MinecraftServerSetting> ServersConfig { get; set; }
        public virtual DbSet<YAMSSetting> Settings { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<MincraftManifestFile> ManifestFiles { get; set; }
        public DbSet<MinecraftJarFile> VersionFiles { get; set; }
        public DbSet<ChattMessage> Chats { get; set; }
        public DbSet<JobSetting> Jobs { get; set; }
        /// <summary>
        /// Logging is done by NLog, this should only be used for reading.
        /// </summary>
        public DbSet<LogRow> YAMSLog { get; set; }
        
    }

}
