using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{
    public class ServerProcessID
    {

        [Key]
        public int Id { get; set; }

        public int PId { get; set; }

        public int ServerId { get; set; }

        public DateTime Added { get; set; }

        public bool Active { get; set; }

    }

}
