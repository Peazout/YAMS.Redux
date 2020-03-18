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
        Clearlogs = 4,
        MinecraftStart = 5,
        MinecraftStop = 6,
        MinecraftRestart = 7,
        MinecraftClearlogs = 8,
        MinecraftCommand = 9,
    }

    public partial class JobSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public JobAction Action { get; set; }

        public string Args { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public int ServerId { get; set; }

    }

}
