using NLog;
using System;
using System.IO;
using YAMS.Redux.Data;
using LogLevel = NLog.LogLevel;

namespace YAMS.Redux.Core.Helpers
{
    public static class InstallHelper
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
        private static LogEventInfo GetLogEvent(LogLevel Lvl, string Message, int ServerId = -1)
        {
            LogEventInfo theEvent = new LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion

        /// <summary>
        /// Remove files that have been renamed to *.OLD
        /// </summary>
        /// <param name="RootFolder"></param>
        /// <returns></returns>
        public static void DeleteOldFiles(string RootFolder)
        {

            //Clear out old files if they exist, if it doesn't work we'll just do it on next startup.
            var files = Directory.GetFiles(RootFolder, "*.OLD");
            if (files != null)
            {
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        MyLog.Warn("Removed old file => " + file);
                    }
                    catch (Exception ex) { MyLog.Debug(ex); };
                }
            }

        }

        /// <summary>
        /// First time we start set things up.
        /// </summary>
        public static void FirstRun()
        {

            MyLog.Info(AppCore.i18t, "Start setup for first run of {AppName}", AppCore.AppName);

            // App settings
            DefaultPaths();
            DefaultSettings();

            // Find minecraft versions
            UpdateHelper.CheckForUpdates();

            // Setup 1 new server.
            MinecraftServerHelper.CreateNewServer("Yet another Minecraft server", DBHelper.GetSetting(YAMSSetting.DefaultServerMemory).GetValueAsInt);

            //Tell the DB that we've run this
            MyLog.Info("Setup of YAMS complete.");
            DBHelper.SetSetting(YAMSSetting.FirstRunCompleted, "true");

        }

        /// <summary>
        /// Setup the standard path of the app
        /// </summary>
        public static void DefaultPaths()
        {
            MyLog.Info("Creating default directories.");
            if (!Directory.Exists(FilesAndFoldersHelper.AppsFolder)) Directory.CreateDirectory(FilesAndFoldersHelper.AppsFolder);

            if (!Directory.Exists(FilesAndFoldersHelper.LibFolder)) Directory.CreateDirectory(FilesAndFoldersHelper.LibFolder);

            if (!Directory.Exists(FilesAndFoldersHelper.StorageFolder)) Directory.CreateDirectory(FilesAndFoldersHelper.StorageFolder);

            if (!Directory.Exists(FilesAndFoldersHelper.JarFolder)) Directory.CreateDirectory(FilesAndFoldersHelper.JarFolder);

        }

        /// <summary>
        /// Setup default settings for new installation and when installing a uppdated version.
        /// </summary>
        public static void DefaultSettings()
        {
            int InstallVersion = 0;
            // Find what version allready installed.
            try { InstallVersion = DBHelper.GetSetting(YAMSSetting.YAMSInstalledVersion).GetValueAsInt; }
            catch { }

            switch (InstallVersion)
            {
                case 0:
                    DBHelper.SetSetting(YAMSSetting.DefaultServerMemory, "1024");
                    DBHelper.SetSetting(YAMSSetting.DefaultAutostartServer, "true");                    
                    DBHelper.SetSetting(YAMSSetting.ListenPortAdmin, "56552"); //Use an IANA legal internal port 49152 - 65535
                    DBHelper.SetSetting(YAMSSetting.ListenPortPublic, Convert.ToString(NetworkHelper.FindNextAvailablePort(80))); //Find nearest open port to 80 for public site
                    DBHelper.SetSetting(YAMSSetting.ExternalIP, NetworkHelper.GetExternalIP().ToString());
                    DBHelper.SetSetting(YAMSSetting.ListenIP, NetworkHelper.GetListenIP().ToString());
                    DBHelper.SetSetting(YAMSSetting.StoragePath, FilesAndFoldersHelper.StorageFolder);
                    DBHelper.SetSetting(YAMSSetting.EnablePublicSite, "true");
                    DBHelper.SetSetting(YAMSSetting.YAMSGuid, Guid.NewGuid().ToString());
                    DBHelper.SetSetting(YAMSSetting.ServerDefaultWait, "10000");
                    DBHelper.SetSetting(YAMSSetting.CultureAndCountry, "sv-SE");

                    // DBHelper.SetSetting("EnableTelnet", "false");
                    // DBHelper.SetSetting("TelnetPort", "56553");
                    // DBHelper.SetSetting("EnablePortForwarding", "true");
                    // DBHelper.SetSetting("EnableOpenFirewall", "true");
                    // DBHelper.SetSetting("UpdateBranch", "live");
                    // DBHelper.SetSetting("EnableJavaOptimisations", "true");

                    DBHelper.SetSetting(YAMSSetting.YAMSInstalledVersion, "1");
                    break;
                    //    DBHelper.SaveSettingYAMS("YAMSInstallVersion", "3");
                    //    goto case 3;
                    //case 3:
                    //default:
                    //    DBHelper.SetSetting("YAMSInstalledVersion", "1");
                    //    break;

            }

        }

    }

}
