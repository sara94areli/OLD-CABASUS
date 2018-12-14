using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Gigamole.Infinitecycleviewpager;
using Newtonsoft.Json;
using Plugin.Connectivity;
using SQLite;
using Android.Support.V4.Content;
using Android.Views.InputMethods;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using System.Threading.Tasks;
using Android.Content.PM;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityDiary",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class ActivityDiary : AppCompatActivity, IDialogInterfaceOnClickListener
    {
        private ViewGroup FondoDiario;
        private int MX, MY;
        private RelativeLayout btnAddDiary;
        private float ScreenWith = 0;
        private float ScrenHeight = 0;
        int contador = 0;
        ProgressBar progressBar;
        PickerDate onDateSetListener;
        TextView lblfechaMidiario;

        ListView contenido;

        bool BanderaPosicionDelBoton = false;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_diary);
            Window.SetStatusBarColor(Color.Black);

            new ShareInside().GuardarPantallaActual("ActivityDiary");

            #region Notificaciones
            var transaccion = FragmentManager.BeginTransaction();
            transaccion.Add(Resource.Id.fragmenthome, new FragmentCampana(), "campana");
            transaccion.Commit();

            var transaccioncarrusel = FragmentManager.BeginTransaction();
            transaccioncarrusel.Add(Resource.Id.fragmentcarrucel, new FragmentCycle(LayoutInflater, null), "cycle");
            transaccioncarrusel.Commit();

            var Recibirnotificacion = new RecibirNotificacion(this);
            RegisterReceiver(Recibirnotificacion, new IntentFilter("Campana"));

            #endregion

            #region Casamiento 

            lblfechaMidiario = FindViewById<TextView>(Resource.Id.lblfechaMidiario);
            lblfechaMidiario.Text = DateTime.Now.ToString("yyy-MM-dd");

            #endregion

            #region Referencias de los botones;

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            var btnnotificaciones = FindViewById<RelativeLayout>(Resource.Id.notificacion);

            FondoDiario = FindViewById<RelativeLayout>(Resource.Id.FondoDiario);
            btnAddDiary = FondoDiario.FindViewById<RelativeLayout>(Resource.Id.btnAddDiary);
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


            #region Seleccion de fecha
            lblfechaMidiario.Click += delegate
            {
                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                int year = calendar.Get(Java.Util.CalendarField.Year);
                int month = calendar.Get(Java.Util.CalendarField.Month);
                int day_of_month = calendar.Get(Java.Util.CalendarField.DayOfMonth);
                DatePickerDialog dialog;

                dialog = new DatePickerDialog(this, Resource.Style.ThemeOverlay_AppCompat_Dialog_Alert,
                onDateSetListener, year, month, day_of_month);
                dialog.Show();
            };
            onDateSetListener = new PickerDate(lblfechaMidiario);

            lblfechaMidiario.TextChanged += async delegate {
                await FiltrosFechasAsync(lblfechaMidiario.Text);
            };

            #endregion

            #region BotonFlotante

            //btnAddDiary.SetOnTouchListener(this);
            var metrics = Resources.DisplayMetrics;
            ScreenWith = metrics.WidthPixels;
            ScrenHeight = metrics.HeightPixels;
            btnAddDiary.SetX(ScreenWith - ConvertDPToPixels(70));
            btnAddDiary.SetY(ScrenHeight - ConvertDPToPixels(160));

            btnAddDiary.Click += delegate {
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
                    Intent intent = new Intent(this, (typeof(Activity_NuevoDiario)));
                    intent.PutExtra("Date", lblfechaMidiario.Text);
                    this.StartActivity(intent);
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, Resource.String.You_cant_make_diarys, ToastLength.Short).Show();
                }
            };

            #endregion

            #region Filtro de diarios
            await FiltrosFechasAsync(lblfechaMidiario.Text);
            #endregion

        }

        public async Task<ImagenesNube> FiltroImagenes(string foto1)
        {
            string serverImagenes = "https://cabasus-mobile.azurewebsites.net/v1/images/" + foto1;
            var uriImagenes = new System.Uri(string.Format(serverImagenes, string.Empty));
            HttpClient ClienteImagenes = new HttpClient();
            ClienteImagenes.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

            var consultaImagenes = await ClienteImagenes.GetAsync(uriImagenes);
            consultaImagenes.EnsureSuccessStatusCode();

            if (consultaImagenes.IsSuccessStatusCode)
            {
                JsonValue ConsultaJson = await consultaImagenes.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ImagenesNube>(ConsultaJson);
                return data;
            }
            else
            {
                return await FiltroImagenes(foto1);
            }
        }
        public async System.Threading.Tasks.Task FiltrosFechasAsync(string fecha)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                #region Progress
                ProgressBar progressBar;
                progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                FindViewById<RelativeLayout>(Resource.Id.FondoDiario).AddView(progressBar, p);

                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                #endregion

                contenido = FindViewById<ListView>(Resource.Id.ListDiary);

                contenido.ScrollStateChanged += delegate {
                    if (contenido.LastVisiblePosition == contenido.Adapter.Count - 1 && contenido.GetChildAt(contenido.ChildCount - 1).Bottom <= contenido.Height)
                    {
                        #region Mover el boton izquierda;

                        ObjectAnimator animX = ObjectAnimator.OfFloat(btnAddDiary, "X", new ShareInside().ConvertDPToPixels(10, Resources.DisplayMetrics.Density));
                        ObjectAnimator animY = ObjectAnimator.OfFloat(btnAddDiary, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                        ObjectAnimator animSX = ObjectAnimator.OfFloat(btnAddDiary, "scaleX", 1f);
                        ObjectAnimator animSY = ObjectAnimator.OfFloat(btnAddDiary, "scaleY", 1f);

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
                        ObjectAnimator animX = ObjectAnimator.OfFloat(btnAddDiary, "X", ScreenWith - new ShareInside().ConvertDPToPixels(70, Resources.DisplayMetrics.Density));
                        ObjectAnimator animY = ObjectAnimator.OfFloat(btnAddDiary, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                        ObjectAnimator animSX = ObjectAnimator.OfFloat(btnAddDiary, "scaleX", 1f);
                        ObjectAnimator animSY = ObjectAnimator.OfFloat(btnAddDiary, "scaleY", 1f);

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

                #region Servidor
                string server = "https://cabasus-mobile.azurewebsites.net/v1/journals";
                var uri = new System.Uri(string.Format(server, string.Empty));
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                var consulta = await Cliente.GetAsync(uri);
                consulta.EnsureSuccessStatusCode();
                #endregion

                if (consulta.IsSuccessStatusCode)
                {
                    JsonValue ConsultaJson = await consulta.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<JornadasNube>>(ConsultaJson);
                    if (data.Count > 0)
                    {
                        #region Jornadas nube por fecha
                        List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                        ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                        foreach (var item in data)
                        {
                            if (item.date.Substring(0, 10) == fecha && item.horse == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected && item.owner == new ShareInside().ConsultarDatosUsuario()[0].id && item.visibility == "private")
                            {
                                #region Servidor Imagenes
                                ModeloFoto1 = await FiltroImagenes(item.images[0]);
                                string Foto1 = System.Text.Encoding.UTF8.GetString(ModeloFoto1.data.data);

                                ModeloFoto2 = await FiltroImagenes(item.images[1]);
                                string Foto2 = System.Text.Encoding.UTF8.GetString(ModeloFoto2.data.data);

                                ModeloFoto3 = await FiltroImagenes(item.images[2]);
                                string Foto3 = System.Text.Encoding.UTF8.GetString(ModeloFoto3.data.data);
                                #endregion

                                var datos = new DiarioLocal();
                                datos.descripcion = item.content;
                                datos.dates = item.date;
                                datos.estado_caballo = item.horse_status;
                                datos.id_diary = item.id;
                                datos.foto1 = Foto1;
                                datos.foto2 = Foto2;
                                datos.foto3 = Foto3;
                                datos.fk_usuario = item.owner;
                                datos.fk_caballo = item.horse;
                                ListaDiario.Add(datos);
                            }
                        }
                        #endregion

                        #region jornadas locales por fecha
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'private' and dates = '" + fecha + "';", (new DiarioLocal()).id_diary);
                        if (ConsultaDiarios.Count > 0)
                        {
                            foreach (var item in ConsultaDiarios)
                            {
                                var datos = new DiarioLocal();
                                datos.id_diary = item.id_diary.ToString();
                                datos.fk_usuario = item.fk_usuario;
                                datos.fk_caballo = item.fk_caballo;
                                datos.dates = item.dates;
                                datos.estado_caballo = item.estado_caballo.ToString();
                                datos.descripcion = item.descripcion;
                                datos.foto1 = item.foto1;
                                datos.foto2 = item.foto2;
                                datos.foto3 = item.foto3;
                                ListaDiario.Add(datos);
                            }
                        }
                        #endregion

                        #region Sin jornadas de la nube ni locales de la fecha seleccionada
                        if (ListaDiario.Count == 0)
                        {
                            var datos = new DiarioLocal();
                            datos.descripcion = GetText(Resource.String.There_arent_diaries);
                            ListaDiario.Add(datos);
                            contenido.Adapter = new AdapterDiarios(this, ListaDiario, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                        #endregion
                        else
                        {
                            contenido.Adapter = new AdapterDiarios(this, ListaDiario, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                    }
                    else
                    {
                        try
                        {
                            #region sin jornadas en la nube
                            #region con jornadas locales
                            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                            var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where dates = '" + fecha + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'private' ;", (new DiarioLocal()).id_diary);
                            ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                            if (ConsultaDiarios.Count > 0)
                            {
                                List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                                foreach (var item in ConsultaDiarios)
                                {
                                    var datos = new DiarioLocal();
                                    datos.id_diary = item.id_diary.ToString();
                                    datos.fk_usuario = item.fk_usuario;
                                    datos.fk_caballo = item.fk_caballo;
                                    datos.dates = item.dates;
                                    datos.estado_caballo = item.estado_caballo.ToString();
                                    datos.descripcion = item.descripcion;
                                    datos.foto1 = item.foto1;
                                    datos.foto2 = item.foto2;
                                    datos.foto3 = item.foto3;
                                    ListaDiario.Add(datos);
                                }
                                contenido.Adapter = new AdapterDiarios(this, ListaDiario, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            }
                            #endregion
                            #region sin jornadas locales
                            else
                            {
                                List<DiarioLocal> ListaDiario = new List<DiarioLocal>();

                                ListaDiario.Add(new DiarioLocal()
                                {
                                    descripcion = GetString(Resource.String.There_arent_diaries)
                                });
                                contenido.Adapter = new AdapterDiarios(this, ListaDiario, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            }
                            #endregion
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                    }
                }
                else
                {
                    try
                    {
                        #region con jornadas locales

                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where dates = '" + fecha + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'private' ;", (new DiarioLocal()).id_diary);
                        ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                        if (ConsultaDiarios.Count > 0)
                        {
                            List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                            foreach (var item in ConsultaDiarios)
                            {
                                var datos = new DiarioLocal();
                                datos.id_diary = item.id_diary.ToString();
                                datos.fk_usuario = item.fk_usuario;
                                datos.fk_caballo = item.fk_caballo;
                                datos.dates = item.dates;
                                datos.estado_caballo = item.estado_caballo.ToString();
                                datos.descripcion = item.descripcion;
                                datos.foto1 = item.foto1;
                                datos.foto2 = item.foto2;
                                datos.foto3 = item.foto3;
                                ListaDiario.Add(datos);
                            }
                            contenido.Adapter = new AdapterDiarios(this, ListaDiario, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                        #endregion
                        #region sin jornadas locales
                        else
                        {
                            List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                            var datos = new DiarioLocal();
                            datos.descripcion = GetText(Resource.String.There_arent_diaries);
                            ListaDiario.Add(datos);
                            contenido.Adapter = new AdapterDiarios(this, ListaDiario, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    }
                }
            }
            else
            {
                ProgressBar progressBar;
                progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                FindViewById<RelativeLayout>(Resource.Id.FondoDiario).AddView(progressBar, p);

                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                contenido = FindViewById<ListView>(Resource.Id.ListDiary);
                contenido.ScrollStateChanged += delegate {
                    if (contenido.LastVisiblePosition == contenido.Adapter.Count - 1 && contenido.GetChildAt(contenido.ChildCount - 1).Bottom <= contenido.Height)
                    {
                        #region Mover el boton izquierda;

                        ObjectAnimator animX = ObjectAnimator.OfFloat(btnAddDiary, "X", new ShareInside().ConvertDPToPixels(10, Resources.DisplayMetrics.Density));
                        ObjectAnimator animY = ObjectAnimator.OfFloat(btnAddDiary, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                        ObjectAnimator animSX = ObjectAnimator.OfFloat(btnAddDiary, "scaleX", 1f);
                        ObjectAnimator animSY = ObjectAnimator.OfFloat(btnAddDiary, "scaleY", 1f);

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
                        ObjectAnimator animX = ObjectAnimator.OfFloat(btnAddDiary, "X", ScreenWith - new ShareInside().ConvertDPToPixels(70, Resources.DisplayMetrics.Density));
                        ObjectAnimator animY = ObjectAnimator.OfFloat(btnAddDiary, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                        ObjectAnimator animSX = ObjectAnimator.OfFloat(btnAddDiary, "scaleX", 1f);
                        ObjectAnimator animSY = ObjectAnimator.OfFloat(btnAddDiary, "scaleY", 1f);

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
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where dates = '" + fecha + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'private' ;", (new DiarioLocal()).id_diary);
                ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                if (ConsultaDiarios.Count > 0)
                {
                    List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                    foreach (var item in ConsultaDiarios)
                    {
                        var datos = new DiarioLocal();
                        datos.id_diary = item.id_diary;
                        datos.fk_usuario = item.fk_usuario;
                        datos.fk_caballo = item.fk_caballo;
                        datos.dates = item.dates;
                        datos.estado_caballo = item.estado_caballo;
                        datos.descripcion = item.descripcion;
                        datos.foto1 = item.foto1;
                        datos.foto2 = item.foto2;
                        datos.foto3 = item.foto3;
                        ListaDiario.Add(datos);
                    }
                    contenido.Adapter = new AdapterDiarios(this, ListaDiario, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);

                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                }
                else
                {
                    List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                    var datos = new DiarioLocal();
                    datos.descripcion = GetText(Resource.String.There_arent_diaries);
                    ListaDiario.Add(datos);
                    contenido.Adapter = new AdapterDiarios(this, ListaDiario, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);

                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                }
            }
        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }
        private float ConvertDPToPixels(int DP)
        {
            var Pix = ((DP) * Resources.DisplayMetrics.Density);
            return Pix;
        }
        public int CurrentTimeMillis()
        {
            return contador++;
        }
        
        public void OnClick(IDialogInterface dialog, int which)
        {
            throw new NotImplementedException();
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

    public class AdapterDiarios : BaseAdapter<DiarioLocal>
    {
        List<DiarioLocal> items;
        Android.App.Activity Context;
        bool bandera;
        ImagenesNube id_Foto1, id_Foto2, id_Foto3;

        public AdapterDiarios(Android.App.Activity context, List<DiarioLocal> items, bool bandera, ImagenesNube _id_Foto1, ImagenesNube _id_Foto2, ImagenesNube _id_Foto3) :
            base()
        {
            this.Context = context;
            this.items = items;
            this.bandera = bandera;
            this.id_Foto1 = _id_Foto1;
            this.id_Foto2 = _id_Foto2;
            this.id_Foto3 = _id_Foto3;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override DiarioLocal this[int position]
        {
            get { return items[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!bandera)
            {
                View viewNodata = convertView;
                viewNodata = Context.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                if (items.Count == 0)
                {
                    var datos = new DiarioLocal();
                    datos.descripcion = Context.GetText(Resource.String.There_arent_diaries);
                    items.Add(datos);
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].descripcion;
                }
                else
                {
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].descripcion;
                }
                return viewNodata;
            }
            else
            {
                var item = items[position];
                View view = convertView;

                view = Context.LayoutInflater.Inflate(Resource.Layout.layout_ContenidoDiario, null);

                var imgEstado = view.FindViewById<ImageView>(Resource.Id.imgEstadoDiario);
                var txtDescripcion = view.FindViewById<TextView>(Resource.Id.lblDescripcionDiario);
                var btnDetalles = view.FindViewById<ImageView>(Resource.Id.btmDetallesDiario);
                var btnOpciones = view.FindViewById<ImageView>(Resource.Id.btnOpcionesDiario);

                try
                {
                    int.Parse(items[position].id_diary);
                    view.FindViewById<TextView>(Resource.Id.txtUbicacionDiario).Text = "Local";
                }
                catch (Exception)
                {

                }

                txtDescripcion.Text = items[position].descripcion;
                if (items[position].estado_caballo == "1")
                {
                    var imgfeliz = ContextCompat.GetDrawable(Context.ApplicationContext, Resource.Drawable.ResumenEmoticonhappyDorado);
                    imgEstado.SetImageDrawable(imgfeliz);
                }
                else if (items[position].estado_caballo == "2")
                {
                    var imgfeliz = ContextCompat.GetDrawable(Context.ApplicationContext, Resource.Drawable.ResumenEmoticonneutraldorado);
                    imgEstado.SetImageDrawable(imgfeliz);
                }
                else
                {
                    var imgfeliz = ContextCompat.GetDrawable(Context.ApplicationContext, Resource.Drawable.ResumenEmoticonsaddorado);
                    imgEstado.SetImageDrawable(imgfeliz);
                }

                btnDetalles.Click += delegate {
                    Android.App.Dialog alertar = new Android.App.Dialog(Context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.layout_DetallesDiario);

                    var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + ".png"));
                    Java.IO.File ImagenUri = new Java.IO.File(RutaImage.ToString());
                    Bitmap ImagenBitMap = BitmapFactory.DecodeFile(ImagenUri.AbsolutePath);
                    alertar.FindViewById<ImageView>(Resource.Id.imgPortada).SetImageBitmap(ImagenBitMap);

                    byte[] imageAsBytes = Base64.Decode((new ShareInside().ConsultarDatosUsuario()[0].photo.data), Base64Flags.Default);
                    var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgUsuarioDetalles).SetImageBitmap(bit);

                    alertar.FindViewById<TextView>(Resource.Id.lblNombreCaballo).Text = new ShareInside().ConsultarPosicionPiker()[0].Name_HoserSelected;

                    alertar.FindViewById<TextView>(Resource.Id.lblNombreUsuario).Text = new ShareInside().ConsultarDatosUsuario()[0].username;

                    alertar.FindViewById<TextView>(Resource.Id.lblFechaDetalles).Text = items[position].dates;

                    alertar.FindViewById<TextView>(Resource.Id.txtDescripcionDetalles).Text = items[position].descripcion;

                    byte[] imageAsBytes1 = Base64.Decode(items[position].foto1, Base64Flags.Default);
                    var bit1 = BitmapFactory.DecodeByteArray(imageAsBytes1, 0, imageAsBytes1.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto1Detalles).SetImageBitmap(bit1);
                    byte[] imageAsBytes2 = Base64.Decode(items[position].foto2, Base64Flags.Default);
                    var bit2 = BitmapFactory.DecodeByteArray(imageAsBytes2, 0, imageAsBytes2.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto2Detalles).SetImageBitmap(bit2);
                    byte[] imageAsBytes3 = Base64.Decode(items[position].foto3, Base64Flags.Default);
                    var bit3 = BitmapFactory.DecodeByteArray(imageAsBytes3, 0, imageAsBytes3.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto3Detalles).SetImageBitmap(bit3);

                    alertar.Show();
                };
                btnOpciones.Click += delegate {
                    Android.App.Dialog alertar = new Android.App.Dialog(Context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.Layout_CustomAlertOptionsJornadas);

                    alertar.FindViewById<TextView>(Resource.Id.lblDescripcionJornada).Text = items[position].descripcion;

                    byte[] imageAsBytes = Base64.Decode(items[position].foto1, Base64Flags.Default);
                    var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto1Reminder).SetImageBitmap(new ShareInside().RedondearbitmapImagen(new ShareInside().getResizedBitmap(bit, 60, 60), 400)); ;
                    byte[] imageAsBytes2 = Base64.Decode(items[position].foto2, Base64Flags.Default);
                    var bit2 = BitmapFactory.DecodeByteArray(imageAsBytes2, 0, imageAsBytes2.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto2Reminder).SetImageBitmap(new ShareInside().RedondearbitmapImagen(new ShareInside().getResizedBitmap(bit2, 60, 60), 400)); ;
                    byte[] imageAsBytes3 = Base64.Decode(items[position].foto3, Base64Flags.Default);
                    var bit3 = BitmapFactory.DecodeByteArray(imageAsBytes3, 0, imageAsBytes3.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto3Reminder).SetImageBitmap(new ShareInside().RedondearbitmapImagen(new ShareInside().getResizedBitmap(bit3, 60, 60), 400)); ;

                    alertar.FindViewById<TextView>(Resource.Id.btnEliminar).Click += async delegate {

                        #region Progress
                        ProgressBar progressBar;
                        progressBar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                        alertar.FindViewById<RelativeLayout>(Resource.Id.OptionsJornadas).AddView(progressBar, p);

                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        #endregion

                        try
                        {
                            int.Parse(items[position].id_diary);

                            try
                            {
                                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                                var ConsultaDiarios = con.Query<DiarioLocal>("delete from diarys where id_diary = " + items[position].id_diary + " and fk_usuario = '" + items[position].fk_usuario + "' and fk_caballo = '" + items[position].fk_caballo + "' and dates = '" + items[position].dates + "';", (new DiarioLocal()).id_diary);
                                items.Remove(items[position]);
                                if (items.Count == 0)
                                {
                                    bandera = false;
                                    GetView(position, convertView, parent);
                                }
                                NotifyDataSetChanged();

                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                alertar.Dismiss();
                            }
                            catch (Exception ex)
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                Toast.MakeText(Context, Resource.String.The_information, ToastLength.Short).Show();
                            }
                        }
                        catch (Exception)
                        {
                            await EliminarImagen(id_Foto1.id);
                            await EliminarImagen(id_Foto2.id);
                            await EliminarImagen(id_Foto3.id);

                            #region Servidor
                            string server = "https://cabasus-mobile.azurewebsites.net/v1/journal/" + items[position].id_diary;
                            var uri = new System.Uri(string.Format(server, string.Empty));
                            HttpClient Cliente = new HttpClient();
                            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                            var eliminar = await Cliente.DeleteAsync(uri);
                            eliminar.EnsureSuccessStatusCode();
                            #endregion
                            #region Eliminar recordatorio nube
                            if (eliminar.IsSuccessStatusCode)
                            {
                                try
                                {
                                    items.Remove(items[position]);
                                    if (items.Count == 0)
                                    {
                                        bandera = false;
                                        GetView(position, convertView, parent);
                                    }
                                    NotifyDataSetChanged();

                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                    alertar.Dismiss();
                                }
                                catch (Exception ex)
                                {
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                    Toast.MakeText(Context, Resource.String.The_information, ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                Toast.MakeText(Context, Resource.String.You_need_internet, ToastLength.Short).Show();
                            }
                                
                            #endregion
                        }
                    };

                    alertar.FindViewById<TextView>(Resource.Id.btnEditar).Click += delegate {
                        try
                        {
                            int.Parse(items[position].id_diary);
                            Intent intent = new Intent(Context, typeof(Activity_NuevoDiario));
                            intent.PutExtra("ActualizarDiario",
                                "true" + "$" +
                                items[position].id_diary);
                            Context.StartActivity(intent);
                            Context.Finish();
                        }
                        catch (Exception)
                        {
                            Intent intent = new Intent(Context, typeof(Activity_NuevoDiario));
                            intent.PutExtra("ActualizarDiario",
                                "true" + "$" +
                                items[position].id_diary + "$" +
                                System.Text.Encoding.UTF8.GetString(id_Foto1.data.data) + "$" +
                                System.Text.Encoding.UTF8.GetString(id_Foto2.data.data) + "$" +
                                System.Text.Encoding.UTF8.GetString(id_Foto3.data.data) + "$" +
                                id_Foto1.id + "$" +
                                id_Foto2.id + "$" +
                                id_Foto3.id);
                            Context.StartActivity(intent);
                            Context.Finish();
                        }
                    };

                    alertar.Show();
                };
                return view;
            }
        }
        public async Task EliminarImagen(string id_imagen)
        {
            #region Servidor
            string server = "https://cabasus-mobile.azurewebsites.net/v1/images/" + id_imagen;
            var uri = new System.Uri(string.Format(server, string.Empty));
            HttpClient Cliente = new HttpClient();
            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

            var eliminar = await Cliente.DeleteAsync(uri);
            eliminar.EnsureSuccessStatusCode();
            #endregion
            #region Eliminar recordatorio nube
            if (eliminar.IsSuccessStatusCode) { }
            else
                await EliminarImagen(id_imagen);
            #endregion
        }
    }
}