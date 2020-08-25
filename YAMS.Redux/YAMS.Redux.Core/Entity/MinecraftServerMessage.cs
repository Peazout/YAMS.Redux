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

        /// <summary>
        /// Get the server message without the leading datetime.
        /// </summary>
        /// <returns></returns>
        public string GetMessageWithOutDateTime()
        {
            string strMessage = Received.Data;
            strMessage = regRemoveDateStamp.Replace(strMessage, "");
            strMessage = regRemoveTimeStamp.Replace(strMessage, "");

            return strMessage;
        }
        /// <summary>
        /// Message without datetime and without level.
        /// </summary>
        /// <returns></returns>
        public string GetMessageWithOutLevel()
        {
            //Work out the error level then remove it from the string
            string str = GetMessageWithOutDateTime();
            Match regMatch = this.regErrorLevel.Match(str);
            return this.regErrorLevel.Replace(str, "").Trim();
        }

        public ChattMessage GetChatMessage()
        {
            ChattMessage ThisMessage = null;
            string str = GetMessageWithOutLevel();
            Match regMatch = regPlayerChat.Match(str);
            if (regMatch.Success)
            {
                ThisMessage = new Chat();
                ThisMessage.Message = str.Replace(regMatch.Groups[0].Value, "").Trim();
                ThisMessage.UserName = regMatch.Groups[0].Value;
                ThisMessage.UserName = ThisMessage.UserName.Substring(1).Replace(">", "");

                return ThisMessage;
            }

            regMatch = regConsoleChat.Match(str);
            if (regMatch.Success)
            {
                ThisMessage = new Chat();
                ThisMessage.Message = str.Replace(regMatch.Groups[0].Value, "").Trim();
                ThisMessage.UserName = regMatch.Groups[0].Value;
                // ThisMessage.UserName = ThisMessage.UserName.Substring(1).Remove((ThisMessage.UserName.Length - 2), 1);

                return ThisMessage;
            }


            regMatch = regPlayerPM.Match(str);
            if (regMatch.Success)
            {
                ThisMessage = new ChattMessage();

            }

            return ThisMessage;
        }

        public LogLevel GetMessageLevel()
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
                        if (regPlayerChat.Match(str).Success || regPlayerPM.Match(str).Success || regConsoleChat.Match(str).Success) { return LogLevel.Chat; }
                        return LogLevel.Info;

                    case "WARNING": return LogLevel.Warn;
                    default: return LogLevel.Error;
                }
            }
            return LogLevel.Error;

        }

    }

}
