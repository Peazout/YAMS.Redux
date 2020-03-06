using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core.Helpers
{
    public class FilesAndFoldersHelper
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

        #region Properties

        public static string RootFolder => new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;

        // Web
        public static string HttpMojang => "https://launchermeta.mojang.com/";
        public static string HttpMojangManifest => "mc/game/version_manifest.json";
        public static string HttpWebAdmin => "http://" + DBHelper.GetSetting(YAMSSetting.ListenIP).GetValue + ":" + DBHelper.GetSetting(YAMSSetting.ListenPortAdmin).GetValue;

        // Folders
        public static string LibFolder => Path.Combine(RootFolder, "lib");
        public static string DBFolder => Path.Combine(RootFolder, "db");
        public static string WebFolder => Path.Combine(RootFolder, "web");
        public static string JarFolder => Path.Combine(RootFolder, "jar");
        public static string BackupFolder => Path.Combine(RootFolder, "Backups");
        public static string WebAdminFolder => Path.Combine(WebFolder, "admin");
        public static string WebTemplatesFolder => Path.Combine(WebFolder, "templates");
        public static string WebAssetsFolder => Path.Combine(WebFolder, "assets");
        public static string StorageFolder => Path.Combine(RootFolder, "servers");
        public static string AppsFolder => Path.Combine(RootFolder, "apps");
        public static string MCServerFolder(int ServerId) { return Path.Combine(StorageFolder, ServerId.ToString()); }
        public static string MCServerWorldFolder(int ServerId) { return Path.Combine(MCServerFolder(ServerId), "world"); }
        public static string MCServerBackupFolder(int ServerId) { return Path.Combine(BackupFolder, ServerId.ToString()); }
        public static string MCServerBackupTemp(int ServerId) { return Path.Combine(MCServerBackupFolder(ServerId), "temp"); }
        public static string MCServerRendersFolder(int ServerId) { return Path.Combine(MCServerFolder(ServerId), "renders"); }
        public static string MCServerTectonicusFolder(int ServerId) { return Path.Combine(MCServerRendersFolder(ServerId), "tectonicus"); }
        public static string MCServerOverviewerFolder(int ServerId) { return Path.Combine(MCServerRendersFolder(ServerId), "overviewer"); }
        public static string MCServerPlayerDataFolder(int ServerId) { return Path.Combine(MCServerWorldFolder(ServerId), "playerdata"); }
        public static string MCServerOverviewerOutputFolder(int ServerId) { return Path.Combine(MCServerOverviewerFolder(ServerId), "output"); }
        public static string MCClientUserFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\");
        public static string MCClientSystemFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"config\systemprofile\AppData\Roaming\.minecraft\");

        // Files
        public static string WebInstallPackageFile => Path.Combine(RootFolder, "YAMS-Web.zip");
        public static string YAMSPropertiesJson => Path.Combine(LibFolder, "properties.json");
        public static string YAMSConfig => Path.Combine(LibFolder, "YAMS.config");
        public static string CEFile => Path.Combine(DBFolder, "dbYAMS.sdf");
        public static string WebAdminLoginFile => Path.Combine(WebAdminFolder, "login.html");
        public static string WebAdminIndexFile => Path.Combine(WebAdminFolder, "index.html");
        public static string PIDFile => Path.Combine(RootFolder, "pids.txt");
        public static string MojangVersionsFile => Path.Combine(LibFolder, "mojang-versions.json");
        public static string YAMSVersionsFile => Path.Combine(LibFolder, "versions.json");
        public static string YAMSUpdater => Path.Combine(RootFolder, "YAMS.Updater.exe");
        public static string YAMSLibraryDll => Path.Combine(RootFolder, "YAMS-Library.dll");
        public static string YAMSService => Path.Combine(RootFolder, "YAMS-Service.exe");
        public static string JarFile(MinecraftServerType servertype, int id) { return Path.Combine(JarFolder, servertype.ToString() + id.ToString() + ".jar"); }
        public static string MCServerArgsFile(int ServerId) { return Path.Combine(MCServerFolder(ServerId), "args.txt"); }
        public static string MCServerEulaFile(int ServerId) { return Path.Combine(MCServerFolder(ServerId), "eula.txt"); }
        public static string MCServerPropertyFile(int ServerId, bool IsUpdateName = false)
        {
            string StrFile = Path.Combine(MCServerFolder(ServerId), "server.properties");
            if (IsUpdateName) StrFile += ".UPDATE";
            return StrFile;
        }
        public static string ReleaseJSON(MinecraftServerType TypeOfServer, int ServerID)
        {
            string strName = "";
            switch (TypeOfServer)
            {
                case MinecraftServerType.Snapshot: strName = "mcServerPreRelease.json"; break;
                case MinecraftServerType.Custom: strName = "mcServerCustomRelease.json"; break;
                case MinecraftServerType.Vanilla:
                default: strName = "mcServerRelease.json"; break;
            }
            return Path.Combine(LibFolder, strName);

        }

        #endregion

        #region Copy

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                MyLog.Trace(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }

        }

        #endregion

    }
}
