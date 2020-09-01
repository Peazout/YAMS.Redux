using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YAMS.Redux.Data;

namespace YAMS.Redux.Core.Entity
{
    public class MinecraftServerMessage
    {
        private readonly Regex regRemoveDateStamp = new Regex(@"^([0-9]+\-[0-9]+\-[0-9]+ )");
        private readonly Regex regRemoveTimeStamp = new Regex(@"^\[([0-9]+:[0-9]+:[0-9]+\] )");
        private readonly Regex regErrorLevel = new Regex(@"^\[([\w\s\#]+)/([A-Z]+)\]: {1}");
        private Regex regPlayerChat = new Regex(@"^(\<([\w-~])+\>){1}");
        private Regex regConsoleChat = new Regex(@"^(\[CONSOLE\]|\[Server\]|\<\*Console\>){1}");
        private Regex regPlayerPM = new Regex(@"^(\[([\w])+\-\>(\w)+\]){1}");
        private Regex regPlayerUUID = new Regex(@"^(UUID of player )([\w]+)( is )([\w\-]+)");
        // Do not match IPv6
        private Regex regPlayerLoggedIn = new Regex(@"^([\w]+)(?:\s*)(?:\[\/[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\:[0-9]+\] logged in with entity id)");
        private Regex regPlayerLoggedOut = new Regex(@"^([\w]+) ?(lost connection)");
        private Regex regServerVersion = new Regex(@"^(?:Starting minecraft server version )");
        private Regex regGameMode = new Regex(@"^(?:Default game type:) ([0-9])");
        // CATCH
        private string _GetMessageWithOutLevel;
        private string _GetMessageWithOutDateTime;


        public DataReceivedEventArgs Received { get; private set; }

        public MinecraftServerMessage(DataReceivedEventArgs args)
        {
            Received = args;
        }

        /// <summary>
        /// Check if message is empty.
        /// </summary>
        public bool IsNullMessage
        {
            get
            {
                //Catch null messages (usually as server is going down)
                if (Received == null || Received.Data == null || Received.Data == ">") return true;
                return false;
            }
        }

        public bool IsUserLogin
        {
            get
            {
                if (regPlayerLoggedIn.Match(GetMessageWithOutLevel()).Success) return true;
                return false;
            }

        }
        public string UserLoginName
        {
            get
            {
                return regPlayerLoggedIn.Match(GetMessageWithOutLevel()).Groups[1].Value;
            }

        }

        public bool IsUserLogout
        {
            get
            {
                if (regPlayerLoggedOut.Match(GetMessageWithOutLevel()).Success) return true;
                return false;
            }

        }
        public string UserLogoutName
        {
            get
            {
                return regPlayerLoggedOut.Match(GetMessageWithOutLevel()).Groups[1].Value;
            }

        }

        public bool IsUUIDInMessage
        {
            get
            {
                if (regPlayerUUID.Match(GetMessageWithOutLevel()).Success) return true;
                return false;
            }
        }
        public string UUID
        {
            get
            {
                return regPlayerUUID.Match(GetMessageWithOutLevel()).Groups[4].Value;
            }

        }
        public string UUIDName
        {
            get
            {
                return regPlayerUUID.Match(GetMessageWithOutLevel()).Groups[2].Value;
            }

        }


        /// <summary>
        /// Get the server message without the leading datetime.
        /// </summary>
        /// <returns></returns>
        public string GetMessageWithOutDateTime()
        {
            if (string.IsNullOrWhiteSpace(_GetMessageWithOutDateTime))
            {
                _GetMessageWithOutDateTime = Received.Data;
                _GetMessageWithOutDateTime = regRemoveDateStamp.Replace(_GetMessageWithOutDateTime, "");
                _GetMessageWithOutDateTime = regRemoveTimeStamp.Replace(_GetMessageWithOutDateTime, "");
            }

            return _GetMessageWithOutDateTime;
        }
        /// <summary>
        /// Message without datetime and without level.
        /// </summary>
        /// <returns></returns>
        public string GetMessageWithOutLevel()
        {
            if (string.IsNullOrWhiteSpace(_GetMessageWithOutLevel))
            {
                //Work out the error level then remove it from the string
                _GetMessageWithOutLevel = GetMessageWithOutDateTime();
                Match regMatch = regErrorLevel.Match(_GetMessageWithOutLevel);
                _GetMessageWithOutLevel = regErrorLevel.Replace(_GetMessageWithOutLevel, "").Trim();
            }

            return _GetMessageWithOutLevel;
        }

        public ChatMessage GetChatMessage()
        {
            ChatMessage ThisMessage = null;
            string str = GetMessageWithOutLevel();
            Match regMatch = regPlayerChat.Match(str);
            if (regMatch.Success)
            {
                ThisMessage = new ChatMessage();
                ThisMessage.Message = str.Replace(regMatch.Groups[0].Value, "").Trim();
                ThisMessage.UserName = regMatch.Groups[0].Value;
                ThisMessage.UserName = ThisMessage.UserName.Substring(1).Replace(">", "");

                return ThisMessage;
            }

            regMatch = regConsoleChat.Match(str);
            if (regMatch.Success)
            {
                ThisMessage = new ChatMessage();
                ThisMessage.Message = str.Replace(regMatch.Groups[0].Value, "").Trim();
                ThisMessage.UserName = regMatch.Groups[0].Value;
                // ThisMessage.UserName = ThisMessage.UserName.Substring(1).Remove((ThisMessage.UserName.Length - 2), 1);

                return ThisMessage;
            }


            regMatch = regPlayerPM.Match(str);
            if (regMatch.Success)
            {
                ThisMessage = new ChatMessage();

            }

            return ThisMessage;
        }

        public ServerMessageLevel GetMessageLevel()
        {
            //Work out the error level then remove it from the string
            string str = GetMessageWithOutDateTime();
            Match regMatch = this.regErrorLevel.Match(str);
            str = this.regErrorLevel.Replace(str, "").Trim();

            if (regMatch.Success)
            {
                switch (regMatch.Groups[2].Value)
                {
                    case "INFO":
                        if (regPlayerChat.Match(str).Success || regPlayerPM.Match(str).Success || regConsoleChat.Match(str).Success) { return ServerMessageLevel.Chat; }
                        return ServerMessageLevel.Info;

                    case "WARNING": return ServerMessageLevel.Warn;
                    default: return ServerMessageLevel.Error;
                }
            }
            return ServerMessageLevel.Error;

        }

    }

}
