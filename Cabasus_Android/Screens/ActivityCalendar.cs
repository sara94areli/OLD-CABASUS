using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Gigamole.Infinitecycleviewpager;
using Square.TimesSquare;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ActivityCalendar : Activity
    {
        List<int> lstImages = new List<int>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_calendario);
            Window.SetStatusBarColor(Color.Black);
            
            new ShareInside().GuardarPantallaActual("ActivityCalendar");
           var transaccion = FragmentManager.BeginTransaction();
            transaccion.Add(Resource.Id.fragmenthome, new FragmentCampana(), "campana");
            transaccion.Commit();

            var Recibirnotificacion = new RecibirNotificacion(this);
            RegisterReceiver(Recibirnotificacion, new IntentFilter("Campana"));


            #region Referencias de los botones;

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            var btnnotificaciones = FindViewById<RelativeLayout>(Resource.Id.notificacion);

            var calendario = FindViewById<CalendarPickerView>(Resource.Id.calendar_view);
            //calendario.Visibility = ViewStates.Invisible;
            var nextYear = DateTime.Now.AddYears(13);
            var agoYears = new DateTime(2000, 01, 01);
            calendario.Init(agoYears, nextYear).WithSelectedDate(DateTime.Now);

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
            calendario.DateSelected += (s, e) => {
                Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                intent.PutExtra("Date", e.Date.ToString("yyyy-MM-dd"));
                this.StartActivity(intent);
                Finish();
            };
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
        public override void OnBackPressed()
        {
            StartActivity(typeof(ActivityHome));
            Finish();
        }
    }
}