using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Util;
using Android.Support.V7.Widget;
using Android.Widget;
using Android.Support.V7.App;
using Android.Graphics;
using SQLite;
using Android.Text;
using Java.Lang;
using Android.Provider;
using Com.Yalantis.Ucrop;
using System.IO;
using Uri = Android.Net.Uri;
using Android.Support.V4.App;
using Newtonsoft.Json;
using System.Net.Http;
using Plugin.Connectivity;
using System.Net.Http.Headers;
using Android.Graphics.Drawables;
using System.Json;
using System.Globalization;
using Android.Content.PM;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityRegistroCaballos", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class ActivityRegistroCaballos : BaseActivity, IDialogInterfaceOnClickListener
    {
        PickerDate onDateSetListener;
        ImageView Foto;
        public string textoDate { get; set; }
        EditText horsename, horseweight, horseheight,avena;
        TextView dob;
        TextView wearable, breed, gender;
        Button cancelar, aceptar;
        SwitchCompat SwtVisibility;
        List<string> razas = new List<string>();
        ShareInside s = new ShareInside();
        ListView textListView;
        EditText buscar;
        LinearLayout lnOcultar;
        ProgressBar progressBar;
        string idhorses;
        string opcion;
        string fecha;

        Bitmap bitCloud;

        #region Tomar Foto e Imagen Galeria

        int camrequestcode = 100;
        Uri cameraUri;

        private const string TAG = "SampleActivity";
        Stream stream;

        private const int REQUEST_SELECT_PICTURE = 0x01;
        private const string SAMPLE_CROPPED_IMAGE_NAME = "SampleCropImage";

        #endregion

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_registrocaballos);
            new ShareInside().GuardarPantallaActual("ActivityRegistroCaballos");
            Window.SetStatusBarColor(Color.Black);
            
            #region Declarar cajas de texto
            horsename = FindViewById<EditText>(Resource.Id.txthorsename);
            horseheight = FindViewById<EditText>(Resource.Id.txtheight);
            horseweight = FindViewById<EditText>(Resource.Id.txthorseweigth);
            wearable = FindViewById<TextView>(Resource.Id.txtwearable);
            breed = FindViewById<TextView>(Resource.Id.txtbreed);
            gender = FindViewById<TextView>(Resource.Id.txtgender);
            dob = FindViewById<TextView>(Resource.Id.txtdob);
            Foto = FindViewById<ImageView>(Resource.Id.btnfoto);
            cancelar = FindViewById<Button>(Resource.Id.btncancel);
            aceptar = FindViewById<Button>(Resource.Id.btndone);
            avena = FindViewById<EditText>(Resource.Id.txtoat);
            //SwtVisibility = FindViewById<SwitchCompat>(Resource.Id.visible);


            horseheight.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            horseheight.Hint =GetText(Resource.String.mts);
            horseweight.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            horseweight.Hint = GetText(Resource.String.kg);
            wearable.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            wearable.Hint = GetText(Resource.String.Coming_soon);
            breed.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            breed.Hint = GetText(Resource.String.Optional);
            gender.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            gender.Hint = GetText(Resource.String.Optional);
            dob.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            dob.Hint = GetText(Resource.String.Optional);
            avena.Hint = GetText(Resource.String.kg);
            avena.SetHintTextColor(Color.Argb(127, 255, 255, 255));
            #endregion

            Drawable drawableImage = GetDrawable(Resource.Drawable.FotoHorse);
            Bitmap bitDrawableImage = ((BitmapDrawable)drawableImage).Bitmap; //BitmapFactory.DecodeResource(this.GetDrawable, Resource.Drawable.FotoHorse);
            
            Foto.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitDrawableImage, (int)ConvertDPToPixels(80)));

            cancelar.Click += delegate {
                StartActivity(typeof(ActivitySettings));
                Finish();
            };

            breed.Click += delegate {

                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.DialogoRazas);
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                //if (new ShareInside().ConsultarIdioma().Idioma == "es")
                //{
                //    var lang = "es";
                //    var consulta = con.Query<Razas>("select Id_Raza, " + lang + " from Razas asc order by " + lang + " ", (new Razas()).en);

                //    textListView = alertar.FindViewById<ListView>(Resource.Id.ListaRazas);
                //    textListView.Adapter = new AdaptadorRazas(this, consulta, alertar, breed,"es");
                //    buscar = alertar.FindViewById<EditText>(Resource.Id.buscar);
                //    buscar.TextChanged += (object sender, TextChangedEventArgs e) =>
                //    {
                //        var consulta2 = con.Query<Razas>("select Id_Raza," + lang + "  from Razas where  " + lang + " like  '" + buscar.Text + "%'", (new Razas()).es);
                //        textListView.Adapter = new AdaptadorRazas(this, consulta2, alertar, breed,"es");
                //    };
                //}
                //else
                //{
                    var consulta = con.Query<Razas>("select * from Razas asc order by  en ");

                    textListView = alertar.FindViewById<ListView>(Resource.Id.ListaRazas);
                    textListView.Adapter = new AdaptadorRazas(this, consulta, alertar, breed);
                    buscar = alertar.FindViewById<EditText>(Resource.Id.buscar);
                    buscar.TextChanged += (object sender, TextChangedEventArgs e) =>
                    {
                        var consulta2 = con.Query<Razas>("select * from Razas where  en like  '" + buscar.Text + "%'");
                        textListView.Adapter = new AdaptadorRazas(this, consulta2, alertar, breed);
                    };
                //}
                alertar.Show();


            };

            gender.Click += delegate
            {
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.DialogoGender);
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                if (new ShareInside().ConsultarIdioma().Idioma == "es")
                {
                    var lang = "es";
                    var consulta = con.Query<Razas>("select id_gender," + lang + " from gender asc order by " + lang + " ", (new Razas()).es);

                    textListView = alertar.FindViewById<ListView>(Resource.Id.Listagender);
                    textListView.Adapter = new AdaptadorGender(this, consulta, alertar, gender,"es");
                }
                else
                {
                    var lang = "en";
                    var consulta = con.Query<Razas>("select id_gender," + lang + " from gender asc order by " + lang + " ", (new Razas()).en);

                    textListView = alertar.FindViewById<ListView>(Resource.Id.Listagender);
                    textListView.Adapter = new AdaptadorGender(this, consulta, alertar, gender,"en");
                }
               
                alertar.Show();
            };

            dob.Click += delegate {
                
                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                int year = calendar.Get(Java.Util.CalendarField.Year);
                int month = calendar.Get(Java.Util.CalendarField.Month);
                int day_of_month = calendar.Get(Java.Util.CalendarField.DayOfMonth);

                DatePickerDialog dialog = new DatePickerDialog(this, Resource.Style.ThemeOverlay_AppCompat_Dialog_Alert,
                   onDateSetListener, year, month, day_of_month);

                dialog.DatePicker.MaxDate = JavaSystem.CurrentTimeMillis();

                dialog.Show();
            };

            onDateSetListener = new PickerDate(dob);
            
            Foto.Click += delegate
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

            idhorses = this.Intent.GetStringExtra("idhorse");

            opcion = this.Intent.GetStringExtra("opcion");

            if (opcion == "2")
            {
                try
                {
                    #region Actualizar caballos 
                    Window.SetSoftInputMode(Android.Views.SoftInput.StateHidden);
                    

                    string server = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + idhorses + "";

                    HttpClient cliente2 = new HttpClient();
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        cliente2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(s.ConsultToken());

                        progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                        FindViewById<RelativeLayout>(Resource.Id.fondogeneral).AddView(progressBar, p);
                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                        var clientePost2 = await cliente2.GetAsync(server);

                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                        clientePost2.EnsureSuccessStatusCode();

                        if (clientePost2.IsSuccessStatusCode)
                        {
                            JsonValue ObjJson = await clientePost2.Content.ReadAsStringAsync();
                            try
                            {
                                var datos = JsonConvert.DeserializeObject<InHorse>(ObjJson);
                                horsename.Text = datos.name;
                                if (datos.weight == 0.0)
                                    horseweight.Hint = GetText(Resource.String.Optional); 
                                else
                                    horseweight.Text = datos.weight.ToString();
                                if (datos.height == 0.0)
                                    horseheight.Hint = GetText(Resource.String.mts); 
                                else
                                    horseheight.Text = datos.height.ToString();
                                if (datos.gender == 0)
                                    gender.Hint = GetText(Resource.String.Optional);
                                else
                                {
                                    gender.Text = IdiomaGender(datos.gender);
                                    gender.Tag = datos.gender;
                                }
                                if (datos.breed == 0)
                                    breed.Hint = GetText(Resource.String.Optional);
                                else
                                {
                                    breed.Text = IdiomaBreed(datos.breed);
                                    breed.Tag = datos.breed;
                                }

                                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                               var consulta= con.Query<ConsumoAvena>("select * from ConsumoAvena where id_caballo='"+idhorses+"'");
                                avena.Text = consulta[0].kilogramos.ToString();
                                byte[] imageAsBytes = Base64.Decode(datos.photo.data, Base64Flags.Default);
                                var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                                Foto.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));
                                

                                //SwtVisibility.Enabled = false;
                                //SwtVisibility.Checked = true;
                                //if (datos.visible)
                                //    SwtVisibility.Checked = true;
                                //else
                                //    SwtVisibility.Checked = false;
                                //ocultar el swithc

                                if (datos.birthday == "1880-01-01T00:00:00.000Z")
                                {
                                    dob.Hint = GetText(Resource.String.Optional);
                                }
                                else
                                {
                                    DateTime dateee = DateTime.Parse(datos.birthday).ToUniversalTime();
                                    dob.Text = dateee.ToString("dd-MM-yyyy");
                                }

                            }
                            catch (System.Exception ex)
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                    }
                    #endregion
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();

                }
            }

            aceptar.Click += async delegate
                    {
                        try
                        {
                            bool no = true, visible = true;
                            string altura = "0", peso = "0",ave="", Breed, Gender;

                            if (dob.Text.Length <= 0)
                            {
                                fecha = "1880-01-01";
                            }
                            else
                            {
                                fecha = DateTime.ParseExact(dob.Text, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                            }

                            //if (SwtVisibility.Checked)
                            //    visible = true;
                            //else
                            //    visible = false;
                            foreach (char item in horseheight.Text.ToString())
                            {
                                if (item == 44)
                                    altura += ".";
                                else
                                    altura += item;
                            }
                            foreach (char item in horseweight.Text.ToString())
                            {
                                if (item == 44)
                                    peso += ".";
                                else
                                    peso += item;
                            }
                            foreach (char item in avena.Text.ToString())
                            {
                                if (item == 44)
                                    ave += ".";
                                else
                                    ave += item;
                            }
                            if (horsename.Text.Length <= 0 || avena.Text.Length<=0 || horseweight.Text.Length<=0)
                            {
                                Toast.MakeText(this,Resource.String.You_need_to, ToastLength.Short).Show();
                                
                            }
                            else if (horsename.Text.Length >= 3)
                            {
                                foreach (char item in horsename.Text)
                                {
                                    if (item != 32)
                                        no = false;
                                }
                                if (no == false)
                                {
                                    if (horseheight.Text.Length <= 0)
                                        altura = "0";
                                    if (horseweight.Text.Length <= 0)
                                        peso = "0";
                                    if (breed.Text.Length <= 0)
                                        breed.Tag = 0;
                                    if (gender.Text.Length <= 0)
                                        gender.Tag = 0;
                                    Breed = breed.Tag.ToString();
                                    Gender = gender.Tag.ToString();

                                    string Photo = "";
                                    using (var stream = new MemoryStream())
                                    {
                                        Bitmap bite = ((BitmapDrawable)Foto.Drawable).Bitmap;
                                        bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                                        var bites = stream.ToArray();
                                        Photo = System.Convert.ToBase64String(bites);
                                    }
                                    if (opcion == "2")
                                    {
                                        string serv = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + idhorses;
                                        string json2 = "application/json";

                                        List<InHorse> hor = new List<InHorse>();
                                        hor.Add(new InHorse()
                                        {
                                            name = horsename.Text,
                                            breed = int.Parse(Breed),
                                            weight = float.Parse(peso),
                                            height = float.Parse(altura),
                                            birthday = fecha,
                                            visible = visible,
                                            gender = int.Parse(Gender),
                                            photo = new Photo()
                                            {
                                                data = Photo,
                                                content_type = "image / png"
                                            }
                                        });

                                        var Cadena = JsonConvert.SerializeObject(hor, Formatting.Indented, new JsonSerializerSettings());
                                        string json = Cadena.Substring(1, Cadena.Length - 2);

                                        HttpClient client = new HttpClient();

                                        if (CrossConnectivity.Current.IsConnected)
                                        {
                                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(s.ConsultToken());

                                            progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                                            p.AddRule(LayoutRules.CenterInParent);
                                            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                                            FindViewById<RelativeLayout>(Resource.Id.fondogeneral).AddView(progressBar, p);
                                            progressBar.Visibility = Android.Views.ViewStates.Visible;
                                            Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                            var cliput = await client.PutAsync(serv, new StringContent(json.ToString(), Encoding.UTF8, json2));
                                            
                                            cliput.EnsureSuccessStatusCode();
                                            if (cliput.IsSuccessStatusCode)
                                            {
                                                await s.GuardadoFotosYConsulta();
                                                await s.GuardarFotosEditaddas(idhorses);
                                                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                                                var consulta = con.Query<ConsumoAvena>("select * from ConsumoAvena where id_caballo='" + idhorses + "'");
                                                if (consulta.Count <= 0)
                                                {
                                                    con.Insert(new ConsumoAvena()
                                                    {
                                                        id_caballo = idhorses,
                                                        kilogramos = double.Parse(ave)
                                                    });
                                                }
                                                else
                                                    con.Query<ConsumoAvena>("update ConsumoAvena set kilogramos="+double.Parse(ave)+" where id_caballo='"+idhorses+"'");
                                                consulta = con.Query<ConsumoAvena>("select * from horses where id='" + idhorses + "'");
                                                if (consulta.Count <= 0)
                                                {
                                                    con.Insert(new Horses()
                                                    {
                                                        id = idhorses,
                                                        owner_name = new ShareInside().ConsultarDatosUsuario()[0].username,
                                                        owner = new ShareInside().ConsultarDatosUsuario()[0].id,
                                                        name = horsename.Text,
                                                        photo = hor[0].photo.data,
                                                        Sync = 0
                                                    });
                                                }
                                                else
                                                    con.Query<Horses>("update Horses set owner_name='" + new ShareInside().ConsultarDatosUsuario()[0].username + "', name='" + horsename.Text + "', photo='" + hor[0].photo.data + "' where id='" + idhorses + "'");
                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                StartActivity(typeof(ActivitySettings));
                                                Finish();
                                            }
                                            else
                                            {
                                                Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                                                fecha = "";
                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                            }
                                        }
                                        else
                                        {
                                            Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        }

                                    }
                                    else
                                    {
                                        string server2 = "https://cabasus-mobile.azurewebsites.net/v1/horses";
                                        string json2 = "application/json";

                                        List<InHorse> hor = new List<InHorse>();
                                        hor.Add(new InHorse()
                                        {
                                            name = horsename.Text,
                                            breed = int.Parse(Breed),
                                            weight = float.Parse(peso),
                                            height = float.Parse(altura),
                                            birthday = fecha,
                                            visible = visible,
                                            gender = int.Parse(Gender),
                                            photo = new Photo()
                                            {
                                                data = Photo,
                                                content_type = "image / png"
                                            }
                                        });

                                        var Cadena = JsonConvert.SerializeObject(hor, Formatting.Indented, new JsonSerializerSettings());
                                        string json = Cadena.Substring(1, Cadena.Length - 2);

                                        HttpClient client = new HttpClient();

                                        if (CrossConnectivity.Current.IsConnected)
                                        {
                                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(s.ConsultToken());

                                            progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                                            p.AddRule(LayoutRules.CenterInParent);
                                            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                                            FindViewById<RelativeLayout>(Resource.Id.fondogeneral).AddView(progressBar, p);

                                            progressBar.Visibility = Android.Views.ViewStates.Visible;
                                            Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                            var clientePost2 = await client.PostAsync(server2, new StringContent(json.ToString(), Encoding.UTF8, json2));

                                          
                                            clientePost2.EnsureSuccessStatusCode();

                                            if (clientePost2.IsSuccessStatusCode)
                                            {
                                                await s.GuardadoFotosYConsulta();
                                                var content = await clientePost2.Content.ReadAsStringAsync();
                                                var data = JsonConvert.DeserializeObject<RootObjectConsulta>(content);
                                                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                                                var datosusuario = new ShareInside().ConsultarDatosUsuario();
                                                var nombreusuario=datosusuario[0].username;
                                                con.Insert(new ConsumoAvena() {
                                                    id_caballo = data.id,
                                                    kilogramos = double.Parse(ave)
                                                });
                                                con.Insert(new Horses()
                                                {
                                                    id = data.id,
                                                    owner_name = new ShareInside().ConsultarDatosUsuario()[0].username,
                                                    owner = data.owner,
                                                    name=data.name,
                                                    photo=data.photo.data,
                                                    Sync = 0
                                                });
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                                StartActivity(typeof(ActivitySettings));
                                                Finish();
                                            }
                                            else
                                            {
                                                Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            }
                                        }
                                        else
                                        {
                                            Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                                            fecha = "";
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        }
                                    }
                                }
                                else
                                    Toast.MakeText(this, Resource.String.You_need_to, ToastLength.Short).Show();
                                fecha = "";
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            }
                            else
                                Toast.MakeText(this, Resource.String.The_name_must, ToastLength.Short).Show();
                            fecha = "";
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }

                        catch (System.Exception ex)
                        {
                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                            fecha = "";
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                    };
            
     }
        private float ConvertDPToPixels(int DP)
        {
            var Pix = ((DP) * Resources.DisplayMetrics.Density);
            return Pix;
        }
        public string IdiomaGender(int Palabra)
        {
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
            var l = new ShareInside().ConsultarIdioma().Idioma;
            var consulta = con.Query<Idiomas>("select " + l + " from gender where id_gender = '" + Palabra + "'");
            string Resultado = "";
            foreach (var item in consulta)
            {
                if (item.en != null)
                {
                    Resultado = item.en;
                    break;
                }
                else if (item.de != null)
                {
                    Resultado = item.de;
                    break;
                }
                else if (item.es != null)
                {
                    Resultado = item.es;
                    break;
                }
                //else if (item.fr != null)
                //{
                //    Resultado = item.fr;
                //    break;
                //}
                //else if (item.it != null)
                //{
                //    Resultado = item.it;
                //    break;
                //}
                //else if (item.no != null)
                //{
                //    Resultado = item.no;
                //    break;
                //}
                //else if (item.ar != null)
                //{
                //    Resultado = item.ar;
                //    break;
                //}
                //else if (item.tr != null)
                //{
                //    Resultado = item.tr;
                //    break;
                //}
                //else if (item.pt != null)
                //{
                //    Resultado = item.pt;
                //    break;
                //}
                //else if (item.zh != null)
                //{
                //    Resultado = item.zh;
                //    break;
                //}
                //else if (item.ru != null)
                //{
                //    Resultado = item.ru;
                //    break;
                //}
            }
            return Resultado;
        }

        public string IdiomaBreed(int Palabra)
        {

            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
          //  var l = new ShareInside().ConsultarIdioma().Idioma;
            var consulta = con.Query<Idiomas>("select en  from Razas where Id_Raza = '" + Palabra + "'");
            string Resultado = "";
            foreach (var item in consulta)
            {
                Resultado = item.en;
                
                //else if (item.de != null)
                //{
                //    Resultado = item.de;
                //    break;
                //}
                //else if (item.es != null)
                //{
                //    Resultado = item.es;
                //    break;
                //}
                ////else if (item.fr != null)
                //{
                //    Resultado = item.fr;
                //    break;
                //}
                //else if (item.it != null)
                //{
                //    Resultado = item.it;
                //    break;
                //}
                //else if (item.no != null)
                //{
                //    Resultado = item.no;
                //    break;
                //}
                //else if (item.ar != null)
                //{
                //    Resultado = item.ar;
                //    break;
                //}
                //else if (item.tr != null)
                //{
                //    Resultado = item.tr;
                //    break;
                //}
                //else if (item.pt != null)
                //{
                //    Resultado = item.pt;
                //    break;
                //}
                //else if (item.zh != null)
                //{
                //    Resultado = item.zh;
                //    break;
                //}
                //else if (item.ru != null)
                //{
                //    Resultado = item.ru;
                //    break;
                //}
            }
            return Resultado;
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
                        Toast.MakeText(this, GetText(Resource.String.You_cant_recover_image), ToastLength.Short).Show();
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
                int newWidth = 200;
                int newHeight = 200;

                float scaleWidth = ((float)newWidth) / width;
                float scaleHeight = ((float)newHeight) / height;

                Matrix matrix = new Matrix();

                matrix.PostScale(scaleWidth, scaleHeight);

                Bitmap resizedBitmap = Bitmap.CreateBitmap(bitmap, 0, 0, width, height, matrix, true);
                bitCloud = Bitmap.CreateBitmap(bitmap, 0, 0, width, height, matrix, true);

                resizedBitmap = (new ShareInside()).RedondearbitmapImagen(resizedBitmap,200);

                Foto.SetImageBitmap(resizedBitmap);
                Foto.SetBackgroundResource(Resource.Drawable.cornerImageButton);
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
           // base.OnBackPressed();

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
    public class InHorse
    {
        public string name { get; set; }
        public int breed { get; set; }
        public float weight { get; set; }
        public float height { get; set; }
        public string birthday { get; set; }
        public bool visible { get; set; }
        public int gender { get; set; }
        public Photo photo { get; set; }
    }

    public class Idiomas
    {
        public int Id_Lenguaje { get; set; }
        public string en { get; set; }
        public string de { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
        public string it { get; set; }
        public string no { get; set; }
        public string ar { get; set; }
        public string tr { get; set; }
        public string pt { get; set; }
        public string zh { get; set; }
        public string ru { get; set; }
    }
    public class PhotoConsulta
    {
        public string data { get; set; }
        public string contentType { get; set; }
    }

    public class RootObjectConsulta
    {
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public int breed { get; set; }
        public int weight { get; set; }
        public int height { get; set; }
        public DateTime birthday { get; set; }
        public int gender { get; set; }
        public List<object> shares { get; set; }
        public List<object> images { get; set; }
        public PhotoConsulta photo { get; set; }
        public bool visible { get; set; }
        public string id { get; set; }
    }
}