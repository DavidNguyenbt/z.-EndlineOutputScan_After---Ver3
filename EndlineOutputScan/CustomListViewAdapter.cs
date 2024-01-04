using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace EndlineOutputScan
{
    class CustomListViewAdapter : BaseAdapter
    {
        Context context;
        string PoFilter;
        public static List<CustomClassAdapter> POList = new List<CustomClassAdapter>();
        public CustomListViewAdapter(Context c, string w)
        {
            context = c;
            PoFilter = w;
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
                listPOView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.CustomListItem, parent, false);
                TextView POName = listPOView.FindViewById<TextView>(Resource.Id.textView1);
                TextView POQty = listPOView.FindViewById<TextView>(Resource.Id.textView2);
                TextView GmtType = listPOView.FindViewById<TextView>(Resource.Id.textView3);
                TextView color = listPOView.FindViewById<TextView>(Resource.Id.textView4);
                listPOView.Tag = new ViewHolder() { vhPOName = POName, vhPOQty = POQty, vhPODtl = GmtType, vhPOSize = color };
            }
            else
            {
                listPOView = (View)convertView;
            }
            ViewHolder vhCurr = (ViewHolder)listPOView.Tag;
            vhCurr.vhPOName.Text = POList[position].POName;
            vhCurr.vhPOQty.Text = POList[position].POQty.ToString();
            vhCurr.vhPODtl.Text = POList[position].PODtl;
            vhCurr.vhPOSize.Text = POList[position].POSize;

            if (position % 2 == 1) listPOView.SetBackgroundColor(Color.ParseColor("#E8F8F5")); //"#FFCCCB4C"
            else listPOView.SetBackgroundColor(Color.ParseColor("#D1F2EB"));

            return listPOView;
        }
        public void Data()
        {
            if (CSDL.drSelectedUPC != null) CSDL.drSelectedUPC = null;

            CSDL.drSelectedUPC = CSDL.dtUPCDetail.Select("UPC_Code='" + PoFilter + "'", "Po_No DESC"); //, DataViewRowState.CurrentRows);

            POList.Clear();

            foreach (DataRow myr in CSDL.drSelectedUPC)
            {
                CustomClassAdapter mycust = new CustomClassAdapter
                {
                    POName = myr["Po_No"].ToString(),
                    POQty = string.IsNullOrEmpty(myr["TtlQty"].ToString()) ? 0 : int.Parse(myr["TtlQty"].ToString()),
                    PODtl = myr["GarmentType"].ToString(),
                    POSize = myr["Color"].ToString(),//show color
                    POShipNo = myr["JobDetId"].ToString()//get shipno
                };
                if (!POList.Contains(mycust)) POList.Add(mycust);
            }
        }
    }
}