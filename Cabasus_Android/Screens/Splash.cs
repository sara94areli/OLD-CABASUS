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
using Android.Views.Animations;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;
using Plugin.Connectivity;
using Android.Content.PM;
using System.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Util;
using Android.Graphics;

namespace Cabasus_Android.Screens
{
    [Activity(MainLauncher = true, NoHistory = true)]
    public class Splash : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);

            var datos=Resources.Configuration.Locale = Java.Util.Locale.Default;

            if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdiomaApp.xml")))
            {
                
            }
            else
            {
                new ShareInside().GuardarIdioma(datos.Language, datos.Country);
            }

            if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DatosDeLogeo.xml")))
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    StartActivity(typeof(ActivityHome));
                    MyFirebaseIIDService mfi = new MyFirebaseIIDService();


                }
                else
                {
                    StartActivity(typeof(ActivitySettings));
                    MyFirebaseIIDService mfi = new MyFirebaseIIDService();

                }
            }
            else
            {
                ActivityLogin viewActivity = new ActivityLogin();
                CountDown countDown = new CountDown(3000, 3000, this, viewActivity);
                countDown.Start();
                StartAnim();
            }
        }
        public void StartAnim()
        {
            Animation anim = AnimationUtils.LoadAnimation(this, Resource.Animation.alpha);
            anim.Reset();

            var lnMain = FindViewById<LinearLayout>(Resource.Id.lnMain);
            lnMain.ClearAnimation();
            lnMain.StartAnimation(anim);

            anim = AnimationUtils.LoadAnimation(this, Resource.Animation.translate);
            anim.Reset();

            var imgMain = FindViewById<ImageView>(Resource.Id.imageView1);
            imgMain.ClearAnimation();
            imgMain.StartAnimation(anim);
        }

        

        protected override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
                PackageManager package = this.PackageManager;
                package.SetComponentEnabledSetting(component, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                var Recibirnotificacion = new RecibirNotificacion(this);
                UnregisterReceiver(Recibirnotificacion);
                new ShareInside().GuardarPantallaActual("Cerrado");


            }
            catch
            {

            }

        }
        protected override void OnRestart()
        {
            base.OnRestart();
            ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
            PackageManager package = this.PackageManager;
            package.SetComponentEnabledSetting(component, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
        }
        protected override void OnResume()
        {
            base.OnResume();
            ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
            PackageManager package = this.PackageManager;
            package.SetComponentEnabledSetting(component, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
        }
        protected override void OnPause()
        {
            try
            {
                base.OnStop();

                ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
                PackageManager package = this.PackageManager;
                package.SetComponentEnabledSetting(component, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                RegistroInternet ri = new RegistroInternet();
                UnregisterReceiver(ri);

            }
            catch
            {

            }
        }
    }
}