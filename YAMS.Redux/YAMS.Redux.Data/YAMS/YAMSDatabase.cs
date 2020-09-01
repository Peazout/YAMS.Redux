using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace YAMS.Redux.Data
{

    public enum DbProvider
    {
        MySQL,
        MSSQL
    }

    public class YAMSDatabase : DbContext
    {

        private static string CONNECTIONSTRING;
        private static DbProvider PROVIDER;

        public YAMSDatabase(string ConnectionString, DbProvider provider)
        {
            CONNECTIONSTRING = ConnectionString;
            PROVIDER = provider;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            switch (PROVIDER)
            {
                case DbProvider.MSSQL:
                optionsBuilder.UseSqlServer(CONNECTIONSTRING,
                providerOptions => providerOptions.EnableRetryOnFailure());
                    break;

                default:
                    optionsBuilder.UseMySQL(CONNECTIONSTRING);
                    break;
            }

            // optionsBuilder.UseLoggerFactory(NLog.LogManager.LogFactory);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<YAMSSettingItem>()
                .Property(c => c.Name)
                .HasConversion<string>();

            modelBuilder.Entity<MinecraftServerSetting>()
            .HasKey(e => new { e.Name, e.ServerId });

            modelBuilder.Entity<MinecraftServer>()
                .Property(c => c.ServerType)
                .HasConversion<string>();

        }

        public virtual DbSet<MinecraftServer> Servers { get; set; }
        public virtual DbSet<ServerProcessID> ServerPID { get; set; }
        public virtual DbSet<MinecraftServerSetting> ServersConfig { get; set; }
        public virtual DbSet<YAMSSettingItem> Settings { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<MinecraftManifestFile> ManifestFiles { get; set; }
        public DbSet<MinecraftJarFile> VersionFiles { get; set; }
        public DbSet<ChatMessage> Chats { get; set; }
        public DbSet<JobSetting> Jobs { get; set; }
        /// <summary>
        /// Logging is done by NLog, this should only be used for reading.
        /// </summary>
        public DbSet<LogRow> YAMSLog { get; set; }
        
    }

}
