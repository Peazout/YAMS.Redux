using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace YAMS.Redux.Data
{
    public class MinecraftServerSetting
    {
        // [Key]
        // [Column(Order = 0)]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Value { get; set; }

        // [Key]
        // [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ServerId { get; set; }
    }

}
