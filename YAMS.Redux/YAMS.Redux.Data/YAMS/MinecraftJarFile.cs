using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{
    public class MinecraftJarFile
    {
        [Key]
        public int Id { get; set; }

        public DateTime Added { get; set; }

        public MinecraftServerType TypeOfServer { get; set; }

        [StringLength(100)]
        public string VersionName { get; set; }

    }

}
