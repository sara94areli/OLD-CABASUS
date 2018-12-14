using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Gigamole.Infinitecycleviewpager;
using Android.Util;
using Android.Text.Format;
using Android.Graphics;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;
using SQLite;
using Plugin.Connectivity;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using Android.Content.PM;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ActivityNuevoRecordatorio : AppCompatActivity
    {
        List<string> lstImages = new List<string>();
        List<string> lstNombre = new List<string>();
        List<string> lstId = new List<string>();

        bool ActualizarRecordatorio = false;
        string Date;
        string id_recordatorio;
        int RecordatorioTipo = 1;
        int SegundosTotales = 900;

        PickerDate onDateSetListener, onDateSetListenerFin;
        TextView TextoInicio, TextoFin;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_nuevo_recordatorio);
            Window.SetStatusBarColor(Color.Black);
            #region Referencias de los botones
            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
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

            #region Selector Caballos
            llenarListaCaballo();

            HorizontalInfiniteCycleViewPager cycleViewPager = FindViewById<HorizontalInfiniteCycleViewPager>(Resource.Id.horizontal_viewPager_nr);
            CaballosUpFormat CaballosUpFormat = new CaballosUpFormat(lstNombre, lstId, BaseContext, cycleViewPager);
            cycleViewPager.Adapter = CaballosUpFormat;
            cycleViewPager.CurrentItem = new ShareInside().ConsultarPosicionPiker()[0].Position_HorseSelected;
            cycleViewPager.PageSelected += delegate
            {
                new ShareInside().GuardarPosicionPiker(lstId[cycleViewPager.RealItem], lstNombre[cycleViewPager.RealItem], cycleViewPager.RealItem);
            };
            #endregion

            #region Datos Usuario
            var ImagenUsuario = FindViewById<ImageView>(Resource.Id.imgUserRecordatorios);
            byte[] imageAsBytes = Base64.Decode((new ShareInside().ConsultarDatosUsuario()[0].photo.data), Base64Flags.Default);
            var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
            ImagenUsuario.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));
            var NombreUsuario = FindViewById<TextView>(Resource.Id.lblNombreUserRecordatorios);
            NombreUsuario.Text = new ShareInside().ConsultarDatosUsuario()[0].username;
            var Fecha = FindViewById<TextView>(Resource.Id.lblFechaDiaRecordatorios);
            Date = this.Intent.GetStringExtra("Date");
            Fecha.Text = Date;
            #endregion

            #region Cancelar
            var btnCancel = FindViewById<TextView>(Resource.Id.btnCancelRecordatorio);
            btnCancel.Click += delegate {
                Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                intent.PutExtra("Date", Fecha.Text);
                this.StartActivity(intent);
                Finish();
            };
            #endregion

            #region txtDescripcion
            var txtDescripcion = FindViewById<EditText>(Resource.Id.txtDescriptionRecordatorio);
            Window.SetSoftInputMode(SoftInput.StateHidden);
            txtDescripcion.Click += delegate
            {
                txtDescripcion.RequestFocus();
                Window.SetSoftInputMode(SoftInput.StateVisible);
            };
            #endregion

            #region Tipo Recordatorio
            var TipoRecordatorio = FindViewById<ImageView>(Resource.Id.btnTypeReminder);
            TipoRecordatorio.Tag = 1;
            TipoRecordatorio.Click += delegate
            {
                Android.App.Dialog alertar = new Android.App.Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                List<TypeRecordatorio> ListaTipo = new List<TypeRecordatorio>();
                List<int> Tiempos = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                foreach (var item in Tiempos)
                {
                    var tipos = new TypeRecordatorio();
                    tipos.Imagen = item;
                    ListaTipo.Add(tipos);
                }

                alertar.SetContentView(Resource.Layout.layout_CustomTipo);
                alertar.FindViewById<ListView>(Resource.Id.ListTipos).Adapter = new AdapterTipo(this, ListaTipo, TipoRecordatorio, alertar, RecordatorioTipo);
                alertar.Show();
            };
            #endregion

            #region Inicio
            var btnInicio = FindViewById<Button>(Resource.Id.btnBegin);
            var btnFin = FindViewById<Button>(Resource.Id.btnEnd);

            btnInicio.Text = DateTime.Now.AddMinutes(15).ToString("dd-MM-yyyy HH:mm:ss");
            btnFin.Text = DateTime.Now.AddMinutes(30).ToString("dd-MM-yyyy HH:mm:ss");

            TextoInicio = new TextView(this);
            btnInicio.Click += delegate
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
            onDateSetListener = new PickerDate(TextoInicio);

            TextoInicio.TextChanged += delegate
            {
                TimePickerFragment frag = TimePickerFragment.NewInstance(delegate (TimeSpan time)
                {
                    btnInicio.Text = TextoInicio.Text + " " + time.ToString();
                });
                frag.Show(FragmentManager, TimePickerFragment.TAG);
            };

            btnInicio.TextChanged += delegate {
                if (DateTime.ParseExact(btnInicio.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                {
                    Toast.MakeText(this, Resource.String.The_start_date_cant, ToastLength.Short).Show();
                    btnInicio.Text = DateTime.Now.AddMinutes(15).ToString("dd-MM-yyyy HH:mm:ss");
                }
            };
            #endregion

            #region Fin 
            TextoFin = new TextView(this);
            btnFin.Click += delegate
            {
                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                int year = calendar.Get(Java.Util.CalendarField.Year);
                int month = calendar.Get(Java.Util.CalendarField.Month);
                int day_of_month = calendar.Get(Java.Util.CalendarField.DayOfMonth);
                DatePickerDialog dialog;

                dialog = new DatePickerDialog(this, Resource.Style.ThemeOverlay_AppCompat_Dialog_Alert,
                onDateSetListenerFin, year, month, day_of_month);
                dialog.Show();
            };
            onDateSetListenerFin = new PickerDate(TextoFin);

            TextoFin.TextChanged += delegate
            {
                TimePickerFragment frag = TimePickerFragment.NewInstance(delegate (TimeSpan time)
                {
                    btnFin.Text = TextoFin.Text + " " + time.ToString();
                });
                frag.Show(FragmentManager, TimePickerFragment.TAG);
            };

            btnFin.TextChanged += delegate {
                if (DateTime.ParseExact(btnFin.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.ParseExact(btnInicio.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                {
                    Toast.MakeText(this, Resource.String.The_end_ate_cant_be_less, ToastLength.Short).Show();
                    btnFin.Text = DateTime.ParseExact(btnInicio.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).AddMinutes(15).ToString("dd-MM-yyyy HH:mm:ss");
                }
            };
            #endregion

            #region Tiempo de notificacion
            var TiempoNotificacion = FindViewById<Button>(Resource.Id.btnNotification);
            TiempoNotificacion.Text = GetText(Resource.String.minutes_before);
            TiempoNotificacion.Click += delegate
            {
                Android.App.Dialog alertar = new Android.App.Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.Layout_Momento);
                int Contador = 5;
                string[] Cadena = new string[6] { "0", "0", "0", "0", "0", "0" };
                alertar.FindViewById<TextView>(Resource.Id.lbl1).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "1");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl2).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "2");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl3).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "3");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl4).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "4");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl5).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "5");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl6).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "6");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl7).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "7");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl8).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "8");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl9).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "9");
                };
                alertar.FindViewById<TextView>(Resource.Id.lbl0).Click += delegate {
                    Contador = Momento(Contador, Cadena, alertar, "0");
                };
                alertar.FindViewById<TextView>(Resource.Id.btnBorrar).Click += delegate {
                    Contador = Borrar(Contador, Cadena, alertar);
                };
                alertar.FindViewById<TextView>(Resource.Id.btnListoMomento).Click += delegate {
                    int seg = (int.Parse(Cadena[4] + Cadena[5]))
                                + (int.Parse(Cadena[2] + Cadena[3]) * (60))
                                + (int.Parse(Cadena[0] + Cadena[1]) * (3600));
                    if ((seg / 60) < 15)
                    {
                        Toast.MakeText(this, Resource.String.The_minimum_time, ToastLength.Short).Show();
                    }
                    else
                    {
                        SegundosTotales = seg;
                        TiempoNotificacion.Text = new ShareInside().StoH(seg) + " "+ Resource.String.hours_before;
                        alertar.Dismiss();
                    }
                };
                alertar.Show();
            };
            #endregion

            string cadena = "";

            try
            {
                cadena = this.Intent.GetStringExtra("ActualizarRecordatorio");
                ActualizarRecordatorio = bool.Parse(cadena.Split('$')[0]);
            }
            catch (System.Exception) { }

            if (ActualizarRecordatorio)
            {
                id_recordatorio = cadena.Split('$')[1];
                try
                {
                    int.Parse(id_recordatorio);
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var ConsultaRecordatorio = con.Query<RedordatoriosLocales>("select * from reminders where id_reminder = " + id_recordatorio + ";", (new RedordatoriosLocales()).id_reminder);

                    Fecha.Text = ConsultaRecordatorio[0].fecha;
                    txtDescripcion.Text = ConsultaRecordatorio[0].descripcion;
                    btnInicio.Text = ConsultaRecordatorio[0].inicio;
                    btnFin.Text = ConsultaRecordatorio[0].fin;
                    TiempoNotificacion.Text = new ShareInside().StoH(int.Parse(ConsultaRecordatorio[0].notificacion)) + " " + Resource.String.hours_before;
                    SegundosTotales = int.Parse(ConsultaRecordatorio[0].notificacion);

                    ActualizarTipo(ConsultaRecordatorio[0].Tipo, TipoRecordatorio);
                }
                catch (Exception)
                {
                    #region Servidor
                    string server = "https://cabasus-mobile.azurewebsites.net/v1/events/" + id_recordatorio;
                    var uri = new System.Uri(string.Format(server, string.Empty));
                    HttpClient Cliente = new HttpClient();
                    Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                    var consultar = await Cliente.GetAsync(uri);
                    consultar.EnsureSuccessStatusCode();
                    #endregion
                    #region consultar recordatorio nube
                    if (consultar.IsSuccessStatusCode)
                    {
                        JsonValue ConsultaJson = await consultar.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<RecordatoriosNube>(ConsultaJson);
                        ActualizarTipo(data.type.ToString(), TipoRecordatorio);
                        Fecha.Text = data.date.Substring(0, 10);
                        txtDescripcion.Text = data.description;
                        btnInicio.Text = DateTime.Parse(data.date.Substring(0, 10)).ToString("dd-MM-yyyy") + " " + data.date.Substring(11, 8);
                        btnFin.Text = DateTime.Parse(data.end_date.Substring(0, 10)).ToString("dd-MM-yyyy") + " " + data.end_date.Substring(11, 8);
                        TiempoNotificacion.Text = new ShareInside().StoH(data.alert_before) + " " + Resource.String.hours_before;
                        SegundosTotales = int.Parse(data.alert_before.ToString());
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                        Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                        intent.PutExtra("Date", Fecha.Text);
                        this.StartActivity(intent);
                        Finish();
                    }
                    #endregion
                }
            }

            #region Guardar
            var Guardar = FindViewById<TextView>(Resource.Id.btnDoneRecordatorio);
            Guardar.Click += async delegate {
                if (string.IsNullOrEmpty(txtDescripcion.Text))
                    Toast.MakeText(this, Resource.String.description_saving_reminder, ToastLength.Short).Show();
                else if (DateTime.ParseExact(btnInicio.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                {
                    Toast.MakeText(this, Resource.String.The_start_date_cant, ToastLength.Short).Show();
                    btnInicio.Text = DateTime.Now.AddMinutes(15).ToString("dd-MM-yyyy HH:mm:ss");
                }
                else if (DateTime.ParseExact(btnFin.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.ParseExact(btnInicio.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                {
                    Toast.MakeText(this, Resource.String.The_end_ate_cant_be_less, ToastLength.Short).Show();
                    btnFin.Text = DateTime.ParseExact(btnInicio.Text, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).AddMinutes(15).ToString("dd-MM-yyyy HH:mm:ss");
                }
                else if ((SegundosTotales / 60) < 15)
                {
                    Toast.MakeText(this, Resource.String.The_minimum_time, ToastLength.Short).Show();
                }
                else
                {
                    #region ProgressBar
                    ProgressBar progressBar;
                    progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                    RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                    p.AddRule(LayoutRules.CenterInParent);
                    progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                    FindViewById<RelativeLayout>(Resource.Id.FondoNuevoRecordatorio).AddView(progressBar, p);

                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                    Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    #endregion
                    try
                    {
                        if (ActualizarRecordatorio)
                        {
                            try
                            {
                                int.Parse(id_recordatorio);
                                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                                var Actualizar = con.Query<RedordatoriosLocales>(
                                    "update reminders set " +
                                    "descripcion = '" + txtDescripcion.Text + "'," +
                                    " inicio = '" + btnInicio.Text + "'," +
                                    " fin = '" + btnFin.Text + "', " +
                                    "notificacion = '" + SegundosTotales + "'," +
                                    " fecha = '" + DateTime.ParseExact(btnInicio.Text.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") + "'," +
                                    " tipo = '" + int.Parse(TipoRecordatorio.Tag.ToString()) + "' " +
                                    "where id_reminder = " + id_recordatorio + ";",
                                    (new RedordatoriosLocales()).id_reminder);
                                con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                                intent.PutExtra("Date", Fecha.Text);
                                this.StartActivity(intent);
                                Finish();
                            }
                            catch (Exception)
                            {
                                #region Servidor
                                string server = "https://cabasus-mobile.azurewebsites.net/v1/events/" + id_recordatorio;
                                string content = "application/json";
                                HttpClient Cliente = new HttpClient();
                                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                                string FechaInicio = DateTime.ParseExact(btnInicio.Text.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                                string TiempoInicio = btnInicio.Text.Substring(11, 8);
                                string Inicio = FechaInicio + "T" + TiempoInicio + ".456Z";

                                string FechaFin = DateTime.ParseExact(btnFin.Text.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                                string TiempoFin = btnFin.Text.Substring(11, 8);
                                string Fin = FechaFin + "T" + TiempoFin + ".456Z";

                                var datos = new ActualizarRecordatorioNube() {
                                    horse = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                                    description = txtDescripcion.Text,
                                    date = Inicio,
                                    end_date = Fin,
                                    type = int.Parse(TipoRecordatorio.Tag.ToString()),
                                    alert_before = SegundosTotales
                                };

                                var json = JsonConvert.SerializeObject(datos, Formatting.Indented, new JsonSerializerSettings());
                                var actualizar = await Cliente.PutAsync(server, new StringContent(json.ToString(), Encoding.UTF8, content));

                                if (actualizar.IsSuccessStatusCode)
                                {
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                    Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                                    intent.PutExtra("Date", Fecha.Text);
                                    this.StartActivity(intent);
                                    Finish();
                                }
                                else
                                {
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                    Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            if (CrossConnectivity.Current.IsConnected)
                            {
                                #region modelo 
                                string FechaInicio = DateTime.ParseExact(btnInicio.Text.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                                string TiempoInicio = btnInicio.Text.Substring(11, 8);
                                string Inicio = FechaInicio + "T" + TiempoInicio + ".456Z";

                                string FechaFin = DateTime.ParseExact(btnFin.Text.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                                string TiempoFin = btnFin.Text.Substring(11, 8);
                                string Fin = FechaFin + "T" + TiempoFin + ".456Z";

                                AgregarRecordatoriosNube RecordatorioNube = new AgregarRecordatoriosNube()
                                {
                                    horse = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                                    description = txtDescripcion.Text,
                                    date = Inicio,
                                    end_date = Fin,
                                    type = int.Parse(TipoRecordatorio.Tag.ToString()),
                                    alert_before = SegundosTotales
                                };
                                #endregion

                                #region Servidor recordatorios
                                string serverRecordatorio = "https://cabasus-mobile.azurewebsites.net/v1/events";
                                string jsonRecordatorio = "application/json";
                                HttpClient clienteRecordatorio = new HttpClient();
                                #endregion

                                try
                                {
                                    #region Guardado de recordatorio
                                    var jsonConvertRecordatorio = JsonConvert.SerializeObject(RecordatorioNube);
                                    clienteRecordatorio.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                                    var respuestaRecordatorio = await clienteRecordatorio.PostAsync(serverRecordatorio, new StringContent(jsonConvertRecordatorio.ToString(), Encoding.UTF8, jsonRecordatorio));
                                    respuestaRecordatorio.EnsureSuccessStatusCode();
                                    if (respuestaRecordatorio.IsSuccessStatusCode)
                                    {
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                                        intent.PutExtra("Date", Fecha.Text);
                                        this.StartActivity(intent);
                                        Finish();
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                                }
                            }
                            else
                            {
                                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                                RecordatorioTipo = int.Parse(TipoRecordatorio.Tag.ToString());
                                var InsertarDiario = con.Query<RedordatoriosLocales>(
                                    "insert into reminders " +
                                    "values(null, " +
                                    "'" + new ShareInside().ConsultarDatosUsuario()[0].id + "'," +
                                    " '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'," +
                                    " '" + txtDescripcion.Text + "'," +
                                    " '" + btnInicio.Text + "'," +
                                    " '" + btnFin.Text + "'," +
                                    " '" + SegundosTotales + "'," +
                                    " '" + DateTime.ParseExact(btnInicio.Text.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") + "'," +
                                    " '" + RecordatorioTipo + "')",
                                    new RedordatoriosLocales().id_reminder);
                                con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                Intent intent = new Intent(this, (typeof(Activity_FiltrosPrincipalesCalendar)));
                                intent.PutExtra("Date", Date);
                                this.StartActivity(intent);
                                Finish();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                    }
                    
                    
                }
            };
            #endregion
        }
        public int Momento(int Contador, string[] Cadena, Android.App.Dialog alertar, string numero)
        {
            if (Contador == 5)
            {
                Cadena[5] = numero;
                Cadena[4] = "0";
                Cadena[3] = "0";
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
                Contador--;
            }
            else if (Contador == 4)
            {
                Cadena[4] = Cadena[5];
                Cadena[3] = "0";
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
                Cadena[5] = numero;
                Contador--;
            }
            else if (Contador == 3)
            {
                Cadena[3] = Cadena[4];
                Cadena[4] = Cadena[5];
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
                Cadena[5] = numero;
                Contador--;
            }
            else if (Contador == 2)
            {
                Cadena[2] = Cadena[3];
                Cadena[3] = Cadena[4];
                Cadena[4] = Cadena[5];
                Cadena[1] = "0";
                Cadena[0] = "0";
                Cadena[5] = numero;
                Contador--;
            }
            else if (Contador == 1)
            {
                Cadena[1] = Cadena[2];
                Cadena[2] = Cadena[3];
                Cadena[3] = Cadena[4];
                Cadena[4] = Cadena[5];
                Cadena[0] = "0";
                Cadena[5] = numero;
                Contador--;
            }
            else if (Contador == 0)
            {
                Cadena[0] = Cadena[1];
                Cadena[1] = Cadena[2];
                Cadena[2] = Cadena[3];
                Cadena[3] = Cadena[4];
                Cadena[4] = Cadena[5];
                Cadena[5] = numero;
                Contador--;
            }
            alertar.FindViewById<TextView>(Resource.Id.lblSegundosMomento).Text = Cadena[4] + Cadena[5];
            alertar.FindViewById<TextView>(Resource.Id.lblMinutosMomento).Text = Cadena[2] + Cadena[3];
            alertar.FindViewById<TextView>(Resource.Id.lblHorasMomento).Text = Cadena[0] + Cadena[1];
            return Contador;
        }
        public int Borrar(int Contador, string[] Cadena, Android.App.Dialog alertar)
        {
            if (Contador == 5)
            {
                Cadena[5] = "0";
                Cadena[4] = "0";
                Cadena[3] = "0";
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
            }
            else if (Contador == 4)
            {
                Cadena[5] = Cadena[4];
                Cadena[4] = "0";
                Cadena[3] = "0";
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
                Contador++;
            }
            else if (Contador == 3)
            {
                Cadena[5] = Cadena[4];
                Cadena[4] = Cadena[3];
                Cadena[3] = "0";
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
                Contador++;
            }
            else if (Contador == 2)
            {
                Cadena[5] = Cadena[4];
                Cadena[4] = Cadena[3];
                Cadena[3] = Cadena[2];
                Cadena[2] = "0";
                Cadena[1] = "0";
                Cadena[0] = "0";
                Contador++;
            }
            else if (Contador == 1)
            {
                Cadena[5] = Cadena[4];
                Cadena[4] = Cadena[3];
                Cadena[3] = Cadena[2];
                Cadena[2] = Cadena[1];
                Cadena[1] = "0";
                Cadena[0] = "0";
                Contador++;
            }
            else if (Contador == 0)
            {
                Cadena[5] = Cadena[4];
                Cadena[4] = Cadena[3];
                Cadena[3] = Cadena[2];
                Cadena[2] = Cadena[1];
                Cadena[1] = Cadena[0];
                Cadena[0] = "0";
                Contador++;
            }
            else
            {
                Contador = 0;
                Borrar(Contador, Cadena, alertar);
            }
            alertar.FindViewById<TextView>(Resource.Id.lblSegundosMomento).Text = Cadena[4] + Cadena[5];
            alertar.FindViewById<TextView>(Resource.Id.lblMinutosMomento).Text = Cadena[2] + Cadena[3];
            alertar.FindViewById<TextView>(Resource.Id.lblHorasMomento).Text = Cadena[0] + Cadena[1];
            return Contador;
        }

        public void llenarListaCaballo()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                var Datos = JsonConvert.DeserializeObject<List<HorsesComplete>>(((IdNameHorse)(new XmlSerializer(typeof(IdNameHorse))).Deserialize(Lectura)).DatosCaballo);
                Lectura.Close();
                foreach (var item in Datos)
                {
                    lstNombre.Add(item.name);
                    lstId.Add(item.id);
                }
            }
            else
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);
                foreach (var item in consulta)
                {
                    lstNombre.Add(item.name);
                    lstId.Add(item.id);
                }
            }

        }

        public void ActualizarTipo(string Tipo, ImageView TipoRecordatorio)
        {
            if (Tipo == "1")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenAspirinaD);
                TipoRecordatorio.Tag = 1;
            }
            else if (Tipo == "2")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenFlechasD);
                TipoRecordatorio.Tag = 2;
            }
            else if (Tipo == "3")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenEstrellaD);
                TipoRecordatorio.Tag = 3;
            }
            else if (Tipo == "4")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenPelotaD);
                TipoRecordatorio.Tag = 4;
            }
            else if (Tipo == "5")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenMedallaD);
                TipoRecordatorio.Tag = 5;
            }
            else if (Tipo == "6")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenCaballoD);
                TipoRecordatorio.Tag = 6;
            }
            else if (Tipo == "7")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenManzanaD);
                TipoRecordatorio.Tag = 7;
            }
            else if (Tipo == "8")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenGraneroD);
                TipoRecordatorio.Tag = 8;
            }
            else if (Tipo == "9")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenEscalaD);
                TipoRecordatorio.Tag = 9;
            }
            else if (Tipo == "10")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenGeringaD);
                TipoRecordatorio.Tag = 10;
            }
            else if (Tipo == "11")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenStetoscopioD);
                TipoRecordatorio.Tag = 11;
            }
            else if (Tipo == "12")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenCuboD);
                TipoRecordatorio.Tag = 12;
            }
            else if (Tipo == "13")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenMapaD);
                TipoRecordatorio.Tag = 13;
            }
            else if (Tipo == "14")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenTijerasD);
                TipoRecordatorio.Tag = 14;
            }
            else if (Tipo == "15")
            {
                TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenClavosD);
                TipoRecordatorio.Tag = 15;
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
    public class TimeNotificacion
    {
        public string Tiempo { get; set; }
    }
    public class TypeRecordatorio
    {
        public int Imagen { get; set; }
    }

    public class TimePickerFragment : DialogFragment, TimePickerDialog.IOnTimeSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "Y:" + typeof(TimePickerFragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<TimeSpan> _timeSelectedHandler = delegate { };

        public static TimePickerFragment NewInstance(Action<TimeSpan> onTimeSet)
        {
            TimePickerFragment frag = new TimePickerFragment();
            frag._timeSelectedHandler = onTimeSet;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            Java.Util.Calendar c = Java.Util.Calendar.Instance;
            int hour = c.Get(Java.Util.CalendarField.HourOfDay);
            int minute = c.Get(Java.Util.CalendarField.Minute);
            bool is24HourView = true;
            TimePickerDialog dialog = new TimePickerDialog(Activity,
                                                           this,
                                                           hour,
                                                           minute,
                                                           is24HourView);
            return dialog;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            //Do something when time chosen by user
            TimeSpan selectedTime = new TimeSpan(hourOfDay, minute, 00);
            Log.Debug(TAG, selectedTime.ToString());
            _timeSelectedHandler(selectedTime);
        }
    }

    public class AdapterTiempoNotificar : BaseAdapter<TimeNotificacion>
    {
        List<TimeNotificacion> items;
        Android.App.Activity Context;
        Button btnNotificar;
        Android.App.Dialog alertar;

        public AdapterTiempoNotificar(Android.App.Activity context, List<TimeNotificacion> items, Button btnNotificacion, Android.App.Dialog _alertar) :
            base()
        {
            this.Context = context;
            this.items = items;
            this.btnNotificar = btnNotificacion;
            this.alertar = _alertar;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override TimeNotificacion this[int position]
        {
            get { return items[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            view = Context.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
            view.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].Tiempo;
            view.FindViewById<TextView>(Resource.Id.lblNoData).Click += delegate {
                btnNotificar.Text = view.FindViewById<TextView>(Resource.Id.lblNoData).Text;
                alertar.Dismiss();
            };
            return view;
        }
    }

    public class AdapterTipo : BaseAdapter<TypeRecordatorio>
    {
        List<TypeRecordatorio> items;
        Android.App.Activity Context;
        ImageView TipoRecordatorio;
        Android.App.Dialog alertar;
        int RecordatorioTipo;

        public AdapterTipo(Android.App.Activity context, List<TypeRecordatorio> items, ImageView _TipoRecordatorio, Android.App.Dialog _alertar, int _RecordatorioTipo) :
            base()
        {
            this.Context = context;
            this.items = items;
            this.TipoRecordatorio = _TipoRecordatorio;
            this.alertar = _alertar;
            this.RecordatorioTipo = _RecordatorioTipo;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override TypeRecordatorio this[int position]
        {
            get { return items[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            view = Context.LayoutInflater.Inflate(Resource.Layout.layout_TipoRecordatorio, null);
            if (items[position].Imagen == 1)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenAspirinaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenAspirinaD);
                    TipoRecordatorio.Tag = 1;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 2)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenFlechasD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenFlechasD);
                    TipoRecordatorio.Tag = 2;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 3)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenEstrellaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenEstrellaD);
                    TipoRecordatorio.Tag = 3;
                    RecordatorioTipo = 3;
                    alertar.Dismiss();

                };
            }
            else if (items[position].Imagen == 4)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenPelotaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenPelotaD);
                    TipoRecordatorio.Tag = 4;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 5)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenMedallaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenMedallaD);
                    TipoRecordatorio.Tag = 5;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 6)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenCaballoD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenCaballoD);
                    TipoRecordatorio.Tag = 6;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 7)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenManzanaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenManzanaD);
                    TipoRecordatorio.Tag = 7;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 8)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenGraneroD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenGraneroD);
                    TipoRecordatorio.Tag = 8;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 9)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenEscalaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenEscalaD);
                    TipoRecordatorio.Tag = 9;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 10)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenGeringaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenGeringaD);
                    TipoRecordatorio.Tag = 10;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 11)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenStetoscopioD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenStetoscopioD);
                    TipoRecordatorio.Tag = 11;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 12)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenCuboD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenCuboD);
                    TipoRecordatorio.Tag = 12;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 13)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenMapaD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenMapaD);
                    TipoRecordatorio.Tag = 13;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 14)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenTijerasD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenTijerasD);
                    TipoRecordatorio.Tag = 14;
                    alertar.Dismiss();
                };
            }
            else if (items[position].Imagen == 15)
            {
                view.FindViewById<ImageView>(Resource.Id.imgImagen).SetImageResource(Resource.Drawable.ResumenClavosD);
                view.FindViewById<ImageView>(Resource.Id.imgImagen).Click += delegate {
                    TipoRecordatorio.SetImageResource(Resource.Drawable.ResumenClavosD);
                    TipoRecordatorio.Tag = 15;
                    alertar.Dismiss();
                };
            }
            return view;
        }
    }

    public class RedordatoriosLocales{
        public int id_reminder { get; set; }
        public string fk_Usuario { get; set; }
        public string fk_caballo { get; set; }
        public string descripcion { get; set; }
        public string inicio { get; set; }
        public string fin { get; set; }
        public string notificacion { get; set; }
        public string Tipo { get; set; }
        public string fecha { get; set; }
    }

    public class ActualizarRecordatorioNube
    {
        public string horse { get; set; }
        public string description { get; set; }
        public string date { get; set; }
        public string end_date { get; set; }
        public int type { get; set; }
        public int alert_before { get; set; }
    }
}