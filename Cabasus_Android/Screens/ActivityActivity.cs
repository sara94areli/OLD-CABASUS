using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V4.View;
using com.refractored;
using Android.Graphics.Drawables;
using Com.Gigamole.Infinitecycleviewpager;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;
using Plugin.Connectivity;
using SQLite;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.Graphics;
using Android.Content.PM;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ActivityActivity : AppCompatActivity
    {
        List<string> lstImages = new List<string>();
        List<string> lstNombre= new List<string>();
        List<string> lstId = new List<string>();
        int PaginaSeleccionada = 0;

        MyAdapter adapter;
        PagerSlidingTabStrip tabStrip;
        ViewPager viewPager;

        private LinearLayout centroOriginal;
        private AnimationDrawable animationDrawable;

        FragmentTransaction transaccion, transaccioncarrusel;

        public override void OnBackPressed()
        {
            StartActivity(typeof(ActivityHome));
            Finish();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            new ShareInside().GuardarPantallaActual("ActivityActivity");
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_activity);
            Window.SetStatusBarColor(Color.Black);
            
            transaccion = FragmentManager.BeginTransaction();
            transaccion.Add(Resource.Id.fragmenthome, new FragmentCampana(), "campana");
            transaccion.Commit();

            transaccioncarrusel = FragmentManager.BeginTransaction();
            transaccioncarrusel.Add(Resource.Id.fragmentcarrucel, new FragmentCycle(LayoutInflater, null), "cycle");
            transaccioncarrusel.Commit();

            var Recibirnotificacion = new RecibirNotificacion(this);
            RegisterReceiver(Recibirnotificacion, new IntentFilter("Campana"));

            viewPager = FindViewById<ViewPager>(Resource.Id.pager);
            tabStrip = FindViewById<PagerSlidingTabStrip>(Resource.Id.tabs);

            adapter = new MyAdapter(SupportFragmentManager, this);

            viewPager.Adapter = adapter;
            tabStrip.SetViewPager(viewPager);
            tabStrip.SetBackgroundColor(Android.Graphics.Color.Black);

            viewPager.PageSelected += delegate 
            {
                PaginaSeleccionada = viewPager.CurrentItem;
                new ShareInside().GuardarEstadoParaLaActividad(new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] + "$" + PaginaSeleccionada);                
            };

            try
            {
                if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                    viewPager.SetCurrentItem(0, false);
                else
                    viewPager.SetCurrentItem(1, false);
            }
            catch
            {
                viewPager.SetCurrentItem(0, false);
            }

            centroOriginal = FindViewById<LinearLayout>(Resource.Id.centroOriginal);

            animationDrawable = (AnimationDrawable)centroOriginal.Background;

            animationDrawable.SetEnterFadeDuration(3000);
            animationDrawable.SetExitFadeDuration(3000);

            animationDrawable.Start();

            var iniciarActividad = FindViewById<TextView>(Resource.Id.textView3);
            var soloYo = FindViewById<TextView>(Resource.Id.textView2);
            var actividades = FindViewById<TextView>(Resource.Id.textView);

  
            #region Referencias de los botones;

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            var btnnotificaciones = FindViewById<RelativeLayout>(Resource.Id.notificacion);

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

            #region boton nueva actividad

            centroOriginal.Click += delegate
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);

                bool bandera = false;

                foreach (var item in consulta)
                {
                    if (item.id== new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected)
                    {
                        bandera = true;
                        break;
                    }
                }

                if (bandera)
                {
                    StartActivity(typeof(ActivityRun));
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.You_have_not_yet_downloaded_any_horse), ToastLength.Short).Show();
                }
                
            };

            #endregion;       
            
            #region filtrar solo yo

            try
            {
                var swonly = FindViewById<Switch>(Resource.Id.swonly);
                if (!File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoParaLaActividad.xml")))
                {
                    var internet = "";
                    if (CrossConnectivity.Current.IsConnected)
                        internet = "1";
                    else
                        internet = "0";
                    new ShareInside().GuardarEstadoParaLaActividad("0," + internet + "$" + PaginaSeleccionada);

                    if (internet == "1")
                    {
                        RunOnUiThread(async () =>
                        {
                            await ActividadesInternetAsync();
                            viewPager.Adapter = adapter;
                            if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                viewPager.SetCurrentItem(0, false);
                            else
                                viewPager.SetCurrentItem(1, false);
                        });
                    }
                    else
                    {
                        RunOnUiThread(async () =>
                        {
                            viewPager.Adapter = adapter;
                            if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                viewPager.SetCurrentItem(0, false);
                            else
                                viewPager.SetCurrentItem(1, false);
                        });
                    }

                }
                else
                {
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        RunOnUiThread(async () =>
                        {
                            new ShareInside().GuardarEstadoParaLaActividad("0," + "1" + "$" + PaginaSeleccionada);

                            await ActividadesInternetAsync();
                            viewPager.Adapter = adapter;
                            if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                viewPager.SetCurrentItem(0, false);
                            else
                                viewPager.SetCurrentItem(1, false);
                        });
                    }
                    else
                    {
                        RunOnUiThread(async () =>
                        {
                            new ShareInside().GuardarEstadoParaLaActividad("0," + "0" + "$" + PaginaSeleccionada);

                            viewPager.Adapter = adapter;
                            if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                viewPager.SetCurrentItem(0, false);
                            else
                                viewPager.SetCurrentItem(1, false);
                        });
                    }
                }

                if (new ShareInside().ConsultarEstadoParaLaActividad().Split(',')[0] == "1")
                    swonly.Checked = true;
                else
                    swonly.Checked = false;

                swonly.CheckedChange += delegate
                {
                    var internet = "";
                    if (CrossConnectivity.Current.IsConnected)
                        internet = "1";
                    else
                        internet = "0";

                    if (swonly.Checked)
                    {
                        new ShareInside().GuardarEstadoParaLaActividad("1," + internet + "$" + PaginaSeleccionada);
                        if (internet == "1")
                        {
                            RunOnUiThread(async () =>
                            {
                                await ActividadesInternetAsync();
                                viewPager.Adapter = adapter;
                                if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                    viewPager.SetCurrentItem(0, false);
                                else
                                    viewPager.SetCurrentItem(1, false);
                            });
                        }
                        else
                        {
                            RunOnUiThread(async () =>
                            {
                                viewPager.Adapter = adapter;
                                if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                    viewPager.SetCurrentItem(0, false);
                                else
                                    viewPager.SetCurrentItem(1, false);
                            });
                        }
                    }
                    else
                    {
                        new ShareInside().GuardarEstadoParaLaActividad("0," + internet + "$" + PaginaSeleccionada);
                        if (internet == "1")
                        {
                            RunOnUiThread(async () =>
                            {
                                await ActividadesInternetAsync();
                                viewPager.Adapter = adapter;
                                if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                    viewPager.SetCurrentItem(0, false);
                                else
                                    viewPager.SetCurrentItem(1, false);
                            });
                        }
                        else
                        {
                            RunOnUiThread(async () =>
                            {
                                viewPager.Adapter = adapter;
                                if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[1] == "0")
                                    viewPager.SetCurrentItem(0, false);
                                else
                                    viewPager.SetCurrentItem(1, false);
                            });
                        }
                    }
                };

            }
            catch (System.Exception)
            {

            }

            #endregion;
            
        }
        
        public async Task ActividadesInternetAsync()
        {
            ProgressBar progressBar = null;
            try
            {
                string ServerConsultarActividades = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "/activities?from=" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd") + "&to=" + DateTime.Now.ToString("yyyy-MM-dd") + "&select=-location";
                //ServerConsultarActividades = "https://cabasus-mobile.azurewebsites.net/v1/horses/5b088ff414075d133c3e38a3/activities?from=2018-06-01&to=2018-06-31&select=-location";
                HttpClient ClienteActividades = new HttpClient();
                ClienteActividades.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                FindViewById<RelativeLayout>(Resource.Id.FondoHome).AddView(progressBar, p);

                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                var SaveConsulActivities = await ClienteActividades.GetAsync(ServerConsultarActividades);

                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                SaveConsulActivities.EnsureSuccessStatusCode();

                if (SaveConsulActivities.IsSuccessStatusCode)
                {
                    var ConsultaJson = await SaveConsulActivities.Content.ReadAsStringAsync();

                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));

                    con.Query<ActividadesCloud>("drop table ActividadesCloud;", new ActividadesCloud().ID_ActividadLocal);
                    con.Query<ActividadesCloud>("CREATE TABLE if not exists ActividadesCloud( ID_ActividadLocal string, ID_Caballo string, ID_Usuario string, Duration double, Slow double, Normal double, Strong double, Horse_Status text, Dates Date, Latitudes longtext, Longitudes longtext, User string );", new ActividadesCloud().ID_ActividadLocal);


                    foreach (var item in JsonConvert.DeserializeObject<List<ActividadCluod>>(ConsultaJson))
                    {
                        var fechabien = item.date.Substring(0, 10);
                        try
                        {
                            con.Insert(new ActividadesCloud()
                            {
                                ID_ActividadLocal = item.id,
                                ID_Caballo = item.horse,
                                ID_Usuario = item.owner.id,
                                Duration = int.Parse(item.duration.ToString()),
                                Slow = item.distance.slow,
                                Normal = item.distance.normal,
                                Strong = item.distance.strong,
                                Horse_Status = item.horse_status,
                                Dates = Convert.ToDateTime(fechabien).ToString("yyyy/MM/dd"),
                                //Latitudes = item.location.coordinates[1].ToString(),
                                //Longitudes = item.location.coordinates[0].ToString(),
                                User = item.owner.username
                            });
                        }
                        catch (System.Exception ex)
                        {
                            Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.the_activities_could_not_be_downloaded), ToastLength.Short).Show();
                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                }
            }
            catch
            {
                Toast.MakeText(this, GetString(Resource.String.the_activities_could_not_be_downloaded), ToastLength.Short).Show();
                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
            }
        }

        public async Task ActividadesInternetAsyncMes(DateTime MesAndMonth, ActivityActivity activity)
        {
            ProgressBar progressBar = null;
            try
            {
                string ServerConsultarActividades = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "/activities?from=" + MesAndMonth.ToString("yyyy-MM-01") + "&to=" + MesAndMonth.ToString("yyyy-MM-31") + "&select=-location";
                //ServerConsultarActividades = "https://cabasus-mobile.azurewebsites.net/v1/horses/5b088ff414075d133c3e38a3/activities?from=2018-06-01&to=2018-06-31&select=-location";
                HttpClient ClienteActividades = new HttpClient();
                ClienteActividades.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                progressBar = new ProgressBar(activity, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                activity.FindViewById<RelativeLayout>(Resource.Id.FondoHome).AddView(progressBar, p);

                progressBar.Visibility = Android.Views.ViewStates.Visible;
                activity.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                var SaveConsulActivities = await ClienteActividades.GetAsync(ServerConsultarActividades);

                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                activity.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);


                SaveConsulActivities.EnsureSuccessStatusCode();

                if (SaveConsulActivities.IsSuccessStatusCode)
                {
                    var ConsultaJson = await SaveConsulActivities.Content.ReadAsStringAsync();

                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));

                    con.Query<ActividadesCloudMes>("drop table if exists ActividadesCloudMes;", new ActividadesCloudMes().ID_ActividadLocal);
                    con.Query<ActividadesCloudMes>("CREATE TABLE if not exists ActividadesCloudMes( ID_ActividadLocal string, ID_Caballo string, ID_Usuario string, Duration double, Slow double, Normal double, Strong double, Horse_Status text, Dates Date, Latitudes longtext, Longitudes longtext, User string );", new ActividadesCloudMes().ID_ActividadLocal);


                    foreach (var item in JsonConvert.DeserializeObject<List<ActividadCluod>>(ConsultaJson))
                    {
                        var fechabien = item.date.Substring(0, 10);
                        try
                        {
                            con.Insert(new ActividadesCloudMes()
                            {
                                ID_ActividadLocal = item.id,
                                ID_Caballo = item.horse,
                                ID_Usuario = item.owner.id,
                                Duration = int.Parse(item.duration.ToString()),
                                Slow = item.distance.slow,
                                Normal = item.distance.normal,
                                Strong = item.distance.strong,
                                Horse_Status = item.horse_status,
                                Dates = Convert.ToDateTime(fechabien).ToString("yyyy/MM/dd"),
                                //Latitudes = item.location.coordinates[1].ToString(),
                                //Longitudes = item.location.coordinates[0].ToString(),
                                User = item.owner.username
                            });
                        }
                        catch (System.Exception ex)
                        {
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            activity.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.the_activities_could_not_be_downloaded), ToastLength.Short).Show();
                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    activity.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                }
            }
            catch
            {
                Toast.MakeText(this, GetString(Resource.String.the_activities_could_not_be_downloaded), ToastLength.Short).Show();
                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                activity.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
            }
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
}