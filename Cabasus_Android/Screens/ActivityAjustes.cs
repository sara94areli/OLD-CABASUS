using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SQLite;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityAjustes", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class ActivityAjustes : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_ajustes);
            new ShareInside().GuardarPantallaActual("ActivityAjustes");
            #region declaracion de botones
            LinearLayout btnvolver = FindViewById<LinearLayout>(Resource.Id.layuoutback);
            LinearLayout cuenta = FindViewById<LinearLayout>(Resource.Id.layoutacount);
            LinearLayout terminos = FindViewById<LinearLayout>(Resource.Id.layouttermnos);
            LinearLayout lenguaje = FindViewById<LinearLayout>(Resource.Id.layoutlenguaje);
            LinearLayout logout = FindViewById<LinearLayout>(Resource.Id.layoutlogout);
            LinearLayout acercade = FindViewById<LinearLayout>(Resource.Id.layoutaboutas);
            ListView listalenguaje;
            TextView condiciones;
            #endregion
            Window.SetStatusBarColor(Color.Black);
            //Volver a pantalla ajustes-caballos
            btnvolver.Click += delegate { StartActivity(typeof(ActivitySettings)); Finish(); };
            
            //Abrir pantalla usuarios para actualizar datos
            cuenta.Click += delegate {
                Intent intent = new Intent(this, (typeof(ActivityRegistro)));
                intent.PutExtra("actualizar", "a");
                this.StartActivity(intent);
                Finish();
            };

            #region abrir  dialogo acerca de 

            acercade.Click += delegate {
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.DialogoAcercade);
                alertar.Show();
            };
            #endregion

            #region Abrir opciones de lenguaje
            lenguaje.Click += delegate {
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.DialogoLenguaje);
                listalenguaje = alertar.FindViewById<ListView>(Resource.Id.Listaidiomas);
                List<Lenguaje> consulta= new List<Lenguaje>();
                consulta.Add(new Lenguaje() {
                    idioma = GetText(Resource.String.Spanish)
                });
                consulta.Add(new Lenguaje()
                {
                    idioma = GetText(Resource.String.English)
                });
                consulta.Add(new Lenguaje()
                {
                    idioma = GetText(Resource.String.German)
                });
                listalenguaje.Adapter = new AdaptadorLenguaje(this, consulta);

                alertar.Show();
            };
            #endregion

            #region Terminos y condiciones de uso

            terminos.Click += delegate {
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.DialogoTermininos);
                condiciones=alertar.FindViewById<TextView>(Resource.Id.txtliga);
                condiciones.PaintFlags |= PaintFlags.UnderlineText;
                condiciones.Click += delegate {
                    var uri = Android.Net.Uri.Parse("http://www.cabasus.com");
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                    Finish();
                };

                alertar.Show();
            };
            #endregion

            #region Cerrar Sesion
            logout.Click += delegate {
                Android.App.Dialog alertar = new Android.App.Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.CustomAlertLogOut);
                alertar.FindViewById<TextView>(Resource.Id.btnYesClose).Click += delegate {
                    ConsultarIdTelefono();
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DatosDeLogeo.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ConsultDataUsers.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "PositionCicleView.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "token.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "tokenphone.xml"));
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "pickerNotificacionIdCaballo.xml"));
                    #region Eliminar de base de datos local
                    try
                    {
                        var con1 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        con1.Query<Horses>("delete from Horses ");
                        con1.Query<Horses>("delete  from DiariosNube");
                        con1.Query<Horses>("delete  from diarys");
                        con1.Query<Horses>("delete  from reminders");
                        con1.Query<Horses>("delete  from AlarmaRecordatorios");
                        var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                        con2.Query<Horses>("delete  from ActividadesCloudMes");
                        con2.Query<Horses>("delete  from ActividadesCloud");
                        con2.Query<Horses>("delete  from ActividadPorCaballo");
                    }
                    catch (Exception) { }
                    #endregion

                    StartActivity(typeof(ActivityLogin));
                    Finish();
                };
                alertar.FindViewById<TextView>(Resource.Id.btnNoClose).Click += delegate {
                    alertar.Dismiss();
                };

                alertar.Show();
            };
            #endregion
        }
        public async void ConsultarIdTelefono()
        {
            try
            {
                #region ConsultarIdTelefono
                string server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                HttpClient cliente = new HttpClient();
                string Token = new ShareInside().ConsultToken();
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token);
                var RespuestaTelefono = await cliente.GetAsync(server);
                var Contenido = await RespuestaTelefono.Content.ReadAsStringAsync();
                var Valores = JsonConvert.DeserializeObject<GetUser>(Contenido);
                #endregion

                #region Eliminareltelefono
                server = "https://cabasus-mobile.azurewebsites.net/v1/phones/" + Valores.phones[0].id.ToString();
                var respuestaEliminar = await cliente.DeleteAsync(server);
                var ContenidoEliminar = await respuestaEliminar.Content.ReadAsStringAsync();
                #endregion
            }
            catch (Exception ex)
            {
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

        public override void OnBackPressed()
        {
            // base.OnBackPressed();
        }
    }
}