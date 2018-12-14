using System;
using System.Collections.Generic;
using Android.Animation;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Plugin.Connectivity;
using SQLite;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using Android.Content;
using System.IO;
using System.Xml.Serialization;
using Android.Text;
using System.Threading.Tasks;
using Android.Support.V4.Widget;
using Android.Graphics;
using Android.Support.V7.App;
using Android.Content.PM;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivitySettings", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class ActivitySettings : AppCompatActivity
    {
        List<HorsesCloud> ListCaballos = new List<HorsesCloud>();

        private ViewGroup FondoParaBotonFlotante;
        private int MX, MY;
        private RelativeLayout btnAddHorse;
        private float ScreenWith = 0;
        private float ScrenHeight = 0;
        int contador = 0;
        ProgressBar progressBar;
        bool BanderaPosicionDelBoton = false;
        EditText buscarrider;
        ListView textListView,listasegura;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_settings);
            Window.SetStatusBarColor(Color.Black);
            new ShareInside().GuardarPantallaActual("ActivitySettings");
            
            var transaccion = FragmentManager.BeginTransaction();
            transaccion.Add(Resource.Id.fragmenthome, new FragmentCampana(), "campana");
            transaccion.Commit();

            var Recibirnotificacion = new RecibirNotificacion(this);
            RegisterReceiver(Recibirnotificacion, new IntentFilter("Campana"));
            
            listasegura = FindViewById<ListView>(Resource.Id.ListaCaballos);
            listasegura.ScrollStateChanged += delegate {
                if (listasegura.LastVisiblePosition==listasegura.Adapter.Count-1&&listasegura.GetChildAt(listasegura.ChildCount-1).Bottom<=listasegura.Height)
                {
                    #region Mover el boton izquierda;
                    
                        ObjectAnimator animX = ObjectAnimator.OfFloat(btnAddHorse, "X", new ShareInside().ConvertDPToPixels(10, Resources.DisplayMetrics.Density));
                        ObjectAnimator animY = ObjectAnimator.OfFloat(btnAddHorse, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                        ObjectAnimator animSX = ObjectAnimator.OfFloat(btnAddHorse, "scaleX", 1f);
                        ObjectAnimator animSY = ObjectAnimator.OfFloat(btnAddHorse, "scaleY", 1f);

                        animX.SetDuration(500);
                        animY.SetDuration(500);
                        animSX.SetDuration(500);
                        animSY.SetDuration(500);

                        AnimatorSet animator = new AnimatorSet();
                        animator.Play(animX).With(animY).With(animSX).With(animSY);
                        animator.Start();
                        BanderaPosicionDelBoton = true;
                    #endregion
                }
                else
                {
                    #region Mover el boton derecha;
                    ObjectAnimator animX = ObjectAnimator.OfFloat(btnAddHorse, "X", ScreenWith - new ShareInside().ConvertDPToPixels(70, Resources.DisplayMetrics.Density));
                    ObjectAnimator animY = ObjectAnimator.OfFloat(btnAddHorse, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                    ObjectAnimator animSX = ObjectAnimator.OfFloat(btnAddHorse, "scaleX", 1f);
                    ObjectAnimator animSY = ObjectAnimator.OfFloat(btnAddHorse, "scaleY", 1f);

                    animX.SetDuration(500);
                    animY.SetDuration(500);
                    animSX.SetDuration(500);
                    animSY.SetDuration(500);

                    AnimatorSet animator = new AnimatorSet();
                    animator.Play(animX).With(animY).With(animSX).With(animSY);
                    animator.Start();
                    BanderaPosicionDelBoton = false;
                    #endregion;
                }


            };
            
            #region Evento click boton ajuestes 
            ImageButton ajustes = FindViewById<ImageButton>(Resource.Id.btnEditSettings);

            ajustes.Click += delegate { StartActivity(typeof(ActivityAjustes)); Finish(); };
            #endregion

            #region Referencias de los botones;

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            var btnnotificaciones = FindViewById<RelativeLayout>(Resource.Id.notificacion);
            var refresh = FindViewById<SwipeRefreshLayout>(Resource.Id.swiperefresh);
            FondoParaBotonFlotante = FindViewById<RelativeLayout>(Resource.Id.FondoParaBoton);
            btnAddHorse = FondoParaBotonFlotante.FindViewById<RelativeLayout>(Resource.Id.btnAddHorse);

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
            btnnotificaciones.Click += delegate
            {

                StartActivity(typeof(ActivityNotificaciones));
                Finish();
            };

            #endregion;

            #region BotonFlotante;

        //    btnAddHorse.SetOnTouchListener(this);
            var metrics = Resources.DisplayMetrics;
            ScreenWith = metrics.WidthPixels;
            ScrenHeight = metrics.HeightPixels;
            btnAddHorse.SetX(ScreenWith - new ShareInside().ConvertDPToPixels(70, Resources.DisplayMetrics.Density));
            btnAddHorse.SetY(ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));

            #endregion;

            #region llenar lista de caballos segun la conexion 
            
            if (CrossConnectivity.Current.IsConnected)
                LLenarListasSegunLaConexion(FindViewById<ListView>(Resource.Id.ListaCaballos), true);
            else
                LLenarListasSegunLaConexion(FindViewById<ListView>(Resource.Id.ListaCaballos), false);

            #endregion;

            #region Refrescar con scroll

            refresh.SetColorSchemeColors(Color.Rgb(197, 152, 5));
            refresh.SetProgressBackgroundColorSchemeColor(Color.Rgb(255, 255, 255));
            refresh.Refresh += async delegate {

                #region llenar lista de caballos segun la conexion 
                if (CrossConnectivity.Current.IsConnected)
                {
                    await new ShareInside().GuardadoFotosYConsulta();
                    LLenarListasSegunLaConexion(FindViewById<ListView>(Resource.Id.ListaCaballos), true);
                }
                else
                    LLenarListasSegunLaConexion(FindViewById<ListView>(Resource.Id.ListaCaballos), false);
                #endregion;
                refresh.Refreshing = false;
            };


            #endregion

            btnAddHorse.Click += delegate {
               
                    //Relative negro para eliminar el menu
                    RelativeLayout General = new RelativeLayout(this);
                    General.LayoutParameters = new RelativeLayout.LayoutParams(-1, -1);
                    General.SetBackgroundColor(Android.Graphics.Color.Argb(127, 0, 0, 0));
                    FindViewById<RelativeLayout>(Resource.Id.FondoParaBoton).AddView(General);

                    //Linear que contiene las opciones del menu
                    LinearLayout OptInsCaballo = new LinearLayout(this);
                    OptInsCaballo.SetBackgroundColor(Android.Graphics.Color.White);
                    OptInsCaballo.Orientation = Orientation.Vertical;
                    var c = new ShareInside();
                    LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, int.Parse(c.ConvertDPToPixels(60, Resources.DisplayMetrics.Density).ToString()));
                    OptInsCaballo.LayoutParameters = p;

                    //decidir donde colocar el menu
                    if (!BanderaPosicionDelBoton)
                    {
                        OptInsCaballo.SetX(btnAddHorse.GetX() - int.Parse(c.ConvertDPToPixels(90, Resources.DisplayMetrics.Density).ToString()));
                        OptInsCaballo.SetY(btnAddHorse.GetY() - int.Parse(c.ConvertDPToPixels(30, Resources.DisplayMetrics.Density).ToString()));
                    }
                    else
                    {
                        OptInsCaballo.SetX(btnAddHorse.GetX() + int.Parse(c.ConvertDPToPixels(30, Resources.DisplayMetrics.Density).ToString()));
                        OptInsCaballo.SetY(btnAddHorse.GetY() - int.Parse(c.ConvertDPToPixels(30, Resources.DisplayMetrics.Density).ToString()));
                    }

                    //agregar las opciones al linear                           
                    TextView btnNuevoCaballo = new TextView(this);
                    btnNuevoCaballo.LayoutParameters = new LinearLayout.LayoutParams(-1, 0, 1);
                    btnNuevoCaballo.Text = GetText( Resource.String.Add);
                    btnNuevoCaballo.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    btnNuevoCaballo.SetTextColor(Android.Graphics.Color.Black);
                    btnNuevoCaballo.Gravity = GravityFlags.Center;
                    btnNuevoCaballo.TextSize = c.ConvertDPToPixels(6, Resources.DisplayMetrics.Density);

                    btnNuevoCaballo.Click += delegate { StartActivity(typeof(ActivityRegistroCaballos)); Finish(); };


                    TextView btnBuscarCaballo = new TextView(this);
                    btnBuscarCaballo.LayoutParameters = new LinearLayout.LayoutParams(-1, 0, 1);
                    btnBuscarCaballo.Text = GetText(Resource.String.Search_a_horse);
                    btnBuscarCaballo.SetTextColor(Android.Graphics.Color.Black);
                    btnBuscarCaballo.Gravity = GravityFlags.Center;
                    btnBuscarCaballo.TextSize = c.ConvertDPToPixels(6, Resources.DisplayMetrics.Density);

                    #region boton para buscar caballos
                    btnBuscarCaballo.Click += delegate
                    {
                        try
                        {
                            FindViewById<RelativeLayout>(Resource.Id.FondoParaBoton).RemoveView(General);
                            Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                            alertar.RequestWindowFeature(1);
                            alertar.SetCancelable(true);
                            alertar.SetContentView(Resource.Layout.DialogoRider);

                            //     textListView = alertar.FindViewById<ListView>(Resource.Id.Listarider);

                            buscarrider = alertar.FindViewById<EditText>(Resource.Id.buscarrider);

                            progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                            RelativeLayout.LayoutParams p1 = new RelativeLayout.LayoutParams(100, 100);
                            p1.AddRule(LayoutRules.CenterInParent);
                            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                            alertar.FindViewById<RelativeLayout>(Resource.Id.dialogorider).AddView(progressBar, p1);

                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            buscarrider.Hint = GetText(Resource.String.Horse_names).ToString();
                            buscarrider.TextChanged += async (object sender, TextChangedEventArgs g) =>
                            {

                                try
                                {
                                    string server = "https://cabasus-mobile.azurewebsites.net/v1/horses/search?name=" + buscarrider.Text;

                                    HttpClient cliente = new HttpClient();
                                    if (CrossConnectivity.Current.IsConnected)
                                    {
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                                        Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                        var clientePost2 = await cliente.GetAsync(server);

                                        clientePost2.EnsureSuccessStatusCode();
                                        if (clientePost2.IsSuccessStatusCode)
                                        {
                                            JsonValue ObjJson = await clientePost2.Content.ReadAsStringAsync();
                                            try
                                            {
                                                var data = JsonConvert.DeserializeObject<List<HorseSearch>>(ObjJson);
                                                textListView = alertar.FindViewById<ListView>(Resource.Id.Listarider);
                                                textListView.Adapter = new AdaptadorHorses(this, data, alertar);
                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            }
                                            catch (Exception ex)
                                            {
                                                Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                // MessageBox("Error", ex.Message);
                                            }
                                        }

                                    }
                                    else
                                    {
                                    }
                                    if (buscarrider.Text.Length == 0)
                                    {
                                        await
                                        Task.Delay(1500);
                                        try
                                        {
                                            List<HorseSearch> data = new List<HorseSearch>();
                                            textListView.Adapter = new AdaptadorHorses(this, data, alertar);
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        }
                                        catch (Exception)
                                        {
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                                }
                            };

                            alertar.Show();


                        }
                        catch (Exception ex)
                        {

                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                        }
                    };
                    #endregion

                    OptInsCaballo.AddView(btnNuevoCaballo);
                    OptInsCaballo.AddView(btnBuscarCaballo);

                    General.AddView(OptInsCaballo);

                    General.Click += delegate
                    {
                        FindViewById<RelativeLayout>(Resource.Id.FondoParaBoton).RemoveView(General);
                    };
                
            };
            
        }

        private async void LLenarListasSegunLaConexion(ListView ListaCaballos, bool internet)
        {
            HorsesCloud h;

            h = new HorsesCloud();
            h.id = "HL5321876515844562";
            h.name = "HL5321876515844562";
            h.owner = "HL5321876515844562";
            h.owner_name = "HL5321876515844562";
            h.photo = "HL5321876515844562";

            if (internet)
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);

                if (consulta.Count <= 0)
                {
                    consulta.Insert(0, h);
                    consulta.Add(new HorsesCloud()
                    {
                        id = "NH5321876515844562",
                        name = "NH5321876515844562",
                        owner = "NH5321876515844562",
                        owner_name = "NH5321876515844562",
                        photo = "NH5321876515844562"
                    });
                }
                else
                {
                    consulta.Insert(0, h);
                }
                
                if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml")))
                {
                    var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                    var data = JsonConvert.DeserializeObject<List<HorsesComplete>>(((IdNameHorse)(new XmlSerializer(typeof(IdNameHorse))).Deserialize(Lectura)).DatosCaballo);
                    Lectura.Close();
                    try
                    {
                        consulta.Add(new HorsesCloud()
                        {
                            id = "HC5321876515844562",
                            name = "HC5321876515844562",
                            owner = "HC5321876515844562",
                            owner_name = "HC5321876515844562",
                            photo = "HC5321876515844562"
                        });

                        foreach (var item in data)
                        {
                            consulta.Add(new HorsesCloud()
                            {
                                id = item.id,
                                name = item.name,
                                owner = item.owner.id,
                                owner_name = item.owner.username,
                                photo = item.photo.data,
                                Sync = 2,
                                shares = item.shares
                            });
                        }
                        ListaCaballos.Adapter = new AdaptadorCaballos(this, consulta);
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                    }
                }
                else
                {
                    StartActivity(typeof(ActivityRegistroCaballos));
                    Toast.MakeText(this, Resource.String.Add_a_horse, ToastLength.Short).Show();
                    Finish();
                }
            }
            else
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);

                if (consulta.Count <= 0)
                {
                    List<HorsesCloud> NH = new List<HorsesCloud>();
                    NH.Add(h);
                    NH.Add(new HorsesCloud()
                    {
                        id = "NH5321876515844562",
                        name = "NH5321876515844562",
                        owner = "NH5321876515844562",
                        owner_name = "NH5321876515844562",
                        photo = "NH5321876515844562"
                    });
                    ListaCaballos.Adapter = new AdaptadorCaballos(this, NH);
                    StartActivity(typeof(ActivityRegistroCaballos));
                    Toast.MakeText(this, Resource.String.Add_a_horse, ToastLength.Short).Show();
                    Finish();
                }
                else
                {
                    consulta.Insert(0, h);
                    ListaCaballos.Adapter = new AdaptadorCaballos(this, consulta);
                }
            }
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
                new ShareInside().GuardarPantallaActual("Cerrado");
                ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
                PackageManager package = this.PackageManager;
                package.SetComponentEnabledSetting(component, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                var Recibirnotificacion = new RecibirNotificacion(this);
                UnregisterReceiver(Recibirnotificacion);
            }
            catch (Exception)
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
                new ShareInside().GuardarPantallaActual("Cerrado");
                ComponentName component = new ComponentName(this, "com.cabasus.myapp.Cabasus_Android.RegistroInternet");
                PackageManager package = this.PackageManager;
                package.SetComponentEnabledSetting(component, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                RegistroInternet ri = new RegistroInternet();
                UnregisterReceiver(ri);
                
            }
            catch (Exception)
            {

            }
        }

        public override void OnBackPressed()
        {
            StartActivity(typeof(ActivityHome));
            Finish();
        }
    }
    public class HorseSearch
    {
        public string name { get; set; }
        public string breed { get; set; }
        public float weight { get; set; }
        public float height { get; set; }
        public string birthday { get; set; }
        public bool visible { get; set; }
        public string gender { get; set; }
        public Photo photo { get; set; }
        public List<ShareSettings> shares { get; set; }
        public string id { get; set; }
        public OwnerSearch owner { get; set; }
    }
    public class OwnerSearch
    {
        public string username { get; set; }
        public string id { get; set; }

    }

}