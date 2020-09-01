using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using YAMS.Redux.Data;


namespace YAMS.Redux.Core.Helpers
{

    public static class DBHelper
    {

        #region NLOG

        /// <summary>
        /// Get our logger, if null get the current one.
        /// </summary>
        private static NLog.Logger MyLog
        {
            get
            {
                if (_MyLog == null) { _MyLog = NLog.LogManager.GetCurrentClassLogger(); }
                return _MyLog;
            }

        }
        private static NLog.Logger _MyLog;
        /// <summary>
        /// Return a logevent for nlog.
        /// </summary>
        private static NLog.LogEventInfo GetLogEvent(NLog.LogLevel Lvl, string Message, int ServerId = -1)
        {
            NLog.LogEventInfo theEvent = new NLog.LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion

        /// <summary>
        /// Our connectname, set with Init()
        /// </summary>
        public static string ConnectionName { get; private set; }
        /// <summary>
        /// Databas provider for us, set with Init()
        /// </summary>
        public static DbProvider Provider { get; private set; }

        public static void Init(string connectionname = "YAMS", DbProvider provider = DbProvider.MySQL)
        {
            // Get connectionname
            ConnectionName = AppCore.Config.GetSection("ConnectionStrings:YAMS").Value;
            if (string.IsNullOrWhiteSpace(ConnectionName)) ConnectionName = connectionname;

            // Get provider
            var pro = AppCore.Config.GetSection("YAMS:DbEngine").Value;
            if (string.IsNullOrWhiteSpace(pro)) Provider = provider;
            else Provider = ParseProvider(pro);

            // Test out the connection
            using (var MyDB = GetNewContext())
            {
                if (!(MyDB.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                {
                    // TODO: Save as windows event the error
                    throw new Exception("Database connection can not be verified.");
                }
                // Exceptions should be pickedup by caller.
                MyDB.Database.OpenConnection();
                MyDB.Database.CloseConnection();

            }

        }

        /// <summary>
        /// Try out the given value and parse it.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static DbProvider ParseProvider(string provider)
        {
            try
            {
                return (DbProvider)Enum.Parse(typeof(DbProvider), provider);
            }
            catch (Exception ex)
            {
                MyLog.Trace("Failed parsing DbProvider: " + ex.ToString());
                MyLog.Warn("Provider for database could not be parsed: " + provider);

                var n = 0;
                foreach (string p in Enum.GetNames(typeof(DbProvider)))
                {
                    n++;
                    MyLog.Warn("(" + n + "):" + p);
                }

                throw;
            }

        }

        /// <summary>
        /// Get a context using ConnectionName for connection string
        /// </summary>
        /// <returns></returns>
        private static YAMSDatabase GetNewContext()
        {
            var DB = new YAMSDatabase(ConnectionName, Provider);
            return DB;
        }

        #region Settings

        /// <summary>
        /// Return all the settings from the YAMSSettings table
        /// </summary>
        /// <returns>DataSet with table</returns>
        public static IEnumerable<YAMSSettingItem> GetSettings()
        {
            using (var db = GetNewContext())
            {
                return db.Settings.ToList();
            }

        }

        public static YAMSSettingItem GetSetting(YAMSSetting name)
        {
            using (var db = GetNewContext())
            {
                var row = (from a in db.Settings where a.Name == name select a).SingleOrDefault();
                // return row;
                if (row != null) { return row; }
                return new YAMSSettingItem();
            }

        }

        /// <summary>
        /// Update setting in DB if exists, else creates the setting and saves to DB.
        /// </summary>
        public static void SetSetting(YAMSSetting name, string value)
        {
            using (var db = GetNewContext())
            {
                var row = (from a in db.Settings where a.Name == name select a).SingleOrDefault();
                if (row == null)
                    db.Settings.Add(new YAMSSettingItem() { Name = name, Value = value });
                else
                    row.Value = value;

                db.SaveChanges();

            }

        }

        #endregion

        #region Minecraft settings

        /// <summary>
        /// Get a setting for the server.
        /// </summary>
        /// <param name="strSettingName"></param>
        /// <param name="intServerID"></param>
        /// <returns></returns>
        public static MinecraftServerSetting GetMCConfig(string name, int ServerID)
        {
            using (var db = GetNewContext())
            {
                var row = (from a in db.ServersConfig where a.Name == name && a.ServerId == ServerID select a).SingleOrDefault();
                if (row != null) { return row; }
            }
            return null;

        }
        /// <summary>
        /// Return all the settings for Minecraft server with given id.
        /// </summary>
        public static IEnumerable<MinecraftServerSetting> GetMCConfig(int serverid)
        {
            using (var db = GetNewContext())
            {
                var query = db.ServersConfig.AsQueryable();
                query = query.Where(s => s.ServerId == serverid);
                return query.ToList();
            }

        }

        /// <summary>
        /// Update setting in DB if exists, else creates the setting and saves to DB.
        /// </summary>
        public static void SetMCConfig(string name, string value, int serverid)
        {
            using (var db = GetNewContext())
            {
                var row = (from a in db.ServersConfig
                           where
                           a.Name == name
                           &&
                           a.ServerId == serverid
                           select a
                                 ).SingleOrDefault();

                // If it didn´t exist, add it.
                if (row == null)
                    db.ServersConfig.Add(new MinecraftServerSetting() { Name = name, Value = value, ServerId = serverid });
                else
                    row.Value = value;

                db.SaveChanges();
            }

        }

        /// <summary>
        /// Remove all settings connected to serverID
        /// </summary>
        public static bool DeleteMCConfig(int serverid)
        {
            using (var db = GetNewContext())
            {
                var items = (from s in db.ServersConfig where s.ServerId == serverid select s);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        db.ServersConfig.Remove(item);
                    }

                    db.SaveChanges();
                    // AddLog("Removed settings from DB for server => " + ServerID, "app", LogLevel.Info);
                }
                else
                {
                    throw new Exception("Settings could not be removed, not found in DB for server => " + serverid);
                    // AddLog("Settings could not be removed, not found in DB for server => " + ServerID, "app", LogLevel.Warn);
                }
            }

            return true;
        }

        #endregion

        #region Minecraft server

        /// <summary>
        /// Get all our Minecraft servers.
        /// </summary>
        internal static List<MinecraftServer> GetServers()
        {

            using (var db = GetNewContext())
            {
                return db.Servers.ToList();

            }

        }

        /// <summary>
        /// Adding server to database with default values.
        /// </summary>
        /// <returns>Serverid for the new server.</returns>
        public static MinecraftServer AddServer(string title, int memory, MinecraftServerType mctype = MinecraftServerType.Vanilla)
        {
            using (var db = GetNewContext())
            {
                var row = new MinecraftServer()
                {
                    Name = title,
                    AssignedMemory = memory,
                    AutoStart = DBHelper.GetSetting(YAMSSetting.DefaultAutostartServer).GetValueAsBool,
                    EnableServerOptimisations = DBHelper.GetSetting(YAMSSetting.EnableJavaOptimisations).GetValueAsBool,
                    ServerType = mctype,
                };

                db.Servers.Add(row);
                db.SaveChanges();

                return row;

            }

        }

        /// <summary>
        /// Get the server object for serverid
        /// </summary>
        public static MinecraftServer GetServer(int serverid)
        {
            using (var db = GetNewContext())
            {
                return (from s in db.Servers where s.Id == serverid select s).SingleOrDefault();
            }
        }

        /// <summary>
        /// Save changes to a minecraft server in db.
        /// </summary>
        /// <param name="server"></param>
        public static void UpdateServer(MinecraftServer server)
        {
            using (var db = GetNewContext())
            {
                db.Servers.Update(server);
                db.SaveChanges();
            }

        }


        #endregion

        #region Server ProcessId

        public static List<ServerProcessID> GetActiveServerPID()
        {
            using (var db = GetNewContext())
            {
                return db.ServerPID.Where(p => p.Active == true).ToList();

            }

        }

        public static void SetUnactiveServerPID(int pid)
        {
            using (var db = GetNewContext())
            {
                var row = db.Set<ServerProcessID>().First(p => p.PId == pid && p.Active == true);
                row.Active = false;
                db.SaveChanges();

            }

        }

        public static void DeleteActiveServerPID(int pid)
        {
            using (var db = GetNewContext())
            {
                var row = db.Set<ServerProcessID>().First(p => p.PId == pid && p.Active == true);
                db.ServerPID.Remove(row);
                db.SaveChanges();

            }

        }
        #endregion

        #region Versions

        /// <summary>
        /// Get the version from db for selected type of server.
        /// </summary>
        public static MinecraftJarFile GetVersionFile(string versionname, MinecraftServerType versiontype)
        {
            using (var db = GetNewContext())
            {
                return db.VersionFiles.SingleOrDefault(v =>
                    v.VersionName == versionname
                    &&
                    v.TypeOfServer == versiontype
                );

            }

        }

        /// <summary>
        /// Get the latest version for selected Minecraft servertype.
        /// </summary>
        public static MinecraftJarFile GetVersionFile(MinecraftServerType versiontype)
        {
            using (var db = GetNewContext())
            {
                return db.VersionFiles
                    .OrderBy(v => v.Added)
                    .FirstOrDefault(v => v.TypeOfServer == versiontype);

            }

        }



        /// <summary>
        /// Save a new version of a minecraft server jar.
        /// </summary>
        public static MinecraftJarFile SetVersionFile(string versionname, MinecraftServerType versiontype)
        {
            using (var db = GetNewContext())
            {
                var row = new MinecraftJarFile()
                {
                    VersionName = versionname,
                    TypeOfServer = versiontype,
                    Added = DateTime.Now,
                };

                db.VersionFiles.Add(row);
                db.SaveChanges();

                return row;
            }

        }

        #endregion

        #region Logs

        /// <summary>
        /// Return logrows recorded with NLog with desc Logged, desc Id.
        /// </summary>
        public static List<LogRow> GetLog(int startindex = -1, int numberofrows = 500, ServerMessageLevel level = ServerMessageLevel.All, int serverid = -1)
        {
            using (var db = GetNewContext())
            {
                var query = db.YAMSLog.AsQueryable();
                if (serverid != -1) query = query.Where(x => x.ServerId == serverid);
                if (startindex > -1) query = query.Where(x => x.Id > startindex);
                if (level != ServerMessageLevel.All) query = query.Where(x => x.Level == level.ToString());
                query = query.OrderByDescending(x => x.Logged).OrderByDescending(x => x.Id);
                query = query.Take(numberofrows);
                // Preform the query.
                return query.ToList();

            }

        }

        /// <summary>
        /// Clearout logrows that was logged before given date.
        /// </summary>
        public static void DeleteLogRows(DateTime before)
        {

            using (var db = GetNewContext())
            {
                var logrows = db.YAMSLog.Select(l => l.Logged >= before).ToArray();
                db.RemoveRange(logrows);
                db.SaveChanges();

            }

        }

        #endregion

        #region Jobs

        /// <summary>
        /// Return all jobs to be done with in the hour and minute requested.
        /// </summary>
        public static IEnumerable<JobSetting> GetJobs(int hour, int minute)
        {
            using (var db = GetNewContext())
            {

                var query = db.Jobs;
                return query.Where(j =>
                (j.Hour == -1 && j.Minute == minute)
                ||
                (j.Hour == hour && j.Minute == minute))
                .ToList();

            }

        }

        #endregion

        #region Player

        /// <summary>
        /// Add a new player to our database.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="serverid"></param>
        /// <param name="GUID"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Player AddPlayer(string username, int serverid, string GUID = "")
        {

            using (var db = GetNewContext())
            {
                Player player = new Player();

                player.Name = username;
                player.ServerId = serverid;
                player.LastConnected = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(GUID)) player.Guid = GUID;

                db.Players.Add(player);
                db.SaveChanges();
                return player;
            }

        }
        /// <summary>
        /// Update player data in the db.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Player UpdatePlayer(Player player)
        {
            using (var db = GetNewContext())
            {
                db.Players.Add(player);
                db.SaveChanges();
                return player;
            }

        }
        /// <summary>
        /// Return player searching with name.
        /// </summary>
        /// <returns></returns>
        public static Player GetPlayerByName(int serverid, string  username)
        {

            using (var db = GetNewContext())
            {
                    return (from p in db.Players where p.Name == username && p.ServerId == serverid select p).SingleOrDefault();
            }

        }
        /// <summary>
        /// Return player searching by UUID.
        /// </summary>
        /// <param name="serverid"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static Player GetPlayerByUUID(int serverid, string uuid)
        {

            using (var db = GetNewContext())
            {
                return (from p in db.Players where p.Guid == uuid && p.ServerId == serverid select p).SingleOrDefault();
            }

        }

        #endregion

        #region Chat

        /// <summary>
        /// Add chatmessage to databas.
        /// </summary>
        /// <param name="row"></param>
        public static void AddChatMessage(ChatMessage row)
        {
            using (YAMSDatabase db = GetNewContext())
            {
                db.Chats.Add(row);
                db.SaveChanges();
            }

        }

        #endregion

    }

}
