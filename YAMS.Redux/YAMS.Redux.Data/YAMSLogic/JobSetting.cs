using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;

namespace YAMS.Redux.Data
{
    public partial class JobSetting
    {

        public JobSettingConfig Config { get; set; }

        public JobSetting()
        {
            Hour = -1;
            Minute = -1;
            ServerId = -1;
        }

        public void ParseArgs()
        {
            Config = JsonConvert.DeserializeObject<JobSettingConfig>(Args);
        }

    }
    
    public class JobSettingConfig
    {

        public JobSettingConfigClearBackup ClearBackup { get; set; }

    }

    public class JobSettingConfigClearBackup
    {
        public bool useExtendedMethod { get; set; }

        public JobSettingConfigPeriod Period { get; set; }

    }

    public class JobSettingConfigPeriod
    {
        public string Name { get; set; }
        public int Value { get; set; }

        /// <summary>
        /// Calculate the date from our values.
        /// </summary>
        /// <returns></returns>
        public DateTime GetDate()
        {
            DateTime DateLimit = DateTime.Now;
            switch (Name.ToLower())
            {
                case "yy":
                    DateLimit = DateLimit.AddYears(-Value);
                    break;
                case "mm":
                    DateLimit = DateLimit.AddMonths(-Value);
                    break;
                case "ww":
                    DateLimit = DateLimit.AddDays(-(Value * 7));
                    break;
                case "dd":
                    DateLimit = DateLimit.AddDays(-Value);
                    break;
                default:
                    // TODO: Something else than this?
                    throw new NotImplementedException("Unknown period for name: " + Name + ", using default days.");

            }

            return DateLimit;

        }

        /// <summary>
        /// Get a list of dates from now back to the end date.
        /// </summary>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public List<DateTime> GetDates(DateTime EndTime)
        {

            DateTime check = DateTime.Now.AddDays(-1);
            List<DateTime> dates = new List<DateTime>();

            do
            {
                dates.Add(check);
                check = check.AddDays(-1);
            } while (check.Subtract(EndTime).TotalDays >= 0);

            return dates;

        }

    }

}
