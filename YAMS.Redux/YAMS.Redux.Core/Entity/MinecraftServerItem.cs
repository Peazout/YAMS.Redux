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
            if (ServerId == -1)
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
                MyLog.Log(GetLogEvent(NLog.LogLevel.Warn, "Expected Java was not found, abort server start. Expected path => " + JavaPath, ServerId));
                return;
            }

            // Checking for server typ jar file
            var jarpath = FilesAndFoldersHelper.JarFile(Data.ServerType, Data.MinecraftJarFileId);
            if (!File.Exists(jarpath))
            {
                MyLog.Log(GetLogEvent(NLog.LogLevel.Error, "Server.jar was not found, expected => " + jarpath, ServerId));
                return;
            }

            // TODO: Remove file add to database?
            if (File.Exists(FilesAndFoldersHelper.MCServerArgsFile(ServerId)))
            {
                StreamReader reader = new StreamReader(FilesAndFoldersHelper.MCServerArgsFile(ServerId));
                String text = reader.ReadToEnd();
                reader.Close();
                Args = text;
            }
            else
            {
                Args = CreateProcessArgs(Args);
            }

            // Now setup the process to run the server.
            JavaRunningMinecraft = new Process();
            JavaRunningMinecraft.StartInfo.UseShellExecute = false;
            JavaRunningMinecraft.StartInfo.FileName = JavaPath;
            JavaRunningMinecraft.StartInfo.Arguments = Args;
            JavaRunningMinecraft.StartInfo.CreateNoWindow = true;
            JavaRunningMinecraft.StartInfo.RedirectStandardError = true;
            JavaRunningMinecraft.StartInfo.RedirectStandardInput = true;
            JavaRunningMinecraft.StartInfo.RedirectStandardOutput = true;
            JavaRunningMinecraft.StartInfo.WorkingDirectory = FilesAndFoldersHelper.MCServerFolder(ServerId);

            //Set up events
            JavaRunningMinecraft.OutputDataReceived += new DataReceivedEventHandler(ServerMessageHandler);
            JavaRunningMinecraft.ErrorDataReceived += new DataReceivedEventHandler(ServerMessageHandler);
            JavaRunningMinecraft.EnableRaisingEvents = true;
            JavaRunningMinecraft.Exited += new EventHandler(ServerExited);

            //Finally start the thing
            JavaRunningMinecraft.Start();
            JavaRunningMinecraft.BeginOutputReadLine();
            JavaRunningMinecraft.BeginErrorReadLine();

            SafeStop = false;

            MyLog.Info(AppCore.i18t, "Server {ServerId} Started with args => " + Args, ServerId);

            //Try and open the firewall port
            if (YAMSDataHelper.GetSettingYAMS("EnableOpenFirewall") == "true") NetworkHelper.OpenFirewallPort(Port, Data.ServerTitle);
            if (YAMSDataHelper.GetSettingYAMS("EnablePortForwarding") == "true") NetworkHelper.OpenUPnP(Port, Data.ServerTitle, ListenIP);

            //Save the process ID so we can kill if there is a crash
            PID = JavaRunningMincraft.Id;
            PIDHandler.AddPID(JavaRunningMincraft.Id);

        }

        /// <summary>
        /// Create string with args/commandline options for running the java server.
        /// </summary>
        /// <param name="given"></param>
        /// <returns></returns>
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
        /// Process messages from the server and console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerMessageHandler(object sender, DataReceivedEventArgs e)
        {
            DateTime timestamp = DateTime.Now;
            var msg = new MinecraftServerMessage(e);
            if (!msg.IsNullMessage) MyLog.Trace("Servermessage => " + msg.Received.Data);
            else return;

            switch (msg.GetMessageLevel())
            {
                case ServerMessageLevel.Chat:
                    ChatMessage chat = msg.GetChatMessage();
                    Player user = DBHelper.GetPlayer(ServerId, "", chat.UserName);
                    if (user == null)
                    {
                        user = DBHelper.AddPlayer(chat.UserName, ServerId);
                    }
                    chat.UserId = user.Id;
                    chat.ServerId = ServerId;
                    DBHelper.AddChatMessage(chat);
                    break;

                case ServerMessageLevel.Error:

                    break;

                default:
                    //See if it's a log in or log out event
                    if (msg.IsUserLogin) DoPlayerLogin(msg.UserLoginName);
                    if (msg.IsUserLogout) DoPlayerLogout(msg.UserLogoutName);
                    if (msg.IsUUIDInMessage) SavePlayerUUID(msg.UUID, msg.UUIDName, ServerId);

                    //See if it's the server version tag
                    if (regServerVersion.Match(str).Success) ServerVersion = str.Replace("Starting minecraft server version ", "");

                    //Detect game type
                    if (regGameMode.Match(str).Success) GameMode = Convert.ToInt32(regGameMode.Match(str).Groups[1].Value);
                    if (str.IndexOf("You need to agree to the EULA in order to run the server. Go to eula.txt for more info.") > -1)
                    {
                        this.AgreeEULA = true;
                    }

                    // Are we saving the world?
                    if (str.Equals("Saved the game") && GameWorldSaved == MessageReceivedBit.Set) GameWorldSaved = MessageReceivedBit.Received;

                    MyLog.Log(GetLogEvent(NLog.LogLevel.Info, str, ServerId));
                    break;

            }

        }

        private void SavePlayerUUID(string uuid, string username, int serverid)
        {
            var p = DBHelper.GetPlayerByName(serverid, username);
            if (p == null)
            {
                DBHelper.AddPlayer(username, serverid, uuid);
            }
            else if (p.Guid == null)
            {
                p.Guid = uuid;
                DBHelper.UpdatePlayer(p);
            }

        }

        private void DoPlayerLogout(string username)
        {
            Players.Remove(username);
            //Check if we should restart the server for an update or a request
            // if (RestartWhenFree) IfEmptyRestart();
        }

        /// <summary>
        /// Login events
        /// </summary>
        private void DoPlayerLogin(string username)
        {
            int intCounter = 0;
            string strSafeName = username;
            while (Players.ContainsKey(strSafeName))
            {
                //Player is logged in? Change their temp name
                intCounter++;
                strSafeName = username + "-" + intCounter.ToString(AppCore.i18t);
            }



            Players.Add(strSafeName, new Player(username, this));
            HasChanged = true;
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
