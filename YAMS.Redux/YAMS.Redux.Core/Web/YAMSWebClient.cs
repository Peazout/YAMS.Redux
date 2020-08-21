using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YAMS.Redux.Data;
using YAMS.Redux.Data.Mojang;
using YAMS.Redux.Core.Helpers;


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

        public IServiceClient ClientMeta { get; set; }
        public IServiceClient Client { get; set; }


        public YAMSWebClient(string serviceurlmeta, string serviceurl)
        {

            ClientMeta = new JsonServiceClient(serviceurlmeta);
            Client = new JsonServiceClient(serviceurl);

        }

        public MojangManifest GetMojangManifestFile(string servicename)
        {
            try
            {
                var response = ClientMeta.Get<MojangManifest>(servicename);
                return response;

            }
            catch (Exception ex)
            {
                MyLog.Error(ex);
                throw;
            }

            return null;

        }

        public MojangDownloadFile GetDownloadFile(string servicename)
        {
            try
            {
                var response = ClientMeta.Get<MojangDownloadFile>(servicename);
                return response;

            }
            catch (Exception ex)
            {
                MyLog.Error(ex);
                throw;
            }

            return null;

        }


        public void GetJarFile(string url, string saveto)
        {
            try
            {
                // FilesAndFoldersHelper.
                var fs = new FileStream(saveto, FileMode.Create);
                var servicename = url.Replace(FilesAndFoldersHelper.HttpMojangJar, "");
                var stream = Client.Get<Stream>(servicename);

                CopyStream(stream, fs);
                stream.Close();
                fs.Close();

            }
            catch (Exception ex)
            {
                MyLog.Error(ex);
                throw;
            }

        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

    }
}
