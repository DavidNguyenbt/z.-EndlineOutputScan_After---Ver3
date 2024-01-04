using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CSDL;

namespace EndlineOutputScan
{
    [Activity(Label = "OutputChecking", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class OutputChecking : Activity
    {
        EditText edJobNo, edPONo;
        Button btFindJob, btFindPONo, btExit;
        ListView lvJobDetail, lvPODetail, lvPOSize;
        DataSet dsPkDtl;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CheckOutput);

            edJobNo = FindViewById<EditText>(Resource.Id.edJobNo);
            edPONo = FindViewById<EditText>(Resource.Id.edPONo);
            btFindJob = FindViewById<Button>(Resource.Id.btFindJob);
            btFindPONo = FindViewById<Button>(Resource.Id.btFindPONo);
            btExit = FindViewById<Button>(Resource.Id.btExit);
            lvJobDetail = FindViewById<ListView>(Resource.Id.lvJobDetail);
            lvPODetail = FindViewById<ListView>(Resource.Id.lvPODetail);
            lvPOSize = FindViewById<ListView>(Resource.Id.lvPOwSize);

            //resizing screen
            var PuvwGroup = (ViewGroup)FindViewById<RelativeLayout>(Resource.Id.relLOutputChk);
            CSDL.ScreenStretching(1, PuvwGroup);

            btFindJob.Text = CSDL.Language("M00194");
            btFindPONo.Text = CSDL.Language("M00021");
            lvPODetail.ItemClick += (sender, e) =>
            {
                string mPOName = lvAdapter_OutputChk_PO.POList[e.Position].POName;
                lvPOSize.Adapter = new lvAdapter_OutputChk_POnSz(this, dsPkDtl.Tables[2], mPOName);
            };

            btFindJob.Click += delegate
            {
                dsPkDtl = CSDL.Cnnt.Proc("InlineQcLineOutputChk", new List<string> { "@JobNo=" + edJobNo.Text.Trim().ToUpper(), "@Fact=" + CSDL.SelectedLine.Substring(0, 2) }); //+ CSDL.SelectedLine.Substring(0, 2) });

                lvJobDetail.Adapter = new lvAdapter_OutputChk_Job(this, dsPkDtl.Tables[0]);
                lvPODetail.Adapter = new lvAdapter_OutputChk_PO(this, dsPkDtl.Tables[1],"");

                Toast.MakeText(this, "Find Job : finish !" + dsPkDtl.Tables[0].Rows.Count, ToastLength.Short).Show();
            };

            btFindPONo.Click += delegate
            {
                lvPODetail.Adapter = new lvAdapter_OutputChk_PO(this, dsPkDtl.Tables[1], edPONo.Text.Trim());
            };
            btExit.Click += delegate
            {
                Finish();
            };
        }
    }
}