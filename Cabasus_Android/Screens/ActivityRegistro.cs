using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.IO;
using Android.Provider;
using Uri = Android.Net.Uri;
using Android.Runtime;
using Android.Graphics;
using Com.Yalantis.Ucrop;
using Java.Lang;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Content.PM;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Dynamic;
using Android.Graphics.Drawables;
using System.Collections.Generic;
using Plugin.Connectivity;
using Android.Util;
using Android.Support.V7.Widget;
using static Android.Provider.Settings;
using System.Threading.Tasks;
using SQLite;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", Label = "ActivityRegistro", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ActivityRegistro : BaseActivity, IDialogInterfaceOnClickListener//, IOnDateSetListener
    {
        string actualizar="b";
        ImageButton btnTomarFoto;
        string Age, Phone;
        int Weight, Height;
        DateTime Ages;
        Button btnCancel, btnSingUp, RbtnTermsConditions, txtAge;
        EditText txtUserName, txtPassword, txtEmail , txtweight, txtheight,txtPhone;

        SwitchCompat SwtVisibility;
        ShareInside CG = new ShareInside();
        ProgressBar progressBar;
        bool Visibility = true;

        Bitmap bitCloud;

        #region Tomar Foto e Imagen Galeria

        int camrequestcode = 100;
        Uri cameraUri;

        private const string TAG = "SampleActivity";
        Stream stream;

        private const int REQUEST_SELECT_PICTURE = 0x01;
        private const string SAMPLE_CROPPED_IMAGE_NAME = "SampleCropImage";

        #endregion

        #region datePicker

        //public DatePickerDialog.IOnDateSetListener onDateSetListener;
        PickerDate onDateSetListener;
        public string textoDate { get; set; }
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ShareInside contenidosG = new ShareInside();

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_registro);
            new ShareInside().GuardarPantallaActual("ActivityRegistro");
            Window.SetStatusBarColor(Color.Black);
            #region Casamiento

            btnTomarFoto = FindViewById<ImageButton>(Resource.Id.tomarFoto);
            btnCancel = FindViewById<Button>(Resource.Id.RCancel);
            btnSingUp = FindViewById<Button>(Resource.Id.RSingUp);
            RbtnTermsConditions = FindViewById<Button>(Resource.Id.RbtnTermsConditions);

            txtUserName = FindViewById<EditText>(Resource.Id.txtUserName);
            txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            txtEmail = FindViewById<EditText>(Resource.Id.txtEmail);
            txtAge = FindViewById<Button>(Resource.Id.txtAge);
            txtweight = FindViewById<EditText>(Resource.Id.txtweight);
            txtheight = FindViewById<EditText>(Resource.Id.txtHeight);
            txtPhone= FindViewById<EditText>(Resource.Id.txtPhone);
            SwtVisibility= FindViewById<SwitchCompat>(Resource.Id.switchvisible);
            #endregion

            txtweight.Hint = GetText(Resource.String.kg);
            txtheight.Hint = GetText(Resource.String.mts);
            txtweight.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            txtheight.SetHintTextColor(Color.Argb(127, 255, 255, 255));

            #region Captura de imagen

            btnTomarFoto.Click += delegate
            {
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(false);
                alertar.SetContentView(Resource.Layout.layout_CustomAlert);
                alertar.Show();

                alertar.FindViewById<Button>(Resource.Id.btnCancel).Click += delegate
                {
                    alertar.Dismiss();
                };

                alertar.FindViewById<Button>(Resource.Id.btnGalery).Click += delegate
                {
                    Intent intent = new Intent();
                    intent.SetType("image/*");
                    intent.SetAction(Intent.ActionGetContent);
                    intent.AddCategory(Intent.CategoryOpenable);
                    StartActivityForResult(Intent.CreateChooser(intent, GetText(Resource.String.select_image)), REQUEST_SELECT_PICTURE);
                    alertar.Dismiss();
                };

                alertar.FindViewById<Button>(Resource.Id.btnCamara).Click += delegate
                {
                    openCamara();
                    alertar.Dismiss();
                };
            };

            #endregion

            btnSingUp.Click += BtnSingUp_Click;

            btnCancel.Click += delegate {
                if (actualizar == "a")
                {
                    StartActivity(typeof(ActivitySettings));
                    Finish();
                }
                else
                {
                    StartActivity(typeof(ActivityLogin));
                    Finish();
                }
            };
            
            txtAge.Click += delegate
            {
                //view.MaxDate = Java.Lang.JavaSystem.CurrentTimeMillis();

                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                Java.Util.Calendar mincalendar = Java.Util.Calendar.Instance;
                mincalendar.Set(1919, 1, 1);
                long mincal = mincalendar.TimeInMillis;
                //int year = calendar.Get(Java.Util.CalendarField.Year);
                int year = calendar.Get(Java.Util.CalendarField.Year);
                int month = calendar.Get(Java.Util.CalendarField.Month);
                int day_of_month = calendar.Get(Java.Util.CalendarField.DayOfMonth);

                DatePickerDialog dialog = new DatePickerDialog(this, Resource.Style.ThemeOverlay_AppCompat_Dialog_Alert,
                   onDateSetListener, year, month, day_of_month);

                dialog.DatePicker.MaxDate = JavaSystem.CurrentTimeMillis();
                dialog.DatePicker.MinDate = mincal;
                dialog.Show();

            };
            onDateSetListener = new PickerDate(txtAge);
            actualizar = this.Intent.GetStringExtra("actualizar");
            if (actualizar== "a")
            {
                ActualizarUsuarios();
            }
        }

        public async void ActualizarUsuarios()
        {
            try
            {
                txtweight.SetHintTextColor(Color.Argb(127, 255, 255, 255));
                txtheight.SetHintTextColor(Color.Argb(127, 255, 255, 255));

                btnSingUp.Text = GetText(Resource.String.Done);
                Window.SetSoftInputMode(Android.Views.SoftInput.StateHidden);
                string server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                string ContentType = "application/json";
                string StringConsultUser = "";
                HttpClient cliente = new HttpClient();
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());

                progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);
                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                var respuestaActualizar = await cliente.GetAsync(server);
                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                if (respuestaActualizar.IsSuccessStatusCode)
                {
                    var Contenido = await respuestaActualizar.Content.ReadAsStringAsync();
                    var Valores = JsonConvert.DeserializeObject<GetUser>(Contenido);
                    txtUserName.Text = Valores.username.ToString();
                    txtEmail.Text = Valores.email.ToString();
                    txtEmail.Enabled = false;
                    txtPassword.Text = "**********";
                    txtPassword.Alpha = .5f;
                    txtEmail.Alpha = .5f;
                    txtPassword.Enabled = false;
                    if (Valores.birthday.ToString("yyyy-MM-dd") == "1918-01-01")
                        txtAge.Text = "";
                    else
                        txtAge.Text = Convert.ToString(Valores.birthday.ToString("yyyy-MM-dd"));

                    if (Valores.height.ToString() == "1")
                    {
                        txtheight.Hint = GetText(Resource.String.mts);
                        txtheight.Text = "";

                    } 
                    else
                        txtheight.Text = Valores.height.ToString();

                    if (Valores.weight.ToString() == "1")
                    {
                        txtweight.Hint = GetText(Resource.String.kg);
                        txtweight.Text = "";
                    }
                        
                    else
                        txtweight.Text = Valores.weight.ToString();

                    SwtVisibility.Checked = Valores.privacy.searchable;
                    string Photo = Valores.photo.data;
                    byte[] ArregloPhoto = Base64.Decode(Photo, Base64Flags.Default);
                    var bite = BitmapFactory.DecodeByteArray(ArregloPhoto, 0, ArregloPhoto.Length);
                    btnTomarFoto.SetImageBitmap(bite);

                    #region TraerDatosPhone
                    server = "https://cabasus-mobile.azurewebsites.net/v1/phones/" + Valores.phones[0].id;
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());
                    var RespuestaPhone = await cliente.GetAsync(server);
                    if (RespuestaPhone.IsSuccessStatusCode)
                    {
                        var DatosPhone = await RespuestaPhone.Content.ReadAsStringAsync();
                        var ValoresPhones = JsonConvert.DeserializeObject<GetPhone>(DatosPhone);
                        if (ValoresPhones.number.ToString() == "1")
                            txtPhone.Text = "";
                        else
                            txtPhone.Text = ValoresPhones.number.ToString();
                    }
                    #endregion
                }
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                StartActivity(typeof(ActivityAjustes));
                Finish();
            }
        }

        public void ActualizarBaseLocal(string nombre, string idowner)
        {
            try
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                con.Query<Horse>("update Horses set owner_name='" + nombre + "' where owner= '" + idowner + "'");
                var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                con2.Query<Horse>("update ActividadesCloudMes set User='" + nombre + "'  where ID_Usuario= '" + idowner + "'");
            }
            catch (System.Exception)
            {
            }

        }

        private async void BtnSingUp_Click(object sender, EventArgs e)
        {
            try
            {
                txtweight.SetHintTextColor(Color.Argb(127, 255, 255, 255));
                txtheight.SetHintTextColor(Color.Argb(127, 255, 255, 255));

                if (actualizar == "a")
                {
                    string server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                    string ContentType = "application/json";
                    HttpClient cliente;
                    string JsonUpdateUser = "";
                    string Photo = "";
                    using (var stream = new MemoryStream())
                    {
                        Bitmap bite = ((BitmapDrawable)btnTomarFoto.Drawable).Bitmap;
                        bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        var bites = stream.ToArray();
                        Photo = System.Convert.ToBase64String(bites);
                    }
                    if (string.IsNullOrWhiteSpace(txtAge.Text))
                        Age = "1928-01-01";
                    else
                    {
                        Ages = Convert.ToDateTime(txtAge.Text);
                        Age = Ages.ToString("yyyy-MM-dd");
                    }

                    if (string.IsNullOrWhiteSpace(txtweight.Text))
                        Weight = 1;
                    else
                        Weight = int.Parse(txtweight.Text);

                    if (string.IsNullOrWhiteSpace(txtheight.Text))
                        Height = 1;
                    else
                        Height = int.Parse(txtheight.Text);

                    if (string.IsNullOrWhiteSpace(txtPhone.Text))
                        Phone = "1";
                    else
                        Phone = txtPhone.Text;

                    List<UpdateUser> ListaActualizarUsuario = new List<UpdateUser>()
                                    {
                                        new UpdateUser()
                                        {
                                            weight = Weight,
                                            height = Height,
                                            birthday = Age,
                                            username = txtUserName.Text,
                                            phones = new List<Phone>(){ new Phone() { key = "w", number = Phone, os = "Android" } },
                                            language = "en",
                                            privacy = new Privacy(){ searchable = Visibility },
                                            photo = new Photo(){ data = Photo, content_type = "image/png"}
                                        }
                                    };

                    dynamic ObjetosUpdateUser = new ExpandoObject();
                    ObjetosUpdateUser.weight = ListaActualizarUsuario[0].weight;
                    ObjetosUpdateUser.height = ListaActualizarUsuario[0].height;
                    ObjetosUpdateUser.birthday = ListaActualizarUsuario[0].birthday;
                    ObjetosUpdateUser.username = ListaActualizarUsuario[0].username;
                    //ObjetosUpdateUser.phones = ListaActualizarUsuario[0].phones;
                    ObjetosUpdateUser.language = ListaActualizarUsuario[0].language;
                    ObjetosUpdateUser.privacy = ListaActualizarUsuario[0].privacy;
                    ObjetosUpdateUser.photo = ListaActualizarUsuario[0].photo;
                    JsonUpdateUser = JsonConvert.SerializeObject(ObjetosUpdateUser, Formatting.Indented, new JsonSerializerSettings());
                    cliente = new HttpClient();
                    server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());
                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                    Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    var respuestaActualizar = await cliente.PutAsync(server, new StringContent(JsonUpdateUser, Encoding.UTF8, ContentType));
                    if (respuestaActualizar.IsSuccessStatusCode)
                    {
                        var ContenidoUpdate = await respuestaActualizar.Content.ReadAsStringAsync();
                        var RespuestaCreatePhone = await CrearPhone(true, ContenidoUpdate);
                        if (RespuestaCreatePhone)
                        {
                            MyFirebaseIIDService MFID = new MyFirebaseIIDService(); ;
                            await CG.GuardarDatosUsuario();

                            await new ShareInside().GuardadoFotosYConsulta();
                            ActualizarBaseLocal(txtUserName.Text, new ShareInside().ConsultarDatosUsuario()[0].id);

                            StartActivity(typeof(ActivitySettings));
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            Finish();
                        }
                        else
                        {
                            Toast.MakeText(this, Resource.String.problem_saving, ToastLength.Long).Show();
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                    }
                    else
                    {
                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        Toast.MakeText(this, Resource.String.problem_updating, ToastLength.Long).Show();
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(txtUserName.Text))
                    {
                        if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                            {
                                if (txtPassword.Text.Length >= 7)
                                {
                                    try
                                    {
                                        progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                                        p.AddRule(LayoutRules.CenterInParent);
                                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                                        FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);
                                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                                        Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                        string server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                                        string ContentType = "application/json";
                                        string StringCreateUser = "";
                                        List<CreateUser> ListaUsuario = new List<CreateUser>()
                                        {
                                new CreateUser()
                                {
                                    email = txtEmail.Text,
                                    password = txtPassword.Text,
                                    language = "en"
                                }
                                        };

                                        dynamic ObjetosCreateUser = new ExpandoObject();
                                        ObjetosCreateUser.email = ListaUsuario[0].email;
                                        ObjetosCreateUser.password = ListaUsuario[0].password;
                                        ObjetosCreateUser.language = ListaUsuario[0].language;
                                        StringCreateUser = JsonConvert.SerializeObject(ObjetosCreateUser, Formatting.Indented, new JsonSerializerSettings());
                                        HttpClient cliente = new HttpClient();
                                        if (CrossConnectivity.Current.IsConnected)
                                        {
                                            //progressBar.Visibility = Android.Views.ViewStates.Visible;
                                            //Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            var respuesta = await cliente.PostAsync(server, new StringContent(StringCreateUser, Encoding.UTF8, ContentType));
                                            var respuestaerrorlogin = await respuesta.Content.ReadAsStringAsync();
                                            try
                                            {
                                                var ErrorCorreoRegistrado = JsonConvert.DeserializeObject<ListaErroresLogin>(respuestaerrorlogin);
                                                if (ErrorCorreoRegistrado.messages[0] == "Email already taken")
                                                {
                                                    Toast.MakeText(this, Resource.String.mail_already_registered, ToastLength.Long).Show();
                                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                }
                                                else
                                                {
                                                    respuesta.EnsureSuccessStatusCode();
                                                    if (respuesta.IsSuccessStatusCode)
                                                    {
                                                        var contentlogin = await respuesta.Content.ReadAsStringAsync();
                                                        server = "https://cabasus-mobile.azurewebsites.net/v1/auth/login";
                                                        string LoginUser = "";
                                                        List<LoginUser> Login = new List<LoginUser>()
                                                    {
                                                        new LoginUser()
                                                        {
                                                            email = txtEmail.Text,
                                                            password = txtPassword.Text
                                                        }
                                                    };
                                                        dynamic LoginUsers = new ExpandoObject();
                                                        LoginUsers.email = Login[0].email;
                                                        LoginUsers.password = Login[0].password;
                                                        LoginUser = JsonConvert.SerializeObject(LoginUsers, Formatting.Indented, new JsonSerializerSettings());
                                                        var respuestaLogin = await cliente.PostAsync(server, new StringContent(LoginUser, Encoding.UTF8, ContentType));
                                                        respuestaLogin.EnsureSuccessStatusCode();
                                                        if (respuestaLogin.IsSuccessStatusCode)
                                                        {
                                                            var content = await respuestaLogin.Content.ReadAsStringAsync();
                                                            Token deserializedProduct = JsonConvert.DeserializeObject<Token>(content);
                                                            var tokenvalidar = new validartoken()
                                                            {
                                                                token = deserializedProduct.token,
                                                                expiration = DateTime.Now.AddDays(1)
                                                            };
                                                           new ShareInside().SaveToken(tokenvalidar);
                                                            string JsonUpdateUser = "";
                                                            string Photo = "";
                                                            using (var stream = new MemoryStream())
                                                            {
                                                                Bitmap bite = ((BitmapDrawable)btnTomarFoto.Drawable).Bitmap;
                                                                bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                                                                var bites = stream.ToArray();
                                                                Photo = System.Convert.ToBase64String(bites);
                                                            }
                                                            if (string.IsNullOrWhiteSpace(txtAge.Text))
                                                                Age = "1918-01-01";
                                                            else
                                                            {
                                                                Ages = Convert.ToDateTime(txtAge.Text);
                                                                Age = Ages.ToString("yyyy-MM-dd");
                                                            }
                                                            if (string.IsNullOrWhiteSpace(txtweight.Text))
                                                                Weight = 1;
                                                            else
                                                                Weight = int.Parse(txtweight.Text);
                                                            if (string.IsNullOrWhiteSpace(txtheight.Text))
                                                                Height = 1;
                                                            else
                                                                Height = int.Parse(txtheight.Text);

                                                            List<UpdateUser> ListaActualizarUsuario = new List<UpdateUser>()
                                                        {
                                                            new UpdateUser()
                                                            {
                                                                weight = Weight,
                                                                height = Height,
                                                                birthday = Age,
                                                                username = txtUserName.Text,
                                                                language = "en",
                                                                privacy = new Privacy(){ searchable = Visibility },
                                                                photo = new Photo(){ data = Photo, content_type = "image/jpeg"}
                                                            }
                                                        };

                                                            dynamic ObjetosUpdateUser = new ExpandoObject();
                                                            ObjetosUpdateUser.weight = ListaActualizarUsuario[0].weight;
                                                            ObjetosUpdateUser.height = ListaActualizarUsuario[0].height;
                                                            ObjetosUpdateUser.birthday = ListaActualizarUsuario[0].birthday;
                                                            ObjetosUpdateUser.username = ListaActualizarUsuario[0].username;
                                                            ObjetosUpdateUser.language = ListaActualizarUsuario[0].language;
                                                            ObjetosUpdateUser.privacy = ListaActualizarUsuario[0].privacy;
                                                            ObjetosUpdateUser.photo = ListaActualizarUsuario[0].photo;
                                                            JsonUpdateUser = JsonConvert.SerializeObject(ObjetosUpdateUser, Formatting.Indented, new JsonSerializerSettings());
                                                            cliente = new HttpClient();
                                                            server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                                                            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());

                                                            var respuestaActualizar = await cliente.PutAsync(server, new StringContent(JsonUpdateUser, Encoding.UTF8, ContentType));
                                                            if (respuestaActualizar.IsSuccessStatusCode)
                                                            {
                                                                var ContenidoUpdate = await respuestaActualizar.Content.ReadAsStringAsync();
                                                                var RespuestaCreatePhone = await CrearPhone(false, ContenidoUpdate);
                                                                if (RespuestaCreatePhone)
                                                                {
                                                                    MyFirebaseIIDService MFID = new MyFirebaseIIDService();
                                                                    Toast.MakeText(this, Resource.String.Congratulations, ToastLength.Long).Show();
                                                                    StartActivity(typeof(ActivityLogin));
                                                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                                    Finish();
                                                                }
                                                                else
                                                                {
                                                                    Toast.MakeText(this, Resource.String.Problem_when_registering, ToastLength.Long).Show();
                                                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                                Toast.MakeText(this, Resource.String.Problem_when_registering, ToastLength.Long).Show();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                            Toast.MakeText(this, respuestaLogin.ReasonPhrase.ToString(), ToastLength.Long).Show();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                        Toast.MakeText(this, respuesta.ReasonPhrase.ToString(), ToastLength.Long).Show();
                                                    }
                                                }
                                            }
                                            catch (System.Exception)
                                            {
                                                respuesta.EnsureSuccessStatusCode();
                                                if (respuesta.IsSuccessStatusCode)
                                                {
                                                    var contentlogin = await respuesta.Content.ReadAsStringAsync();
                                                    var MensajeError = JsonConvert.DeserializeObject<MensajeErrorLogin>(contentlogin);
                                                    if (MensajeError.status == "unverified")
                                                    {
                                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                        Toast.MakeText(this, Resource.String.Mail_is_already, ToastLength.Long).Show();
                                                    }
                                                    else if (MensajeError.status == "created")
                                                    {
                                                        server = "https://cabasus-mobile.azurewebsites.net/v1/auth/login";
                                                        string LoginUser = "";
                                                        List<LoginUser> Login = new List<LoginUser>()
                                                    {
                                                        new LoginUser()
                                                        {
                                                            email = txtEmail.Text,
                                                            password = txtPassword.Text
                                                        }
                                                    };
                                                        dynamic LoginUsers = new ExpandoObject();
                                                        LoginUsers.email = Login[0].email;
                                                        LoginUsers.password = Login[0].password;
                                                        LoginUser = JsonConvert.SerializeObject(LoginUsers, Formatting.Indented, new JsonSerializerSettings());
                                                        var respuestaLogin = await cliente.PostAsync(server, new StringContent(LoginUser, Encoding.UTF8, ContentType));
                                                        respuestaLogin.EnsureSuccessStatusCode();
                                                        if (respuestaLogin.IsSuccessStatusCode)
                                                        {
                                                            var content = await respuestaLogin.Content.ReadAsStringAsync();
                                                            Token deserializedProduct = JsonConvert.DeserializeObject<Token>(content);
                                                            var tokenvalidar = new validartoken()
                                                            {
                                                                token = deserializedProduct.token,
                                                                expiration = DateTime.Now.AddDays(1)
                                                            };
                                                           CG.SaveToken(tokenvalidar);
                                                            string JsonUpdateUser = "";
                                                            string Photo = "";
                                                            using (var stream = new MemoryStream())
                                                            {
                                                                Bitmap bite = ((BitmapDrawable)btnTomarFoto.Drawable).Bitmap;
                                                                bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                                                                var bites = stream.ToArray();
                                                                Photo = System.Convert.ToBase64String(bites);
                                                            }
                                                            if (string.IsNullOrWhiteSpace(txtAge.Text))
                                                                Age = "1918-01-01";
                                                            else
                                                            {
                                                                Ages = Convert.ToDateTime(txtAge.Text);
                                                                Age = Ages.ToString("yyyy-MM-dd");
                                                            }
                                                            if (string.IsNullOrWhiteSpace(txtweight.Text))
                                                                Weight = 1;
                                                            else
                                                                Weight = int.Parse(txtweight.Text);
                                                            if (string.IsNullOrWhiteSpace(txtheight.Text))
                                                                Height = 1;
                                                            else
                                                                Height = int.Parse(txtheight.Text);

                                                            List<UpdateUser> ListaActualizarUsuario = new List<UpdateUser>()
                                                        {
                                                            new UpdateUser()
                                                            {
                                                                weight = Weight,
                                                                height = Height,
                                                                birthday = Age,
                                                                username = txtUserName.Text,
                                                                language = "en",
                                                                privacy = new Privacy(){ searchable = Visibility },
                                                                photo = new Photo(){ data = Photo, content_type = "image/jpeg" }
                                                            }
                                                        };

                                                            dynamic ObjetosUpdateUser = new ExpandoObject();
                                                            ObjetosUpdateUser.weight = ListaActualizarUsuario[0].weight;
                                                            ObjetosUpdateUser.height = ListaActualizarUsuario[0].height;
                                                            ObjetosUpdateUser.birthday = ListaActualizarUsuario[0].birthday;
                                                            ObjetosUpdateUser.username = ListaActualizarUsuario[0].username;
                                                            ObjetosUpdateUser.language = ListaActualizarUsuario[0].language;
                                                            ObjetosUpdateUser.privacy = ListaActualizarUsuario[0].privacy;
                                                            ObjetosUpdateUser.photo = ListaActualizarUsuario[0].photo;
                                                            JsonUpdateUser = JsonConvert.SerializeObject(ObjetosUpdateUser, Formatting.Indented, new JsonSerializerSettings());
                                                            cliente = new HttpClient();
                                                            server = "https://cabasus-mobile.azurewebsites.net/v1/profile";
                                                            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());

                                                            var respuestaActualizar = await cliente.PutAsync(server, new StringContent(JsonUpdateUser, Encoding.UTF8, ContentType));
                                                            if (respuestaActualizar.IsSuccessStatusCode)
                                                            {
                                                                var ContenidoUpdate = await respuestaActualizar.Content.ReadAsStringAsync();
                                                                var RespuestaCreatePhone = await CrearPhone(false, ContenidoUpdate);
                                                                if (RespuestaCreatePhone)
                                                                {
                                                                    MyFirebaseIIDService MFID = new MyFirebaseIIDService();
                                                                    Toast.MakeText(this, Resource.String.Congratulations, ToastLength.Long).Show();
                                                                    StartActivity(typeof(ActivityLogin));
                                                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                                    Finish();
                                                                }
                                                                else
                                                                {
                                                                    Toast.MakeText(this, Resource.String.problem_saving, ToastLength.Long).Show();
                                                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                                Toast.MakeText(this, Resource.String.Problem_when_registering, ToastLength.Long).Show();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                    Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Long).Show();
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                                    }
                                }
                                else
                                {
                                    Toast.MakeText(this, Resource.String.Caracteres, ToastLength.Long).Show();
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, Resource.String.Empty_mail, ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, Resource.String.Empty_password, ToastLength.Long).Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.Empty_user, ToastLength.Long).Show();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Long).Show();
                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
            }
        }

        public async Task<bool> CrearPhone(bool Actualizar, string DatosUsuario)
        {
            bool Correcto = false;
            if (Actualizar)
            {
                HttpClient cliente = new HttpClient();
                string ContentType = "application/json";
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());
                var ValoresPhone = JsonConvert.DeserializeObject<UserPhone>(DatosUsuario);
                if (string.IsNullOrWhiteSpace(txtPhone.Text))
                    Phone = "1";
                else
                    Phone = txtPhone.Text;

                dynamic ObjetosCreatePhone = new ExpandoObject();
                ObjetosCreatePhone.number = Phone;
                ObjetosCreatePhone.os = "Android";
                ObjetosCreatePhone.token = Secure.GetString(this.ContentResolver, Secure.AndroidId);
                string JsonUserPhone = JsonConvert.SerializeObject(ObjetosCreatePhone, Formatting.Indented, new JsonSerializerSettings());

                string server = "https://cabasus-mobile.azurewebsites.net/v1/phones/" + ValoresPhone.phones[ValoresPhone.phones.Count - 1];
                var RespuestaCreatePhone = await cliente.PutAsync(server, new StringContent(JsonUserPhone, Encoding.UTF8, ContentType));
                if (RespuestaCreatePhone.IsSuccessStatusCode)
                {
                    var ContenidoCreatePhone = await RespuestaCreatePhone.Content.ReadAsStringAsync();
                    TokenPhone deserializedProduct = JsonConvert.DeserializeObject<TokenPhone>(ContenidoCreatePhone);
                    CG.SaveTokenPhone(deserializedProduct.id);
                    Correcto = true;
                }
                else
                {
                    Correcto = false;
                }
                return Correcto;
            }
            else
            {
                //Region para crear la api de phone
                HttpClient cliente = new HttpClient();
                string ContentType = "application/json";
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(CG.ConsultToken());

                var ValoresPhone = JsonConvert.DeserializeObject<GetUser>(DatosUsuario);
                ValoresPhone.photo.content_type = "image/jpeg";
                dynamic TraerUsuario = new ExpandoObject();
                TraerUsuario.id = ValoresPhone.id;
                TraerUsuario.createdAt = ValoresPhone.createdAt;
                TraerUsuario.updatedAt = ValoresPhone.updatedAt;
                TraerUsuario.email = ValoresPhone.email;
                TraerUsuario.weight = ValoresPhone.weight;
                TraerUsuario.height = ValoresPhone.height;
                TraerUsuario.birthday = ValoresPhone.birthday.ToString("yyyy-MM-dd");
                TraerUsuario.username = ValoresPhone.username;
                TraerUsuario.phones = ValoresPhone.phones;
                TraerUsuario.language = ValoresPhone.language;
                TraerUsuario.privacy = ValoresPhone.privacy;
                TraerUsuario.photo = ValoresPhone.photo;
                TraerUsuario.permissions = ValoresPhone.permissions;

                dynamic ObjetosCreatePhone = new ExpandoObject();
                ObjetosCreatePhone.owner = TraerUsuario;
                ObjetosCreatePhone.number = txtPhone.Text;
                ObjetosCreatePhone.os = "Android";
                ObjetosCreatePhone.token = Secure.GetString(this.ContentResolver, Secure.AndroidId);
                string JsonUserPhone = JsonConvert.SerializeObject(ObjetosCreatePhone, Formatting.Indented, new JsonSerializerSettings());

                string server = "https://cabasus-mobile.azurewebsites.net/v1/profile/phones";
                var RespuestaCreatePhone = await cliente.PostAsync(server, new StringContent(JsonUserPhone, Encoding.UTF8, ContentType));
                RespuestaCreatePhone.EnsureSuccessStatusCode();
                if (RespuestaCreatePhone.IsSuccessStatusCode)
                {
                    var ContenidoCreatePhone = await RespuestaCreatePhone.Content.ReadAsStringAsync();
                    TokenPhone deserializedProduct = JsonConvert.DeserializeObject<TokenPhone>(ContenidoCreatePhone);
                    CG.SaveTokenPhone(deserializedProduct.id);
                    Correcto = true;
                }
                else
                {
                    Correcto = false;
                }
                return Correcto;
            }
        }
        
        private void openCamara()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);

            Java.IO.File _dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim), "CabasusApp");

            if (!_dir.Exists())
            {
                _dir.Mkdirs();
            }

            Java.IO.File _file = new Java.IO.File(_dir, "Cabasus"
                + Java.Lang.String.ValueOf(JavaSystem.CurrentTimeMillis()) + ".png");

            cameraUri = Uri.FromFile(_file);
            intent.PutExtra(MediaStore.ExtraOutput, cameraUri);
            intent.PutExtra("return data", true);
            StartActivityForResult(intent, camrequestcode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                if (requestCode == camrequestcode)
                    StartCropActivity(cameraUri);

                if (requestCode == REQUEST_SELECT_PICTURE)
                {
                    Uri selectedUri = data.Data;
                    if (selectedUri != null)
                    {
                        stream = ContentResolver.OpenInputStream(selectedUri);
                        Bitmap bitmap = BitmapFactory.DecodeStream(stream);

                        if (bitmap.Width < 100 | bitmap.Height < 100)
                        {
                            Android.App.AlertDialog.Builder mensajeError = new Android.App.AlertDialog.Builder(this);
                            mensajeError.SetTitle(GetText(Resource.String.Image_too_small));
                            mensajeError.SetMessage(GetText(Resource.String.image_with_192));
                            mensajeError.SetCancelable(false);
                            mensajeError.SetPositiveButton("Ok", this);
                            mensajeError.Show();
                        }
                        else
                            StartCropActivity(data.Data);

                    }

                    else
                        Toast.MakeText(this, Resource.String.You_cant_recover_image, ToastLength.Short).Show();
                }
                else if (requestCode == UCrop.RequestCrop)
                {
                    HandleCropResult(data);
                }
                if (resultCode.ToString().Equals(UCrop.ResultError.ToString()))
                    HandleCropError(data);
            }
        }

        private void StartCropActivity(Uri uri)
        {
            string destinationFileName = "Cabasus.png";
            UCrop uCrop = UCrop.Of(uri, Uri.FromFile(new Java.IO.File(CacheDir, destinationFileName))).WithAspectRatio(1, 1).WithMaxResultSize(125, 125);
            UCrop.Options options = new UCrop.Options();

            options.SetCompressionQuality(100);
            options.SetShowCropGrid(false);

            options.SetCompressionFormat(Bitmap.CompressFormat.Png);
            options.SetFreeStyleCropEnabled(false);
            options.SetToolbarColor(Color.Black);
            options.SetToolbarWidgetColor(Color.White);
            options.SetStatusBarColor(Color.Black);
            options.SetToolbarTitle(GetString(Resource.String.select_image));
            uCrop.WithOptions(options);

            uCrop.Start(this);
        }

        private void HandleCropResult(Intent result)
        {
            Uri resultUri = UCrop.GetOutput(result);
            if (resultUri != null)
            {
                #region Imagen
                /*
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, resultUri);
                bitmap = RedondearbitmapImagen(bitmap, 125);

                btnTomarFoto.SetImageBitmap(bitmap);
                btnTomarFoto.SetBackgroundResource(Resource.Drawable.cornerImageButton);
                */
                #endregion

                stream = ContentResolver.OpenInputStream(resultUri);

                Bitmap bitmap = BitmapFactory.DecodeStream(stream);

                int width = bitmap.Width;
                int height = bitmap.Height;
                int newWidth = 150;
                int newHeight = 150;

                float scaleWidth = ((float)newWidth) / width;
                float scaleHeight = ((float)newHeight) / height;

                Matrix matrix = new Matrix();

                matrix.PostScale(scaleWidth, scaleHeight);

                Bitmap resizedBitmap = Bitmap.CreateBitmap(bitmap, 0, 0, width, height, matrix, true);
                bitCloud = Bitmap.CreateBitmap(bitmap, 0, 0, width, height, matrix, true);

                resizedBitmap = (new ShareInside()).RedondearbitmapImagen(resizedBitmap, 200);

                btnTomarFoto.SetImageBitmap(resizedBitmap);
                btnTomarFoto.SetBackgroundResource(Resource.Drawable.cornerImageButton);
            }
            else
            {
                Toast.MakeText(this, Resource.String.You_cant_recover_image, ToastLength.Short).Show();
            }
        }

        private void HandleCropError(Intent result)
        {
            Throwable cropError = UCrop.GetError(result);
            if (cropError != null)
            {
                System.Diagnostics.Debug.WriteLine(cropError.Message);
                Toast.MakeText(this, cropError.Message, ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
            }
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            throw new NotImplementedException();
        }

        public override void OnBackPressed()
        {
            if (actualizar != "a")
            {
                StartActivity(typeof(ActivityLogin));
                Finish();
            }
        }

        /*public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            textoDate = dayOfMonth.ToString();
        }*/

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
    public class BaseActivity : AppCompatActivity
    {


        protected const int REQUEST_STORAGE_READ_ACCESS_PERMISSION = 101;
        protected const int REQUEST_STORAGE_WRITE_ACCESS_PERMISSION = 102;

        private Android.Support.V7.App.AlertDialog mAlertDialog;


        protected override void OnStop()
        {
            base.OnStop();
            if (mAlertDialog != null && mAlertDialog.IsShowing)
            {
                mAlertDialog.Dismiss();
            }
        }

        protected void RequestPermission(string permission, string rationale, int requestCode)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
            {
                ShowAlertDialog("permission_title_rationale",
                            rationale,
                            (sender, args) =>
                            {
                                ActivityCompat.RequestPermissions(this, new System.String[] { permission }, requestCode);
                            },
                            "ok",
                            null,
                            "cancel");
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new System.String[] { permission }, requestCode);
            }
        }

        protected void ShowAlertDialog(string title, string message, EventHandler<DialogClickEventArgs> onPositiveButtionClicked, string positiveButtonText, EventHandler<DialogClickEventArgs> onNegativeButtionClicked, string negativeButtonText)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton(positiveButtonText, onPositiveButtionClicked);
            builder.SetNegativeButton(negativeButtonText, onNegativeButtionClicked);
            mAlertDialog = builder.Show();
        }

    }

}