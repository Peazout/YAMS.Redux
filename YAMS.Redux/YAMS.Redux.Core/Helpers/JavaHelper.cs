using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YAMS.Redux.Core.Helpers
{

    public static class JavaHelper
    {
        private static string JRERegKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
        private static string JDKRegKey = "SOFTWARE\\JavaSoft\\Java Development Kit";

        /// <summary>
        /// Detects if the JRE is installed using the regkey
        /// </summary>
        /// <returns>boolean indicating if the JRE is installed</returns>
        public static bool HasJRE()
        {
            try
            {
                RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey subKey = rk.OpenSubKey(JRERegKey, false);
                if (subKey != null) return true;
                else
                {
                    //CHeck 64bit
                    rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    subKey = rk.OpenSubKey(JRERegKey, false);
                    if (subKey != null) return true;
                    else return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detects if the JDK is installed using the regkey
        /// </summary>
        /// <returns>boolean indicating if the JDK is installed</returns>
        public static bool HasJDK()
        {
            try
            {
                RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey subKey = rk.OpenSubKey(JDKRegKey, false);
                if (subKey != null) return true;
                else
                {
                    //CHeck 64bit
                    rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    subKey = rk.OpenSubKey(JDKRegKey, false);
                    if (subKey != null) return true;
                    else return false;
                }
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// Creat path to java.exe based on what is installed.
        /// If JDK is installed that is first choise.
        /// </summary>
        /// <returns>String path to java</returns>
        public static string GetJavaExe()
        {
            var strFileName = Path.Combine(JavaPath(), "java.exe");
            if (HasJDK()) strFileName = Path.Combine(JavaPath("jdk"), "java.exe");

            return strFileName;
        }

        /// <summary>
        /// Get java version from registy, First 64bit if installed else 32bit.
        /// </summary>
        /// <param name="strType">jre or jdk</param>
        /// <returns></returns>
        public static string JavaVersion(string strType = "jre")
        {
            string StrReturn = GetJavaVersion(RegistryView.Registry64, strType);
            if (string.IsNullOrEmpty(StrReturn)) StrReturn = GetJavaVersion(RegistryView.Registry32, strType);
            return StrReturn;
        }

        private static string GetJavaVersion(RegistryView BitVersion, string StrType = "jre")
        {
            string strKey = "";
            switch (StrType)
            {
                case "jre":
                    strKey = JRERegKey;
                    break;
                case "jdk":
                    strKey = JDKRegKey;
                    break;
            }

            RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, BitVersion);
            RegistryKey subKey = rk.OpenSubKey(strKey, false);
            if (subKey != null) return subKey.GetValue("CurrentVersion").ToString();
            return "";
        }

        /// <summary>
        /// Get the path to java binaries. First 64bit else 32bit
        /// </summary>
        /// <param name="strType">jre or jdk</param>
        /// <returns></returns>
        public static string JavaPath(string strType = "jre")
        {
            string StrReturn = GetJavaPath(RegistryView.Registry64, strType);
            if (string.IsNullOrEmpty(StrReturn)) StrReturn = GetJavaPath(RegistryView.Registry32, strType);
            return StrReturn;
        }
        private static string GetJavaPath(RegistryView BitVersion, string javatype = "jre")
        {
            string strKey = "";
            switch (javatype)
            {
                case "jre":
                    strKey = JRERegKey;
                    break;
                case "jdk":
                    strKey = JDKRegKey;
                    break;
            }

            strKey = Path.Combine(strKey, JavaVersion(javatype));
            RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, BitVersion);
            RegistryKey subKey = rk.OpenSubKey(strKey, false);
            if (subKey != null) return Path.Combine(subKey.GetValue("JavaHome").ToString(), "bin\\");
            return "";

        }

    }

}
