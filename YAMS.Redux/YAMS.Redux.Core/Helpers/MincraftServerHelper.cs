using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Text;
using YAMS.Redux.Core.Entity;
using YAMS.Redux.Data;
using LogLevel = NLog.LogLevel;

namespace YAMS.Redux.Core.Helpers
{

    public static class MinecraftServerHelper
    {

        public static Dictionary<int, MinecraftServerItem> Servers = new Dictionary<int, MinecraftServerItem> { };

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
        private static LogEventInfo GetLogEvent(LogLevel Lvl, string Message, int ServerId = -1)
        {
            LogEventInfo theEvent = new LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion


        public static void Init()
        {
            LoadServerList();
            // Autostart the servers.
            foreach (var server in Servers)
            {
                if (server.Value.Data.AutoStart) server.Value.Start();

            }

        }

        /// <summary>
        /// Load our list of server from the db and create objects to run the servers.
        /// </summary>
        private static void LoadServerList()
        {
            foreach (var row in DBHelper.GetServers())
            {
                var server = new MinecraftServerItem();
                server.Data = row;
                Servers.Add(server.ServerId, server);

            }

        }

        /// <summary>
        /// Create new Minecraft server.
        /// </summary>
        public static int CreateNewServer(string title, int memory, bool fromweb = false)
        {

            if (!File.Exists(FilesAndFoldersHelper.YAMSPropertiesJson))
            {
                string Message = "YAMSPropertiesJson could not be found for creation of server: " + FilesAndFoldersHelper.YAMSPropertiesJson;
                MyLog.Log(GetLogEvent(LogLevel.Error, Message));

                throw new FileNotFoundException(Message);

            }

            // TODO: Read the property stuff in the server item.
            string json = File.ReadAllText(FilesAndFoldersHelper.YAMSPropertiesJson);
            JObject jProps = JObject.Parse(json);

            // Adding to data 
            var server = DBHelper.AddServer(title, memory);

            // Add what version of minecraft to run
            var version = DBHelper.GetVersionFile(MinecraftServerType.Vanilla);
            server.MinecraftJarFileId = version.Id;
            DBHelper.UpdateServer(server);

            // Now the settings rom the YAMS prop files. User changable defaults.
            foreach (JObject option in jProps["options"])
            {
                DBHelper.SetMCConfig((string)option["key"], (string)option["default"], server.Id);
            }

            // Could not be used for tester also?
            if (fromweb)
            {
                // YAMSDataHelper.SaveSettingMC("server-ip", NetworkHelper.GetListenIP().ToString(), ServerID);
                // YAMSDataHelper.SaveSettingMC("server-port", FindNextFreePort().ToString(), ServerID);
            }

            // Create folders now that all database stuff went ok
            // TODO: If folders allready exists, then what?
            if (!Directory.Exists(FilesAndFoldersHelper.MCServerFolder(server.Id))) Directory.CreateDirectory(FilesAndFoldersHelper.MCServerFolder(server.Id));
            if (!Directory.Exists(FilesAndFoldersHelper.MCServerWorldFolder(server.Id))) Directory.CreateDirectory(FilesAndFoldersHelper.MCServerWorldFolder(server.Id));
            if (!Directory.Exists(FilesAndFoldersHelper.MCServerBackupFolder(server.Id))) Directory.CreateDirectory(FilesAndFoldersHelper.MCServerBackupFolder(server.Id));
            if (!Directory.Exists(FilesAndFoldersHelper.MCServerRendersFolder(server.Id))) Directory.CreateDirectory(FilesAndFoldersHelper.MCServerRendersFolder(server.Id));

            MyLog.Log(NLog.LogLevel.Info, "Created new MCServer with id => {ServerID}", server.Id);

            return server.Id;

        }

        /// <summary>
        /// See if there are minecraft servers runnning that we are responsible for but not manage to shutdown
        /// becuse of crach/restart.
        /// </summary>
        public static void KillGhostServers()
        {
            var list = DBHelper.GetActiveServerPID();

            foreach (var row in list)
            {

                try
                {
                    Process.GetProcessById(row.PId).Kill();
                    MyLog.Warn(AppCore.i18t, "Killed a overlocked processes {PId}", row.PId);
                    // TODO: Verify it´s closed?
                    // TODO: Just remove it.  DBHelper.DeleteActiveServerPID(row.PId);
                    DBHelper.SetUnactiveServerPID(row.PId);

                }
                catch (Exception ex)
                {
                    MyLog.Warn(ex, "Process {pid} not killed", row.PId);
                    throw;
                }

            }

        }

    }

}
