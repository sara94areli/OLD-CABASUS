using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Android.Support.V4.Widget;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Android.Graphics;
using SQLite;
using Android.Content.PM;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityNotificaciones", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class ActivityNotificaciones : Activity
    {
        ProgressBar progressBar;
        List<Notificaciones> datacompleta = new List<Notificaciones>();
        protected override async void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_notificaciones);
            new ShareInside().GuardarPantallaActual("ActivityNotificaciones");
            Window.SetStatusBarColor(Color.Black);
            #region Referencias de los botones;
            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            var refresh = FindViewById<SwipeRefreshLayout>(Resource.Id.swiperefresh);
            #endregion;

            #region Abrir las diferentes pantalla con los diferentes botones;
            btnhome.Click += delegate
            {
                StartActivity(typeof(ActivityHome));
                Finish();
            };

            btnactivity.Click += delegate
            {
                StartActivity(typeof(ActivityActivity));
                Finish();
            };

            btndiary.Click += delegate
            {
                StartActivity(typeof(ActivityDiary));
                Finish();
            };

            btncalendar.Click += delegate
            {
                StartActivity(typeof(ActivityCalendar));
                Finish();
            };

            btn_settings.Click += delegate
            {
                StartActivity(typeof(ActivitySettings));
                Finish();
            };
            #endregion;

            System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));

            LlenarNotificaciones(1);

            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Notification.sqlite"));
            var consulta = con.Query<Notificar>("delete from notificar");
            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.CancelAll();


            #region Refrescar con scroll

            refresh.SetColorSchemeColors(Color.Rgb(197, 152, 5));
            refresh.SetProgressBackgroundColorSchemeColor(Color.Rgb(255, 255, 255));
            refresh.Refresh += async delegate {
               
                LlenarNotificaciones(2);

                refresh.Refreshing = false;
            };

            #endregion

        }
        private async void LlenarNotificaciones(int op)
        {
            #region Generar consulta
            try
            {
                ListView listanoti = FindViewById<ListView>(Resource.Id.ListaNotificaciones);
                string servidor = "https://cabasus-mobile.azurewebsites.net/v1/profile/pending_shares";
                HttpClient Cliente = new HttpClient();
                if (CrossConnectivity.Current.IsConnected)
                {

                    Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                    if (op == 1)
                    {
                        progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                        FindViewById<RelativeLayout>(Resource.Id.FondoParaBoton).AddView(progressBar, p);

                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    }

                    var Notificacion = await Cliente.GetAsync(servidor);

                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                    Notificacion.EnsureSuccessStatusCode();
                    if (Notificacion.IsSuccessStatusCode)
                    {
                        JsonValue ConsultaJson = await Notificacion.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<List<Notificaciones>>(ConsultaJson);
                        if (data.Count == 0)
                        {
                            data.Add(new Notificaciones()
                            {
                                id = "123456789"
                            });
                            listanoti.Adapter = new AdaptadorNotificaciones(this, data);
                        }
                        else
                        {
                            datacompleta.Clear();
                            foreach (var item in data)
                            {
                                for (int i = 0; i < item.shares.Count; i++)
                                {
                                    datacompleta.Add(new Notificaciones()
                                    {

                                        name = item.name,
                                        id = item.id,
                                        owner = item.owner,
                                        shares = new List<shares>(){ new shares(){
                                            _id =item.shares[i]._id,
                                            updatedAt=item.shares[i].updatedAt,
                                            user=new user(){
                                                id=item.shares[i].user.id,
                                                username=item.shares[i].user.username

                                        }}}
                                    });
                                }
                            }
                            listanoti.Adapter = new AdaptadorNotificaciones(this, datacompleta);
                        }

                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.The_information , ToastLength.Short).Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
            }
            #endregion
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
    public class Notificar
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Notificar { get; set; }
        public string Titulo { get; set; }
        public string Cuerpo { get; set; }
    }
}