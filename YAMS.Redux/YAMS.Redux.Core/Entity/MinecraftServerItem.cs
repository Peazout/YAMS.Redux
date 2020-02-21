using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using YAMS.Redux.Core.Helpers;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core.Entity
{
    public class MinecraftServerItem
    {

        public MinecraftServer Data { get; set; }

        public Dictionary<string, Player> Players { get; set; }

        public bool Running { get; set; }

        public Process JavaRunningMinecraft { get; set; }

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
        private static LogEventInfo GetLogEvent(NLog.LogLevel Lvl, string Message, int ServerId = -1)
        {
            LogEventInfo theEvent = new LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion

        public MinecraftServerItem()
        {
            Players = new Dictionary<string, Player>();

        }

        public bool IsReadyForRestart()
        {

            if ((Data.AllowAutoUpdate & AutoUpdateFlags.IfEmpty) == AutoUpdateFlags.IfEmpty)
            {
                // Are players connected.
                if (Players.Count > 0) return false;

            }

            return true;

        }

        public bool IsAutoUpdateSet
        {
            get
            {
                if ((Data.AllowAutoUpdate & AutoUpdateFlags.AutoUpdate) != AutoUpdateFlags.AutoUpdate) return true;
                return false;
            }
        }

        

        public void Restart(int waittime)
        {
            if (Running)
            {
                MyLog.Log(GetLogEvent(NLog.LogLevel.Warn, "Restarting server.", Data.Id));
                Stop();
                System.Threading.Thread.Sleep(waittime);
                Start();
            }


        }

        public void Start()
        {
            if (Running) return;
            if (Data.Id == -1)
            {
                MyLog.Error("Can not start a server without ID."); // Do we need this, we throw a exception
                throw new Exception("No MinecraftServer ID");
            }

            //TODO: Check property file, is there changes done on HDD/DB what do we count as master.
            string Args = "";
            string JavaPath = JavaHelper.GetJavaExe();

            // Checking java bin
            if (!File.Exists(JavaPath))
            {
                MyLog.Log(GetLogEvent(NLog.LogLevel.Warn, "Expected Java was not found, abort server start. Expected path => " + JavaPath, Data.Id));
                return;
            }

            // Checking for server typ jar file
            var jarpath = FilesAndFoldersHelper.JarFile(Data.ServerType, Data.MinecraftJarFileId);
            if (!File.Exists(jarpath))
            {
                MyLog.Log(GetLogEvent(NLog.LogLevel.Error, "Server.jar was not found, expected => " + jarpath, Data.Id));
                return;
            }

        }

        /// <summary>
        /// Send the stop command too the server for it to stop.
        /// </summary>
        /// <param name="forcestop">Stop the processes instead.</param>
        public void Stop(bool forcestop = false)
        {
            if (!Running) return;

            if (forcestop) JavaRunningMinecraft.Kill();
            else
            {
                Send("stop");
                JavaRunningMinecraft.WaitForExit();
            }

            JavaRunningMinecraft.CancelErrorRead();
            JavaRunningMinecraft.CancelOutputRead();
            string msg = "Server stopped";
            if (forcestop) msg += " by force";
            MyLog.Log(GetLogEvent(NLog.LogLevel.Info, msg, Data.Id));

        }

        #region Commands

        /// <summary>
        /// Send the command to the standard input of the process runing the Minecraft jar.
        /// </summary>
        /// <param name="strMessage"></param>
        public void Send(string strMessage)
        {
            if (!Running) return;
            JavaRunningMinecraft.StandardInput.WriteLine(strMessage);
        }
        /// <summary>
        /// Command to Minecraft server.
        /// </summary>
        public void Save(int waittime)
        {
            Send("save-all");
            //Generally this needs a long wait
            System.Threading.Thread.Sleep(waittime);

        }
        /// <summary>
        /// Command to Minecraft server.
        /// </summary>
        public void EnableSaving(int waittime)
        {
            Send("save-on");
            //Generally this needs a long wait
            System.Threading.Thread.Sleep(waittime);
        }
        /// <summary>
        /// Command to Minecraft server.
        /// </summary>
        public void DisableSaving(int waittime)
        {
            Send("save-off");
            //Generally this needs a long wait
            System.Threading.Thread.Sleep(waittime);
        }
        /// <summary>
        /// Command to Minecraft server.
        /// </summary>
        public void Whisper(string strUsername, string strMessage)
        {
            this.Send("tell " + strUsername + " " + strMessage);
        }

        #endregion

    }

}
