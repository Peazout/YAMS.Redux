using System.ComponentModel.DataAnnotations;

namespace YAMS.Redux.Data
{
    public class MinecraftServer
    {
        [Key]
        public int ServerId { get; set; }

        [StringLength(255)]
        public string ServerName { get; set; }

        public bool ServerEnableServerOptimisations { get; set; }

        public int ServerAssignedMemory { get; set; }

        public bool ServerAutoStart { get; set; }

        [StringLength(100)]
        public string ServerType { get; set; }

    }

}
