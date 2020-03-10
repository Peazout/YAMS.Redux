using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using YAMS.Redux.Core.Entity;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core.Helpers
{
    public static class JobHelper
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

        public static Timer WorkTimer { get; set; }

        public static void Init()
        {
            WorkTimer = new Timer(new TimerCallback(Tick), null, 0, 1 * 60 * 1000);

        }

        /// <summary>
        /// Preforming the jobs
        /// </summary>
        /// <param name="t"></param>
        public static void Tick(object t)
        {
            DateTime datNow = DateTime.Now;
            int minutes = datNow.Minute;
            int hour = datNow.Hour;

            // TODO: Should we check DNS name?
            // if (intMinutes % 5 == 0 && YAMSDataHelper.GetSettingYAMS("DNSName") != "") NetworkHelper.UpdateDNS();

            IEnumerable<JobSetting> jobs = null;
            try
            {
                jobs = DBHelper.GetJobs(hour, minutes);
                if (jobs == null)
                {
                    MyLog.Debug("No jobs to at this time.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MyLog.Error(ex, "Error when geting jobs from DB.");
                return;
            }


            foreach (var row in jobs)
            {
                row.ParseArgs();
                if (row.ServerId != -1)
                {
                    var server = MinecraftServerHelper.Servers[Convert.ToInt32(row.ServerId)];
                    SelectAndPreformJob(row, server);
                }
                else SelectAndPreformJob(row, null);

            }

        }

        private static void SelectAndPreformJob(JobSetting row, MinecraftServerItem server)
        {

            switch (row.Action)
            {
                case JobAction.Backup:
                    if (server.HasChanged) BackupHelper.CreateBackup(server);
                    break;

                case JobAction.ClearBackup:
                    BackupHelper.ClearBackups(server,row.Config.ClearBackup);
                    break;

                case JobAction.Update:
                    UpdateHelper.CheckForUpdates(); // Get the latestsversions
                    UpdateHelper.UpdateServers();
                    break;

                default:
                    MyLog.Warn("Invalid entry in Job database => " + row.Action);
                    break;

            }

        }

    }

}
