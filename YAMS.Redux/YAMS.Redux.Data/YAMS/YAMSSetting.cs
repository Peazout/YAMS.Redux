using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{
    public class YAMSSetting
    {

        [Key]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Value { get; set; }

    }

}
