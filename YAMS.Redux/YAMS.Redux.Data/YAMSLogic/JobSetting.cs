using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YAMS.Redux.Data
{
    public partial class JobSetting
    {

        public JobSetting()
        {
            Hour = -1;
            Minute = -1;
            ServerId = -1;
        }

    }

}
