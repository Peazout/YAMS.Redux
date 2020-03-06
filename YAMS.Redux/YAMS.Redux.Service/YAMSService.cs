using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using YAMS.Redux.Core;

namespace YAMS.Redux.Service
{
    public class YAMSService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry(AppCore.AppName + " service Startup", EventLogEntryType.Information);
            // If you need to debug this installer class, uncomment the line below
            // System.Diagnostics.Debugger.Break();
            AppCore.Execute();

            base.OnStart(args); // This should be last?

        }

        protected override void OnStop()
        {
            AppCore.End();
            // If you need to debug this installer class, uncomment the line below
            // System.Diagnostics.Debugger.Break();
            EventLog.WriteEntry(AppCore.AppName + " Shutdown", EventLogEntryType.Information);

            base.OnStop();
        }

    }

}
