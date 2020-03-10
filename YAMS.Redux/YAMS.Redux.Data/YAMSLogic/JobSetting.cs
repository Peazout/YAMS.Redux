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
        public int Period { get; set; }
        public int Value { get; set; }
    }


}
