using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace YAMS.Redux.Data
{
    public class ChattMessage
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [NotMapped]
        public string UserName { get; set; }

        public DateTime Logged { get; set; }

        [StringLength(512)]
        public string Message { get; set; }

        public int ServerId { get; set; }

    }

}
