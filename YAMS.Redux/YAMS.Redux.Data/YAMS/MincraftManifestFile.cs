using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{
    public class MinecraftManifestFile
    {

        [Key]
        public int Id { get; set; }

        public DateTime Added { get; set; }

        [Required]
        [StringLength(255)]
        public string URL { get; set; }

        [Required]
        [StringLength(100)]
        public string ETag { get; set; }

        public MinecraftServerType TypeOfServer { get; set; }

    }

}
