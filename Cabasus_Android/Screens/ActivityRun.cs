using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using SQLite;
using System;
using System.IO;
namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityRun", Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class ActivityRun : Activity, IOnMapReadyCallback
    {
        //Cosas para el mapa
        GoogleMap mapa;
        CameraUpdate camera;
        ProgressBar progressBar;
        BroadcastActividadServicio receiver;
        BroadcastActividadServicioMapa receivermapa;
        //Contador del tiempo
        public int tiempo = 0;
        //Permisos
        private const int MY_PERMISSION_REQUEST_CODE = 7171;
        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 7172;

        protected async override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_run);
            new ShareInside().GuardarPantallaActual("ActivityRun");
            Window.SetStatusBarColor(Color.Black);
                        
            #region inizializar mapa y copiar bases de datos
            SetUpMap();
            (new ShareInside()).CopyDocuments("ActividadesLocal.sqlite", "ActividadesLocal.db");
            (new ShareInside()).CopyDocuments("VaciadoActividades.sqlite", "VaciadoActividades.db");
            #endregion;

            #region Checar permisos 

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
                && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation
                }, MY_PERMISSION_REQUEST_CODE);
            }
            else
            {
                if (CheckPlayServices())
                {
                }
            }

            #endregion;

            receiver = new BroadcastActividadServicio(this);
            receivermapa = new BroadcastActividadServicioMapa(this);

            #region obtener la primera ubicacion para colocar el punto
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Position position = null;
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 1;

                progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);

                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                if (CrossGeolocator.IsSupported)
                {
                    try
                    {
                        position = await locator.GetPositionAsync(TimeSpan.FromSeconds(5), null, true);
                    }
                    catch (Exception ex) { Toast.MakeText(this, Resource.String.The_first_location, ToastLength.Short).Show(); }
                }

                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                LatLng LatLong = new LatLng(position.Latitude, position.Longitude);

                camera = CameraUpdateFactory.NewLatLngZoom(LatLong, 18);
                mapa.AnimateCamera(camera);
                mapa.MyLocationEnabled = true;

            }
            catch { }
            #endregion;

            //referencia para la imagen y el nombre del caballo
            var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + ".png"));

            Java.IO.File ImagenUri = new Java.IO.File(RutaImage.ToString());
            Bitmap ImagenBitMap = BitmapFactory.DecodeFile(ImagenUri.AbsolutePath);
            FindViewById<ImageView>(Resource.Id.imgCaballo).SetImageBitmap(ImagenBitMap);

            FindViewById<TextView>(Resource.Id.lblNombreCaballo).Text = new ShareInside().ConsultarPosicionPiker()[0].Name_HoserSelected;

            //referencia a los botones
            var btnIniciar = FindViewById<Button>(Resource.Id.btnIniciar);
            var btnDetener = FindViewById<Button>(Resource.Id.btnCancelar);

            #region saber si se esta realizando una actividad;

            if (new ShareInside().ConsultarActividadEnProgreso().Id_HorseSelected != "0")
            {
                btnDetener.Text = GetText(Resource.String.Stop).ToString();
                btnIniciar.Alpha = 0.5f;
                btnIniciar.Enabled = false;
            }

            #endregion;

            btnIniciar.Click += delegate
            {
                //Configuracion basica de la pantalla
                File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));
                (new ShareInside()).CopyDocuments("ActividadesLocal.sqlite", "ActividadesLocal.db");
                //progressBar.Visibility = Android.Views.ViewStates.Visible;
                //Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                btnDetener.Text = GetText(Resource.String.Stop).ToString();
                btnIniciar.Alpha = 0.5f;
                btnIniciar.Enabled = false;

                new ShareInside().GuardarActividadEnProgreso(new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected, FindViewById<TextView>(Resource.Id.lblNombreCaballo).Text);

                //await StartListening();

                var intent = new Intent(this, typeof(ServicioActividadForegroud));
                if (savedInstanceState != null)
                {
                    intent.PutExtras(savedInstanceState);
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    StartForegroundService(intent);
                }
                else
                {
                    StartService(intent);
                }
            };

            btnDetener.Click += delegate 
            {
                double TotalLow = 0, TotalNormal = 0, TotalStrong = 0;

                if (btnDetener.Text == GetText(Resource.String.Cancel).ToString())
                {
                    StartActivity(typeof(ActivityActivity));
                    Finish();
                }
                else
                {
                    var intent = new Intent(this, typeof(ServicioActividadForegroud));
                    if (savedInstanceState != null)
                    {
                        intent.PutExtras(savedInstanceState);
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        StopService(intent);
                    }
                    else
                    {
                        StopService(intent);
                    }
                    
                    //vaciar actividad a la tabla principal 
                    var conAct = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));
                    var conVaciAct = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));

                    var TodaActividad = conAct.Query<Actividad>("Select * from Actividad", (new Actividad()).Latitud);

                    //colocar el progressbar
                    /*progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                    RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                    p.AddRule(LayoutRules.CenterInParent);
                    progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                    FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);

                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                    Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);*/

                    for (int i = 0; i < TodaActividad.Count; i++)
                    {
                        Location locationA = new Location("punto A");
                        Location locationB = new Location("punto B");

                        try
                        {
                            if (TodaActividad[i].Velocidad == 1 && TodaActividad[i + 1].Velocidad == 1)
                            {
                                locationA.Latitude = TodaActividad[i].Latitud;
                                locationA.Longitude = TodaActividad[i].Longitud;

                                locationB.Latitude = TodaActividad[i + 1].Latitud;
                                locationB.Longitude = TodaActividad[i + 1].Longitud;

                                TotalLow += locationA.DistanceTo(locationB);
                            }
                            else if (TodaActividad[i].Velocidad == 2 && TodaActividad[i + 1].Velocidad == 2)
                            {
                                locationA.Latitude = TodaActividad[i].Latitud;
                                locationA.Longitude = TodaActividad[i].Longitud;

                                locationB.Latitude = TodaActividad[i + 1].Latitud;
                                locationB.Longitude = TodaActividad[i + 1].Longitud;

                                TotalNormal += locationA.DistanceTo(locationB);
                            }
                            else if (TodaActividad[i].Velocidad == 3 && TodaActividad[i + 1].Velocidad == 3)
                            {
                                locationA.Latitude = TodaActividad[i].Latitud;
                                locationA.Longitude = TodaActividad[i].Longitud;

                                locationB.Latitude = TodaActividad[i + 1].Latitud;
                                locationB.Longitude = TodaActividad[i + 1].Longitud;

                                TotalStrong += locationA.DistanceTo(locationB);
                            }
                            else if (TodaActividad[i].Velocidad == 1 && TodaActividad[i + 1].Velocidad == 2 || TodaActividad[i].Velocidad == 2 && TodaActividad[i + 1].Velocidad == 1)
                            {
                                locationA.Latitude = TodaActividad[i].Latitud;
                                locationA.Longitude = TodaActividad[i].Longitud;

                                locationB.Latitude = TodaActividad[i + 1].Latitud;
                                locationB.Longitude = TodaActividad[i + 1].Longitud;

                                var mitades = locationA.DistanceTo(locationB);

                                TotalLow += mitades / 2;
                                TotalNormal += mitades / 2;
                            }
                            else if (TodaActividad[i].Velocidad == 1 && TodaActividad[i + 1].Velocidad == 3 || TodaActividad[i].Velocidad == 3 && TodaActividad[i + 1].Velocidad == 1)
                            {
                                locationA.Latitude = TodaActividad[i].Latitud;
                                locationA.Longitude = TodaActividad[i].Longitud;

                                locationB.Latitude = TodaActividad[i + 1].Latitud;
                                locationB.Longitude = TodaActividad[i + 1].Longitud;

                                var mitades = locationA.DistanceTo(locationB);

                                TotalLow += mitades / 2;
                                TotalStrong += mitades / 2;
                            }
                            else if (TodaActividad[i].Velocidad == 2 && TodaActividad[i + 1].Velocidad == 3 || TodaActividad[i].Velocidad == 3 && TodaActividad[i + 1].Velocidad == 2)
                            {
                                locationA.Latitude = TodaActividad[i].Latitud;
                                locationA.Longitude = TodaActividad[i].Longitud;

                                locationB.Latitude = TodaActividad[i + 1].Latitud;
                                locationB.Longitude = TodaActividad[i + 1].Longitud;

                                var mitades = locationA.DistanceTo(locationB);

                                TotalNormal += mitades / 2;
                                TotalStrong += mitades / 2;
                            }
                        }
                        catch { };
                    }

                    string LAT = "", LON = "";
                    foreach (var item in TodaActividad)
                    {
                        LAT += item.Velocidad + "$" + item.Latitud+"$";
                        LON += item.Velocidad + "$" + item.Longitud + "$";
                    }

                    #region alertdialog para el estado del caballo

                    Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                    //Dialog alertar = new Dialog(this, Android.Resource.Style.ThemeTranslucentNoTitleBarFullScreen);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(false);
                    alertar.SetContentView(Resource.Layout.layout_CustoAlertRun);
                    alertar.Show();

                    alertar.FindViewById<TextView>(Resource.Id.Very_Well_Trained).Click += delegate
                    {
                        alertar.Dismiss();
                        conVaciAct.Insert(new ActividadPorCaballo
                        {
                            ID_Caballo = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                            ID_Usuario = new ShareInside().ConsultarDatosUsuario()[0].username,
                            Duration = tiempo,
                            Slow = TotalLow,
                            Normal = TotalNormal,
                            Strong = TotalStrong,
                            Horse_Status = "1",
                            Dates = System.DateTime.Now.ToString("yyyy/MM/dd"),
                            Latitudes = LAT,
                            Longitudes = LON
                        });

                        //actualizar caballo para sicronizar despues 5aee28cc3cabe61088767e45
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);

                        File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));

                        StartActivity(typeof(ActivityActivity));
                        Finish();
                    };

                    alertar.FindViewById<TextView>(Resource.Id.Well_Trained).Click += delegate
                    {
                        alertar.Dismiss();
                        conVaciAct.Insert(new ActividadPorCaballo
                        {
                            ID_Caballo = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                            ID_Usuario = new ShareInside().ConsultarDatosUsuario()[0].username,
                            Duration = tiempo,
                            Slow = TotalLow,
                            Normal = TotalNormal,
                            Strong = TotalStrong,
                            Horse_Status = "2",
                            Dates = System.DateTime.Now.ToString("yyyy/MM/dd"),
                            Latitudes = LAT,
                            Longitudes = LON
                        });

                        //actualizar caballo para sicronizar despues 5aee28cc3cabe61088767e45
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);

                        File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));

                        StartActivity(typeof(ActivityActivity));
                        Finish();
                    };

                    alertar.FindViewById<TextView>(Resource.Id.Normal).Click += delegate
                    {
                        alertar.Dismiss();                        
                        conVaciAct.Insert(new ActividadPorCaballo
                        {
                            ID_Caballo = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                            ID_Usuario = new ShareInside().ConsultarDatosUsuario()[0].username,
                            Duration = tiempo,
                            Slow = TotalLow,
                            Normal = TotalNormal,
                            Strong = TotalStrong,
                            Horse_Status = "3",
                            Dates = System.DateTime.Now.ToString("yyyy/MM/dd"),
                            Latitudes = LAT,
                            Longitudes = LON
                        });

                        //actualizar caballo para sicronizar despues 5aee28cc3cabe61088767e45
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);

                        File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));

                        StartActivity(typeof(ActivityActivity));
                        Finish();
                    };

                    alertar.FindViewById<TextView>(Resource.Id.Underweight).Click += delegate
                    {
                        alertar.Dismiss();
                        conVaciAct.Insert(new ActividadPorCaballo
                        {
                            ID_Caballo = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                            ID_Usuario = new ShareInside().ConsultarDatosUsuario()[0].username,
                            Duration = tiempo,
                            Slow = TotalLow,
                            Normal = TotalNormal,
                            Strong = TotalStrong,
                            Horse_Status = "4",
                            Dates = System.DateTime.Now.ToString("yyyy/MM/dd"),
                            Latitudes = LAT,
                            Longitudes = LON
                        });

                        //actualizar caballo para sicronizar despues 5aee28cc3cabe61088767e45
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);

                        File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));

                        StartActivity(typeof(ActivityActivity));
                        Finish();
                    };

                    alertar.FindViewById<TextView>(Resource.Id.Overweight).Click += delegate
                    {
                        alertar.Dismiss();
                        conVaciAct.Insert(new ActividadPorCaballo
                        {
                            ID_Caballo = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                            ID_Usuario = new ShareInside().ConsultarDatosUsuario()[0].username,
                            Duration = tiempo,
                            Slow = TotalLow,
                            Normal = TotalNormal,
                            Strong = TotalStrong,
                            Horse_Status = "5",
                            Dates = System.DateTime.Now.ToString("yyyy/MM/dd"),
                            Latitudes = LAT,
                            Longitudes = LON
                        });

                        //actualizar caballo para sicronizar despues 5aee28cc3cabe61088767e45
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);

                        File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));

                        StartActivity(typeof(ActivityActivity));
                        Finish();
                    };

                    #endregion;
                }
            };
        }

        private bool CheckPlayServices()
        {
            int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GooglePlayServicesUtil.IsUserRecoverableError(resultCode))
                {
                    GooglePlayServicesUtil.GetErrorDialog(resultCode, this, PLAY_SERVICES_RESOLUTION_REQUEST).Show();
                }
                else
                {
                    Toast.MakeText(ApplicationContext, Resource.String.This_device_is_not_support, ToastLength.Long).Show();
                    Finish();
                }
                return false;
            }
            return true;
        }

        public void actualizarmapa(double lat, double lon, double KH)
        {
            CircleOptions circleOptions;
            LatLng LatLong = new LatLng(lat, lon);
            if (KH >= 0.1 && KH <= 11.8)
            {
                circleOptions = new CircleOptions();
                circleOptions.InvokeCenter(LatLong);
                circleOptions.InvokeRadius(5);
                circleOptions.InvokeStrokeColor(Android.Graphics.Color.Transparent);
                circleOptions.InvokeFillColor(Android.Graphics.Color.Rgb(51, 255, 255));
                Circle newCircle = mapa.AddCircle(circleOptions);
                newCircle.Visible = true;
            }
            else if (KH > 11.8 && KH <= 20.8)
            {
                circleOptions = new CircleOptions();
                circleOptions.InvokeCenter(LatLong);
                circleOptions.InvokeRadius(5);
                circleOptions.InvokeStrokeColor(Android.Graphics.Color.Transparent);
                circleOptions.InvokeFillColor(Android.Graphics.Color.Rgb(0, 102, 204));
                Circle newCircle = mapa.AddCircle(circleOptions);
                newCircle.Visible = true;
            }
            else if (KH > 20.8)
            {
                circleOptions = new CircleOptions();
                circleOptions.InvokeCenter(LatLong);
                circleOptions.InvokeRadius(5);
                circleOptions.InvokeStrokeColor(Android.Graphics.Color.Transparent);
                circleOptions.InvokeFillColor(Android.Graphics.Color.Rgb(25, 25, 112));
                Circle newCircle = mapa.AddCircle(circleOptions);
                newCircle.Visible = true;
            }
            camera = CameraUpdateFactory.NewLatLngZoom(LatLong, 18);
            mapa.AnimateCamera(camera);
            mapa.MyLocationEnabled = true;
        }

        private void SetUpMap()
        {
            if (mapa == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
            }
        }

        #region cilco de vida;
        public void OnMapReady(GoogleMap googleMap)
        {
            mapa = googleMap;
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
            RegisterReceiver(receiver, new IntentFilter("IntentActividad"));
            RegisterReceiver(receivermapa, new IntentFilter("IntentActividadMapa"));
            ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
            PackageManager package = this.PackageManager;
            package.SetComponentEnabledSetting(component, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
        }
        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                UnregisterReceiver(receiver);
                UnregisterReceiver(receivermapa);
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
        #endregion;
        
    }

    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new[] { "IntentActividad" })]
    public class BroadcastActividadServicio : BroadcastReceiver
    {
        ActivityRun Actividad = null;

        public BroadcastActividadServicio(ActivityRun ac)
        {
            Actividad = ac;
        }

        public BroadcastActividadServicio()
        {

        }
        public override void OnReceive(Context context, Intent intent)
        {
            var Tiempo = intent.GetStringExtra("Tiempo");
            var RTime = intent.GetIntExtra("RT", 0);

            //update textview in fragment
            if (Actividad != null)
            {
                Actividad.FindViewById<TextView>(Resource.Id.lblDuracionReal).Text = Tiempo;
                Actividad.tiempo = RTime;
            }
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new[] { "IntentActividadMapa" })]
    public class BroadcastActividadServicioMapa : BroadcastReceiver
    {
        ActivityRun Actividad = null;

        public BroadcastActividadServicioMapa(ActivityRun ac)
        {
            Actividad = ac;
        }

        public BroadcastActividadServicioMapa()
        {

        }
        public override void OnReceive(Context context, Intent intent)
        {
            var lat = intent.GetDoubleExtra("Latitud", 0);
            var lon = intent.GetDoubleExtra("Longitud", 0);
            var Intensidad = intent.GetStringExtra("intencidad");
            var Velocidad = intent.GetStringExtra("Velocidad");

            //update textview in fragment
            if (Actividad != null)
            {
                Actividad.FindViewById<TextView>(Resource.Id.lblVelocidadReal).Text = Velocidad;
                Actividad.FindViewById<TextView>(Resource.Id.lblIntensidadReal).Text = Intensidad;

                Actividad.actualizarmapa(lat, lon, double.Parse(Velocidad.Split('k')[0]));
            }
        }
    }
}

