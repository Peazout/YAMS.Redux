using System;
using System.Collections.Generic;
using System.Text;

namespace YAMS.Redux.Data
{
    public partial class YAMSSettingItem
    {

        public string GetValue
        {
            get { return Value; }
        }

        public int GetValueAsInt
        {
            get
            {
                try { return int.Parse(Value); }
                catch { }
                return 0;
            }
        }


        public bool GetValueAsBool
        {
            get
            {
                try { return bool.Parse(Value); }
                catch { }
                return false;
            }

        }

    }

}
