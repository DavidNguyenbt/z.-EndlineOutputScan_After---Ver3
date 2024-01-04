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
    class CustomClassAdapter
    {
        public string POName { set; get; }
        public string PODtl { set; get; }
        public int POQty { set; get; }
        public int POSwOP { set; get; }
        public int POFGWH { set; get; }
        public int POSwBL { set; get; }
        public string POSize { set; get; }
        public string POShipNo { set; get; }
    }
}