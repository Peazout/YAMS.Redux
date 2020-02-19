using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace YAMS.Redux.Data
{
    public class YAMSDatabase : DbContext
    {

        private static string CONNECTIONNAME;

        public YAMSDatabase(string ConnectionName = "YAMS")
        {
            CONNECTIONNAME = ConnectionName;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings[CONNECTIONNAME].ConnectionString,
            providerOptions => providerOptions.EnableRetryOnFailure());
            // optionsBuilder.UseLoggerFactory(MyLoggerFactory);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<YAMSSettingItem>()
                .Property(c => c.Name)
                .HasConversion<string>();

            modelBuilder.Entity<MinecraftServer>()
                .Property(c => c.ServerType)
                .HasConversion<string>();

        }

        public virtual DbSet<MinecraftServer> Servers { get; set; }
        public virtual DbSet<ServerProcessID> ServerPID { get; set; }
        public virtual DbSet<MinecraftServerSetting> ServersConfig { get; set; }
        public virtual DbSet<YAMSSettingItem> Settings { get; set; }
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
