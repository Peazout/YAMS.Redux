using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
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
        private static Logger MyLog
        {
            get
            {
                if (_MyLog == null) { _MyLog = LogManager.GetCurrentClassLogger(); }
                return _MyLog;
            }

        }
        private static NLog.Logger _MyLog;
        /// <summary>
        /// Return a logevent for nlog.
        /// </summary>
        private static LogEventInfo GetLogEvent(NLog.LogLevel Lvl, string Message, int ServerId = -1)
        {
            LogEventInfo theEvent = new LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion

        /// <summary>
        /// Our connectname, set with Init()
        /// </summary>
        public static string ConnectionName { get; private set; }

        public static void Init(string connectionname = "YAMS")
        {
            ConnectionName = connectionname;

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
        /// Get a context using ConnectionName for connection string
        /// </summary>
        /// <returns></returns>
        private static YAMSDatabase GetNewContext()
        {
            var DB = new YAMSDatabase(ConnectionName);
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
                if (row != null) { return row; }
            }
            return null;

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
            using (var DBContext = GetNewContext())
            {
                return (from s in DBContext.Servers where s.Id == serverid select s).SingleOrDefault();
            }
        }

        #endregion

        #region Server ProcessId

        public static List<ServerProcessID> GetActiveServerPID()
        {
            using (var db = GetNewContext())
            {
                return db.ServerPID.Include(p => p.Active == true).ToList();

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

    }

}
