using NLog;
using System;
using System.Collections.Generic;
using System.Text;

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
        private static LogEventInfo GetLogEvent(LogLevel Lvl, string Message, int ServerId = -1)
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

        public static void CheckForUpdates()
        {



        }

    }

}
