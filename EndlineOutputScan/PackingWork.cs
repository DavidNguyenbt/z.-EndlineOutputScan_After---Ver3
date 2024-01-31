using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using A1ATeam;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using ZXing.Mobile;
using System.IO;

namespace EndlineOutputScan
{
    [Activity(Label = "PackingWork", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class PackingWork : Activity
    {
        AlertDialog alert;
        MediaPlayer MyBeep;
        EditText edUPC, edOutputByPO, edSumOutputByPO;
        TextView tvUPC, tvTotalPOUPC, tvOutputUPC, tvSelectedDate, tvLineNOpertr, tvPOUPC, tvUPCCount, tvDate, txtversion;
        ListView lvPOListUPC;
        Button btDateSelect, btFindUPC, btManualUpdate, btStart, btRestore, btSave, btChkPOCpnss;
        CheckBox chkbCamera, chkbUpOption, cbAutoSelectPO;
        string myMessage = "", strCaptureResult = "", _error = "", bkupMsg = "", mDataDate = "";
        int POPosInDataR = 0, rowerror = 0, MaxCol = 0, QtyLimit = 0, QtyAccm = 0, QtyTdAnL = 0;
        Timer tmUPC = new Timer();
        Timer tmUpdate = new Timer();

        List<string> strIdentityRtrv = new List<string>();
        List<string> strPosOfIden = new List<string>();
        int intUPC = 0;
        bool PvsP = true;


        DataTable dtPackingDetail = new DataTable();
        DataTable dtOutputRetrieved = new DataTable();
        DataTable dtPackingDetail_backup = new DataTable();
        MobileBarcodeScanner scanner;
        View overlay;
        Button flash;
        TextView t;
        bool blFN = true, edit = false;
        DataRow drTemp = null; //<---- remove here to back option 1

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (CSDL.LogInCheck)
            {
                base.OnCreate(savedInstanceState);
                //global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                SetContentView(Resource.Layout.PackingGeneral);

                MobileBarcodeScanner.Initialize(Application);
                scanner = new MobileBarcodeScanner();

                MyBeep = MediaPlayer.Create(this, Resource.Raw.alert);

                edUPC = FindViewById<EditText>(Resource.Id.edUPC);
                edOutputByPO = FindViewById<EditText>(Resource.Id.edOutputByPO);
                edSumOutputByPO = FindViewById<EditText>(Resource.Id.edSumOutputByPO);
                tvTotalPOUPC = FindViewById<TextView>(Resource.Id.tvTotalPOUPC);
                tvOutputUPC = FindViewById<TextView>(Resource.Id.tvOutputUPC);
                tvSelectedDate = FindViewById<TextView>(Resource.Id.tvSelectedDate);
                tvLineNOpertr = FindViewById<TextView>(Resource.Id.tvLineNOpertr);
                tvPOUPC = FindViewById<TextView>(Resource.Id.tvPOUPC);
                tvUPCCount = FindViewById<TextView>(Resource.Id.tvUPCCount);
                tvUPC = FindViewById<TextView>(Resource.Id.tvUPC);
                tvDate = FindViewById<TextView>(Resource.Id.tvDate);
                txtversion = FindViewById<TextView>(Resource.Id.version); txtversion.Text = CSDL.version;
                lvPOListUPC = FindViewById<ListView>(Resource.Id.lvPOListUPC);
                btDateSelect = FindViewById<Button>(Resource.Id.btDateSelect);
                btFindUPC = FindViewById<Button>(Resource.Id.btFindUPC);
                btManualUpdate = FindViewById<Button>(Resource.Id.btManualUpdate);
                btStart = FindViewById<Button>(Resource.Id.btStart);
                btRestore = FindViewById<Button>(Resource.Id.btRestore);
                btSave = FindViewById<Button>(Resource.Id.btSave);
                chkbCamera = FindViewById<CheckBox>(Resource.Id.checkbox);
                chkbUpOption = FindViewById<CheckBox>(Resource.Id.cbUpOptn);
                cbAutoSelectPO = FindViewById<CheckBox>(Resource.Id.cbAutoSelectPO);
                btChkPOCpnss = FindViewById<Button>(Resource.Id.btChkPOCpnss);

                #region(change object language)
                tvUPC.Text = CSDL.Language("M00181");
                btFindUPC.Text = CSDL.Language("M00025");
                tvTotalPOUPC.Text = "PO: -------------------";
                btStart.Text = CSDL.Language("M00182");
                cbAutoSelectPO.Text = CSDL.Language("M00183");
                tvOutputUPC.Text = CSDL.Language("M00184");
                tvDate.Text = CSDL.Language("M00026");
                btSave.Text = CSDL.Language("M00074");
                btRestore.Text = CSDL.Language("M00185");
                btManualUpdate.Text = CSDL.Language("M00186");
                chkbCamera.Text = CSDL.Language("M00187");
                chkbUpOption.Text = CSDL.Language("M00188");
                btChkPOCpnss.Text = CSDL.Language("M00193");
                edOutputByPO.Text = "";
                edSumOutputByPO.Text = "";
                #endregion


                #region(add column header for Output tables)
                dtPackingDetail.Columns.Add("PoNo", typeof(string));//0
                dtPackingDetail.Columns.Add("TF01", typeof(int));//1
                dtPackingDetail.Columns.Add("TF02", typeof(int));//2
                dtPackingDetail.Columns.Add("TF03", typeof(int));//3
                dtPackingDetail.Columns.Add("TF04", typeof(int));//4
                dtPackingDetail.Columns.Add("TF05", typeof(int));//5
                dtPackingDetail.Columns.Add("TF06", typeof(int));
                dtPackingDetail.Columns.Add("TF07", typeof(int));
                dtPackingDetail.Columns.Add("TF08", typeof(int));
                dtPackingDetail.Columns.Add("TF09", typeof(int));
                dtPackingDetail.Columns.Add("TF10", typeof(int));
                dtPackingDetail.Columns.Add("TF11", typeof(int));
                dtPackingDetail.Columns.Add("TF12", typeof(int));
                dtPackingDetail.Columns.Add("TF13", typeof(int));
                dtPackingDetail.Columns.Add("TF14", typeof(int));
                dtPackingDetail.Columns.Add("TF15", typeof(int));
                dtPackingDetail.Columns.Add("TF16", typeof(int));
                dtPackingDetail.Columns.Add("TF17", typeof(int));
                dtPackingDetail.Columns.Add("TF18", typeof(int));
                dtPackingDetail.Columns.Add("TF19", typeof(int));
                dtPackingDetail.Columns.Add("TF20", typeof(int));
                dtPackingDetail.Columns.Add("TF21", typeof(int));
                dtPackingDetail.Columns.Add("TF22", typeof(int));
                dtPackingDetail.Columns.Add("TF23", typeof(int));
                dtPackingDetail.Columns.Add("TF24", typeof(int));
                dtPackingDetail.Columns.Add("TotalPacked", typeof(int));//25
                dtPackingDetail.Columns.Add("JobNo", typeof(string));//26
                dtPackingDetail.Columns.Add("JobDetId", typeof(int));
                dtPackingDetail.Columns.Add("ColorId", typeof(int));
                dtPackingDetail.Columns.Add("SizeId", typeof(int));
                dtPackingDetail.Columns.Add("Identity", typeof(string)); //30 Identity = PoNo + JobDetId + JobNo + ColorId + SizeId
                dtPackingDetail.Columns.Add("Sizx", typeof(string)); //31
                dtPackingDetail.Columns.Add("QtyLimit", typeof(string)); //32

                //dtOutputRetrieved.Columns.Add("PoNo", typeof(string));
                //dtOutputRetrieved.Columns.Add("TF01", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF02", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF03", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF04", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF05", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF06", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF07", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF08", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF09", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF10", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF11", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF12", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF13", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF14", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF15", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF16", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF17", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF18", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF19", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF20", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF21", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF22", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF23", typeof(int));
                //dtOutputRetrieved.Columns.Add("TF24", typeof(int));
                //dtOutputRetrieved.Columns.Add("TotalPacked", typeof(int));
                //dtOutputRetrieved.Columns.Add("JobNo", typeof(string));
                //dtOutputRetrieved.Columns.Add("JobDetId", typeof(int));
                //dtOutputRetrieved.Columns.Add("ColorId", typeof(int));
                //dtOutputRetrieved.Columns.Add("SizeId", typeof(int));
                //dtOutputRetrieved.Columns.Add("Identity", typeof(string));
                //dtOutputRetrieved.Columns.Add("Sizx", typeof(string));
                //dtOutputRetrieved.Columns.Add("QtyLimit", typeof(string)); //21

                dtOutputRetrieved = dtPackingDetail.Clone();
                dtPackingDetail_backup = dtPackingDetail.Clone();
                #endregion



                tmUPC.Enabled = true;
                tmUPC.Interval = 5;
                tmUPC.Elapsed += TmUPC_Elapsed;

                tmUpdate.Enabled = true;
                tmUpdate.Interval = CSDL.updateTime * 60000;
                tmUpdate.Stop();
                tmUpdate.Elapsed += TmUpdate_Elapsed;

                tvPOUPC.Text = "Require UPC";
                tvUPCCount.Text = "";

                CSDL.currDatetime = DateTime.Now;
                tvSelectedDate.Text = CSDL.currDatetime.ToString("MMM dd, yyyy");
                mDataDate = CSDL.currDatetime.ToString("yyyyMMdd");

                tvLineNOpertr.Text = CSDL.SelectedLine + " : " + CSDL.username;
                tvSelectedDate.Click += delegate
                {
                    string strOutputTable = "";
                    foreach (DataRow outr in dtPackingDetail.Rows)
                    {
                        foreach (DataColumn outc in dtPackingDetail.Columns)
                        {
                            strOutputTable += outr[outc].ToString() + "=";
                        }
                        strOutputTable += "\n";
                    }

                    SaveTXT(strOutputTable);
                };
                tvSelectedDate.LongClick += delegate
                  {
                      try
                      {
                          Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;

                          var filesList = Directory.GetFiles(System.IO.Path.Combine(GetExternalFilesDir(null).AbsolutePath, PackageName)).OrderByDescending(f => File.GetCreationTime(f)).ToArray();//Directory.GetFiles(System.IO.Path.Combine(sdCard.AbsolutePath, PackageName)).Reverse().ToArray();

                          Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                          Android.App.AlertDialog.Builder bb = new AlertDialog.Builder(this);
                          string[] ls = filesList.Select(s => System.IO.Path.GetFileName(s)).ToArray();

                          Dialog d = new Dialog(this);
                          b.SetSingleChoiceItems(ls, -1, (s, a) =>
                            {
                                //Toast.MakeText(this, filesList[a.Which], ToastLength.Long).Show();
                                NextRun(filesList[a.Which]);
                                d.Dismiss();
                            });

                          d = b.Create();
                          d.Show();

                          void NextRun(string f)
                          {
                              Java.IO.File file = new Java.IO.File(f);

                              System.IO.StreamReader read = new StreamReader(f);
                              string ch = read.ReadToEnd();
                              Toast.MakeText(this, ch, ToastLength.Long).Show();

                              if (ch != "")
                              {
                                  DataTable dt = dtPackingDetail.Clone();
                                  string[] OutRow = ch.Contains("\n") ? ch.Split("\n") : ch.Split("|");
                                  int i = 0;
                                  int z = 0;
                                  for (i = 0; i < OutRow.Count(); i++)
                                  {
                                      if (OutRow[i].Length > 20)
                                      {
                                          DataRow ResOutputRow = dt.NewRow();

                                          List<string> OutCell = OutRow[i].Split('=').ToList();
                                          if (OutCell.Count() < 32)
                                          {
                                              for (int j = 0; j < 10; j++) OutCell.Insert(15, "");
                                          }
                                          for (z = 0; z < OutCell.Count - 1; z++)
                                          {
                                              string vl = OutCell[z].ToString();
                                              if (z == 0 || z == 26 || z == 30 || z == 31 || z == 32)
                                              {
                                                  ResOutputRow[z] = vl == "" ? "" : OutCell[z];
                                              }
                                              else
                                              {
                                                  ResOutputRow[z] = vl.Trim().Length == 0 ? 0 : int.Parse(vl);
                                              }
                                          }
                                          dt.Rows.Add(ResOutputRow);
                                          dt.AcceptChanges();
                                      }
                                  }

                                  int tt = 0;
                                  if (dt.Rows.Count > 0) tt = int.Parse(dt.Compute("SUM(TotalPacked)", "").ToString());

                                  LinearLayout ln = new LinearLayout(this) { Orientation = Android.Widget.Orientation.Vertical };
                                  TextView txt = new TextView(this) { Text = "Total Output (" + System.IO.Path.GetFileNameWithoutExtension(f) + ") : " + tt + (f.Contains("Backup") ? " (Backup File)" : "") }; ln.AddView(txt);
                                  HorizontalScrollView hri = new HorizontalScrollView(this); ln.AddView(hri);
                                  ListView lst = new ListView(this); hri.AddView(lst);
                                  lst.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { 200, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 }, true);

                                  bb.SetPositiveButton("APPLY CHANGE", (s, a) =>
                                  {
                                      dtPackingDetail.Rows.Clear();
                                      dtPackingDetail.Merge(dt);

                                      edit = true;

                                      RefreshOutputShow(CSDL.TimeArray.Count);
                                  });
                                  bb.SetNegativeButton("QUIT", (s, a) => { });

                                  bb.SetView(ln);
                                  bb.SetCancelable(false);
                                  bb.Create().Show();
                              }
                          }
                      }
                      catch { }
                  };

                try
                {
                    //resizing layout
                    var lvVwGroup = (ViewGroup)FindViewById<RelativeLayout>(Resource.Id.relLPackGen);
                    CSDL.ScreenStretching(1, lvVwGroup);

                    ISharedPreferences pre = GetSharedPreferences("server", FileCreationMode.Private);
                    string ch = pre.GetString("timefr", "").ToString();

                    //if (CSDL.strTimeTitle == "")
                    //{
                    //    if (ch != "")
                    //    {
                    //        CSDL.strTimeTitle = ch;
                    //        string[] timfr = ch.Split(',');
                    //        for (int i = 0; i < 14; i++) //14
                    //        {
                    //            if (timfr[i] != "") CSDL.TimeArray[i] = int.Parse(timfr[i]);
                    //        }
                    //    };
                    //}

                    ch = pre.GetString("intTmStop", "").ToString();
                    if (ch != "") CSDL.intTmStop = int.Parse(ch);

                    pre = GetSharedPreferences("CrosChk", FileCreationMode.Private);
                    ch = pre.GetString("QCChk", "").ToString();
                    if (ch != "") CSDL.CompIns = bool.Parse(ch);
                    else CSDL.CompIns = false;

                    if (CSDL.CompIns) tvDate.SetTextColor(Color.Black);
                    else tvDate.SetTextColor(Color.Red);

                    CSDL.drSelectedUPC = null;

                    //if (CSDL.dtUPCDetail.Rows.Count > 0) CSDL.dtUPCDetail.Clear();
                    //string c = "exec GetLoadData 5,'',''";
                    //CSDL.dtUPCDetail = CSDL.Cnnt.Doc(c).Tables[0];
                    //tvUPCCount.Text = CSDL.dtUPCDetail.Rows.Count.ToString();

                    int MyCol = 0;// get current timeframe HERE
                    int CurrentTime = int.Parse(DateTime.Now.ToString("HHmm"));
                    if (CurrentTime < CSDL.TimeArray[0]) CurrentTime = CurrentTime + 2400;

                    for (int itime = 0; itime < CSDL.TimeArray.Count; itime++)
                    {
                        int time = CSDL.TimeArray[itime] < CSDL.TimeArray[0] ? CSDL.TimeArray[itime] + 2400 : CSDL.TimeArray[itime];
                        if (CurrentTime >= time) MyCol = itime + 1;
                    }

                    //Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    //b.SetMessage(string.Join("-", CSDL.TimeArray));
                    //b.Create().Show();

                    if (CSDL.LogInCheck)
                    {
                        InitializeData(mDataDate, MyCol);
                    }

                    if (CSDL.CompIns) CheckInspectedVsOutput();

                    tmUpdate.Start();

                }
                catch
                {
                    Toast.MakeText(this, "ERROR: Có lỗi khi khởi tạo thông tin !!!", ToastLength.Short).Show();
                }

                cbAutoSelectPO.Checked = false;
                chkbUpOption.Checked = true; //only update last 02 hours
                chkbCamera.Checked = false;
                edUPC.Text = "";

                tvPOUPC.Click += delegate
                {
                    try
                    {
                        AlertDialog.Builder bd = new AlertDialog.Builder(this);
                        HorizontalScrollView hr = new HorizontalScrollView(this) { LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1 + CSDL.height / 2) };
                        ListView lv = new ListView(this) { LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, CSDL.height / 2) };
                        lv.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dtPackingDetail, new List<int> { 80 }, true, true, true);
                        hr.AddView(lv);
                        bd.SetView(hr);
                        bd.Create().Show();
                    }
                    catch (Exception ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
                };

                btChkPOCpnss.Click += (sender, e) =>
                {
                    var intent = new Intent(this, typeof(OutputChecking));
                    StartActivity(intent);
                };
                btChkPOCpnss.LongClick += delegate
                  {
                      try
                      {
                          Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                          Android.App.AlertDialog.Builder bb = new AlertDialog.Builder(this);

                          LinearLayout ln = new LinearLayout(this) { Orientation = Android.Widget.Orientation.Vertical };

                          TextView txt = new TextView(this) { Text = "PO" }; ln.AddView(txt);
                          EditText ed = new EditText(this) { Hint = "Input PO", LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent) }; ln.AddView(ed);

                          b.SetPositiveButton("ALL LINE", (s, a) =>
                          {
                              if (ed.Text != "") Run("4", ed.Text);
                          });
                          b.SetNegativeButton("LINE", (s, a) =>
                          {
                              if (ed.Text != "") Run("3", ed.Text);
                          });
                          b.SetNeutralButton("QUIT", (s, a) => { });

                          b.SetView(ln);
                          b.SetCancelable(false);
                          b.Create().Show();

                          void Run(string i, string po)
                          {
                              DataTable dt = CSDL.Cnnt.Doc("exec EndlineScanOutputCheckUPCData " + i + ",'','" + po + "','" + CSDL.SelectedLine + "'").Tables[0];

                              HorizontalScrollView v = new HorizontalScrollView(this);
                              ListView lst = new ListView(this); v.AddView(lst);

                              lst.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { 100, 200, 200, 100, 100, 150, 150, 150, 100, 100, 100, 100 }, true);

                              bb.SetView(v);
                              bb.Create().Show();
                          }
                      }
                      catch { }
                  };

                edUPC.LongClick += async delegate
                {
                    string x = await Clipboard.GetTextAsync();
                    edUPC.Text = x;
                };

                edUPC.TextChanged += (sender, e) =>
                {
                    if (edUPC.ToString().Trim() != "")
                    {
                        //if (CSDL.dtUPCDetail.Rows.Count > 0)
                        {
                            intUPC = 0;
                            tmUPC.Start();
                        }
                        //else
                        //{
                        //    Toast.MakeText(this, "UPC " + CSDL.Language("M00153"), ToastLength.Short).Show();
                        //}
                    }
                };

                tvPOUPC.TextChanged += delegate
                {
                    if (tvPOUPC.Text.Trim() != "" && edUPC.Text != "") CountingOutputByUPC();
                };

                lvPOListUPC.ItemClick += LvPOListUPC_ItemClick;

                tvPOUPC.LongClick += delegate
                {
                    tvPOUPC.Text = "";
                    CSDL.UPCString = "";
                    CSDL.SelectedUPCPO = "";
                    CSDL.SelectedUPCPOQty = 0;
                };
                btFindUPC.Click += BtFindUPC_Click;
                btFindUPC.LongClick += delegate
                {
                    try
                    {
                        Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                        HorizontalScrollView mn = new HorizontalScrollView(this);
                        LinearLayout ln = new LinearLayout(this);
                        ListView ls = new ListView(this);

                        ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(CSDL.dtUPCDetail, new List<int> { 200 }, true);

                        ln.AddView(ls);
                        mn.AddView(ln);
                        b.SetView(mn);

                        b.Create().Show();
                    }
                    catch { }
                };
                btManualUpdate.Click += delegate
                {
                    BtManualUpdate_Click();
                    Toast.MakeText(this, myMessage, ToastLength.Short).Show();
                };
                btManualUpdate.LongClick += delegate
                {
                    UpDate();
                    if (blFN) Toast.MakeText(this, MyCol.ToString() + "..." + CSDL.Language("M00025") + "..." + dtPackingDetail.Rows.Count.ToString() + "=r | c=" + MaxCol.ToString(), ToastLength.Long).Show();
                    else Toast.MakeText(this, _error, ToastLength.Long).Show();
                };
                btRestore.Click += BtRestore_Click;
                btRestore.LongClick += delegate
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    builder.SetTitle(CSDL.Language("M00046"));
                    builder.SetMessage(CSDL.Language("M00166"));
                    builder.SetNegativeButton(CSDL.Language("M00081"), (sender, agv) =>
                    {
                        try
                        {
                            string ch = "delete from Tx_SP_FoldCheck where FDDate='" + CSDL.currDatetime.ToString("yyyyMMdd") + "' and Type='1' and FacLine = '" + CSDL.SelectedLine + "'";
                            //Toast.MakeText(this, ch, ToastLength.Long).Show();
                            CSDL.Cnnt.Ghi(ch);
                            Toast.MakeText(this, CSDL.Language("M00081") + " : " + CSDL.Language("M00082"), ToastLength.Short).Show();
                        }
                        catch
                        {
                            Toast.MakeText(this, CSDL.Language("M00081") + " : " + CSDL.Language("M00017"), ToastLength.Short).Show();
                        }
                        alert.Cancel();
                    });
                    builder.SetPositiveButton(CSDL.Language("M00050"), (sender, agv) =>
                   {
                       alert.Cancel();
                   });

                    alert = builder.Create();
                    alert.Show();
                };
                btSave.LongClick += delegate
                {
                    try
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(this);
                        builder.SetTitle(CSDL.Language("M00046"));
                        builder.SetMessage(CSDL.Language("M00167"));
                        builder.SetNegativeButton(CSDL.Language("M00067"), (sender, agv) =>
                        {
                            BtSave_LongClick();
                            if (bkupMsg != "") Toast.MakeText(this, bkupMsg, ToastLength.Short).Show();
                            alert.Cancel();
                        });
                        builder.SetPositiveButton(CSDL.Language("M00050"), (sender, agv) =>
                        {
                            alert.Cancel();
                        });

                        alert = builder.Create();
                        alert.Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, CSDL.Language("M00046") + " : " + ex.Message, ToastLength.Short).Show();
                    }
                };
                btSave.Click += delegate
                {
                    InitializeData(CSDL.currDatetime.ToString("yyyyMMdd"), 14);

                    if (int.Parse(CSDL.currDatetime.ToString("yyyyMMdd")) < int.Parse(DateTime.Now.ToString("yyyyMMdd"))) edUPC.Visibility = ViewStates.Invisible;
                    else edUPC.Visibility = ViewStates.Visible;

                    int Mcount = 0;
                    foreach (DataRow dr in dtPackingDetail.Rows)
                    {
                        for (int i = 1; i <= 14; i++)
                        {
                            if ((string.IsNullOrEmpty(dr[i].ToString()) ? 0 : int.Parse(dr[i].ToString())) > 0) Mcount++;
                        }
                    }
                    Toast.MakeText(this, "[" + CSDL.currDatetime.ToString("yyyyMMdd") + "] dtPackingDetail contains: " + Mcount.ToString() + " data...", ToastLength.Short).Show();
                };

                btDateSelect.Click += delegate
                {
                    DateSelect_OnClick();
                };

                tvDate.LongClick += delegate
                {
                    ISharedPreferences pre = GetSharedPreferences("CrosChk", FileCreationMode.Private);
                    ISharedPreferencesEditor editor = pre.Edit();

                    if (tvDate.TextColors.DefaultColor == Color.Black)
                    {
                        tvDate.SetTextColor(Color.Red);
                        CSDL.CompIns = false;
                        editor.Clear();
                        editor.PutString("QCChk", CSDL.CompIns.ToString());
                        editor.Apply();
                        Toast.MakeText(this, "Output is INDEPENDENTLY collected", ToastLength.Short).Show();
                    }
                    else
                    {
                        tvDate.SetTextColor(Color.Black);
                        CSDL.CompIns = true;
                        editor.Clear();
                        editor.PutString("QCChk", CSDL.CompIns.ToString());
                        editor.Apply();
                        Toast.MakeText(this, "You are about to CROSSCHECK with QC Inspection", ToastLength.Short).Show();
                    }

                };
                btStart.Click += delegate { BtStart_Click(); };
                tvUPC.Click += delegate
                {
                    edUPC.Text = "";
                    tmUPC.Stop();
                    intUPC = 0;
                };

                edOutputByPO.LongClick += delegate
                {
                    if (dtPackingDetail.Rows.Count > 0)
                    {
                        try
                        {
                            bool c = true;
                            foreach (DataRow r in dtPackingDetail.Rows)
                            {
                                if (string.IsNullOrEmpty(r["JobNo"].ToString()))
                                {
                                    c = false;
                                    rowerror = dtPackingDetail.Rows.IndexOf(r);
                                    Toast.MakeText(this, "dtPackingDetail: error row at " + rowerror.ToString(), ToastLength.Long).Show();
                                    break;
                                }
                            }
                            if (c) Toast.MakeText(this, "dtPackingDetail: no error row", ToastLength.Long).Show();
                        }
                        catch (Exception ex) { Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show(); }
                    }
                };
                edSumOutputByPO.Click += delegate
                {
                    edSumOutputByPO.Text = "";
                    List<string> lsPO = new List<string>();
                    foreach (DataRow dr in dtPackingDetail.Rows) if (!lsPO.Contains(dr["PoNo"].ToString())) lsPO.Add(dr["PoNo"].ToString());

                    int sumTotal = 0;
                    string d = "";
                    for (int ii = 0; ii < lsPO.Count(); ii++) // check every PO
                    {
                        d += "   " + lsPO[ii].ToString() + ": " + dtPackingDetail.Compute("Sum(TotalPacked)", "PoNo='" + lsPO[ii].ToString() + "'").ToString() + "\n";
                        sumTotal += int.Parse(dtPackingDetail.Compute("Sum(TotalPacked)", "PoNo='" + lsPO[ii].ToString() + "'").ToString());
                    }
                    edSumOutputByPO.Text = CSDL.Language("M00168") + ": < " + sumTotal.ToString() + " >" + "\n" + d;
                };

                void DateSelect_OnClick()
                {
                    DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        CSDL.currDatetime = time;
                        tvSelectedDate.Text = CSDL.currDatetime.ToString("MMM dd, yyyy");
                        mDataDate = CSDL.currDatetime.ToString("yyyyMMdd");

                        if (DateTime.Now.ToString("yyyyMMdd") == mDataDate) edUPC.Enabled = true;
                        else edUPC.Enabled = false;

                        //Toast.MakeText(this, DateTime.Now.ToString("yyyyMMdd") + "|" + mDataDate + "|" + edUPC.Focusable, ToastLength.Long).Show();

                        InitializeData(mDataDate, 14);
                    });
                    frag.Show(FragmentManager, DatePickerFragment.TAG);
                }

                //try
                //{
                //    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                //    HorizontalScrollView mn = new HorizontalScrollView(this);
                //    LinearLayout ln = new LinearLayout(this);
                //    ListView ls = new ListView(this);

                //    ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(CSDL.Cnnt.Doc("exec EndlineScanOutputGetUPCData '4064055073156','F3A01'").Tables[0], new List<int> { 200 }, true);

                //    ln.AddView(ls);
                //    mn.AddView(ln);
                //    b.SetView(mn);

                //    b.Create().Show();
                //}
                //catch { }

                if (dtPackingDetail_backup.Rows.Count == 0)
                {
                    Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
                    Java.IO.File dir = new Java.IO.File(System.IO.Path.Combine(sdCard.AbsolutePath, PackageName));
                    Java.IO.File file = new Java.IO.File(dir, CSDL.SelectedLine + " " + DateTime.Now.ToString("dd-MM-yy") + "-Backup.txt");

                    if (file.Exists())
                    {
                        System.IO.StreamReader read = new StreamReader(file.Path);
                        string ch = read.ReadToEnd();

                        if (ch != "")
                        {
                            string[] OutRow = ch.Contains("\n") ? ch.Split("\n") : ch.Split("|");
                            int i = 0;
                            int z = 0;
                            for (i = 0; i < OutRow.Count(); i++)
                            {
                                if (OutRow[i].Length > 20)
                                {
                                    DataRow ResOutputRow = dtPackingDetail_backup.NewRow();

                                    List<string> OutCell = OutRow[i].Split('=').ToList();
                                    if (OutCell.Count() < 32)
                                    {
                                        for (int j = 0; j < 10; j++) OutCell.Insert(15, "");
                                    }
                                    for (z = 0; z < OutCell.Count - 1; z++)
                                    {
                                        string vl = OutCell[z].ToString();
                                        if (z == 0 || z == 26 || z == 30 || z == 31 || z == 32)
                                        {
                                            ResOutputRow[z] = vl == "" ? "" : OutCell[z];
                                        }
                                        else
                                        {
                                            ResOutputRow[z] = vl.Trim().Length == 0 ? 0 : int.Parse(vl);
                                        }
                                    }
                                    dtPackingDetail_backup.Rows.Add(ResOutputRow);
                                    dtPackingDetail_backup.AcceptChanges();
                                }
                            }
                        }
                    }
                }
            }

        }

        private void InitializeData(string mDate, int myCol)
        {
            RetrieveOuputFromSQL(mDate);

            dtPackingDetail.Rows.Clear();

            dtPackingDetail.Merge(dtOutputRetrieved);

            //dtOutputRetrieved.Clear();

            CSDL.LogInCheck = false; // to run only 1 times

            Toast.MakeText(this, "... " + CSDL.Language("M00033") + " " + dtPackingDetail.Rows.Count.ToString() + " " + CSDL.Language("M00034") + " ...", ToastLength.Short).Show();

            RefreshOutputShow(myCol);

            //Android.App.AlertDialog.Builder a = new AlertDialog.Builder(this);
            //HorizontalScrollView ln = new HorizontalScrollView(this);
            //ListView ls = new ListView(this);
            //ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dtPackingDetail, new List<int> { 100 }, true);

            //ln.AddView(ls);
            //a.SetView(ln);

            //a.Create().Show();
        }

        int MyCol = 0;
        private void UpDate()
        {
            int MyCount = 0;
            MaxCol = 0;
            try
            {
                RetrieveOuputFromSQL(mDataDate);
                blFN = true;
                // get current timeframe HERE
                int MyCol = 0;
                int CurrentTime = int.Parse(DateTime.Now.ToString("HHmm"));
                if (CurrentTime < CSDL.TimeArray[0]) CurrentTime = CurrentTime + 2400;

                for (int itime = 0; itime < CSDL.TimeArray.Count; itime++)
                {
                    int time = CSDL.TimeArray[itime] < CSDL.TimeArray[0] ? CSDL.TimeArray[itime] + 2400 : CSDL.TimeArray[itime];
                    if (CurrentTime >= time) MyCol = itime + 1;
                }
                //Toast.MakeText(this, MyCol.ToString(), ToastLength.Long).Show();
                if (MyCol < (CSDL.TimeArray.Count - 1))
                {
                    if (edit) MyCol = CSDL.TimeArray.Count - 2;

                    foreach (DataRow r in dtPackingDetail.Rows)
                    {
                        for (int i = 1; i <= MyCol; i++) //((MyCol - 1) <= 0 ? MyCol : (MyCol - 1))
                        {
                            int InsrOrUpdat = 4;
                            if (strIdentityRtrv.Contains(r["Identity"].ToString() + i.ToString())) // NEED TO INDICATE TIMEFRAME
                            {
                                //Toast.MakeText(this, "identity= "+ r["Identity"].ToString() + "      Colum= " + MyCol.ToString(), ToastLength.Long).Show();
                                int rRetrvIndex = strPosOfIden.IndexOf(r["Identity"].ToString()); // get the position of Identity in table NOTE THAT: identity is unique in dtOutputRetrieved - based on MYCOL
                                if (r[i].ToString() != "")
                                {
                                    int qtyDetail = r[i].ToString() == "" ? 0 : int.Parse(r[i].ToString());
                                    int qtyRetrieve = dtOutputRetrieved.Rows[rRetrvIndex][i].ToString() == "" ? 0 : int.Parse(dtOutputRetrieved.Rows[rRetrvIndex][i].ToString());

                                    //Toast.MakeText(this, "update TU column = " + i.ToString() + " DEN column =" + MyCol.ToString(), ToastLength.Long).Show();
                                    //Toast.MakeText(this, "qtyDetail = " + qtyDetail.ToString() + " qtyRetrieve =" + qtyRetrieve.ToString(), ToastLength.Long).Show();

                                    if (qtyDetail != qtyRetrieve) InsrOrUpdat = 2;
                                }
                            }
                            else
                            {
                                if ((string.IsNullOrEmpty(r[i].ToString()) ? 0 : int.Parse(r[i].ToString())) > 0) InsrOrUpdat = 1;
                            }
                            if (InsrOrUpdat != 4)
                            {
                                UpdateNEWESToutput(InsrOrUpdat, r, i);
                                MyCount++;
                                Task.Delay(250);//=======================================DELAY================================
                            }
                            if (i > MaxCol) MaxCol = i;
                        }
                    }
                    if (chkbCamera.Checked) BtStart_Click();
                }
                else tmUpdate.Stop();

                edit = false;

                Finish();
            }
            catch
            {
                blFN = false;
                myMessage = "TotalREFRESH: ..." + CSDL.Language("M00053") + "...";
            }
            if (blFN) myMessage = "..." + CSDL.Language("M00051") + ": " + MyCount.ToString() + CSDL.Language("M00034") + "...";

            string strOutputTable = "";
            foreach (DataRow outr in dtPackingDetail.Rows)
            {
                foreach (DataColumn outc in dtPackingDetail.Columns)
                {
                    strOutputTable += outr[outc].ToString() + "=";
                }
                strOutputTable += "\n";
            }

            SaveTXT(strOutputTable);
        }
        private void BtSave_LongClick()
        {
            string strOutputTable = "";
            bkupMsg = "";
            foreach (DataRow outr in dtPackingDetail.Rows)
            {
                foreach (DataColumn outc in dtPackingDetail.Columns)
                {
                    strOutputTable += outr[outc].ToString() + "=";
                }
                strOutputTable += "\n";
            }

            if (strOutputTable != "")
            {
                ISharedPreferences pre = GetSharedPreferences("OutputTable", FileCreationMode.Private);
                ISharedPreferencesEditor editor = pre.Edit();
                editor.Clear();
                editor.PutString("strOutputTable", strOutputTable);
                editor.Apply();
                bkupMsg = CSDL.Language("M00074") + " dtPackingDetail: " + dtPackingDetail.Rows.Count.ToString() + " " + CSDL.Language("M00034") + " (row)";

                SaveTXT(strOutputTable);
            }
            else
            {
                bkupMsg = CSDL.Language("M00054") + ": strOutputTable = " + strOutputTable;
            }

        }
        private void SaveTXT(string str)
        {
            try
            {
                //Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
                //Java.IO.File dir = new Java.IO.File(sdCard, PackageName);
                Java.IO.File dir = new Java.IO.File(this.GetExternalFilesDir(null), PackageName);
                if (!dir.Exists()) dir.Mkdirs();

                Java.IO.File file = new Java.IO.File(dir, CSDL.SelectedLine + " " + DateTime.Now.ToString("dd-MM-yy") + ".txt");

                if (!file.Exists()) file.CreateNewFile();

                Java.IO.FileWriter writer = new Java.IO.FileWriter(file);
                // Writes the content to the file
                writer.Write(str);
                writer.Flush();
                writer.Close();

                Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();
            }
            catch { }
        }
        private void SaveTXTBackup(string str)
        {
            try
            {
                //    Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
                //    Java.IO.File dir = new Java.IO.File(System.IO.Path.Combine(sdCard.AbsolutePath, PackageName));
                Java.IO.File dir = new Java.IO.File(this.GetExternalFilesDir(null), PackageName);
                if (!dir.Exists()) dir.Mkdirs();

                Java.IO.File file = new Java.IO.File(dir, CSDL.SelectedLine + " " + DateTime.Now.ToString("dd-MM-yy") + "-Backup.txt");

                if (!file.Exists()) file.CreateNewFile();

                Java.IO.FileWriter writer = new Java.IO.FileWriter(file);
                // Writes the content to the file
                writer.Write(str);
                writer.Flush();
                writer.Close();

                Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();
            }
            catch { }
        }
        private void BtRestore_Click(object sender, EventArgs e)
        {
            try
            {
                btManualUpdate.Background.SetTint(Color.ParseColor("#ff9800"));

                if (dtPackingDetail.Rows.Count > 0) dtPackingDetail.Rows.Clear();

                ISharedPreferences pre = GetSharedPreferences("OutputTable", FileCreationMode.Private);
                string ch = pre.GetString("strOutputTable", "").ToString();

                if (ch != "")
                {
                    //Toast.MakeText(this, "Chuoi =" + ch, ToastLength.Short).Show();
                    string[] OutRow = ch.Contains("\n") ? ch.Split("\n") : ch.Split("|");
                    int i = 0;
                    int z = 0;
                    for (i = 0; i < OutRow.Count() - 1; i++)
                    {
                        if (OutRow[i].Length > 20)
                        {
                            DataRow ResOutputRow = dtPackingDetail.NewRow();

                            List<string> OutCell = OutRow[i].Split('=').ToList();
                            if (OutCell.Count() < 32)
                            {
                                for (int j = 0; j < 10; j++) OutCell.Insert(15, "");
                            }
                            for (z = 0; z < OutCell.Count - 1; z++)
                            {
                                if (z == 0 || z == 26 || z == 30 || z == 31 || z == 32)
                                {
                                    ResOutputRow[z] = OutCell[z].ToString() == "" ? "" : OutCell[z];
                                }
                                else
                                {
                                    ResOutputRow[z] = OutCell[z].ToString().Trim().Length == 0 ? 0 : int.Parse(OutCell[z]);
                                }
                            }
                            dtPackingDetail.Rows.Add(ResOutputRow);
                            dtPackingDetail.AcceptChanges();
                        }
                    }
                    //Toast.MakeText(this, "dt Row after =" + dtPackingDetail.Rows.Count.ToString(), ToastLength.Short).Show();
                    //Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    //b.SetMessage(ch);
                    //b.Create().Show();
                    RefreshOutputShow(CSDL.TimeArray.Count);
                }
                else
                {
                    Toast.MakeText(this, CSDL.Language("M00043") + ": ch.lenght= " + ch.Length.ToString(), ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Restore ERROR =" + ex.Message, ToastLength.Short).Show();
            }
        }

        private async void BtStart_Click()
        {
            {
                scanner.UseCustomOverlay = true;
                overlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.CameraScan, null);
                flash = overlay.FindViewById<Button>(Resource.Id.btFlash);
                flash.Click += (mysender, z) => scanner.ToggleTorch();

                scanner.CustomOverlay = overlay;
                scanner.AutoFocus();
                var result = await scanner.Scan(new MobileBarcodeScanningOptions { UseNativeScanning = true });

                HandleScanResultLogin(result);
            };
        }

        private void HandleScanResultLogin(ZXing.Result result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                try
                {
                    strCaptureResult = result.Text;
                    edUPC.Text = strCaptureResult;
                }
                catch (Exception ex)
                {
                    t.Text = ex.ToString();
                    t.Visibility = ViewStates.Visible;
                }
            }
        }

        private void BtManualUpdate_Click()
        {
            RetrieveOuputFromSQL(mDataDate);
            blFN = true;
            int MyCol = 0;// get current timeframe HERE

            int CurrentTime = int.Parse(DateTime.Now.ToString("HHmm"));
            if (CurrentTime < CSDL.TimeArray[0]) CurrentTime = CurrentTime + 2400;

            for (int itime = 0; itime < CSDL.TimeArray.Count; itime++)
            {
                int time = CSDL.TimeArray[itime] < CSDL.TimeArray[0] ? CSDL.TimeArray[itime] + 2400 : CSDL.TimeArray[itime];
                if (CurrentTime >= time) MyCol = itime + 1;
            }

            //string ins = "";
            if (MyCol < (CSDL.TimeArray.Count - 1))
            {
                foreach (DataRow r in dtPackingDetail.Rows)
                {
                    for (int i = ((MyCol - 1) <= 0 ? MyCol : (MyCol - 1)); i <= MyCol; i++)
                    {
                        int InsrOrUpdat = 4;
                        if (strIdentityRtrv.Contains(r["Identity"].ToString() + i.ToString())) // NEED TO INDICATE TIMEFRAME
                        {
                            int rRetrvIndex = strPosOfIden.IndexOf(r["Identity"].ToString()); // get the position of Identity in table NOTE THAT: identity is unique in dtOutputRetrieved - based on MYCOL
                            if (r[i].ToString() != "")
                            {
                                int qtyDetail = r[i].ToString() == "" ? 0 : int.Parse(r[i].ToString());
                                int qtyRetrieve = dtOutputRetrieved.Rows[rRetrvIndex][i].ToString() == "" ? 0 : int.Parse(dtOutputRetrieved.Rows[rRetrvIndex][i].ToString());
                                if (qtyDetail > qtyRetrieve) InsrOrUpdat = 2;
                            }
                        }
                        else
                        {
                            if ((string.IsNullOrEmpty(r[i].ToString()) ? 0 : int.Parse(r[i].ToString())) > 0) InsrOrUpdat = 1;
                        }
                        //check row to update
                        if (InsrOrUpdat != 4)
                        {
                            UpdateNEWESToutput(InsrOrUpdat, r, i);
                            Task.Delay(250);//=======================================DELAY================================
                        }
                    }
                }
                //Toast.MakeText(this, ins, ToastLength.Long).Show();
                if (blFN) myMessage = "..." + CSDL.Language("M00051") + "...";
                else myMessage = "Refresh2HOURS:..." + CSDL.Language("M00053") + ". " + CSDL.Language("M00054") + "...";

                if (chkbCamera.Checked) BtStart_Click();
            }
            else tmUpdate.Stop();
        }
        private void UpdateNEWESToutput(int InsrOrUpdat, DataRow r, int MyCol)
        {
            try
            {
                SqlConnection con = new SqlConnection(CSDL.chuoi);
                con.Open();
                SqlCommand cmd = new SqlCommand("EndlineScanOP", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@InsrOrUpdat", InsrOrUpdat);
                cmd.Parameters.Add("@PKInsNo", r["PoNo"].ToString() + "-" + int.Parse(r["JobDetId"].ToString()).ToString("00"));
                cmd.Parameters.Add("@FDDate", CSDL.currDatetime.ToString("yyyyMMdd"));
                cmd.Parameters.Add("@JobNo", r["JobNo"].ToString());
                cmd.Parameters.Add("@JobDetId", r["JobDetId"]);
                cmd.Parameters.Add("@Type", 1);
                cmd.Parameters.Add("@ColorId", r["ColorId"]);
                cmd.Parameters.Add("@SizeId", r["SizeId"]);
                cmd.Parameters.Add("@InseamId", CSDL.SelectedLine.Substring(1, 1) == "1" ? 0 : 1);
                cmd.Parameters.Add("@ActualQty", r[MyCol]);
                cmd.Parameters.Add("@CreateUser", CSDL.user);
                cmd.Parameters.Add("@CreateDateTime", CSDL.currDatetime.ToString("yyyyMMdd") + " " + CSDL.TimeArray[MyCol - 1].ToString("0000").Substring(0, 2) + ":" + (int.Parse(CSDL.TimeArray[MyCol - 1].ToString("0000").Substring(2, 2)) + 1).ToString("00") + ":00.000");
                cmd.Parameters.Add("@OLD_RecNo", r["TotalPacked"]);
                cmd.Parameters.Add("@FDMark", MyCol.ToString());
                cmd.Parameters.Add("@HostName", r["Identity"].ToString());
                cmd.Parameters.Add("@FacLine", CSDL.SelectedLine);
                cmd.Parameters.Add("@SysCreateDate", CSDL.currDatetime);
                cmd.Parameters.Add("@ComputerNetName", r["Sizx"].ToString() + "/" + Build.Manufacturer + "/" + Build.Serial + "-" + CSDL.version + "/" + r["QtyLimit"].ToString());
                cmd.ExecuteNonQuery();
                con.Close();
                btManualUpdate.Background.SetTint(Color.ParseColor("#ff99cc00"));
            }
            catch (Exception ex)
            {
                btManualUpdate.Background.SetTint(Color.ParseColor("#ff9800"));
                _error = "UpdateNewestOutput: " + ex.ToString();
                blFN = false;
            }
        }

        private void CheckInspectedVsOutput()
        {
            try
            {
                int TTPass = 0;
                int TTPack = 1;
                foreach (DataRow dr in dtPackingDetail.Rows)
                {
                    TTPack += dr["TotalPacked"].ToString() == "" ? 0 : int.Parse(dr["TotalPacked"].ToString());
                };
                CSDL kn = new CSDL();
                DataTable dtt = CSDL.Cnnt.Doc("select sum(AccQty) as TTPass from EndlineQcReportGmtPass where  FacLine='" + CSDL.SelectedLine + "' and Date = '" + DateTime.Now.ToString("yyyyMMdd") + "'").Tables[0]; //CSDL.currDatetime.ToString("yyyyMMdd")
                if (dtt.Rows.Count > 0) { TTPass = int.Parse(string.IsNullOrEmpty(dtt.Rows[0][0].ToString()) ? "0" : dtt.Rows[0][0].ToString()); }
                if (TTPass < TTPack) PvsP = false;
                Toast.MakeText(this, "Total Pass = " + TTPass + " | " + (TTPack - 1) + " = Total Pack", ToastLength.Short).Show();
            }
            catch
            {
                Toast.MakeText(this, "ERROR: " + CSDL.Language("M00169") + " !!!", ToastLength.Short).Show();
            };
        }

        private void RetrieveOuputFromSQL(string rtrDate)
        {

            strPosOfIden.Clear();
            strIdentityRtrv.Clear();
            dtOutputRetrieved.Clear();

            string c = "select PKInsNo,FDMark,ActualQty,OLD_RecNo,JobNo,JobDetId,ColorId,SizeId,HostName,ComputerNetName from Tx_SP_FoldCheck where FDDate='" + rtrDate + "' and FacLine='" + CSDL.SelectedLine + "'and Type in ('1','3') and FDMark is not null and HostName is not null";
            DataTable dtRtrvInfo = new DataTable();
            dtRtrvInfo = CSDL.Cnnt.Doc(c, 60).Tables[0];
            //Toast.MakeText(this, "table retrive rows = "+dtRtrvInfo.Rows.Count.ToString(), ToastLength.Short).Show();

            foreach (DataRow dr in dtRtrvInfo.Rows) //convert retrived data into formatted table "dtOutputRetrieved"
            {
                if (!strPosOfIden.Contains(dr["HostName"]))
                {
                    if (dr["ActualQty"].ToString() != "")
                    {
                        DataRow r = dtOutputRetrieved.NewRow();
                        r["PoNo"] = dr["PKInsNo"].ToString().Substring(0, dr["PKInsNo"].ToString().Length - 3);
                        r[int.Parse(dr["FDMark"].ToString())] = dr["ActualQty"]; //FDMark will store "hour"
                        r["TotalPacked"] = dr["ActualQty"]; // Recalculating "TotalPacked" number
                        r["JobNo"] = dr["JobNo"];
                        r["JobDetId"] = dr["JobDetId"];
                        r["ColorId"] = dr["ColorId"];
                        r["SizeId"] = dr["SizeId"];
                        r["Identity"] = dr["HostName"];
                        r["Sizx"] = dr["ComputerNetName"].ToString().Split('/')[0];
                        r["QtyLimit"] = dr["ComputerNetName"].ToString().Split('/').Last();
                        dtOutputRetrieved.Rows.Add(r);
                        dtOutputRetrieved.AcceptChanges();
                        strPosOfIden.Add(dr["HostName"].ToString());
                    }
                }
                else
                {
                    int pos = strPosOfIden.IndexOf(dr["HostName"].ToString());
                    int ActualQty = dr["ActualQty"].ToString().Trim().Length == 0 ? 0 : int.Parse(dr["ActualQty"].ToString());
                    int AccmQty = dtOutputRetrieved.Rows[pos][int.Parse(dr["FDMark"].ToString())].ToString().Trim().Length == 0 ? 0 : int.Parse(dtOutputRetrieved.Rows[pos][int.Parse(dr["FDMark"].ToString())].ToString());
                    dtOutputRetrieved.Rows[pos][int.Parse(dr["FDMark"].ToString())] = (ActualQty + AccmQty).ToString();

                    AccmQty = dtOutputRetrieved.Rows[pos]["TotalPacked"].ToString().Trim().Length == 0 ? 0 : int.Parse(dtOutputRetrieved.Rows[pos]["TotalPacked"].ToString());
                    dtOutputRetrieved.Rows[pos]["TotalPacked"] = (ActualQty + AccmQty).ToString();
                    dtOutputRetrieved.AcceptChanges();

                    //Toast.MakeText(this, "tbOUTPUTRETRIEVE dong=" + pos.ToString() + "cot=" + dr["FDMark"].ToString() + "value=" + dtOutputRetrieved.Rows[pos][int.Parse(dr["FDMark"].ToString())].ToString(), ToastLength.Short).Show();
                }
                //Toast.MakeText(this, "cot in PackingDetail = "+dr["FDMark"].ToString() , ToastLength.Short).Show();

                if (!strIdentityRtrv.Contains(dr["HostName"].ToString() + dr["FDMark"].ToString())) strIdentityRtrv.Add(dr["HostName"].ToString() + dr["FDMark"].ToString());
            }

            //Android.App.AlertDialog.Builder a = new AlertDialog.Builder(this);
            //HorizontalScrollView ln = new HorizontalScrollView(this);
            //ListView ls = new ListView(this);
            //ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dtOutputRetrieved, new List<int> { 100 }, true);

            //ln.AddView(ls);
            //a.SetView(ln);

            //a.Create().Show();
        }

        private void TmUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (chkbUpOption.Checked) BtManualUpdate_Click(); // this is to update last 02 hours
                else UpDate(); // this is to update all data

                if (myMessage != "") Toast.MakeText(this, myMessage, ToastLength.Short).Show();
                myMessage = "";
            });
        }

        private void UPCDetail_Loading()
        {
            tvUPCCount.Text = "";
            try
            {
                //if (CSDL.dtUPCDetail.Rows.Count > 0) CSDL.dtUPCDetail.Clear();
                //string c = "exec GetLoadData 5,'',''";
                //CSDL.dtUPCDetail = CSDL.Cnnt.Doc(c).Tables[0];
                //tvUPCCount.Text = CSDL.dtUPCDetail.Rows.Count.ToString();

                if (edUPC.Text.Contains("\n")) edUPC.Text = edUPC.Text.Replace("\n", "");
                DataTable dt = CSDL.Cnnt.Doc("exec EndlineScanOutputGetUPCData '" + edUPC.Text + "','" + CSDL.SelectedLine + "'").Tables[0];

                if (dt.Rows.Count > 0)
                {
                    if (CSDL.dtUPCDetail.Rows.Count > 0) CSDL.dtUPCDetail.Rows.Clear();
                    CSDL.dtUPCDetail = dt;
                    tvUPCCount.Text = CSDL.dtUPCDetail.Rows.Count.ToString();

                    RefreshingUPC();
                }
                else Toast.MakeText(Application.Context, "UPC is invalid !!!", ToastLength.Long).Show();
            }
            catch (SqlException ex)
            {
                myMessage = "..." + CSDL.Language("M00053") + ". " + CSDL.Language("M00017") + "..." + "\n" + ex.Message;
            }
            finally
            {
                myMessage = "UPC: " + CSDL.Language("M00033") + ": " + CSDL.dtUPCDetail.Rows.Count.ToString() + " " + CSDL.Language("M00034");
            }
        }

        private void BtFindUPC_Click(object sender, EventArgs e)
        {
            //UPCDetail_Loading();
            CSDL.dtUPCDetail.Rows.Clear();
            //RefreshingUPC();
            lvPOListUPC.Adapter = null;
            edUPC.Text = ""; CSDL.UPCString = "";

            Toast.MakeText(this, "UPC: " + CSDL.Language("M00033") + ": " + CSDL.dtUPCDetail.Rows.Count.ToString() + " " + CSDL.Language("M00034"), ToastLength.Short).Show();
        }

        private void RefreshingUPC()
        {
            try
            {
                //Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                //b.SetMessage(edUPC.Text + " - " + CSDL.UPCString);
                //b.Create().Show();

                if (edUPC.Text.Contains("\n")) edUPC.Text = edUPC.Text.Replace("\n", "");

                if (CSDL.dtUPCDetail.Rows.Count == 0 || CSDL.dtUPCDetail.Select("UPC_Code='" + edUPC.Text + "'").Length == 0)
                {
                    DataTable dt = CSDL.Cnnt.Doc("exec EndlineScanOutputGetUPCData '" + edUPC.Text + "','" + CSDL.SelectedLine + "'").Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        CSDL.dtUPCDetail.Merge(dt);
                        tvUPCCount.Text = CSDL.dtUPCDetail.Rows.Count.ToString();

                        Run();
                    }
                    else Toast.MakeText(Application.Context, "UPC is invalid !!!", ToastLength.Long).Show();
                }
                else Run();

                void Run()
                {
                    if (edUPC.Text != CSDL.UPCString)
                    {
                        tvPOUPC.Text = "";
                        drTemp = null;

                        CSDL.UPCString = edUPC.Text; // test value "8988285071145"   edUPC.Text.Trim();        

                        lvPOListUPC.Adapter = new CustomListViewAdapter(this, edUPC.Text.Trim());//update new adapter--------------------------.TRIM()?

                        tvTotalPOUPC.Text = CSDL.Language("M00170") + " " + lvPOListUPC.Count.ToString() + " PO: ";

                        bool exe = false;
                        if (CSDL.drSelectedUPC.Length == 1 && cbAutoSelectPO.Checked)
                        {
                            POPosInDataR = 0;
                            drTemp = CSDL.drSelectedUPC[0];
                            exe = true;

                            CSDL.SelectedUPCPO = CustomListViewAdapter.POList[0].POName;
                            CSDL.SelectedUPCPOQty = CustomListViewAdapter.POList[0].POQty;
                            CSDL.SelectedGmtType = CustomListViewAdapter.POList[0].PODtl;
                            CSDL.SelectedColor = CustomListViewAdapter.POList[0].POSize;
                            CSDL.SelectedShipNo = CustomListViewAdapter.POList[0].POShipNo;

                            tvPOUPC.Text = CustomListViewAdapter.POList[0].POName; // this will call CountingOutput()
                        }
                        else if (lvPOListUPC.Count >= 1 && !exe)
                        {
                            // for selecting PO automatically
                            //check PO position in the listview 
                            bool MCheck = false;
                            for (int z = 0; z < CustomListViewAdapter.POList.Count; z++)
                            {
                                if (CustomListViewAdapter.POList[z].POName == CSDL.SelectedUPCPO
                                    && CustomListViewAdapter.POList[z].PODtl == CSDL.SelectedGmtType
                                    && CustomListViewAdapter.POList[z].POSize == CSDL.SelectedColor
                                    && CustomListViewAdapter.POList[z].POShipNo == CSDL.SelectedShipNo)
                                {
                                    POPosInDataR = z;
                                    break;
                                }
                            }

                            //check PO position in the array datarow - maybe its not the same sequence with listview
                            foreach (DataRow dr in CSDL.drSelectedUPC)
                            {
                                if (dr["Po_No"].ToString().Trim() == CSDL.SelectedUPCPO
                                    && dr["GarmentType"].ToString() == CSDL.SelectedGmtType
                                    && dr["Color"].ToString() == CSDL.SelectedColor
                                    && dr["JobDetId"].ToString() == CSDL.SelectedShipNo)
                                {
                                    drTemp = dr;
                                    MCheck = true;
                                    break;
                                }
                            }

                            if (MCheck)
                            {
                                tvPOUPC.Text = CSDL.SelectedUPCPO;
                                Toast.MakeText(this, "PO: " + CSDL.SelectedUPCPO + " " + CSDL.Language("M00171") + " !!", ToastLength.Short).Show();
                            }
                            else
                            {
                                Toast.MakeText(this, CSDL.Language("M00172") + " : " + edUPC.Text, ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            //POPosInDataR = 0;
                            tvPOUPC.Text = "";
                            Toast.MakeText(this, CSDL.Language("M00173") + " UPC : " + edUPC.Text, ToastLength.Long).Show();
                        }
                    }
                    else
                    {
                        if (CSDL.SelectedUPCPO != "" && edUPC.Text.Trim() != "")
                        {
                            tvPOUPC.Text = CSDL.SelectedUPCPO;
                            Toast.MakeText(this, "PO: " + tvPOUPC.Text + " " + CSDL.Language("M00171") + " !!", ToastLength.Short).Show();
                        }
                        else if (CSDL.SelectedUPCPO == "")
                        {
                            Toast.MakeText(this, CSDL.Language("M00173") + " PO. " + CSDL.Language("M00172"), ToastLength.Long).Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "RefreshingUPC: " + ex.ToString(), ToastLength.Short).Show();
            }
        }
        bool reloadqty = false;
        private void CountingOutputByUPC(bool check = true)
        {
            try
            {
                bool isFinish = true;
                PvsP = true; QtyLimit = 0; QtyAccm = 0;

                if (CSDL.CompIns) CheckInspectedVsOutput();
                //Toast.MakeText(this, "CSDL.CompIns=" + CSDL.CompIns.ToString() + "   |  PvsP=" + PvsP.ToString(), ToastLength.Short).Show();
                if (PvsP)
                {
                    btManualUpdate.Background.SetTint(Color.ParseColor("#ff9800")); // server not synchronized
                    edOutputByPO.SetBackgroundColor(Color.ParseColor("#ffffecb3"));
                    edSumOutputByPO.SetBackgroundColor(Color.ParseColor("#ffffecb3"));

                    int CurrentTime = int.Parse(DateTime.Now.ToString("HHmm"));
                    if (CurrentTime < CSDL.TimeArray[0]) CurrentTime = CurrentTime + 2400;

                    int last = CSDL.TimeArray[CSDL.TimeArray.Count - 1] < CSDL.TimeArray[0] ? CSDL.TimeArray[CSDL.TimeArray.Count - 1] + 2400 : CSDL.TimeArray[CSDL.TimeArray.Count - 1];

                    if (edUPC.Text.Trim().Length > 0 && CurrentTime >= CSDL.TimeArray[0] && CurrentTime <= last)
                    {
                        int MyCol = 0; // get current timeframe HERE
                        for (int itime = 0; itime < CSDL.TimeArray.Count; itime++)
                        {
                            int time = CSDL.TimeArray[itime] < CSDL.TimeArray[0] ? CSDL.TimeArray[itime] + 2400 : CSDL.TimeArray[itime];
                            if (CurrentTime >= time) MyCol = itime + 1;
                        }
                        bool run = true;

                        if (CSDL.drSelectedUPC.Count() > POPosInDataR & drTemp != null)
                        {
                            string stIdentity = tvPOUPC.Text + drTemp["JobDetId"] + drTemp["JobNo"] + drTemp["ColorId"] + drTemp["SizeId"];

                            QtyLimit = string.IsNullOrEmpty(drTemp["LoadingQty"].ToString()) ? 0 : int.Parse(drTemp["LoadingQty"].ToString());
                            QtyAccm = string.IsNullOrEmpty(drTemp["SumOutput"].ToString()) ? 0 : int.Parse(drTemp["SumOutput"].ToString());

                            if (QtyAccm == 0 && CSDL.CheckQtyAcum)
                            {
                                try
                                {
                                    string ss = "select isnull(sum(ActualQty),0) TotalPacked from Tx_SP_FoldCheck where FDDate between '" + CSDL.currDatetime.AddDays(-90).ToString("yyyyMMdd") + "' and '" + CSDL.currDatetime.AddDays(-1).ToString("yyyyMMdd") + "' and PKInsNo like '" + CSDL.SelectedUPCPO + "%' and Type in ('1','3') " +
                                                            "and FacLine = '" + CSDL.SelectedLine + "' and JobNo = '" + drTemp["JobNo"].ToString() + "' and SizeId = '" + drTemp["SizeId"].ToString() + "' and ColorId = '" + drTemp["ColorId"].ToString() + "' and JobDetId = '" + drTemp["JobDetId"].ToString() + "' and FDMark is not null and HostName is not null";
                                    DataTable qtyac = CSDL.Cnnt.Doc(ss).Tables[0];

                                    QtyAccm = int.Parse(qtyac.Rows[0][0].ToString()); drTemp["SumOutput"] = QtyAccm; CSDL.CheckQtyAcum = false;
                                }
                                catch { run = false; }
                            }

                            string str = "";
                            List<string> MyPOList = new List<string>();
                            foreach (DataRow dr in dtPackingDetail.Rows) if (!MyPOList.Contains(dr["Identity"].ToString())) MyPOList.Add(dr["Identity"].ToString()); //str += dr["Identity"].ToString() + " \n"; }

                            #region Get Total Qty from Server 
                            //string ch = "select FacLine,isnull(sum(ActualQty),0) TotalPacked from Tx_SP_FoldCheck where FDDate='" + CSDL.currDatetime.ToString("yyyyMMdd") + "' and PKInsNo like '" + CSDL.SelectedUPCPO + "%' and Type in ('1','3') " +
                            //                                "and JobNo = '" + drTemp["JobNo"].ToString() + "' and SizeId = '" + drTemp["SizeId"].ToString() + "' and JobDetId = '" + drTemp["JobDetId"].ToString() + "' and FDMark is not null and HostName is not null group by FacLine";
                            ////"select isnull(sum(ActualQty),0) TotalPacked from Tx_SP_FoldCheck where FDDate='" + CSDL.currDatetime.ToString("yyyyMMdd") + "' and PKInsNo like '" + CSDL.SelectedUPCPO + "%' and Type in ('1','3') " +
                            ////                   "and JobNo = '" + drTemp["JobNo"].ToString() + "' and SizeId = '" + drTemp["SizeId"].ToString() + "' and FacLine = '" + CSDL.SelectedLine + "' and FDMark is not null and HostName is not null";
                            //DataTable ds = CSDL.Cnnt.Doc(ch).Tables[0];
                            //int TodayTotalPk = ds.Rows.Count > 0 ? Convert.ToInt32(ds.Compute("SUM(TotalPacked)", string.Empty)) : 0;//int.Parse(ds.Tables[0].Rows[0][0].ToString());
                            //int TodayTotalbyLine1 = 0;

                            //DataRow[] drr = ds.Select("FacLine = '" + CSDL.SelectedLine + "'");
                            //if (drr.Length > 0)
                            //{
                            //    TodayTotalbyLine1 = int.Parse(drr[0][1].ToString());
                            //}

                            //int UPCPos, TodayTotalbyLine2;
                            //if (MyPOList.Contains(stIdentity)) { UPCPos = MyPOList.IndexOf(stIdentity); TodayTotalbyLine2 = int.Parse(dtPackingDetail.Rows[UPCPos]["TotalPacked"].ToString()); }
                            //else { UPCPos = -1; TodayTotalbyLine2 = TodayTotalbyLine1; }

                            ////Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                            ////b.SetTitle("today=" + TodayTotalPk + " limit=" + QtyLimit + " Qty=" + QtyAccm + " " + stIdentity);
                            ////b.SetMessage(ch);
                            ////b.Create().Show();

                            //if ((QtyLimit - (QtyAccm + QtyTdAnL + TodayTotalPk + (TodayTotalbyLine2 - TodayTotalbyLine1))) > 0 && run)
                            //{
                            //    if (UPCPos >= 0)
                            //    {
                            //        //int TodayTotalPk = dtPackingDetail.Rows[UPCPos]["TotalPacked"].ToString().Trim().Length == 0 ? 1 : (int)dtPackingDetail.Rows[UPCPos]["TotalPacked"] + 1;


                            //        //Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                            //        //b.SetTitle("today=" + TodayTotalPk + " limit=" + QtyLimit + " Qty=" + QtyAccm + " " + stIdentity);
                            //        //b.SetMessage(str);
                            //        //b.Create().Show();

                            //        dtPackingDetail.Rows[UPCPos][MyCol] = dtPackingDetail.Rows[UPCPos][MyCol].ToString().Trim().Length == 0 ? 1 : (int)dtPackingDetail.Rows[UPCPos][MyCol] + 1;
                            //        dtPackingDetail.Rows[UPCPos]["TotalPacked"] = dtPackingDetail.Rows[UPCPos]["TotalPacked"].ToString().Trim().Length == 0 ? 1 : (int)dtPackingDetail.Rows[UPCPos]["TotalPacked"] + 1; ;
                            //        dtPackingDetail.AcceptChanges();

                            //    }
                            //    else
                            //    {
                            //        if (string.IsNullOrEmpty(drTemp["JobNo"].ToString()))
                            //        {
                            //            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            //            alert.SetTitle("ERROR");
                            //            alert.SetMessage("Detect 1 ERROR row  (POPosInDataR = " + POPosInDataR.ToString() + "/drSelectedUPC row = " + CSDL.drSelectedUPC.Count().ToString() + " - columns = " + CSDL.drSelectedUPC[POPosInDataR].Table.Columns.Count.ToString() + ")");
                            //            alert.SetNegativeButton("Acknowlegde", (sender, agvs) =>
                            //            {
                            //                alert.Dispose();
                            //            });

                            //        }
                            //        else
                            //        {
                            //            //Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                            //            //b.SetTitle("limit=" + QtyLimit + " Qty=" + QtyAccm + " " + stIdentity);
                            //            //b.SetMessage(str);
                            //            //b.Create().Show();

                            //            DataRow newUPCrow = dtPackingDetail.NewRow();
                            //            newUPCrow["PoNo"] = tvPOUPC.Text;
                            //            newUPCrow[MyCol] = 1; ///////////////////////////////////////////////////////////////add output here
                            //            newUPCrow["TotalPacked"] = 1;
                            //            newUPCrow["JobNo"] = drTemp["JobNo"].ToString();
                            //            newUPCrow["JobDetId"] = string.IsNullOrEmpty(drTemp["JobDetId"].ToString()) ? 0 : (int)drTemp["JobDetId"];
                            //            newUPCrow["ColorId"] = string.IsNullOrEmpty(drTemp["ColorId"].ToString()) ? 0 : (int)drTemp["ColorId"];
                            //            newUPCrow["SizeId"] = string.IsNullOrEmpty(drTemp["SizeId"].ToString()) ? 0 : (int)drTemp["SizeId"];
                            //            newUPCrow["Identity"] = stIdentity;
                            //            newUPCrow["Sizx"] = drTemp["Sizx"].ToString();
                            //            newUPCrow["QtyLimit"] = QtyLimit.ToString() + ":" + QtyAccm.ToString() + ":" + CSDL.OpenTime;
                            //            dtPackingDetail.Rows.Add(newUPCrow);
                            //            dtPackingDetail.AcceptChanges();
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    isFinish = false;
                            //    MyBeep.Start();
                            //    edOutputByPO.SetBackgroundColor(Color.Red);
                            //    edSumOutputByPO.SetBackgroundColor(Color.Red);
                            //    if (run)
                            //    {
                            //        Toast.MakeText(this, CSDL.Language("M00124") + "... " + CSDL.Language("M00174"), ToastLength.Long).Show();
                            //        Toast.MakeText(this, "PO = " + tvPOUPC.Text + "\n" + "Size = " + drTemp["Sizx"].ToString() + "\n" + CSDL.Language("M00175") + "= " + QtyLimit.ToString() + "\n" + CSDL.Language("M00176") + "= " + QtyAccm.ToString() + "\n" + CSDL.Language("M00177") + "= " + (TodayTotalPk - 1).ToString(), ToastLength.Long).Show();
                            //    }
                            //    else Toast.MakeText(this, "Connect to server failed !!!", ToastLength.Long).Show();
                            //}

                            //if (isFinish)
                            //{
                            //    RefreshOutputShow(MyCol);
                            //    BtSave_LongClick(); // save back up
                            //    Toast.MakeText(this, CSDL.Language("M00178") + " !", ToastLength.Short).Show();
                            //    edOutputByPO.SetSelection(edOutputByPO.Text.Count()); //to scroll textview
                            //    if (chkbCamera.Checked) BtStart_Click();
                            //}
                            #endregion

                            #region Not get Qty from Server 
                            bool SaveBackup = false;
                            if (MyPOList.Contains(stIdentity))
                            {
                                int UPCPos = MyPOList.IndexOf(stIdentity);
                                int TodayTotalPk = dtPackingDetail.Rows[UPCPos]["TotalPacked"].ToString().Trim().Length == 0 ? 1 : (int)dtPackingDetail.Rows[UPCPos]["TotalPacked"] + 1;


                                if ((QtyLimit - (QtyAccm + QtyTdAnL + TodayTotalPk)) >= 0) // this is to check Output vs Order Qty
                                {
                                    dtPackingDetail.Rows[UPCPos][MyCol] = dtPackingDetail.Rows[UPCPos][MyCol].ToString().Trim().Length == 0 ? 1 : (int)dtPackingDetail.Rows[UPCPos][MyCol] + 1;
                                    dtPackingDetail.Rows[UPCPos]["TotalPacked"] = TodayTotalPk;
                                    dtPackingDetail.Rows[UPCPos]["QtyLimit"] = (QtyAccm + TodayTotalPk).ToString() + "|" + drTemp["LoadingQty"].ToString();
                                    dtPackingDetail.AcceptChanges();

                                    SaveBackup = true;
                                }
                                //else if (check)
                                //{
                                //    DataTable dt = CSDL.Cnnt.Doc("exec EndlineScanOutputGetUPCData '" + drTemp["UPC_Code"].ToString() + "','" + CSDL.SelectedLine + "'").Tables[0];

                                //    if (dt.Rows.Count > 0)
                                //    {
                                //        //drTemp["LoadingQty"] = dt.Select("Po_No = '" + drTemp["Po_No"].ToString() + "' and UPC_Code = '" + drTemp["UPC_Code"].ToString() + "'")[0]["LoadingQty"];

                                //        foreach (DataRow r in CSDL.dtUPCDetail.Rows)
                                //        {
                                //            if (r["Po_No"].ToString() == drTemp["Po_No"].ToString() && r["UPC_Code"].ToString() == drTemp["UPC_Code"].ToString())
                                //            {
                                //                DataRow row = dt.Select("Po_No = '" + drTemp["Po_No"].ToString() + "'")[0];
                                //                r["LoadingQty"] = row["LoadingQty"];
                                //                drTemp["LoadingQty"] = row["LoadingQty"];
                                //                break;
                                //            }
                                //        }
                                //    }

                                //    CountingOutputByUPC(false);
                                //}
                                else
                                {
                                    isFinish = false;
                                    MyBeep.Start();
                                    edOutputByPO.SetBackgroundColor(Color.Red);
                                    edSumOutputByPO.SetBackgroundColor(Color.Red);
                                    Toast.MakeText(this, CSDL.Language("M00124") + "... " + CSDL.Language("M00174"), ToastLength.Long).Show();
                                    Toast.MakeText(this, "PO = " + tvPOUPC.Text + "\n" + "Size = " + drTemp["Sizx"].ToString() + "\n" + CSDL.Language("M00175") + "= " + QtyLimit.ToString() + "\n" + CSDL.Language("M00176") + "= " + QtyAccm.ToString() + "\n" + CSDL.Language("M00177") + "= " + (TodayTotalPk - 1).ToString(), ToastLength.Long).Show();
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(drTemp["JobNo"].ToString()))
                                {
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle("ERROR");
                                    alert.SetMessage("Detect 1 ERROR row  (POPosInDataR = " + POPosInDataR.ToString() + "/drSelectedUPC row = " + CSDL.drSelectedUPC.Count().ToString() + " - columns = " + CSDL.drSelectedUPC[POPosInDataR].Table.Columns.Count.ToString() + ")");
                                    alert.SetNegativeButton("Acknowlegde", (sender, agvs) =>
                                    {
                                        alert.Dispose();
                                    });

                                }
                                else
                                {
                                    DataRow newUPCrow = dtPackingDetail.NewRow();
                                    newUPCrow["PoNo"] = tvPOUPC.Text;
                                    newUPCrow[MyCol] = 1; ///////////////////////////////////////////////////////////////add output here
                                    newUPCrow["TotalPacked"] = 1;
                                    newUPCrow["JobNo"] = drTemp["JobNo"].ToString();
                                    newUPCrow["JobDetId"] = string.IsNullOrEmpty(drTemp["JobDetId"].ToString()) ? 0 : (int)drTemp["JobDetId"];
                                    newUPCrow["ColorId"] = string.IsNullOrEmpty(drTemp["ColorId"].ToString()) ? 0 : (int)drTemp["ColorId"];
                                    newUPCrow["SizeId"] = string.IsNullOrEmpty(drTemp["SizeId"].ToString()) ? 0 : (int)drTemp["SizeId"];
                                    newUPCrow["Identity"] = stIdentity;
                                    newUPCrow["Sizx"] = drTemp["Sizx"].ToString();
                                    newUPCrow["QtyLimit"] = (QtyAccm + 1).ToString() + "|" + drTemp["LoadingQty"].ToString();
                                    dtPackingDetail.Rows.Add(newUPCrow);
                                    dtPackingDetail.AcceptChanges();

                                    SaveBackup = true;
                                }
                            }

                            if (isFinish)
                            {
                                RefreshOutputShow(MyCol);
                                BtSave_LongClick(); // save back up
                                Toast.MakeText(this, CSDL.Language("M00178") + " !", ToastLength.Short).Show();
                                edOutputByPO.SetSelection(edOutputByPO.Text.Count()); //to scroll textview
                                if (chkbCamera.Checked) BtStart_Click();
                            }

                            if (SaveBackup)
                            {
                                DataRow[] r = dtPackingDetail_backup.Select("Identity = '" + stIdentity + "'");

                                if (r.Length > 0)
                                {
                                    int TodayTotalPk = r[0]["TotalPacked"].ToString().Trim().Length == 0 ? 1 : (int)r[0]["TotalPacked"] + 1;
                                    r[0][MyCol] = r[0][MyCol].ToString().Trim().Length == 0 ? 1 : (int)r[0][MyCol] + 1;
                                    r[0]["TotalPacked"] = TodayTotalPk;
                                    r[0]["QtyLimit"] = (QtyAccm + TodayTotalPk).ToString() + "|" + drTemp["LoadingQty"].ToString();
                                    dtPackingDetail_backup.AcceptChanges();
                                }
                                else
                                {
                                    DataRow newUPCrow = dtPackingDetail_backup.NewRow();
                                    newUPCrow["PoNo"] = tvPOUPC.Text;
                                    newUPCrow[MyCol] = 1; ///////////////////////////////////////////////////////////////add output here
                                    newUPCrow["TotalPacked"] = 1;
                                    newUPCrow["JobNo"] = drTemp["JobNo"].ToString();
                                    newUPCrow["JobDetId"] = string.IsNullOrEmpty(drTemp["JobDetId"].ToString()) ? 0 : (int)drTemp["JobDetId"];
                                    newUPCrow["ColorId"] = string.IsNullOrEmpty(drTemp["ColorId"].ToString()) ? 0 : (int)drTemp["ColorId"];
                                    newUPCrow["SizeId"] = string.IsNullOrEmpty(drTemp["SizeId"].ToString()) ? 0 : (int)drTemp["SizeId"];
                                    newUPCrow["Identity"] = stIdentity;
                                    newUPCrow["Sizx"] = drTemp["Sizx"].ToString();
                                    newUPCrow["QtyLimit"] = (QtyAccm + 1).ToString() + "|" + drTemp["LoadingQty"].ToString();
                                    dtPackingDetail_backup.Rows.Add(newUPCrow);
                                    dtPackingDetail_backup.AcceptChanges();
                                }

                                string strOutputTable = "";
                                foreach (DataRow outr in dtPackingDetail_backup.Rows)
                                {
                                    foreach (DataColumn outc in dtPackingDetail_backup.Columns)
                                    {
                                        strOutputTable += outr[outc].ToString() + "=";
                                    }
                                    strOutputTable += "\n";
                                }

                                SaveTXTBackup(strOutputTable);
                            }

                            #endregion
                        }
                        else
                        {
                            Toast.MakeText(this, "..." + CSDL.Language("M00153") + " !!! " + CSDL.Language("M00054") + "...", ToastLength.Short).Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, "..." + CSDL.Language("M00179") + " !!!...", ToastLength.Short).Show();
                    }
                }
                else
                {
                    MyBeep.Start();
                    Toast.MakeText(this, CSDL.Language("M00180"), ToastLength.Long).Show();
                    edOutputByPO.SetBackgroundColor(Color.Yellow);
                    edSumOutputByPO.SetBackgroundColor(Color.Yellow);
                }
                edUPC.Text = "";

                //Android.App.AlertDialog.Builder a = new AlertDialog.Builder(this);
                //HorizontalScrollView ln = new HorizontalScrollView(this);
                //ListView ls = new ListView(this);
                //ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dtPackingDetail, new List<int> { 100 }, true);

                //ln.AddView(ls);
                //a.SetView(ln);

                //a.Create().Show();
            }
            catch (Exception ex) { Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show(); }
        }

        private void RefreshOutputShow(int TF)
        {
            edOutputByPO.Text = "";
            edSumOutputByPO.Text = "";
            List<bool> shInfo = new List<bool> { CSDL.blShPO, true, CSDL.blShSizeId, CSDL.blShJobNo };
            List<string> lsPO = new List<string>();
            foreach (DataRow dr in dtPackingDetail.Rows)
            {
                string hostNm = dr["PoNo"].ToString() + "|" + dr["Sizx"].ToString() + "|" + dr["SizeId"].ToString() + "|" + dr["JobNo"].ToString();
                if (!lsPO.Contains(hostNm)) lsPO.Add(hostNm);
            }

            //Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
            //HorizontalScrollView main = new HorizontalScrollView(this);
            //ListView ls = new ListView(this); main.AddView(ls);
            //ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dtPackingDetail, new List<int> { 200 }, true);

            //b.SetView(main);
            //b.Create().Show();

            if (TF >= CSDL.TimeArray.Count) TF = CSDL.TimeArray.Count - 1;

            for (int i = 1; i <= TF; i++) // check every hourly column +1
            {
                int sumOutputhour = 0;
                int sumTotal = 0;
                string c = "";
                string d = "";
                for (int ii = 0; ii < lsPO.Count(); ii++) // check every PO
                {
                    //format string to display
                    string[] strShInfo = lsPO[ii].ToString().Split('|');
                    string strFNInfo = "";
                    for (int m = 0; m < shInfo.Count; m++)
                    {
                        if (shInfo[m]) strFNInfo += strShInfo[m] + (m == shInfo.LastIndexOf(true) ? "" : " | ");
                    }

                    //calculate quantity to display
                    object sumObject;
                    sumObject = dtPackingDetail.Compute("Sum(TF" + i.ToString("00") + ")", "PoNo='" + lsPO[ii].Split('|')[0].Trim() + "' and SizeId='" + lsPO[ii].Split('|')[2].Trim() + "' and JobNo='" + lsPO[ii].Split('|')[3].Trim() + "'");
                    int sums = sumObject.ToString().Length == 0 ? 0 : int.Parse(sumObject.ToString());
                    if (sums > 0)
                    {
                        c += "   " + strFNInfo + ": " + sumObject.ToString() + "\n";
                        sumOutputhour += sums;
                    }
                    if (i == 1)
                    {
                        string it = lsPO[ii]; string itt = it;
                        string filter = "PoNo='" + lsPO[ii].Split('|')[0].Trim() + "' and SizeId='" + lsPO[ii].Split('|')[2].Trim() + "' and JobNo='" + lsPO[ii].Split('|')[3].Trim() + "'";
                        d += "   " + strFNInfo + ": " + dtPackingDetail.Compute("Sum(TotalPacked)", "PoNo='" + lsPO[ii].Split('|')[0].Trim() + "' and SizeId='" + lsPO[ii].Split('|')[2].Trim() + "' and JobNo='" + lsPO[ii].Split('|')[3].Trim() + "'").ToString() + "\n";
                        sumTotal += int.Parse(dtPackingDetail.Compute("Sum(TotalPacked)", filter).ToString());
                    }
                }
                edOutputByPO.Text += FmTime(CSDL.TimeArray[i - 1] + (i == 0 ? 0 : 1)) + "-" + FmTime(CSDL.TimeArray[i]) + " < " + CSDL.Language("M00189") + ": " + sumOutputhour.ToString() + " >" + "\n" + c + "\n";
                if (i == 1) edSumOutputByPO.Text = CSDL.Language("M00177") + " (PO|Size):<" + sumTotal.ToString() + ">" + "\n" + d;
            }
        }

        private string FmTime(int mytime)
        {
            if (mytime.ToString().Length > 2 && mytime.ToString().Length < 5)
            {
                return mytime.ToString("0000").Substring(0, 2) + ":" + mytime.ToString().Substring(mytime.ToString().Length - 2, 2);
            }
            else return "Err";
        }
        private Handler h = new Handler();
        private void TmUPC_Elapsed(object sender, ElapsedEventArgs e)
        {
            h.Post(() =>
            {
                intUPC++;
                if (intUPC > CSDL.intTmStop && edUPC.Text.Trim() != "") //5*20 ms
                {
                    tmUPC.Stop();
                    intUPC = 0;
                    RefreshingUPC();
                }
            });
        }
        private void LvPOListUPC_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

            string pono = CustomListViewAdapter.POList[e.Position].POName;
            string gmt = CustomListViewAdapter.POList[e.Position].PODtl;
            string shipno = CustomListViewAdapter.POList[e.Position].POShipNo;
            int qty = CustomListViewAdapter.POList[e.Position].POQty;

            b.SetMessage("PO : " + pono + " | Gmt : " + gmt + " | ShipNo : " + shipno + " | OrderQty : " + qty);
            b.SetTitle("Select PO");
            b.SetPositiveButton("YES", (s, a) =>
            {
                drTemp = null;
                POPosInDataR = e.Position; // this row HAVE TO BE 1st because tvPOUPC change -> run another void
                if (CSDL.drSelectedUPC.Count() > POPosInDataR) drTemp = CSDL.drSelectedUPC[POPosInDataR];

                CSDL.SelectedUPCPO = CustomListViewAdapter.POList[POPosInDataR].POName;
                CSDL.SelectedUPCPOQty = CustomListViewAdapter.POList[POPosInDataR].POQty;
                CSDL.SelectedGmtType = CustomListViewAdapter.POList[POPosInDataR].PODtl;
                CSDL.SelectedColor = CustomListViewAdapter.POList[POPosInDataR].POSize;
                CSDL.SelectedShipNo = CustomListViewAdapter.POList[POPosInDataR].POShipNo;
                tvPOUPC.Text = CSDL.SelectedUPCPO;

                CSDL.CheckQtyAcum = true;
            });
            b.SetNegativeButton("NO", (s, a) => { });

            b.SetCancelable(false);
            b.Create().Show();
        }

        public override void OnBackPressed()
        {

            CSDL.SelectedUPCPO = "";
            CSDL.UPCString = "";
            Finish();
        }
    }
    public class DatePickerFragment : DialogFragment,
                                      DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> dateSelectedHandler = delegate { };

        public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected)
        {
            DatePickerFragment frag = new DatePickerFragment
            {
                dateSelectedHandler = onDateSelected
            };
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently = DateTime.Now;
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                                                           this,
                                                           currently.Year,
                                                           currently.Month - 1,
                                                           currently.Day);
            return dialog;
        }

        public void OnDateSet(Android.Widget.DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            Log.Debug(TAG, selectedDate.ToString("MMMM dd, yyyy")); //ToLongDateString()ToString("MMMM dd, yyyy")
            dateSelectedHandler(selectedDate);
        }
    }
}