using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{

    public class Player
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Guid { get; set; }

        public DateTime LastConnected { get; set; }

        public int ServerId { get; set; }

    }

}
