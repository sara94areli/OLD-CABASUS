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
using Com.Gigamole.Infinitecycleviewpager;
using Android.Graphics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Json;
using Newtonsoft.Json;
using Android.Util;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using Plugin.Connectivity;
using SQLite;
using Android.Net;
using Android.Support.V4.View;
using Android.Content.PM;
using Android.Support.Design.Widget;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, NoHistory =true)]
    public class ActivityHome : Activity
    {
        FragmentTransaction transaccion,transaccioncarrusel, transaccionHome;

        private const string RSS_link = "https://cabasus-mobile.azurewebsites.net/web/rss";
        private const string RSS_to_json = "https://api.rss2json.com/v1/api.json?rss_url=";
        LinearLayout ContentHotTips;
        ViewPager pagerContentRSS;
        TabLayout tabLayoutRSS;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            new ShareInside().AlarmNotifiation(this, 20000);
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_home);
            
            transaccion = FragmentManager.BeginTransaction();
            transaccion.Add(Resource.Id.fragmenthome, new FragmentCampana(),"campana");
            transaccion.Commit();

            transaccioncarrusel = FragmentManager.BeginTransaction();
            transaccioncarrusel.Add(Resource.Id.fragmentcarrucel, new FragmentCycle(LayoutInflater, null), "cycle");
            transaccioncarrusel.Commit();
            
            var Recibirnotificacion = new RecibirNotificacion(this);
            RegisterReceiver(Recibirnotificacion, new IntentFilter("Campana"));

            new ShareInside().GuardarPantallaActual("ActivityHome");
            Window.SetStatusBarColor(Color.Black);
            
            #region Referencias de los botones;

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            var btnnotificaciones = FindViewById<RelativeLayout>(Resource.Id.notificacion);

            ContentHotTips = FindViewById<LinearLayout>(Resource.Id.ContentHotTips);
            pagerContentRSS = FindViewById<ViewPager>(Resource.Id.pagerContentRSS);
            tabLayoutRSS = FindViewById<TabLayout>(Resource.Id.tab_layout_rss);
            
            #endregion;

            #region Abrir las diferentes pantalla con los diferentes botones;

            btnhome.Click += delegate {
                StartActivity(typeof(ActivityHome));
                Finish();
            };

            btnactivity.Click += delegate {
                StartActivity(typeof(ActivityActivity));
                Finish();
            };

            btndiary.Click += delegate {
                StartActivity(typeof(ActivityDiary));
                Finish();
            };

            btncalendar.Click += delegate {
                StartActivity(typeof(ActivityCalendar));
                Finish();
            };

            btn_settings.Click += delegate {
                StartActivity(typeof(ActivitySettings));
                Finish();
            };

            btnnotificaciones.Click += delegate
            {

                StartActivity(typeof(ActivityNotificaciones));
                Finish();
            };
            #endregion;
            
            var ImagenUsuario = FindViewById<ImageView>(Resource.Id.ImgUsuarioHome);
            try
            {
                byte[] imageAsBytes = Base64.Decode((new ShareInside().ConsultarDatosUsuario()[0].photo.data), Base64Flags.Default);
                var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                ImagenUsuario.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));

            }
            catch (Exception)
            {
                ImagenUsuario.SetImageResource(Resource.Drawable.Foto);
            }
            var NombreUsuario = FindViewById<TextView>(Resource.Id.lblUsuarioHome);
            NombreUsuario.Text = new ShareInside().ConsultarDatosUsuario()[0].username;
            
            loadData(savedInstanceState);

            transaccionHome = FragmentManager.BeginTransaction();
            transaccionHome.Add(Resource.Id.FragmentHome, new ClassFragmentHome(LayoutInflater), "Home");
            transaccionHome.Commit();
            
        }

        private void loadData(Bundle savedInstanceState)
        {
            bool internet = false;

            var intent = new Intent(this, typeof(ServicioForegroudSincronizar));
            if (savedInstanceState != null)
            {
                intent.PutExtras(savedInstanceState);
            }


            if (CrossConnectivity.Current.IsConnected)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    StartForegroundService(intent);
                }
                else
                {
                    StartService(intent);
                }

                internet = true;
            }

            StringBuilder stBuilder = new StringBuilder(RSS_to_json);
            stBuilder.Append(RSS_link);

            new LoadDataAsync(this, tabLayoutRSS, pagerContentRSS, internet, LayoutInflater, ContentHotTips).Execute(stBuilder.ToString());            
        }
        
        public override void OnBackPressed()
        {
            StartActivity(typeof(ActivityHome));
            Finish();
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
            new ShareInside().GenerarToken();
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

    public class RecibirNotificacion : BroadcastReceiver
    {
        public static Activity activity;
        public RecibirNotificacion(Activity home)
        {
            activity = home;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                activity = (Activity)context;

                if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings" || new ShareInside().ConsultarPantallaActual() == "ActivityCalendar")
                {
                    Fragment fragmento = activity.FragmentManager.FindFragmentByTag("campana");
                    var tran = activity.FragmentManager.BeginTransaction();
                    tran.Detach(fragmento);
                    tran.Attach(fragmento);
                    tran.Commit();
                }
                else
                {
                    Fragment fragment = activity.FragmentManager.FindFragmentByTag("campana");
                    var tra = activity.FragmentManager.BeginTransaction();
                    tra.Detach(fragment);
                    tra.Attach(fragment);
                    tra.Commit();

                    Fragment frag = activity.FragmentManager.FindFragmentByTag("cycle");
                    var fc = activity.FragmentManager.BeginTransaction();
                    fc.Detach(frag);
                    fc.Attach(frag);
                    fc.Commit();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
    
}