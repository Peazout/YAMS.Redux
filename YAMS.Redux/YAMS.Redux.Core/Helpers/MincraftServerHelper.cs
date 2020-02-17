using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YAMS.Redux.Data;
using LogLevel = NLog.LogLevel;

namespace YAMS.Redux.Core.Helpers
{

    public static class MincraftServerHelper
    {

        public static Dictionary<int, MinecraftServer> Servers = new Dictionary<int, MinecraftServer> { };

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

    }

}
