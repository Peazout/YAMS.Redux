using NLog;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core.Helpers
{
    public static class UpdateHelper
    {
        public static bool Paused { get; set; }

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

        public static void Init()
        {
            Paused = false;

        }

        /// <summary>
        /// Check for and add/download new versions of the jars.
        /// </summary>
        public static void CheckForUpdates()
        {

            if (Paused)
            {
                MyLog.Trace("Update functions are paused.");
                return;
            }

            CheckForUpdates(MinecraftServerType.Vanilla);
            CheckForUpdates(MinecraftServerType.Snapshot);
            //TODO: Check for installpackage YAMS.Redux           

        }

        /// <summary>
        /// Check for and add/download jars for the given servertype.
        /// </summary>
        private static void CheckForUpdates(MinecraftServerType servertype)
        {

            // Checking for updates Mojang
            var web = new Web.YAMSWebClient(FilesAndFoldersHelper.HttpMojangMeta, FilesAndFoldersHelper.HttpMojangJar);
            var manifest = web.GetMojangManifestFile(FilesAndFoldersHelper.HttpMojangManifest);
            var release = manifest.GetRelease(servertype);

            if (DBHelper.GetVersionFile(release.id, servertype) == null)
            {
                var download = web.GetDownloadFile(release.Url);
                var row = DBHelper.SetVersionFile(release.id, servertype);
                var filename = FilesAndFoldersHelper.JarFile(servertype, row.Id);
                web.GetJarFile(download.downloads.server.url, filename);

            }

        }


        public static void UpdateServers()
        {

            if (Paused)
            {
                MyLog.Trace("Update functions are paused.");
                return;
            }

            foreach (var sv in MinecraftServerHelper.Servers)
            {
                
                if (!sv.Value.IsAutoUpdateSet) break;
                MyLog.Info(AppCore.i18t,"Checking if server {title} needs update.", sv.Value.Data.Name);

                // What is the latests
                var ver = DBHelper.GetVersionFile(sv.Value.Data.ServerType);
                if (sv.Value.Data.MinecraftJarFileId == ver.Id) { MyLog.Log(GetLogEvent(NLog.LogLevel.Info, "Allready running the latest version.", sv.Value.Data.Id)); break; }
                else
                {
                    MyLog.Log(GetLogEvent(NLog.LogLevel.Warn, "Update jar version from " + sv.Value.Data.MinecraftJarFileId  + " to " + ver.Id + ".", sv.Value.Data.Id));
                    sv.Value.Data.MinecraftJarFileId = ver.Id;
                    DBHelper.UpdateServer(sv.Value.Data);
                }
                // Can we also restart server so it can run the new version.
                if (!sv.Value.IsReadyForRestart()) { MyLog.Warn("Server not ready for restart."); break; }
                else
                {
                    sv.Value.Restart();
                }
                                               
            }

            MyLog.Info("Finished update check");

        }

    }

}
