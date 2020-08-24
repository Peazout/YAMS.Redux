using NLog;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
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

        public bool HasChanged { get; set; }

        public int ServerId => Data.Id;


        private int WaitTime
        {
            get
            {
                int t = 1000;
                try
                {
                    t = DBHelper.GetSetting(YAMSSetting.ServerDefaultWait).GetValueAsInt;
                }
                catch { throw; }
                return t;

            }

        }

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



        public void Restart()
        {
            if (Running)
            {
                MyLog.Log(GetLogEvent(NLog.LogLevel.Warn, "Restarting server.", Data.Id));
                Stop();
                System.Threading.Thread.Sleep(WaitTime);
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

            // TODO: Remove file add to database?
            if (File.Exists(FilesAndFoldersHelper.MCServerArgsFile(Data.Id)))
            {
                StreamReader reader = new StreamReader(FilesAndFoldersHelper.MCServerArgsFile(Data.Id));
                String text = reader.ReadToEnd();
                reader.Close();
                Args = text;
            }
            else
            {
                Args = CreateProcessArgs(Args);
            }



        }

        private string CreateProcessArgs(string given)        
        {
            //
            // Added from:
            // https://www.spigotmc.org/threads/guide-optimizing-spigot-remove-lag-fix-tps-improve-performance.21726/page-10#post-1055873
            //
            var args = new StringBuilder();
            args.Append("-server ");

            //If we have enabled the java optimisations add the additional
            //arguments. See http://www.minecraftforum.net/viewtopic.php?f=1012&t=68128
            if ((bool)Data.EnableServerOptimisations)
            {
                // Memory
                args.Append(" -Xmx" + Data.AssignedMemory + "M -Xms" + Data.AssignedMemory + @"M ");

                // GC Cores
                var intGCCores = Environment.ProcessorCount - 1;
                if (intGCCores == 0) intGCCores = 1;
                args.Append(" -XX:ParallelGCThreads = " + intGCCores + " ");



                // Needed for some of the options
                args.Append(" -UnlockExperimentalVMOptions ");

                // the rest
                args.Append(" -XX:+AlwaysPreTouch ");
                args.Append(" -XX:+UseLargePagesInMetaspace ");
                args.Append(" -XX:+UseConcMarkSweepGC ");
                args.Append(" -XX:+UseParNewGC ");
                args.Append(" -XX:+CMSIncrementalPacing ");

            }

            // After this the jarfilename will be added.
            args.Append(" -jar ");

            // And return
            return args.ToString();

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
        public void Save()
        {
            Send("save-all");
            //Generally this needs a long wait
            Thread.Sleep(WaitTime);

        }
        /// <summary>
        /// Command to Minecraft server.
        /// </summary>
        public void EnableSaving()
        {
            Send("save-on");
            //Generally this needs a long wait
            Thread.Sleep(WaitTime);
        }
        /// <summary>
        /// Command to Minecraft server.
        /// </summary>
        public void DisableSaving()
        {
            Send("save-off");
            //Generally this needs a long wait
            Thread.Sleep(WaitTime);
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
