using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{

    public enum YAMSSetting
    {
        NotSet = 0,
        FirstRunCompleted,
        DefaultServerMemory,
        DefaultAutostartServer,
        EnableJavaOptimisations,
        ListenPortAdmin,
        ListenPortPublic,
        ExternalIP,
        ListenIP,
        StoragePath,
        YAMSGuid,
        YAMSInstalledVersion,
        EnablePublicSite,
        ServerDefaultWait,

    }

    public partial class YAMSSettingItem
    {

        [Key]
        [StringLength(255)]
        public YAMSSetting Name { get; set; }

        [StringLength(255)]
        public string Value { get; set; }
               
    }

}
