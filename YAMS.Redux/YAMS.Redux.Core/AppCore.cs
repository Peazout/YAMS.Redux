using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace YAMS.Redux.Core
{
    public static class AppCore
    {

        private static Logger MyLog { get; set; }

        public static bool IsService { get; private set; }
        public static string YAMSVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
                // string assemblyVersion = Assembly.LoadFile('your assembly file').GetName().Version.ToString();
                // string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
                // string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            }
        }

        /// <summary>
        /// Main starting point for app/service.
        /// </summary>
        public static void Execute()
        {
            if (MyLog == null) MyLog = LogManager.GetCurrentClassLogger();
            MyLog.Info("*** Execute of {YAMSVersion} ***", YAMSVersion);

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

            }
            catch (Exception ex)
            {
                MyLog.Error(ex, "Datasource connection failed.");
                throw; // We need to end here, no point with out data?
            }



            MyLog.Info("Start completed, YAMS.Redux is now running.");

        }

    }

}
