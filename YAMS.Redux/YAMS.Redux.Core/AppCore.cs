using NLog;
using System;
using System.Diagnostics;
using System.Reflection;
using YAMS.Redux.Core.Helpers;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core
{
    public static class AppCore
    {

        private static Logger MyLog { get; set; }

        public static bool IsService { get; private set; }
        public static string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
                // string assemblyVersion = Assembly.LoadFile('your assembly file').GetName().Version.ToString();
                // string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
                // string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            }
        }

        public static string AppName
        {
            get
            {
                return "YAMS.Redux";
            }

        }

        /// <summary>
        /// Main starting point for app/service.
        /// </summary>
        public static void Execute()
        {
            if (MyLog == null) MyLog = LogManager.GetCurrentClassLogger();
            MyLog.Info("*** Execute of {Version} ***", Version);

            // Check if we are a service
            Process cp = Process.GetCurrentProcess();
            if (cp.SessionId > 0)
                IsService = false;
            else
                IsService = true;
            MyLog.Debug("Running as service => " + IsService);
            // Done

            // Open connection to database.
            try
            {
                DBHelper.Init(); // Check connection to our database.
            }
            catch (Exception ex)
            {
                MyLog.Error(ex, "Datasource connection failed.");
                throw; // We need to end here, no point with out data?
            }

            // Check for old installfiles.
            InstallHelper.DeleteOldFiles(FilesAndFoldersHelper.RootFolder);

            // Check if this is the first time we run.
            if (!DBHelper.GetSetting(YAMSSetting.FirstRunCompleted).GetValueAsBool) InstallHelper.FirstRun();
            // And if it is not first run, still check if we have new settins to add or update
            InstallHelper.DefaultSettings();




            throw new NotImplementedException("Execute function not completed.");

            // Done
            MyLog.Info("Start completed, YAMS.Redux is now running.");

        }

    }

}
