using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{

    public enum JobAction
    {
        NotSet = 0,
        Backup = 1,
        ClearBackup = 2,
        Update = 3,
    }

    public partial class JobSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public JobAction Action { get; set; }

        public string Config { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public int ServerId { get; set; }

    }

    public class JobSettingConfig
    {
        public JobSettingConfigClearBackup ClearBackup { get; set; }

    }

    public class JobSettingConfigClearBackup
    {
        public int Period { get; set; }
        public int Value { get; set; }
    }

}
