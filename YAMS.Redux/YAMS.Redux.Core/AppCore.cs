using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using YAMS.Redux.Core.Helpers;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core
{
    public static class AppCore
    {

        private static Logger MyLog { get; set; }

        public static IConfiguration Config { get; set; }

        public static CultureInfo i18t
        {
            get
            {
                var str = DBHelper.GetSetting(Data.YAMSSetting.CultureAndCountry);
                if (string.IsNullOrWhiteSpace( str.GetValue)) return new CultureInfo("sv-SE");
                return new CultureInfo(str.GetValue);
            }

        }

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

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
            // .AddJsonFile($"appconfig.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
            Config = builder.Build();

            if (MyLog == null) MyLog = LogManager.GetCurrentClassLogger();
            MyLog.Info(i18t, "*** Execute of {Version} ***", Version);

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

            // Check for left over minecraft servers running.
            MinecraftServerHelper.KillGhostServers();

            UpdateHelper.Init();
            UpdateHelper.CheckForUpdates();

            // Now start servers
            MinecraftServerHelper.Init();

            // Start the jobs thread.
            JobHelper.Init();

            // Start the webserver for user interface.

            throw new NotImplementedException("Execute function not completed.");

            // Done
            MyLog.Info("Start completed, YAMS.Redux is now running.");

        }


        /// <summary>
        /// Whatever Executegets going, we teardown.
        /// </summary>
        public static void End()
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();

            throw new NotImplementedException("End function not completed.");

        }

    }

}
