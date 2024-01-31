using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using CSDL;
using Java.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UpdateManager;
using Xamarin.Essentials;
using ZXing.Mobile;
using static Android.App.ActionBar;
using static System.Collections.Specialized.BitVector32;
using Orientation = Android.Widget.Orientation;

namespace EndlineOutputScan
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorLandscape, Icon = "@drawable/Clothing")]
    public class MainActivity : Activity
    {
        MediaPlayer LGSound;
        EditText id, pass;
        Spinner spFacSelect, spLineSelect, spLanguage;
        MobileBarcodeScanner scanner;
        View overlay;
        Button flash;
        TextView t, user, workingtime;
        RelativeLayout lvVwGroup;

        string strSelectedFac, strSelectedLine, saveline = "", savefac = "", strLang, loginHis = "", AuthrG = "", WrkHrId = "", ShiftId = "";

        DataSet dsLang = new DataSet();

        bool isadShow = false, forceUpdate = false, newVer = false;
        string LinkUpdateAPP = "http://192.168.10.133/ScanOutputEndSWL.apk";
        Android.App.AlertDialog ad;
        long TTDL = 0, TTRC = 0;
        int PercentComplete = 0, Progr = 0;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            //ToolbarResource = Resource.Layout.LogIn;
            //base.OnCreate(savedInstanceState);
            //global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //LoadApplication(new App());
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LogIn);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            UserDialogs.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //RequestInit.Init(this);
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());


            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            LGSound = MediaPlayer.Create(this, Resource.Raw.notification2);

            id = FindViewById<EditText>(Resource.Id.tbUserID);
            TextView server = FindViewById<TextView>(Resource.Id.tvServer);
            pass = FindViewById<EditText>(Resource.Id.tbPassword);
            spFacSelect = FindViewById<Spinner>(Resource.Id.spFacSelect);
            spLineSelect = FindViewById<Spinner>(Resource.Id.spLineSelect);
            spLanguage = FindViewById<Spinner>(Resource.Id.spLanguage);

            Button login = FindViewById<Button>(Resource.Id.btLogIn);
            Button loginqr = FindViewById<Button>(Resource.Id.btLogInQR);
            Button exit = FindViewById<Button>(Resource.Id.btExit);
            Button btSelectLineQR = FindViewById<Button>(Resource.Id.btSelectLineQR);
            t = FindViewById<TextView>(Resource.Id.t1);
            user = FindViewById<TextView>(Resource.Id.lbUserID);
            TextView lbTitle = FindViewById<TextView>(Resource.Id.lbTitle);
            TextView lbPassword = FindViewById<TextView>(Resource.Id.lbPassword);
            TextView lbFac = FindViewById<TextView>(Resource.Id.lbFac);
            TextView lbLine = FindViewById<TextView>(Resource.Id.lbLine);
            workingtime = FindViewById<TextView>(Resource.Id.tvworkingtime); workingtime.TextFormatted = Html.FromHtml("<u>" + workingtime.Text + "</u>");

            ISharedPreferences pre = GetSharedPreferences("ScanOutput", FileCreationMode.Private);
            string ch = pre.GetString("chuoi", "").ToString();

            ISharedPreferencesEditor ed = pre.Edit();

            if (ch != "")
            {
                CSDL.chuoi = ch;
                CSDL.Cnnt = new Connect(CSDL.chuoi);

                Load();
            }
            else
            {
                Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);
                List<string> ls = new List<string> { "A1A - VietNam", "TRAX - Thailand", "TAC - Cambodia" };
                b.SetTitle("Your server is in : ");
                b.SetSingleChoiceItems(ls.ToArray(), -1, (s, a) =>
                {
                    switch (a.Which)
                    {
                        case 0:
                            CSDL.chuoi = CSDL.chuoi1;
                            break;
                        case 1:
                            CSDL.chuoi = CSDL.chuoi2;
                            break;
                        case 2:
                            CSDL.chuoi = CSDL.chuoi3;
                            break;
                    }
                    //Toast.MakeText(this, CSDL.chuoi, ToastLength.Long).Show();
                    ed.PutString("chuoi", CSDL.chuoi);
                    ed.Commit();

                    CSDL.Cnnt = new Connect(CSDL.chuoi);
                    Load();

                    ((Dialog)s).Dismiss();
                });

                b.SetCancelable(false);
                b.Create().Show();
            }

            //CheckServer();

            void CheckServer(bool run = true)
            {
                try
                {
                    SqlConnection con = new SqlConnection(CSDL.chuoi);
                    con.Open();
                    con.Close();

                    Load();
                }
                catch (SqlException ex)
                {
                    if (CSDL.checkIP)
                    {
                        WebClient wb = new WebClient();
                        string str = wb.DownloadString("http://api.hostip.info/get_json.php");

                        GetData gd = JsonConvert.DeserializeObject<GetData>(str);

                        string ip = gd.IP;
                        if (ip != "")
                        {
                            IPSERVER server = CSDL.IPSERVER.Where(s => s.IP.Contains(ip)).FirstOrDefault();

                            CSDL.chuoi = server.Server;
                        }

                        CSDL.checkIP = false;
                        if (run) CheckServer(false);
                    }
                    else
                    {
                        CSDL.ShowMessage(lvVwGroup, ex.ToString());//Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                        var itent = new Intent(this, typeof(ServerActivity));
                        StartActivity(itent);
                        Finish();
                    }
                }
            }


            void Load()
            {
                CSDL.LogInCheck = false;


                //initialize for screen stretching
                var metric = Application.Context.Resources.DisplayMetrics;
                CSDL.width = metric.WidthPixels;
                CSDL.height = metric.HeightPixels;
                CSDL.density = metric.Density;
                CSDL.SizingScrRt = CSDL.width / (1024 * CSDL.density);
                CSDL.SizingScrRtH = CSDL.height / (536 * CSDL.density);
                CSDL.TextRatio = 0.9;
                //Toast.MakeText(this, "Res=" + CSDL.width + "x" + CSDL.height + " | Density=" + Resources.DisplayMetrics.Density + " | TextRatio=" + CSDL.TextRatio + " | SizingScrRt=" + CSDL.SizingScrRt + "\n"
                //                        + CSDL.chuoi.Split(";")[0], ToastLength.Short).Show();

                LGSound.Start();

                lbTitle.LongClick += delegate
                {
                    Android.App.AlertDialog.Builder albd = new Android.App.AlertDialog.Builder(this);
                    albd.SetSingleChoiceItems(loginHis.Split('\n'), -1, (s, e) => { });
                    albd.SetPositiveButton("Exit", (s, e) => { });
                    albd.Create().Show();
                };

                spFacSelect.ItemSelected += (sender, e) =>
                {
                    Spinner spinner = (Spinner)sender;
                    strSelectedFac = spinner.GetItemAtPosition(e.Position).ToString();

                    List<string> LineList = new List<string>();
                    //if (saveline != "") LineList.Add(saveline);
                    foreach (DataRow dr in dsLang.Tables[2].Rows)
                    {
                        if (!LineList.Contains(dr[0].ToString()) && strSelectedFac == dr[0].ToString().Substring(0, 2)) LineList.Add(dr[0].ToString());
                    }
                    LineList.Add("LineTest");
                    ArrayAdapter adapterLS = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, LineList);
                    adapterLS.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    spLineSelect.Adapter = adapterLS;
                    if (LineList.Contains(saveline)) spLineSelect.SetSelection(LineList.IndexOf(saveline));

                };

                spLineSelect.ItemSelected += (sender, e) =>
                {
                    strSelectedLine = spLineSelect.GetItemAtPosition(e.Position).ToString();
                };
                spLanguage.ItemSelected += (sender, e) =>
                {
                    Spinner spinner = (Spinner)sender;
                    strLang = spinner.GetItemAtPosition(e.Position).ToString();

                    DataRow[] drLa = dsLang.Tables[0].Select("Language='" + strLang + "'");
                    if (drLa.Length > 0)
                    {
                        DataRow dRw = drLa[0];
                        CSDL.LangRef = string.IsNullOrEmpty(dRw[1].ToString()) ? 1 : int.Parse(dRw[1].ToString());
                    }

                    CSDL.ltLang.Clear();
                    foreach (DataRow drT in dsLang.Tables[1].Rows)
                    {
                        CSDL.ltLang.Add(new ShCTN_ClassAdapterPOnCTN { ShCTN_ClPOorCTNCode = drT[0].ToString(), ShCTN_ClQtyorCTNNo = drT[CSDL.LangRef].ToString() });
                    }

                    //Toast.MakeText(this, CSDL.ltLang.FindIndex(x => x.ShCTN_ClPOorCTNCode == "M00001").ToString(), ToastLength.Long).Show();
                    lbTitle.Text = CSDL.Language("M00001");
                    user.Text = CSDL.Language("M00002");
                    lbPassword.Text = CSDL.Language("M00003");
                    lbFac.Text = CSDL.Language("M00006");
                    lbLine.Text = CSDL.Language("M00164");

                    login.Text = lbTitle.Text;
                    loginqr.Text = CSDL.Language("M00005");
                    btSelectLineQR.Text = CSDL.Language("M00165");
                    exit.Text = CSDL.Language("M00004");

                    server.Text = CSDL.Language("M00007");

                    //UpdateChecker();
                    UpdateVersion();
                };

                login.Click += delegate
                {
                    if (forceUpdate && newVer)
                    {
                        Toast.MakeText(this, "Update require !!", ToastLength.Long).Show();
                        //UpdateChecker();
                        UpdateVersion();
                    }
                    else
                    {
                        //try
                        {
                            //var itent = new Intent(this, typeof(PackingWork));
                            //StartActivity(itent);
                            //*

                            CSDL.HideKeyboard(this, CurrentFocus);
                            if (pass.Text != "")
                            {
                                AuthrG = "";
                                DataTable dtUserdetail = LoginAccessing(id.Text, pass.Text);

                                if (dtUserdetail.Rows.Count == 1)
                                {
                                    if (savefac.Trim() == strSelectedFac.Trim() || savefac.Trim() == "") //same fac
                                    {
                                        if (saveline.Trim() == strSelectedLine.Trim() || saveline.Trim() == "") //same fac + same line
                                        {
                                            LoginSuccess(dtUserdetail);
                                        }
                                        else //same fac different line
                                        {
                                            if (dtUserdetail.Rows[0]["FacLine"].ToString().Contains("Ad")) LoginSuccess(dtUserdetail);
                                            else AuthrLogin(2, dtUserdetail);
                                        }
                                    }
                                    else // different fac
                                    {
                                        if (dtUserdetail.Rows[0]["FacLine"].ToString().Contains("Ad")) LoginSuccess(dtUserdetail);
                                        else AuthrLogin(4, dtUserdetail);
                                    }
                                }
                                else if (dtUserdetail.Rows.Count == 0)
                                {
                                    Toast.MakeText(this, CSDL.Language("M00012"), ToastLength.Short).Show();
                                    id.Text = "";
                                    pass.Text = "";
                                }
                                else
                                {
                                    Toast.MakeText(this, CSDL.error, ToastLength.Short).Show();
                                    id.Text = "";
                                    pass.Text = "";
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, CSDL.Language("M00153"), ToastLength.Long).Show();
                            }
                        }
                        //catch
                        //{
                        //    Toast.MakeText(this, CSDL.Language("M00019"), ToastLength.Short).Show();
                        //};
                    }
                };

                login.LongClick += delegate
                {
                    PopupWindow puCPW;
                    View tempview;

                    LayoutInflater layoutInflater = (LayoutInflater)BaseContext.GetSystemService("layout_inflater");
                    tempview = layoutInflater.Inflate(Resource.Layout.ChangePW, null);
                    puCPW = new PopupWindow(tempview, LayoutParams.WrapContent, LayoutParams.WrapContent) //1280x724
                    {
                        OutsideTouchable = false
                    };
                    EditText pued1 = tempview.FindViewById<EditText>(Resource.Id.tbUserID);
                    EditText pued2 = tempview.FindViewById<EditText>(Resource.Id.tbPassword);
                    EditText pued3 = tempview.FindViewById<EditText>(Resource.Id.tbRePassword);

                    Button pubtSave = tempview.FindViewById<Button>(Resource.Id.btLogIn);
                    Button pubtExit = tempview.FindViewById<Button>(Resource.Id.btExit);

                    //resizing layout
                    var lvVwGroup = (ViewGroup)tempview.FindViewById<RelativeLayout>(Resource.Id.relCPW);
                    CSDL.ScreenStretching(1, lvVwGroup);

                    puCPW.Focusable = true;
                    puCPW.Update();
                    puCPW.ShowAsDropDown(id, -100, 5);

                    pubtExit.Click += delegate { puCPW.Dismiss(); };

                    pubtSave.Click += delegate
                    {
                        if (pued2.Text == pued3.Text && pued2.Text != "")
                        {
                            string c = "select * from InLineQcUserDetail where EmployeeCode='" + id.Text + "'COLLATE SQL_Latin1_General_CP1_CS_AS and Password='" + pued1.Text + "'COLLATE SQL_Latin1_General_CP1_CS_AS";
                            int i = CSDL.Cnnt.Doc(c).Tables[0].Rows.Count;
                            if (i == 1)
                            {
                                c = "update InLineQcUserDetail set Password = '" + pued2.Text + "' where EmployeeCode = '" + id.Text + "'";
                                CSDL.Cnnt.Ghi(c);
                                puCPW.Dismiss();
                                Toast.MakeText(this, CSDL.Language("M00013"), ToastLength.Short).Show();
                            }
                            else Toast.MakeText(this, CSDL.Language("M00014"), ToastLength.Short).Show();
                        }
                        else Toast.MakeText(this, CSDL.Language("M00015"), ToastLength.Short).Show();
                    };
                };

                loginqr.Click += async delegate
                {
                    CSDL.LogInCheck = true;

                    scanner.UseCustomOverlay = true;
                    overlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.CameraScan, null);
                    flash = overlay.FindViewById<Button>(Resource.Id.btFlash);
                    flash.Click += (sender, e) => scanner.ToggleTorch();

                    scanner.CustomOverlay = overlay;
                    scanner.AutoFocus();
                    var result = await scanner.Scan(new MobileBarcodeScanningOptions { UseNativeScanning = true });

                    HandleScanResultLogin(result);
                };
                btSelectLineQR.Click += async delegate
                {
                    scanner.UseCustomOverlay = true;
                    overlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.CameraScan, null);
                    flash = overlay.FindViewById<Button>(Resource.Id.btFlash);
                    flash.Click += (sender, e) => scanner.ToggleTorch();

                    scanner.CustomOverlay = overlay;
                    scanner.AutoFocus();
                    var result = await scanner.Scan(new MobileBarcodeScanningOptions { UseNativeScanning = true });

                    HandleScanResultSelectLine(result);
                };
                exit.Click += delegate
                {
                    Finish();
                    CSDL.LogInCheck = false;
                };
                exit.LongClick += delegate
                {
                    Android.App.AlertDialog.Builder bd = new Android.App.AlertDialog.Builder(this);
                    bd.SetMessage(CSDL.lvMessage);
                    bd.Create().Show();
                };
                server.Click += delegate
                {
                    var itent = new Intent(this, typeof(ServerActivity));
                    StartActivity(itent);
                    Finish();
                };
                server.LongClick += delegate
                {
                    UpdateVersion();
                    //UpdateChecker();
                };
                user.LongClick += delegate
                {
                    ISharedPreferences pre2 = GetSharedPreferences("OutputTable", FileCreationMode.Private);
                    ISharedPreferencesEditor editor = pre2.Edit();
                    editor.Clear();
                    editor.PutString("strOutputTable", "");
                    editor.Apply();

                    Toast.MakeText(this, "History deleted !!!", ToastLength.Long).Show();
                };
                workingtime.Click += delegate
                {
                    SetWorkingTime();
                };

                void SetWorkingTime(string sh = "")
                {
                    DataSet ds = new DataSet();
                    ds = CSDL.Cnnt.Doc("exec GetDataFromQuery 46,'" + strSelectedFac + "','','','',''");

                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    Dialog d = new Dialog(this);

                    LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

                    LinearLayout ln1 = new LinearLayout(this) { Orientation = Orientation.Horizontal }; layout.AddView(ln1);
                    TextView tv1 = new TextView(this) { Text = "Shift : " }; ln1.AddView(tv1);
                    EditText ed1 = new EditText(this) { Text = sh, LayoutParameters = new ViewGroup.LayoutParams(200, ViewGroup.LayoutParams.WrapContent), TextAlignment = TextAlignment.Center }; ed1.Focusable = false; ln1.AddView(ed1);
                    Button bt1 = new Button(this) { Text = "ADD MORE" }; ln1.AddView(bt1);
                    Button bt2 = new Button(this) { Text = "DELETE" }; ln1.AddView(bt2);
                    Button bt3 = new Button(this) { Text = "CLEAR ALL" }; ln1.AddView(bt3);

                    ListView ls = new ListView(this); layout.AddView(ls);
                    ls.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(ds.Tables[1], new List<int> { 100, 200, 200 }, true);// { SingleClicked = true, ClickedItemColor = Color.Green };

                    ed1.Click += delegate
                    {
                        Android.App.AlertDialog.Builder b1 = new AlertDialog.Builder(this);
                        Dialog d1 = new Dialog(this);

                        string[] it = ds.Tables[0].Select().Select(s => s[0] + " : " + s[1]).ToArray();
                        b1.SetTitle("Select the start time of working : ");
                        b1.SetSingleChoiceItems(it, -1, (s, a) =>
                        {
                            ed1.Text = it[a.Which];
                            d1.Dismiss();
                        });

                        d1 = b1.Create();
                        d1.Show();
                    };
                    bt1.Click += delegate
                    {
                        if (ed1.Text != "")
                        {
                            Android.App.AlertDialog.Builder b1 = new AlertDialog.Builder(this);
                            Dialog d1 = new Dialog(this);

                            string[] it = dsLang.Tables[2].Select("FacLine like '" + strSelectedFac + "%'").Select(s => s[0].ToString()).ToArray();
                            bool[] check = new bool[it.Length];
                            for (int i = 0; i < check.Length; i++) check[i] = false;

                            b1.SetTitle("Select the Facline : ");
                            b1.SetMultiChoiceItems(it, check, (s, a) =>
                            {
                                check[a.Which] = !check[a.Which];
                            });

                            b1.SetPositiveButton("OK", (s, a) =>
                            {
                                string ms = "", qry = "";
                                string shn = ed1.Text.Split(':')[0].Trim();

                                for (int i = 0; i < it.Length; i++)
                                {
                                    if (check[i])
                                    {
                                        if (ds.Tables[1].Select("Facline = '" + it[i] + "'").Length > 0) ms += it[i] + ",";
                                        else qry += "insert into InlineWrkHrSection (SectionNm,ShiftNm) values ('" + it[i] + "','" + shn + "') \n";
                                    }
                                }

                                if (qry != "") CSDL.Cnnt.Ghi(qry);

                                if (CSDL.Cnnt.ErrorMessage != "") Toast.MakeText(this, CSDL.Cnnt.ErrorMessage, ToastLength.Long).Show();
                                else
                                {
                                    SetWorkingTime(ed1.Text);
                                    if (ms != "") Toast.MakeText(this, "The selected line (" + ms + ") is existing", ToastLength.Long).Show();
                                    else Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();
                                }

                                d1.Dismiss();
                                d.Dismiss();
                            });

                            d1 = b1.Create();
                            d1.Show();
                        }
                        else Toast.MakeText(this, "Please select the shift time first !!!", ToastLength.Long).Show();
                    };
                    bt2.Click += delegate
                    {
                        Android.App.AlertDialog.Builder b1 = new AlertDialog.Builder(this);
                        Dialog d1 = new Dialog(this);

                        string[] it = ds.Tables[1].Select().Select(s => s[1].ToString()).ToArray();
                        bool[] check = new bool[it.Length];
                        for (int i = 0; i < check.Length; i++) check[i] = false;

                        b1.SetTitle("Select the Facline : ");
                        b1.SetMultiChoiceItems(it, check, (s, a) =>
                        {
                            check[a.Which] = !check[a.Which];
                        });

                        b1.SetPositiveButton("OK", (s, a) =>
                        {
                            string qry = "";

                            for (int i = 0; i < it.Length; i++)
                            {
                                if (check[i]) qry += "delete from InlineWrkHrSection where SectionNm = '" + it[i] + "' \n";
                            }

                            if (qry != "") CSDL.Cnnt.Ghi(qry);

                            if (CSDL.Cnnt.ErrorMessage != "") Toast.MakeText(this, CSDL.Cnnt.ErrorMessage, ToastLength.Long).Show();
                            else
                            {
                                SetWorkingTime(ed1.Text);
                                Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();
                            }

                            d1.Dismiss();
                            d.Dismiss();
                        });

                        d1 = b1.Create();
                        d1.Show();
                    };
                    bt3.Click += delegate
                    {
                        CSDL.Cnnt.Ghi("delete from InlineWrkHrSection where SectionNm like '" + strSelectedFac + "%'");

                        if (CSDL.Cnnt.ErrorMessage != "") Toast.MakeText(this, CSDL.Cnnt.ErrorMessage, ToastLength.Long).Show();
                        else
                        {
                            SetWorkingTime(ed1.Text);
                            Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();
                        }

                        d.Dismiss();
                    };

                    b.SetView(layout);
                    d = b.Create();
                    d.Show();
                }

                try
                {
                    CSDL kn = new CSDL();

                    CSDL.blShPO = string.IsNullOrEmpty(pre.GetString("blShPONo", "").ToString()) ? true : bool.Parse(pre.GetString("blShPONo", "").ToString());
                    CSDL.blShJobNo = string.IsNullOrEmpty(pre.GetString("blShJobNo", "").ToString()) ? false : bool.Parse(pre.GetString("blShJobNo", "").ToString());
                    CSDL.blShSizeId = string.IsNullOrEmpty(pre.GetString("blShSizeId", "").ToString()) ? false : bool.Parse(pre.GetString("blShSizeId", "").ToString());

                    CSDL.Cnnt = new Connect(CSDL.chuoi);

                    WrkHrId = pre.GetString("WrkHrId", "").ToString();
                    ShiftId = pre.GetString("ShiftId", "").ToString();

                    //Toast.MakeText(this, CSDL.chuoi.Substring(0, 27), ToastLength.Short).Show();

                    pre = GetSharedPreferences("login", FileCreationMode.Private);
                    id.Text = pre.GetString("user", "").ToString();
                    saveline = pre.GetString("line", "").ToString();
                    loginHis = pre.GetString("logHis", "").ToString();

                    string strLan = "exec GetLoadData 12,'','" + saveline + "'";

                    dsLang = CSDL.Cnnt.Doc(strLan);

                    DataRow[] tempr = dsLang.Tables[3].Select("Descpt = 'SCNUPCESWL'");
                    LinkUpdateAPP = tempr[0][0].ToString();
                    forceUpdate = string.IsNullOrEmpty(tempr[0]["ForceUpdate"].ToString()) ? false : (int.Parse(tempr[0]["ForceUpdate"].ToString()) > 0 ? true : false);
                    CSDL.updateTime = string.IsNullOrEmpty(tempr[0][1].ToString()) ? 10 : int.Parse(tempr[0][1].ToString()); //update time

                    //restore memorized LANGUAGE
                    string f = pre.GetString("Lang", "").ToString();
                    List<string> mLange = new List<string>();
                    foreach (DataRow dr in dsLang.Tables[0].Rows)
                    {
                        if (!mLange.Contains(dr[0].ToString())) mLange.Add(dr[0].ToString());
                    }
                    ArrayAdapter ada = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, mLange);
                    ada.SetDropDownViewResource(Android.Resource.Layout.SimpleListItemSingleChoice);
                    spLanguage.Adapter = ada;
                    if (mLange.Contains(f)) spLanguage.SetSelection(mLange.IndexOf(f));


                    //restore memorized FACLIST
                    savefac = pre.GetString("Fac", "").ToString();
                    List<string> FacList = new List<string>();
                    foreach (DataRow dr in dsLang.Tables[2].Rows)
                    {
                        if (!FacList.Contains(dr[0].ToString().Substring(0, 2))) FacList.Add(dr[0].ToString().Substring(0, 2));
                    }
                    ArrayAdapter adaterL = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, FacList);
                    adaterL.SetDropDownViewResource(Android.Resource.Layout.SimpleListItemSingleChoice);
                    spFacSelect.Adapter = adaterL;
                    if (FacList.Contains(savefac)) spFacSelect.SetSelection(FacList.IndexOf(savefac));

                    //resizing layout
                    lvVwGroup = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
                    CSDL.ScreenStretching(1, lvVwGroup);

                    //UpdateVersion();
                }
                catch (Exception ex)
                {
                    CSDL.ShowMessage(lvVwGroup, "Unable to connect to server or Language database invalid" + "\n" + ex.Message);//Toast.MakeText(this, "Unable to connect to server or Language database invalid" + "\n" + ex.Message, ToastLength.Short).Show();
                }
            }
        }
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
        private void AuthrLogin(int Level, DataTable dtLoginResult)
        {
            PopupWindow puCPW;
            View tempview;

            LayoutInflater layoutInflater = (LayoutInflater)BaseContext.GetSystemService("layout_inflater");
            tempview = layoutInflater.Inflate(Resource.Layout.AuthorityLogin, null);
            puCPW = new PopupWindow(tempview, LayoutParams.WrapContent, LayoutParams.WrapContent) //1280x724
            {
                OutsideTouchable = false
            };
            EditText pued1 = tempview.FindViewById<EditText>(Resource.Id.tbUserID);
            EditText pued2 = tempview.FindViewById<EditText>(Resource.Id.tbPassword);

            Button pubtSave = tempview.FindViewById<Button>(Resource.Id.btLogIn);
            Button pubtExit = tempview.FindViewById<Button>(Resource.Id.btExit);

            //resizing layout
            var lvVwGroup = (ViewGroup)tempview.FindViewById<RelativeLayout>(Resource.Id.relAuthrLog);
            CSDL.ScreenStretching(1, lvVwGroup);

            puCPW.Focusable = true;
            puCPW.Update();
            puCPW.ShowAsDropDown(id, -100, 5);

            pubtExit.Click += delegate { puCPW.Dismiss(); };

            pubtSave.Click += delegate
            {
                DataTable dtAuthrPss = LoginAccessing(pued1.Text, pued2.Text);
                if (dtAuthrPss.Rows.Count > 0)
                {
                    if (dtAuthrPss.Rows[0]["FacLine"].ToString().Contains("Ad") || (dtAuthrPss.Rows[0]["FacLine"].ToString().Contains(strSelectedFac) && (string.IsNullOrEmpty(dtAuthrPss.Rows[0]["ManLevel"].ToString()) ? 0 : int.Parse(dtAuthrPss.Rows[0]["ManLevel"].ToString())) >= Level))
                    {
                        AuthrG = pued1.Text;
                        LoginSuccess(dtLoginResult);
                    }
                    else Toast.MakeText(this, "Authority deny !! Require LEVEL >= " + Level.ToString(), ToastLength.Long).Show();
                }
                else Toast.MakeText(this, "No database / cant connect to server", ToastLength.Long).Show();

                puCPW.Dismiss();
            };
        }
        private void LoginSuccess(DataTable dtLoginDetail)
        {
            Toast.MakeText(this, CSDL.Language("M00020"), ToastLength.Short).Show();
            CSDL.user = id.Text.ToString();
            CSDL.mk = pass.Text.ToString();
            CSDL.SelectedLine = strSelectedLine;
            DataRow r = dtLoginDetail.Rows[0];
            CSDL.username = r["EmployeeName"].ToString();

            try
            {
                //if (!string.IsNullOrEmpty(WrkHrId))
                {
                    string str = " create table #t (WrkHrId int, ShiftId int) "
                                + " Declare @Sql nvarchar(max) "
                                + " set @Sql = 'select WrkHrId, ' + DATENAME(weekday, getdate()) + ' as ShiftId from InlineWrkHrSection where SectionNm = ''" + strSelectedLine + "'' and WrkHrId = ''" + WrkHrId + "'''"
                                + " insert into #t EXEC sp_executesql @Sql"
                                + " select top 1 [WrkHr01],[WrkHr02],[WrkHr03],[WrkHr04],[WrkHr05],[WrkHr06],[WrkHr07],[WrkHr08],[WrkHr09],[WrkHr10],[WrkHr11],[WrkHr12],[WrkHr13],[WrkHr14] "
                                + " from InlineWrkHrMaster a right join #t b on a.WrkHrId = b.WrkHrId and a.ShiftId = b.ShiftId "
                                + " drop table #t ";

                    DataTable dt = CSDL.Cnnt.Doc("exec GetDataFromQuery 29,'" + strSelectedFac + "','" + CSDL.SelectedLine + "','','',''").Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        CSDL.TimeArray.Clear();
                        for (int i = 0; i < 24; i++)
                        {
                            if (!string.IsNullOrEmpty(dt.Rows[0][i + 3].ToString())) CSDL.TimeArray.Add(int.Parse(dt.Rows[0][i + 3].ToString()));
                        }
                    }
                }

                CSDL.LogInCheck = true;
                if (loginHis.Split('\n').Length >= 20)
                {
                    loginHis = loginHis.Substring(loginHis.IndexOf('\n') + 1, loginHis.Length - loginHis.IndexOf('\n') - 1);
                }
                loginHis += "Old: " + savefac + " | " + saveline + " | New: " + strSelectedFac + " | " + strSelectedLine + " | " + id.Text + " | " + AuthrG + " | " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "\n";

                var itent = new Intent(this, typeof(PackingWork));
                StartActivity(itent);
                //Finish();

                savefac = strSelectedFac;
                saveline = strSelectedLine;

                ISharedPreferences pre = GetSharedPreferences("login", FileCreationMode.Private);
                ISharedPreferencesEditor editor = pre.Edit();

                editor.Clear();
                editor.PutString("user", id.Text);
                editor.PutString("line", strSelectedLine);
                editor.PutString("Fac", strSelectedFac);
                editor.PutString("Lang", strLang);
                editor.Apply();
            }
            catch (Exception Ex)
            {
                CSDL.LogInCheck = false;
                Toast.MakeText(this, CSDL.Language("M00018") + "/ n" + Ex.ToString(), ToastLength.Long).Show();
            }
        }

        private DataTable LoginAccessing(string ten, string mk)
        {
            SqlConnection con = new SqlConnection(CSDL.chuoi);
            con.Open();
            SqlCommand cmd = new SqlCommand("InlineQcLogin", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@EmplID", ten);
            cmd.Parameters.Add("@Pass", mk);
            cmd.ExecuteNonQuery();
            SqlDataAdapter MReader = new SqlDataAdapter { SelectCommand = cmd };

            DataTable dt = new DataTable();
            MReader.Fill(dt);
            con.Close();
            return dt;
        }

        private void UpdateVersion()
        {
            Toast.MakeText(this, "Update !!!", ToastLength.Long).Show();

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            SetPermision();

            Update.CheckUpdate(this, false, false, new AlertDialog.Builder(this), LinkUpdateAPP, "ScanOutputEndSWL.apk", CSDL.Language("M00146"), CSDL.Language("M00140"), CSDL.Language("M00140"), CSDL.Language("M00144"), CSDL.Language("M00145"));
        }
        private void UpdateChecker()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt > 23)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
                    {
                        Android.Support.V4.App.ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage }, 1000);
                    }
                }
                if (!isadShow)
                {
                    newVer = false;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LinkUpdateAPP);
                    // If required by the server, set the credentials.
                    //request.Credentials = CredentialCache.DefaultCredentials;
                    //request.IfModifiedSince = DateTime.Parse("01-01-1990");

                    using (WebResponse response = request.GetResponse())
                    {
                        string m_filePath = Android.OS.Environment.ExternalStorageDirectory + "/Download/" + "ScanOutputEndSWL.apk";

                        DateTime dt = System.IO.File.GetCreationTime(m_filePath); //.GetLastWriteTime("/sdcard/Dowload/Inspections.Inspections-Signed.apk");
                        DateTime appDate = DateTime.Parse(response.Headers["Last-Modified"].ToString());
                        //mToast("dt=" + dt.ToString() + " | NewAppDate = " + appDate.ToString(), 5000);
                        if (appDate > dt)
                        {
                            try
                            {
                                newVer = true;
                                ad = new Android.App.AlertDialog.Builder(this).Create();
                                ad.SetTitle(CSDL.Language("M00146"));
                                ad.SetMessage(CSDL.Language("M00140") + "\n \n" + CSDL.Language("M00143") + "\n \n" + "Last-Modified:" + appDate.ToString() + "\n" + "CreationTime:" + dt);
                                ad.SetButton(CSDL.Language("M00144"), delegate
                                {
                                    isadShow = false;
                                    try
                                    {
                                        //System.IO.File.Delete(Android.OS.Environment.DirectoryDownloads + "/SEWTVDisplay.SEWTVDisplay-Signed.apk");//To Delete old file...
                                        //DownloadFile(LinkUpdateAPP, Android.OS.Environment.ExternalStorageDirectory + "/Download/" + "TTPefmTVIndivSewL.apk"); // To download new file
                                        var webClient = new WebClient();
                                        var url = new System.Uri(LinkUpdateAPP);

                                        webClient.OpenRead(url);
                                        TTDL = Convert.ToInt64(webClient.ResponseHeaders["Content-Length"]);

                                        webClient.DownloadFileAsync(url, m_filePath);

                                        webClient.DownloadProgressChanged += (sender, e) =>
                                        {
                                            TTRC = e.BytesReceived;
                                            PercentComplete = (int)(TTRC * 100 / TTDL);
                                            if (Progr == 0) ProgressCalling();
                                        };

                                        webClient.DownloadFileCompleted += async (s, e) =>
                                        {
                                            //File apkFile = new File(m_filePath);
                                            //Android.Net.Uri apkUri = Android.Net.Uri.FromFile(apkFile);
                                            //Intent webIntent = new Intent(Intent.ActionInstallPackage);//Intent.ACTION_VIEW
                                            //webIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
                                            //webIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.GrantPersistableUriPermission);
                                            //webIntent.PutExtra(Intent.ExtraNotUnknownSource, true);
                                            //Application.Context.StartActivity(webIntent);
                                            //Finish();

                                            await Launcher.OpenAsync(new OpenFileRequest
                                            {
                                                File = new ReadOnlyFile(m_filePath)

                                            });
                                        };
                                    }
                                    catch (ActivityNotFoundException ex)
                                    {
                                        ad = new Android.App.AlertDialog.Builder(this).Create();
                                        ad.SetTitle("WebClient()");
                                        ad.SetMessage(CSDL.Language("M00019") + ex);
                                        ad.SetCanceledOnTouchOutside(true);
                                        ad.Show();
                                    }
                                });
                                ad.SetButton2(CSDL.Language("M00145"), delegate
                                {
                                    isadShow = false;
                                    return;
                                });
                                ad.SetCanceledOnTouchOutside(true);
                                ad.Show();
                                isadShow = true;
                            }
                            catch (System.Exception ex)
                            {
                                isadShow = false;
                                ad = new Android.App.AlertDialog.Builder(this).Create();
                                ad.SetTitle("AlertDialogDownloadFail");
                                ad.SetMessage(ex.Message);
                                ad.SetCanceledOnTouchOutside(true);
                                ad.Show();
                            }
                        }
                        //else
                        //{
                        //    try
                        //    {
                        //        string file = Android.OS.Environment.ExternalStorageDirectory + "/download/" + "ScanOutputEndSWL.apk";

                        //        //PackageManager manager = PackageManager;
                        //        //PackageInfo info = manager.GetPackageInfo(PackageName, 0);
                        //        //PackageInfo info1 = manager.GetPackageArchiveInfo(file, 0);

                        //        newVer = true;
                        //        ad = new Android.App.AlertDialog.Builder(this).Create();

                        //        ad.SetMessage(CSDL.Language("M00146"));
                        //        ad.SetButton(CSDL.Language("M00141"), async (s, a) =>
                        //        {
                        //            await Launcher.OpenAsync(new OpenFileRequest
                        //            {
                        //                File = new ReadOnlyFile(file)

                        //            });
                        //        });

                        //        ad.SetCanceledOnTouchOutside(true);
                        //        ad.Show();
                        //        isadShow = true;

                        //        //if (info1.VersionCode > info.VersionCode)
                        //        //{
                        //        //    newVer = true;
                        //        //    ad = new Android.App.AlertDialog.Builder(this).Create();

                        //        //    ad.SetMessage(CSDL.Language("M00146"));
                        //        //    ad.SetButton(CSDL.Language("M00141"), (s, a) =>
                        //        //    {
                        //        //        Java.IO.File apkFile = new Java.IO.File(file);
                        //        //        Android.Net.Uri uri = Android.Net.Uri.FromFile(apkFile);//Android.Net.Uri.FromFile(apkFile);

                        //        //        Intent webIntent = new Intent(Intent.ActionInstallPackage);//Intent.ACTION_VIEW
                        //        //        webIntent.SetDataAndType(uri, "application/vnd.android.package-archive");
                        //        //        webIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.GrantPersistableUriPermission);
                        //        //        webIntent.PutExtra(Intent.ExtraNotUnknownSource, true);
                        //        //        Application.Context.StartActivity(webIntent);
                        //        //    });

                        //        //    ad.SetCanceledOnTouchOutside(true);
                        //        //    ad.Show();
                        //        //    isadShow = true;
                        //        //}
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Toast.MakeText(this, "Check Version : " + ex.ToString(), ToastLength.Long).Show();
                        //    }
                        //}
                    }
                }
            }
            catch (Exception ext)
            {
                ad = new Android.App.AlertDialog.Builder(this).Create();
                ad.SetTitle("UpdateChecker() - WebResponse ?");
                ad.SetMessage(ext.Message);
                ad.SetCanceledOnTouchOutside(true);
                ad.Show();
            }
        }
        private void SetPermision()
        {
            Permission read = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage);
            Permission write = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage);

            if (read != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage, }, 0);
            if (write != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage, }, 0);
        }
        private async void ProgressCalling()
        {
            Progr++;
            using (IProgressDialog pros = UserDialogs.Instance.Progress("Downloading", null, null, true, MaskType.Black))
            {
                do
                {
                    pros.PercentComplete = PercentComplete;
                    await Task.Delay(500);
                } while (PercentComplete < 100);
            }
            Progr = 0;
        }

        private void HandleScanResultLogin(ZXing.Result result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                try
                {
                    //Vibrator vib = (Vibrator)GetSystemService(VibratorService);
                    //VibrationEffect effect = VibrationEffect.CreateOneShot(50, VibrationEffect.DefaultAmplitude);
                    //vib.Vibrate(effect);
                    //vib.Vibrate(50); 

                    id.Text = "";
                    id.Text = result.Text;
                    //if (new List<string> { "A1A", "TAC", "TRR", "AOI" }.Contains(id.Text.Substring(0, 3)))
                    {
                        DataTable dtUserdetail = LoginAccessing(id.Text, "");
                        if (dtUserdetail.Rows.Count == 1)
                        {
                            LoginSuccess(dtUserdetail);
                        }
                        else
                        {
                            Toast.MakeText(this, CSDL.Language("M00012"), ToastLength.Short).Show();
                            id.Text = "";
                        }
                    }
                    //else
                    //{
                    //    Toast.MakeText(this, CSDL.Language("M00016"), ToastLength.Short).Show();
                    //}
                }
                catch (Exception ex)
                {
                    CSDL.LogInCheck = false;
                    t.Text = ex.ToString();
                    t.Visibility = ViewStates.Visible;
                    Toast.MakeText(this, CSDL.Language("M00017"), ToastLength.Short).Show();
                    id.Text = "";
                }
            }
        }

        private void HandleScanResultSelectLine(ZXing.Result result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                try
                {
                    id.Text = "";
                    id.Text = result.Text;
                    if (id.Text.Substring(0, 2) == strSelectedFac)
                    {
                        CSDL.SelectedLine = id.Text;
                        Toast.MakeText(this, CSDL.SelectedLine.ToString(), ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(this, CSDL.Language("M00012"), ToastLength.Short).Show();
                    }
                }
                catch (Exception ex)
                {
                    CSDL.LogInCheck = false;
                    t.Text = ex.ToString();
                    t.Visibility = ViewStates.Visible;
                    Toast.MakeText(this, CSDL.Language("M00017"), ToastLength.Short).Show();
                    id.Text = "";
                }
            }
        }
    }
    class GetData
    {
        public string Country_Name { get; set; }
        public string Country_Code { get; set; }
        public string City { get; set; }
        public string IP { get; set; }
    }
}