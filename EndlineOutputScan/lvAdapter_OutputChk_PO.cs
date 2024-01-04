using System.Data;
using System.Collections.Generic;

using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace EndlineOutputScan
{
    class lvAdapter_OutputChk_PO : BaseAdapter
    {
        Context context;
        DataTable mdt;
        string strFt;
        public static List<CustomClassAdapter> POList = new List<CustomClassAdapter>();
        public lvAdapter_OutputChk_PO(Context c, DataTable dt, string filter)
        {
            context = c;
            mdt = dt;
            strFt = filter;
            Data();
        }

        public override int Count
        {
            get { return POList.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        // create a new ImageView for each item referenced by the Adapter
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View listPOView = convertView;

            if (listPOView == null)
            {
                listPOView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.JobnPOview, parent, false);
                TextView POName = listPOView.FindViewById<TextView>(Resource.Id.textView1);
                TextView PODtl = listPOView.FindViewById<TextView>(Resource.Id.textView2);
                TextView ReqQty = listPOView.FindViewById<TextView>(Resource.Id.textView3);
                TextView SwOP = listPOView.FindViewById<TextView>(Resource.Id.textView4);
                TextView FGWH = listPOView.FindViewById<TextView>(Resource.Id.textView5);
                TextView SwBl = listPOView.FindViewById<TextView>(Resource.Id.textView6);

                //resizing layout
                var lvVwGroup = (ViewGroup)listPOView.FindViewById<RelativeLayout>(Resource.Id.rlLJobnPOvw);
                CSDL.ScreenStretching(1, lvVwGroup);

                listPOView.Tag = new ViewHolder()
                {
                    vhPOName = POName,
                    vhPODtl = PODtl,
                    vhPOQty = ReqQty,
                    vhPOSwOP = SwOP,
                    vhPOFGWH = FGWH,
                    vhPOBL = SwBl
                };
            }
            else
            {
                listPOView = (View)convertView;
            }
            ViewHolder vhCurr = (ViewHolder)listPOView.Tag;
            vhCurr.vhPOName.Text = POList[position].POName;
            vhCurr.vhPODtl.Text = POList[position].PODtl.ToString();
            vhCurr.vhPOQty.Text = POList[position].POQty.ToString();
            vhCurr.vhPOSwOP.Text = POList[position].POSwOP.ToString();
            vhCurr.vhPOFGWH.Text = POList[position].POFGWH.ToString();
            vhCurr.vhPOBL.Text = POList[position].POSwBL.ToString();

            if (position % 2 == 1) listPOView.SetBackgroundColor(Color.ParseColor("#E8F8F5")); //"#FFCCCB4C"
            else listPOView.SetBackgroundColor(Color.ParseColor("#D1F2EB"));

            return listPOView;
        }
        public void Data()
        {
            POList.Clear();
            DataRow[] mydr = mdt.Select("PONo like '%" + strFt + "'");
            foreach (DataRow myr in mydr)
            {
                POList.Add(new CustomClassAdapter
                {
                    POName = myr["PONo"].ToString(),
                    PODtl = myr["Color"].ToString(),
                    POQty = string.IsNullOrEmpty(myr["ReqQty"].ToString()) ? 0 : int.Parse(myr["ReqQty"].ToString()),
                    POSwOP = string.IsNullOrEmpty(myr["SwOP"].ToString()) ? 0 : int.Parse(myr["SwOP"].ToString()),
                    POFGWH = string.IsNullOrEmpty(myr["FgWH"].ToString()) ? 0 : int.Parse(myr["FgWH"].ToString()),
                    POSwBL = string.IsNullOrEmpty(myr["SwBL"].ToString()) ? 0 : int.Parse(myr["SwBL"].ToString())
                });
            }
        }
    }
}
