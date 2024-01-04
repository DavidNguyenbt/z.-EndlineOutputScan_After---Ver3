using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace EndlineOutputScan
{
    class ViewHolder : Java.Lang.Object
    {
        public TextView vhPOName { get; set; }
        public TextView vhPOQty { get; set; }
        public TextView vhPODtl { get; set; }
        public TextView vhPOSwOP { get; set; }
        public TextView vhPOFGWH { get; set; }
        public TextView vhPOBL { get; set; }
        public TextView vhPOSize { get; set; }

    }
}