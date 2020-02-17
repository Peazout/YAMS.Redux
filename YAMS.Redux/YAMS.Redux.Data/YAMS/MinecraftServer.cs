using System.ComponentModel.DataAnnotations;

namespace YAMS.Redux.Data
{

    public enum MinecraftServerType
    {
        Unknown = 0,
        Default,
        Vanilla,
        Snapshot, // Pre
        // Spigot, // https://www.spigotmc.org/wiki/about-spigot/
        Custom
    }

    public partial class MinecraftServer
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        public bool EnableServerOptimisations { get; set; }

        public int AssignedMemory { get; set; }

        public bool AutoStart { get; set; }

        [StringLength(100)]
        public MinecraftServerType ServerType { get; set; }

    }

}
