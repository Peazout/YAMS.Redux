using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text;
using YAMS.Redux.Data;
using YAMS.Redux.Data.Mojang;

namespace YAMS.Redux.Core.Web
{
    public class YAMSWebClient
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
        private static NLog.LogEventInfo GetLogEvent(NLog.LogLevel Lvl, string Message, int ServerId = -1)
        {
            NLog.LogEventInfo theEvent = new NLog.LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion

        public IServiceClient Client { get; set; }

        public YAMSWebClient(string serviceurl)
        {

            Client = new JsonServiceClient(serviceurl);
           

        }

        public MojangManifest GetMojangManifestFile(string servicename)
        {
            try
            {
                var response = Client.Get<MojangManifest>(servicename);
                return response;

            } 
            catch (Exception ex)
            {
                throw;
            }

        }

        public MojangDownloadFile GetDownloadFile(string servicename)
        {
            try
            {
                var response = Client.Get<MojangDownloadFile>(servicename);
                return response;

            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public void GetJarFile(string url,string saveto)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
