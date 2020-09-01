using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace YAMS.Redux.Data
{

    public enum ServerMessageLevel
    {
        All = 0,
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warn = 4,
        Error = 5,
        Fatal = 6,
        Chat = 7
    }

    [Table("YAMSLogs")]
    public partial class LogRow
    {
        [Key]
        public int Id { get; set; }

        public string Application { get; set; }

        public DateTime? Logged { get; set; }

        [Required]
        [StringLength(50)]
        public string Level { get; set; }

        // [StringLength(255)]
        public string Message { get; set; }

        [StringLength(250)]
        public string Logger { get; set; }

        // [StringLength(255)]
        public string Exception { get; set; }

        public int? ServerId { get; set; }
    }

}