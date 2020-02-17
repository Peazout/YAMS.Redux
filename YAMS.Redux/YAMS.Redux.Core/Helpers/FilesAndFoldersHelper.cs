using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core.Helpers
{
    public class FilesAndFoldersHelper
    {

        #region Properties

        public static string RootFolder => new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;

        //Folders
        public static string LibFolder => Path.Combine(RootFolder, "lib");
        public static string DBFolder => Path.Combine(RootFolder, "db");
        public static string WebFolder => Path.Combine(RootFolder, "web");
        public static string BackupFolder => Path.Combine(RootFolder, "Backups");

        // Files
        public static string WebInstallPackageFile => Path.Combine(RootFolder, "YAMS-Web.zip");
        public static string YAMSPropertiesJson => Path.Combine(LibFolder, "properties.json");
        public static string YAMSConfig => Path.Combine(LibFolder, "YAMS.config");
        public static string CEFile => Path.Combine(DBFolder, "dbYAMS.sdf");

        // Web
        public static string HttpMCVersionFile => "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public static string HttpMCClient => "https://s3.amazonaws.com/MinecraftDownload/launcher/minecraft.jar";
        // public static string HttpMCServerJar(string VersionId) { return "https://s3.amazonaws.com/Minecraft.Download/versions/" + VersionId + "/minecraft_server." + VersionId + ".jar"; }
        public static string HttpWebAdmin => "http://" + DBHelper.GetSetting(YAMSSetting.ListenIP).GetValue + ":" + DBHelper.GetSetting(YAMSSetting.ListenPortAdmin).GetValue;

        // Folders
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
        public static string WebAdminLoginFile => Path.Combine(WebAdminFolder, "login.html");
        public static string WebAdminIndexFile => Path.Combine(WebAdminFolder, "index.html");
        public static string PIDFile => Path.Combine(RootFolder, "pids.txt");
        public static string MojangVersionsFile => Path.Combine(LibFolder, "mojang-versions.json");
        public static string YAMSVersionsFile => Path.Combine(LibFolder, "versions.json");
        public static string YAMSUpdater => Path.Combine(RootFolder, "YAMS.Updater.exe");
        public static string YAMSLibraryDll => Path.Combine(RootFolder, "YAMS-Library.dll");
        public static string YAMSService => Path.Combine(RootFolder, "YAMS-Service.exe");
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
        public static string LibServerJar(MinecraftServerType TypeOfServer, bool IsUpdateName = false)
        {
            string TmpStr = Path.Combine(new string[] { RootFolder, "lib", ServerJarName(TypeOfServer, IsUpdateName) });
            return TmpStr;
        }
        public static string MCServerJar(MinecraftServerType TypeOfServer, int ServerID, bool IsUpdateInName = false)
        {
            string TmpStr = Path.Combine(new string[] { MCServerFolder(ServerID), ServerJarName(TypeOfServer, ServerID, IsUpdateInName) });
            return TmpStr;
        }
        public static string ServerJarName(MinecraftServerType TypeOfServer, bool IsUpdateInName = false)
        {
            return ServerJarName(TypeOfServer, -1, IsUpdateInName);
        }
        public static string ServerJarName(MinecraftServerType TypeOfServer, int ServerID = -1, bool IsUpdateInName = false)
        {
            string JarFile = "";
            switch (TypeOfServer)
            {
                case MinecraftServerType.Snapshot:
                    JarFile = "minecraft_server_pre.jar";
                    break;
                case MinecraftServerType.Custom:
                    JarFile = Convert.ToString(DBHelper.GetMCConfig("ServerCustomJAR", ServerID).Value);
                    break;

                case MinecraftServerType.Vanilla:
                default:
                    JarFile = "minecraft_server.jar";
                    break;
            }

            if (IsUpdateInName) JarFile += ".UPDATE";
            return JarFile;
        }

        #endregion

    }
}
