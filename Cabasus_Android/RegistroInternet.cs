using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Widget;
using Cabasus_Android.Screens;
using SQLite;

namespace Cabasus_Android
{
    [BroadcastReceiver (Name = "com.cabasus.myapp.Cabasus_Android.RegistroInternet",Enabled =true)]
    [IntentFilter(new string[] { "android.net.conn.CONNECTIVITY_CHANGE" })]
    public class RegistroInternet : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var service = context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;
            var networkinfo = service.ActiveNetworkInfo;
            if (networkinfo != null)
            {
                //Toast.MakeText(context, "Internet", ToastLength.Short).Show();
                var Pantalla = new ShareInside().ConsultarPantallaActual();
                if (Pantalla == "ActivityActivity")
                {
                    context.StartActivity(typeof(ActivityActivity));
                }
                else if (Pantalla == "ActivityHome")
                {
                    context.StartActivity(typeof(ActivityHome));
                }
                else if (Pantalla == "ActivitySettings")
                {
                    context.StartActivity(typeof(ActivitySettings));
                }
                else if (Pantalla == "ActivityDiary")
                {
                    context.StartActivity(typeof(ActivityDiary));

                }
                else if (Pantalla == "ActivityLogin")
                {

                }
            }
            else {
                // Toast.MakeText(context, "No Internet", ToastLength.Short).Show();
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);
                if (consulta.Count <= 0)
                {
                    context.StartActivity(typeof(ActivitySettings));
                }
                else
                {
                    var Pantalla = new ShareInside().ConsultarPantallaActual();
                    if (Pantalla == "ActivityActivity")
                    {
                        context.StartActivity(typeof(ActivityActivity));
                    }
                    else if (Pantalla == "ActivityHome")
                    {
                        context.StartActivity(typeof(ActivityHome));
                    }
                    else if (Pantalla == "ActivitySettings")
                    {
                        context.StartActivity(typeof(ActivitySettings));
                    }
                    else if (Pantalla== "ActivityDiary")
                    {
                        context.StartActivity(typeof(ActivityDiary));

                    }
                    else if (Pantalla == "ActivityLogin")
                    {

                    }
                }
            }
        }
    }
}