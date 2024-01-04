using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CSDL;
using Google.Android.Material.Snackbar;

namespace EndlineOutputScan
{
    class CSDL
    {
        public static string chuoi = "";
        public static string chuoi3 = @"Data Source=192.168.54.8;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=Admin@168*;Connect Timeout=30;Encrypt=False;";
        public static string chuoi2 = "Data Source=192.168.50.253;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=Sql4116!;Connect Timeout=10;Encrypt=False;";
        public static string chuoi1 = "Data Source=192.168.1.245;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=prog2;Password=iop;Connect Timeout=10;Encrypt=False;";
        public static string user = "";
        public static string mk = "", error = "", lvMessage = "";
        public static string username = "";
        public static string SelectedLine = "", UPCString = "", SelectedUPCPO = "", SelectedGmtType = "", SelectedColor = "", SelectedShipNo = "",
                            strTimeTitle = "", version = "V4.6", OpenTime = DateTime.Now.ToString("yyMMdd HH:mm");
        public static DateTime currDatetime;
        public static int i = 0, SelectedUPCPOQty = 0, updateTime = 10;
        public static DataTable dtUPCDetail = new DataTable();
        public static DataRow[] drSelectedUPC;
        public static bool CompIns, LogInCheck = false, blShPO = true, blShJobNo = false, blShSizeId = false, CheckQtyAcum = true;
        public static List<int> TimeArray = new List<int> { 0730, 0830, 0930, 1030, 1130, 1330, 1430, 1530, 1630, 1730, 1830, 1930, 2030, 2130, 2230 };
        public static List<ShCTN_ClassAdapterPOnCTN> ltLang = new List<ShCTN_ClassAdapterPOnCTN>();
        public static Connect Cnnt = new Connect(chuoi1);

        public static int width = 0, height = 0, LangRef = 0, intTmStop = 20; //timerStop 5*16
        public static double TextRatio = 1, SizingScrRt = 0, density = 0, SizingScrRtH = 0;

        public SqlConnection con;
        public static bool Olig = false, checkIP = true;
        public static List<IPSERVER> IPSERVER = new List<IPSERVER>
        {
            new IPSERVER
            {
                IP = "203.172.56.66",
                Server = "Data Source=192.168.50.253;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=Sql4116!;Connect Timeout=30;Encrypt=False;"
            },
            new IPSERVER
            {
                IP = "103.216.48.174",
                Server = "Data Source=192.168.54.8;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=Admin@168*;Connect Timeout=30;Encrypt=False;"
            },
            new IPSERVER
            {
                IP = "123.30.96.58",
                Server = "Data Source=192.168.1.245;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=007;Connect Timeout=30;Encrypt=False;"
            }
        };

        public CSDL()
        {
            con = new SqlConnection(chuoi);
        }
        public static void HideKeyboard(Context c, View v)
        {
            InputMethodManager h = (InputMethodManager)c.GetSystemService(Context.InputMethodService);
            View myview = v;
            if (myview != null)
            {
                h.HideSoftInputFromWindow(myview.WindowToken, HideSoftInputFlags.None);
            }
        }
        public int Kiemtra(string c)
        {
            int i = 0;
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(c, con);
                SqlDataReader read = cmd.ExecuteReader();
                if (read.Read() == true)
                    i = 1;
                else
                    i = 0;
                con.Close();
            }
            catch (Exception ex)
            {
                i = 2;
                error = ex.ToString();
            }
            return i;
        }

        public DataTable Doc(string c)
        {
            con.Open();
            SqlDataAdapter doc = new SqlDataAdapter(c, con);
            DataTable dt = new DataTable();
            doc.Fill(dt);
            con.Close();
            return dt;
        }

        public void Ghi(string c)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand(c, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static string Language(string c)
        {
            try { return CSDL.ltLang[CSDL.ltLang.FindIndex(x => x.ShCTN_ClPOorCTNCode == c)].ShCTN_ClQtyorCTNNo; }
            catch { return "error"; }
        }
        public static void ScreenStretching(int ScaleParent, ViewGroup viewGroup)
        {
            if (ScaleParent == 1)
            {
                ViewGroup.MarginLayoutParams mlPara;
                try
                {
                    mlPara = (ViewGroup.MarginLayoutParams)viewGroup.LayoutParameters;
                    mlPara.SetMargins((int)(CSDL.SizingScrRt * mlPara.LeftMargin), (int)(CSDL.SizingScrRtH * mlPara.TopMargin), (int)(CSDL.SizingScrRt * mlPara.RightMargin), (int)(CSDL.SizingScrRtH * mlPara.BottomMargin));
                }
                catch
                {
                    ScaleParent = 0;
                }
                finally
                {
                    if (ScaleParent == 1 && viewGroup.LayoutParameters.Height > 0 && viewGroup.LayoutParameters.Width > 0)
                    {
                        mlPara = (ViewGroup.MarginLayoutParams)viewGroup.LayoutParameters;
                        mlPara.SetMargins((int)(CSDL.SizingScrRt * mlPara.LeftMargin), (int)(CSDL.SizingScrRtH * mlPara.TopMargin), (int)(CSDL.SizingScrRt * mlPara.RightMargin), (int)(CSDL.SizingScrRtH * mlPara.BottomMargin));
                        viewGroup.LayoutParameters = mlPara;
                        viewGroup.LayoutParameters.Width = (int)(CSDL.SizingScrRt * mlPara.Width);
                        viewGroup.LayoutParameters.Height = (int)(CSDL.SizingScrRtH * mlPara.Height);
                    }
                };
            }
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                View childView = viewGroup.GetChildAt(i);
                Type tp = childView.GetType();
                lvMessage += i.ToString() + ":" + Application.Context.Resources.GetResourceName(childView.Id).Split('/')[1] + " | " + childView.LayoutParameters.Width.ToString() + "x" + childView.LayoutParameters.Height.ToString() + "\n";

                ViewGroup.MarginLayoutParams rlLayoutTv = (ViewGroup.MarginLayoutParams)childView.LayoutParameters;

                rlLayoutTv.SetMargins((int)(CSDL.SizingScrRt * rlLayoutTv.LeftMargin), (int)(CSDL.SizingScrRt * rlLayoutTv.TopMargin), (int)(CSDL.SizingScrRt * rlLayoutTv.RightMargin), (int)(CSDL.SizingScrRt * rlLayoutTv.BottomMargin));
                childView.LayoutParameters = rlLayoutTv;

                int Owid = childView.LayoutParameters.Width;
                int Ohei = childView.LayoutParameters.Height;

                childView.LayoutParameters.Width = (int)(CSDL.SizingScrRt * rlLayoutTv.Width);
                childView.LayoutParameters.Height = (int)(CSDL.SizingScrRtH * rlLayoutTv.Height);

                int Nwid = childView.LayoutParameters.Width;
                int Nhei = childView.LayoutParameters.Height;

                try
                {
                    if (((ViewGroup)childView).ChildCount > 0)
                    {
                        ViewGroup vv = childView as ViewGroup;
                        ScreenStretching(0, vv);
                    }
                }
                catch
                {
                    if (tp.ToString().Contains("EditText"))
                    {
                        EditText mojt = childView as EditText;
                        float texS = mojt.TextSize;
                        mojt.TextSize = (float)((Nwid * texS / Owid) * 0.81 / CSDL.density);
                        lvMessage += mojt.TextSize.ToString() + "\n";
                        //if (SizingScrRt < 1) mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio * 0.6);
                        //else mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio);
                    }
                    else if (tp.ToString().Contains("TextView"))
                    {
                        TextView mojt = childView as TextView;
                        float texS = mojt.TextSize;
                        mojt.TextSize = (float)((Nwid * texS / Owid) * CSDL.TextRatio / CSDL.density);
                        lvMessage += mojt.TextSize.ToString() + "\n";
                        //mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio);
                    }
                    else if (tp.ToString().Contains("CheckBox"))
                    {
                        CheckBox mojt = childView as CheckBox;
                        float texS = mojt.TextSize;
                        mojt.TextSize = (float)((Nwid * texS / Owid) * CSDL.TextRatio / CSDL.density);
                        lvMessage += mojt.TextSize.ToString() + "\n";

                        int ORLm = ((ViewGroup.MarginLayoutParams)mojt.LayoutParameters).LeftMargin;
                        mojt.ScaleX = ((float)Nwid / Owid + 1) / 2;
                        mojt.ScaleY = ((float)Nhei / Ohei + 1) / 2;
                        ((ViewGroup.MarginLayoutParams)mojt.LayoutParameters).LeftMargin = (int)(ORLm - (1 - mojt.ScaleX) * mojt.LayoutParameters.Width / 2);
                    }
                    else if (tp.ToString().Contains("RadioButton"))
                    {
                        RadioButton mojt = childView as RadioButton;
                        float texS = mojt.TextSize;
                        mojt.TextSize = (float)((Nwid * texS / Owid) * CSDL.TextRatio / CSDL.density);
                        lvMessage += mojt.TextSize.ToString() + "\n";

                        int ORLm = ((ViewGroup.MarginLayoutParams)mojt.LayoutParameters).LeftMargin;
                        mojt.ScaleX = ((float)Nwid / Owid + 1) / 2;
                        mojt.ScaleY = ((float)Nhei / Ohei + 1) / 2;
                        ((ViewGroup.MarginLayoutParams)mojt.LayoutParameters).LeftMargin = (int)(ORLm - (1 - mojt.ScaleX) * mojt.LayoutParameters.Width / 2);
                    }
                    else if (tp.ToString().Contains("Button"))
                    {
                        Button mojt = childView as Button;
                        float texS = mojt.TextSize;
                        mojt.TextSize = (float)((Nwid * texS / Owid) * CSDL.TextRatio / CSDL.density);
                        lvMessage += mojt.TextSize.ToString() + "\n";
                    }
                    //continue;
                }

            }
        }
        public static void ShowMessage(View view, string msg, int time = 5000)
        {
            var sn = Snackbar.Make(view, msg, time);
            sn.Show();
        }

        //public static void ScreenStretching(Context t, ViewGroup viewGroup)
        //{
        //    //var viewGroup = (ViewGroup)FindViewById<RelativeLayout>(Resource.Id.rlMnPlanLoadLayout);
        //    for (int i = 0; i < viewGroup.ChildCount; i++)
        //    {
        //        var childView = viewGroup.GetChildAt(i);
        //        RelativeLayout.LayoutParams rlLayoutTv = (RelativeLayout.LayoutParams)childView.LayoutParameters;
        //        rlLayoutTv.SetMargins((int)(CSDL.SizingScrRt * rlLayoutTv.LeftMargin), (int)(CSDL.SizingScrRtH * rlLayoutTv.TopMargin), (int)(CSDL.SizingScrRt * rlLayoutTv.RightMargin), (int)(CSDL.SizingScrRtH * rlLayoutTv.BottomMargin));
        //        childView.LayoutParameters = rlLayoutTv;
        //        childView.LayoutParameters.Width = (int)(CSDL.SizingScrRt * rlLayoutTv.Width);
        //        childView.LayoutParameters.Height = (int)(CSDL.SizingScrRtH * rlLayoutTv.Height);

        //        Type tp = childView.GetType();

        //        if (tp.ToString().Contains("EditText"))
        //        {
        //            EditText mojt = childView as EditText;
        //            if (SizingScrRt < 1) mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio * 0.6);
        //            else mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio); //
        //        }
        //        else if (tp.ToString().Contains("TextView"))
        //        {
        //            TextView mojt = childView as TextView;
        //            mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio);
        //        }
        //        else if (tp.ToString().Contains("CheckBox"))
        //        {
        //            CheckBox mojt = childView as CheckBox;
        //            mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio);
        //        }
        //        else if (tp.ToString().Contains("RadioButton"))
        //        {
        //            RadioButton mojt = childView as RadioButton;
        //            mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio);
        //        }
        //        else if (tp.ToString().Contains("Button"))
        //        {
        //            Button mojt = childView as Button;
        //            mojt.TextSize = (float)(mojt.TextSize * SizingScrRt * CSDL.TextRatio);
        //        }
        //    }
        //}


    }
    class IPSERVER
    {
        public string IP { get; set; }
        public string Server { get; set; }
    }
}