using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YAMS.Redux.Core.Entity;

namespace YAMS.Redux.Core.Helpers
{
    public static class BackupHelper
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
        /// <param name="Lvl"></param>
        /// <param name="Message"></param>
        /// <param name="ServerId"></param>
        /// <returns></returns>
        private static NLog.LogEventInfo GetLogEvent(NLog.LogLevel Lvl, string Message, int ServerId = -1)
        {
            NLog.LogEventInfo theEvent = new NLog.LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion
               

        public static void CreateBackup(MinecraftServerItem server)
        {
            MyLog.Log(GetLogEvent(NLog.LogLevel.Info, "Backing up: " + server.Data.Name, server.ServerId));

            //Check for a backup dir and create if not
            if (!Directory.Exists(FilesAndFoldersHelper.MCServerWorldFolder(server.ServerId))) Directory.CreateDirectory(FilesAndFoldersHelper.MCServerWorldFolder(server.ServerId));

            //Command minecraft jar to force a save.
            server.Save();
            server.DisableSaving();

            //Find all the directories that start with "world"
            if (Directory.Exists(FilesAndFoldersHelper.MCServerBackupTemp(server.ServerId))) Directory.Delete(FilesAndFoldersHelper.MCServerBackupTemp(server.ServerId), true);
            if (!Directory.Exists(FilesAndFoldersHelper.MCServerBackupTemp(server.ServerId))) Directory.CreateDirectory(FilesAndFoldersHelper.MCServerBackupTemp(server.ServerId));

            string[] dirs = Directory.GetDirectories(FilesAndFoldersHelper.MCServerFolder(server.ServerId), "world*");
            foreach (string dir in dirs)
            {
                //Copy world to a temp Dir
                DirectoryInfo thisDir = new DirectoryInfo(dir);
                FilesAndFoldersHelper.Copy(dir, Path.Combine(FilesAndFoldersHelper.MCServerBackupTemp(server.ServerId), thisDir.Name));
            }

            //Re-enable saving then force another save
            server.EnableSaving();
            server.Save();

            //Now zip up temp dir and move to backups
            string FileName = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("D2") + "-" + DateTime.Now.Day.ToString("D2") + "-" + DateTime.Now.Hour.ToString("D2") + "-" + DateTime.Now.Minute.ToString("D2") + server.Data.Name + ".zip";

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(FilesAndFoldersHelper.MCServerBackupTemp(server.ServerId));
                zip.Save(Path.Combine(FilesAndFoldersHelper.MCServerBackupFolder(server.ServerId), FileName));
            }

            //If the server is empty, reset the HasChanged
            if (server.Players.Count == 0) server.HasChanged = false;

        }

        internal static void ClearBackups(MinecraftServerItem server)
        {
            throw new NotImplementedException();
        }
    }

}
