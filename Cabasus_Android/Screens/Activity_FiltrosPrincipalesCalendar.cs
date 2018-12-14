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
using Android.Support.V7.App;
using Android.Support.V4.View;
using Android.Support.V4;
using com.refractored;
using Java.Lang;
using Android.Support.V4.App;
using Android.Graphics.Drawables;
using Android.Animation;
using Android.Graphics;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;
using Plugin.Connectivity;
using SQLite;
using Android.Content.PM;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Activity_FiltrosPrincipalesCalendar : AppCompatActivity
    {
        MyAdapterFp adapter;
        PagerSlidingTabStrip tabStrip;
        ViewPager viewPager;

        private int MX, MY;
        int contador = 0;
        private RelativeLayout btnAddNew;
        private float ScreenWith = 0;
        private float ScrenHeight = 0;
        private ViewGroup FondoBotonFiltrosFlotante;
        bool BanderaPosicionDelBoton = false;
        string Date;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.Layout_FiltrosPrincipalesCalendar);
            new ShareInside().AlarmNotifiation(this, 20000);
            Window.SetStatusBarColor(Color.Black);

            new ShareInside().GuardarPantallaActual("ActivityFiltros");

            #region ProgressBar
            ProgressBar progressBar;
            progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
            p.AddRule(LayoutRules.CenterInParent);
            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

            FindViewById<RelativeLayout>(Resource.Id.FondoBotonFiltros).AddView(progressBar, p);

            progressBar.Visibility = Android.Views.ViewStates.Visible;
            Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
            #endregion
            Date = this.Intent.GetStringExtra("Date");
            FindViewById<TextView>(Resource.Id.txtfechaFiltros).Text = Date;
            Adaptador();
            #region Referencias de los botones;

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);

            FondoBotonFiltrosFlotante = FindViewById<RelativeLayout>(Resource.Id.FondoBotonFiltros);
            btnAddNew = FondoBotonFiltrosFlotante.FindViewById<RelativeLayout>(Resource.Id.btnAddNew);

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

            #endregion;

            Android.App.FragmentTransaction transaccioncarrusel = FragmentManager.BeginTransaction();
            transaccioncarrusel.Add(Resource.Id.fragmentcarrucel, new FragmentCycle(LayoutInflater, Date), "cycle");
            transaccioncarrusel.Commit();

            #region BotonFlotante
            //btnAddNew.SetOnTouchListener(this);
            var metrics = Resources.DisplayMetrics;
            ScreenWith = metrics.WidthPixels;
            ScrenHeight = metrics.HeightPixels;
            btnAddNew.SetX(30);
            btnAddNew.SetY(ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));

            btnAddNew.Click += delegate {
                ////Relative negro para eliminar el menu
                //RelativeLayout General = new RelativeLayout(this);
                //General.LayoutParameters = new RelativeLayout.LayoutParams(-1, -1);
                //General.SetBackgroundColor(Android.Graphics.Color.Argb(127, 0, 0, 0));
                //FindViewById<RelativeLayout>(Resource.Id.FondoBotonFiltros).AddView(General);

                ////Linear que contiene las opciones del menu
                //LinearLayout OptInsNew = new LinearLayout(this);
                //OptInsNew.SetBackgroundColor(Android.Graphics.Color.White);
                //OptInsNew.Orientation = Orientation.Vertical;
                //var c = new ShareInside();
                //LinearLayout.LayoutParams par = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, int.Parse(c.ConvertDPToPixels(60, Resources.DisplayMetrics.Density).ToString()));
                //OptInsNew.LayoutParameters = par;

                ////decidir donde colocar el menu
                //OptInsNew.SetX(btnAddNew.GetX() + int.Parse(c.ConvertDPToPixels(30, Resources.DisplayMetrics.Density).ToString()));
                //OptInsNew.SetY(btnAddNew.GetY() - int.Parse(c.ConvertDPToPixels(30, Resources.DisplayMetrics.Density).ToString()));

                ////agregar las opciones al linear                           
                //TextView btnNuevoRecordatorio = new TextView(this);
                //btnNuevoRecordatorio.LayoutParameters = new LinearLayout.LayoutParams(-1, 0, 1);
                //btnNuevoRecordatorio.Text = GetText(Resource.String.New_Reminder);
                //btnNuevoRecordatorio.SetTextColor(Android.Graphics.Color.Black);
                //btnNuevoRecordatorio.Gravity = GravityFlags.Center;
                //btnNuevoRecordatorio.TextSize = c.ConvertDPToPixels(6, Resources.DisplayMetrics.Density);

                //TextView btnNuevaJornada = new TextView(this);
                //btnNuevaJornada.LayoutParameters = new LinearLayout.LayoutParams(-1, 0, 1);
                //btnNuevaJornada.Text = GetText(Resource.String.New_Journal);
                //btnNuevaJornada.SetTextColor(Android.Graphics.Color.Black);
                //btnNuevaJornada.Gravity = GravityFlags.Center;
                //btnNuevaJornada.TextSize = c.ConvertDPToPixels(6, Resources.DisplayMetrics.Density);

                //OptInsNew.AddView(btnNuevoRecordatorio);
                //OptInsNew.AddView(btnNuevaJornada);

                //General.AddView(OptInsNew);

                //btnNuevoRecordatorio.Click += delegate
                //{
                //    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                //    var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);

                //    bool bandera = false;

                //    foreach (var item in consulta)
                //    {
                //        if (item.id == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected)
                //        {
                //            bandera = true;
                //            break;
                //        }
                //    }

                //    if (bandera)
                //    {
                //        Intent intent = new Intent(this, (typeof(ActivityNuevoRecordatorio)));
                //        intent.PutExtra("Date", Date);
                //        this.StartActivity(intent);
                //        Finish();
                //    }
                //    else
                //    {
                //        Toast.MakeText(this, Resource.String.You_cant_make_reminders, ToastLength.Short).Show();
                //    }

                //};

                //btnNuevaJornada.Click += delegate
                //{
                //    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                //    var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);

                //    bool bandera = false;

                //    foreach (var item in consulta)
                //    {
                //        if (item.id == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected)
                //        {
                //            bandera = true;
                //            break;
                //        }
                //    }

                //    if (bandera)
                //    {
                //        Intent intent = new Intent(this, (typeof(ActivityNuecaJornada)));
                //        intent.PutExtra("Date", Date);
                //        this.StartActivity(intent);
                //        Finish();
                //    }
                //    else
                //    {
                //        Toast.MakeText(this, Resource.String.You_cant_make_journals, ToastLength.Short).Show();
                //    }
                //};

                //General.Click += delegate
                //{
                //    FindViewById<RelativeLayout>(Resource.Id.FondoBotonFiltros).RemoveView(General);
                //};

                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);

                bool bandera = false;

                foreach (var item in consulta)
                {
                    if (item.id == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected)
                    {
                        bandera = true;
                        break;
                    }
                }

                if (bandera)
                {
                    Intent intent = new Intent(this, (typeof(ActivityNuevoRecordatorio)));
                    intent.PutExtra("Date", Date);
                    this.StartActivity(intent);
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, Resource.String.You_cant_make_reminders, ToastLength.Short).Show();
                }
            };
            #endregion

            progressBar.Visibility = Android.Views.ViewStates.Invisible;
            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

            new ShareInside().AlarmNotifiation(this, 20000);
            
        }
        
        public void Adaptador()
        {
            adapter = new MyAdapterFp(SupportFragmentManager, this, Date);
            viewPager = FindViewById<ViewPager>(Resource.Id.pager_Fp);
            tabStrip = FindViewById<PagerSlidingTabStrip>(Resource.Id.tabs_Fp);

            viewPager.Adapter = adapter;
            tabStrip.SetViewPager(viewPager);
            tabStrip.SetBackgroundColor(Android.Graphics.Color.Black);
        }

        public int CurrentTimeMillis()
        {
            return contador++;
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