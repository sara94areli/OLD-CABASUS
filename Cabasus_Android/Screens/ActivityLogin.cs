 using System;
using System.Json;
using System.Net.Http;
using System.Text;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Plugin.Connectivity;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;
using Android.Util;
using System.Dynamic;
using static Android.Provider.Settings;
using Android.Content;
using Android.Content.PM;
using HockeyApp.Android;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityLogin", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Custom.Dark")]
    public class ActivityLogin : Activity
    {
        TextView txtLoginMail, txtLoginPass, btnCreate, btnForgot;
        Button btnLogin;
        ShareInside CG = new ShareInside();
        ProgressBar progressBar;
        public const string TAG = "ActivityLogin";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_login);
            new ShareInside().GuardarActividadEnProgreso("0", "0");
            CrashManager.Register(this, "fe22c248646e45e9975d484519298d3b");

            new ShareInside().GuardarPantallaActual("ActivityLogin");
            Window.SetStatusBarColor(Color.Black);
            /*if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.SetStatusBarColor(Android.Graphics.Color.Blue);
                Window.SetNavigationBarColor(Color.AliceBlue);
            }

            /*this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.TranslucentNavigation);*/

            #region Referencias del xml;

            var btnFacebook = FindViewById<LinearLayout>(Resource.Id.btnLoginFacebook);
            var btnGoogle = FindViewById<LinearLayout>(Resource.Id.btnLoginGoogle);
            var Principal = FindViewById<RelativeLayout>(Resource.Id.Principal);
            btnLogin = FindViewById<Button>(Resource.Id.btnLoginLogin);
            btnCreate = FindViewById<TextView>(Resource.Id.btnLoginNewAccount);
            txtLoginMail = FindViewById<TextView>(Resource.Id.txtLoginMail);
            txtLoginPass = FindViewById<TextView>(Resource.Id.txtLoginPass);
            btnForgot = FindViewById<TextView>(Resource.Id.btnForgot);
            #endregion;

            #region Redondear botones facebook/google;

            GradientDrawable gd = new GradientDrawable();
            gd.SetColor(Color.Rgb(59, 89, 152));
            gd.SetCornerRadius(1000);
            btnFacebook.SetBackgroundDrawable(gd);
            GradientDrawable gd2 = new GradientDrawable();
            gd2.SetColor(Color.Rgb(211, 72, 54));
            gd2.SetCornerRadius(1000);
            btnGoogle.SetBackgroundDrawable(gd2);

            #endregion;

            #region botones;
            btnFacebook.Visibility = ViewStates.Invisible;
            btnGoogle.Visibility= ViewStates.Invisible;
            //btnFacebook.Click += delegate {
            //    Toast.MakeText(this, "Facebook", ToastLength.Short).Show();
            //};
            //btnGoogle.Click += delegate {
            //    Toast.MakeText(this, "Google", ToastLength.Short).Show();
            //};

            Principal.Click += delegate {
                hideSoftKeyboard(this);
            };

            btnLogin.Click += async delegate {
                await Logear();
                //StartActivity(typeof(ActivityHome));
            };

            btnCreate.Click += delegate {
                StartActivity(typeof(ActivityRegistro));
                Finish();
            };

            #endregion;

            (new ShareInside()).CopyDocuments("DBLocal.sqlite", "DBLocal.db");
            (new ShareInside()).CopyDocuments("VaciadoActividades.sqlite", "VaciadoActividades.db");
            (new ShareInside()).CopyDocuments("Notification.sqlite", "Notification.db");

            if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RecordarEmail.xml")))
                txtLoginMail.Text = new ShareInside().ConsultarRecordarEmail();

            btnForgot.Click += delegate {
                StartActivity(typeof(ActivityRecovery));
                Finish();
            };

            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    if (key != null)
                    {
                        var value = Intent.Extras.GetString(key);
                        Log.Debug(TAG, "Key: {0} Value: {1}", key, value);
                    }
                }
            }
        }

        private async Task Logear()
        {
            if (!string.IsNullOrWhiteSpace(txtLoginMail.Text))
            {
                if (!string.IsNullOrWhiteSpace(txtLoginPass.Text))
                {
                    string server = "https://cabasus-mobile.azurewebsites.net/v1/auth/login";
                    string json = "application/json";
                    JsonObject jsonObject = new JsonObject();
                    jsonObject.Add("email",txtLoginMail.Text );
                    jsonObject.Add("password",txtLoginPass.Text);
                    HttpClient cliente = new HttpClient();

                    #region CargarLoading
                    progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                    RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                    p.AddRule(LayoutRules.CenterInParent);
                    progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                    FindViewById<RelativeLayout>(Resource.Id.Principal).AddView(progressBar, p);
                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    #endregion

                    try
                    {
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            progressBar.Visibility = Android.Views.ViewStates.Visible;
                            Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            var respuesta = await cliente.PostAsync(server, new StringContent(jsonObject.ToString(), Encoding.UTF8, json));
                            var contenterror = await respuesta.Content.ReadAsStringAsync();
                            var otroerror = contenterror.Split('"');
                            //respuesta.EnsureSuccessStatusCode();
                            if (respuesta.IsSuccessStatusCode)
                            {
                                var content = await respuesta.Content.ReadAsStringAsync();
                                Token deserializedProduct = JsonConvert.DeserializeObject<Token>(content);
                                var tokenvalidar = new validartoken()
                                {
                                    token = deserializedProduct.token,
                                    expiration = DateTime.Now.AddDays(1)
                                };
                                CG.SaveToken(tokenvalidar);
                                if (deserializedProduct.token != "")
                                {
                                    await CG.GuardarDatosUsuario();
                                    //await new ShareInside().GuardadoFotosYConsulta();

                                    string serverConsulHorse = "https://cabasus-mobile.azurewebsites.net/v1/users/" + new ShareInside().ConsultarDatosUsuario()[0].id + "/horses?shared=true";
                                    var URIConsultaHorse = new System.Uri(string.Format(serverConsulHorse, string.Empty));
                                    HttpClient Cliente = new HttpClient();
                                    Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                                    var SaveConsultHorse = await Cliente.GetAsync(URIConsultaHorse);
                                    SaveConsultHorse.EnsureSuccessStatusCode();

                                    if (SaveConsultHorse.IsSuccessStatusCode)
                                    {
                                        JsonValue ConsultaJson = await SaveConsultHorse.Content.ReadAsStringAsync();
                                        var data = JsonConvert.DeserializeObject<List<HorsesComplete>>(ConsultaJson);
                                        if (data.Count > 0)
                                        {
                                            new ShareInside().GuardarCaballos(ConsultaJson);
                                            foreach (var item in data)
                                            {
                                                var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), item.id + ".png"));
                                                if (!File.Exists(RutaImage.ToString()))
                                                {
                                                    byte[] ArregloImagen = Base64.Decode(item.photo.data, Base64Flags.Default);
                                                    var biteImage = BitmapFactory.DecodeByteArray(ArregloImagen, 0, ArregloImagen.Length);
                                                    var biteMapRedondoImage = new ShareInside().RedondearbitmapImagen(biteImage, 200);

                                                    byte[] bitmapData;
                                                    using (var stream = new MemoryStream())
                                                    {
                                                        biteMapRedondoImage.Compress(Bitmap.CompressFormat.Png, 0, stream);
                                                        bitmapData = stream.ToArray();
                                                    }
                                                    File.WriteAllBytes(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/" + item.id + ".png"), bitmapData);
                                                }
                                            }
                                            new ShareInside().GuardarEstadoNotificacion(await new ShareInside().consultaparanotiicacionAsync());
                                            new ShareInside().GuardarPosicionPiker(data[0].id, data[0].name, 0);
                                            new ShareInside().GuardarDatosDeLogueo(txtLoginMail.Text, txtLoginPass.Text, "Es");
                                            new ShareInside().GuardarRecordarEmail(txtLoginMail.Text);
                                            
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            //UpdatePhone();
                                            MyFirebaseIIDService MFID = new MyFirebaseIIDService();
                                            StartActivity(typeof(ActivityHome));
                                            Finish();
                                        }
                                        else
                                        {

                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            StartActivity(typeof(ActivitySettings));
                                            Finish();
                                        }
                                    }
                                    else
                                    {
                                        JsonValue ConsultaJson = await SaveConsultHorse.Content.ReadAsStringAsync();
                                        
                                        Toast.MakeText(this, ConsultaJson.ToString()/*Resource.String.The_information*/, ToastLength.Short).Show();
                                    }
                                }
                            }
                            else
                            {
                                if (otroerror[3] == "instance.password does not meet minimum length of 7")
                                {
                                    Toast.MakeText(this, Resource.String.The_password, ToastLength.Long).Show();
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                }
                                else if (otroerror[3] == "Invalid user or password")
                                {
                                    Toast.MakeText(this, Resource.String.Invalid, ToastLength.Long).Show();
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                }
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Long).Show();
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, ex.Message/*Resource.String.The_information*/, ToastLength.Long).Show();
                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    }

                }
                else
                {
                    Toast.MakeText(this, Resource.String.Empty_password, ToastLength.Long).Show();
                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                }
            }
            else
            {
                Toast.MakeText(this, Resource.String.Empty_mail, ToastLength.Long).Show();
                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
            }
        }

        private async void UpdatePhone()
        {
            try
            {
                string server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                HttpClient cliente = new HttpClient();
                string ContentType = "application/json";
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());
                var respuestaActualizar = await cliente.GetAsync(server);
                if (respuestaActualizar.IsSuccessStatusCode)
                {
                    var ContenidoUpdate = await respuestaActualizar.Content.ReadAsStringAsync();
                    var ValoresPhone = JsonConvert.DeserializeObject<GetUser>(ContenidoUpdate);
                    ValoresPhone.photo.content_type = "image/jpeg";

                    dynamic ObjetosCreatePhone = new ExpandoObject();
                    ObjetosCreatePhone.number = ValoresPhone.phones[0].number;
                    ObjetosCreatePhone.os = "Android";
                    ObjetosCreatePhone.token = Secure.GetString(this.ContentResolver, Secure.AndroidId); 
                    var id = Android.Provider.Settings.System.GetString(this.ContentResolver, Android.Provider.Settings.System.AndroidId);
                    string JsonUserPhone = JsonConvert.SerializeObject(ObjetosCreatePhone, Formatting.Indented, new JsonSerializerSettings());

                    server = "https://cabasus-mobile.azurewebsites.net/v1/phones/" + ValoresPhone.phones[0].id;
                    var RespuestaCreatePhone = await cliente.PutAsync(server, new StringContent(JsonUserPhone, Encoding.UTF8, ContentType));
                    if (RespuestaCreatePhone.IsSuccessStatusCode)
                    {
                        var ContenidoCreatePhone = await RespuestaCreatePhone.Content.ReadAsStringAsync();
                        TokenPhone deserializedProduct = JsonConvert.DeserializeObject<TokenPhone>(ContenidoCreatePhone);
                        CG.SaveTokenPhone(deserializedProduct.id);
                        //Toast.MakeText(this, "Telefono Actualizado", ToastLength.Long).Show();
                    }
                    else
                    {
                        //Toast.MakeText(this, "Telefono No Actualizado", ToastLength.Long).Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message/*Resource.String.The_information*/, ToastLength.Long).Show();
            }

        }

        public static void hideSoftKeyboard(Activity activity)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Activity.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(activity.CurrentFocus.WindowToken,0);
        }

        public override void OnBackPressed()
        {
          
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

            string dpi = new ShareInside().getDensityDpi(this);
            if (dpi == "xxxhdpi")
            {

            }
            else if (dpi == "xxhdpi")
            {

            }
            else if (dpi == "xhdpi")
            {
                txtLoginMail.TextSize = 15;
                txtLoginPass.TextSize = 15;
                btnLogin.TextSize = 15;
                btnCreate.TextSize = 13;
                btnForgot.TextSize = 13;
            }
            else if (dpi == "hdpi")
            {
                txtLoginMail.TextSize = 14;
                txtLoginPass.TextSize = 14;
                btnLogin.TextSize = 14;
                btnCreate.TextSize = 12;
                btnForgot.TextSize = 12;
            }
            else if (dpi == "mdpi")
            {

            }
            else
            {

            }

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