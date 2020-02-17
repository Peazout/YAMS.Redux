using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace YAMS.Redux.Core.Helpers
{
    public static class NetworkHelper
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

        private const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";

        /// <summary>
        /// Get the first IP address on the system
        /// </summary>
        /// <returns>IP Address</returns>
        public static IPAddress GetListenIP()
        {
            IPHostEntry ipListen = Dns.GetHostEntry("");
            int i = 0;
            while (ipListen.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                i++;
            }
            return ipListen.AddressList[i];

        }

        /// <summary>
        /// Grab the external IP address using icanhazip.com
        /// </summary>
        /// <returns>IPAddress</returns>
        public static IPAddress GetExternalIP(string strUrl = "http://ipv4.icanhazip.com/")
        {
            string strExternalIPChecker = strUrl;
            WebClient wcGetIP = new WebClient();
            UTF8Encoding utf8 = new UTF8Encoding();
            string strResponse = "";
            try
            {
                strResponse = utf8.GetString(wcGetIP.DownloadData(strExternalIPChecker));
                strResponse = strResponse.Replace("\n", "");
            }
            catch (WebException e)
            {
                MyLog.Warn(e, "Unable to determine external IP.");
            }

            if (strResponse == "") strResponse = GetExternalIP("http://yams.in/getip.php").ToString();

            IPAddress ipExternal = null;
            ipExternal = IPAddress.Parse(strResponse);
            return ipExternal;

        }

        /// <summary>
        /// ### By Matt Brindley: http://www.mattbrindley.com/developing/windows/net/detecting-the-next-available-free-tcp-port/ ###
        /// Check if startPort is available, incrementing and
        /// checking again if it's in use until a free port is found
        /// </summary>
        /// <param name="startPort">The first port to check</param>
        /// <returns>The first available port</returns>
        public static int FindNextAvailablePort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                string.Concat("Global/", PortReleaseGuid));
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties =
                    IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints =
                    ipGlobalProperties.GetActiveTcpListeners();

                do
                {
                    if (!isAvailable)
                    {
                        port++;
                        isAvailable = true;
                    }

                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != port) continue;
                        isAvailable = false;
                        break;
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable)
                    throw new NotImplementedException();

                return port;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

    }

}
