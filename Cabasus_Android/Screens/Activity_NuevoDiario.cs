using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Gigamole.Infinitecycleviewpager;
using Newtonsoft.Json;
using Plugin.Connectivity;
using SQLite;
using Java.Lang;
using Uri = Android.Net.Uri;
using Com.Yalantis.Ucrop;
using Android.Graphics.Drawables;
using Android.Views.InputMethods;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using System.Threading.Tasks;
using Android.Content.PM;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", Label = "Activity_NuevoDiario")]
    public class Activity_NuevoDiario : BaseActivity, IDialogInterfaceOnClickListener
    {
        List<string> lstImages = new List<string>();
        List<string> lstNombre = new List<string>();
        List<string> lstId = new List<string>();
        bool ActualizarDiario = false;
        string Date;
        int EstadoDelCaballo = 1;
        string id_diario;
        Uri cameraUri;
        int camrequestcode = 100, ContaFoto = 0;
        private const int REQUEST_SELECT_PICTURE = 0x01;
        Stream stream;
        ImageView Foto1, Foto2, Foto3;
        string stringFoto1 = "", stringFoto2 = "", stringFoto3 = "";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_NuevoDiario);
            Window.SetStatusBarColor(Color.Black);
            #region Abrir las diferentes pantalla con los diferentes botones;
            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            btnhome.Click += delegate { StartActivity(typeof(ActivityHome)); Finish(); };
            btnactivity.Click += delegate { StartActivity(typeof(ActivityActivity)); Finish(); };
            btndiary.Click += delegate { StartActivity(typeof(ActivityDiary)); Finish(); };
            btncalendar.Click += delegate { StartActivity(typeof(ActivityCalendar)); Finish(); };
            btn_settings.Click += delegate { StartActivity(typeof(ActivitySettings)); Finish(); };
            #endregion;

            #region Seleccion de caballos
            llenarListaCaballo();

            HorizontalInfiniteCycleViewPager cycleViewPager = FindViewById<HorizontalInfiniteCycleViewPager>(Resource.Id.horizontal_viewPager_nd);
            CaballosUpFormat CaballosUpFormat = new CaballosUpFormat(lstNombre, lstId, BaseContext, cycleViewPager);
            cycleViewPager.Adapter = CaballosUpFormat;
            cycleViewPager.CurrentItem = new ShareInside().ConsultarPosicionPiker()[0].Position_HorseSelected;
            cycleViewPager.PageSelected += delegate
            {
                new ShareInside().GuardarPosicionPiker(lstId[cycleViewPager.RealItem], lstNombre[cycleViewPager.RealItem], cycleViewPager.RealItem);
            };
            #endregion

            #region Datos Usuario
            var ImagenUsuario = FindViewById<ImageView>(Resource.Id.imgUsuarioDiary);
            byte[] imageAsBytes = Base64.Decode((new ShareInside().ConsultarDatosUsuario()[0].photo.data), Base64Flags.Default);
            var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
            ImagenUsuario.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));
            var NombreUsuario = FindViewById<TextView>(Resource.Id.lblNombreUser);
            NombreUsuario.Text = new ShareInside().ConsultarDatosUsuario()[0].username;
            var Fecha = FindViewById<TextView>(Resource.Id.lblFechaDiaDiary);
            Date = this.Intent.GetStringExtra("Date");
            Fecha.Text = Date;
            #endregion

            #region Cancelar
            var btnCancel = FindViewById<TextView>(Resource.Id.btnCancelDiary);
            btnCancel.Click += delegate { StartActivity(typeof(ActivityDiary)); Finish(); };
            #endregion

            #region Estado del caballo
            var btnFeliz = FindViewById<ImageView>(Resource.Id.imgFeliz);
            var btnNormal = FindViewById<ImageView>(Resource.Id.imgNormal);
            var btnTriste = FindViewById<ImageView>(Resource.Id.imgTriste);
            btnFeliz.Click += delegate
            {
                EstadoDelCaballo = 1;
                btnFeliz.Background = null;
                btnNormal.Background = null;
                btnTriste.Background = null;
                var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonhappyDorado);
                btnFeliz.SetImageDrawable(imgfeliz);
                var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonneutral);
                btnNormal.SetImageDrawable(imgnormal);
                var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonsad);
                btnTriste.SetImageDrawable(imgtriste);
            };
            btnNormal.Click += delegate
            {
                EstadoDelCaballo = 2;
                btnFeliz.Background = null;
                btnNormal.Background = null;
                btnTriste.Background = null;
                var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonhappy);
                btnFeliz.SetImageDrawable(imgfeliz);
                var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonneutraldorado);
                btnNormal.SetImageDrawable(imgnormal);
                var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonsad);
                btnTriste.SetImageDrawable(imgtriste);
            };
            btnTriste.Click += delegate
            {
                EstadoDelCaballo = 3;
                btnFeliz.Background = null;
                btnNormal.Background = null;
                btnTriste.Background = null;
                var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonhappy);
                btnFeliz.SetImageDrawable(imgfeliz);
                var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonneutral);
                btnNormal.SetImageDrawable(imgnormal);
                var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonsaddorado);
                btnTriste.SetImageDrawable(imgtriste);
            };
            #endregion

            #region Descripcion
            var txtComentario = FindViewById<EditText>(Resource.Id.txtCpmentarioDiario);
            Window.SetSoftInputMode(SoftInput.StateHidden);
            txtComentario.Click += delegate {
                txtComentario.RequestFocus();
                Window.SetSoftInputMode(SoftInput.StateVisible);
            };
            #endregion

            #region Fotos
            Foto1 = FindViewById<ImageView>(Resource.Id.Foto1);
            Foto2 = FindViewById<ImageView>(Resource.Id.Foto2);
            Foto3 = FindViewById<ImageView>(Resource.Id.Foto3);

            Foto1.Click += delegate
            {
                ContaFoto = 1;
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(false);
                alertar.SetContentView(Resource.Layout.layout_CustomAlert);
                alertar.Show();

                alertar.FindViewById<Button>(Resource.Id.btnCancel).Click += delegate
                {
                    ContaFoto = 0;
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
            Foto2.Click += delegate
            {
                ContaFoto = 2;
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(false);
                alertar.SetContentView(Resource.Layout.layout_CustomAlert);
                alertar.Show();

                alertar.FindViewById<Button>(Resource.Id.btnCancel).Click += delegate
                {
                    ContaFoto = 0;
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
            Foto3.Click += delegate
            {
                ContaFoto = 3;
                Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(false);
                alertar.SetContentView(Resource.Layout.layout_CustomAlert);
                alertar.Show();

                alertar.FindViewById<Button>(Resource.Id.btnCancel).Click += delegate
                {
                    ContaFoto = 0;
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
            string cadena = "";
            try
            {
                cadena = this.Intent.GetStringExtra("ActualizarDiario");
                ActualizarDiario = bool.Parse(cadena.Split('$')[0]);
            }
            catch (System.Exception) { }

            if (ActualizarDiario)
            {
                id_diario = cadena.Split('$')[1];
                try
                {
                    int.Parse(id_diario);

                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where id_diary = " + id_diario + ";", (new DiarioLocal()).id_diary);

                    Fecha.Text = ConsultaDiarios[0].dates;
                    if (ConsultaDiarios[0].estado_caballo == "1")
                    {
                        EstadoDelCaballo = 1;
                        btnFeliz.Background = null;
                        btnNormal.Background = null;
                        btnTriste.Background = null;
                        var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonhappyDorado);
                        btnFeliz.SetImageDrawable(imgfeliz);
                        var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonneutral);
                        btnNormal.SetImageDrawable(imgnormal);
                        var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonsad);
                        btnTriste.SetImageDrawable(imgtriste);
                    }
                    else if (ConsultaDiarios[0].estado_caballo == "2")
                    {
                        EstadoDelCaballo = 2;
                        btnFeliz.Background = null;
                        btnNormal.Background = null;
                        btnTriste.Background = null;
                        var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonhappy);
                        btnFeliz.SetImageDrawable(imgfeliz);
                        var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonneutraldorado);
                        btnNormal.SetImageDrawable(imgnormal);
                        var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonsad);
                        btnTriste.SetImageDrawable(imgtriste);
                    }
                    else
                    {
                        EstadoDelCaballo = 3;
                        btnFeliz.Background = null;
                        btnNormal.Background = null;
                        btnTriste.Background = null;
                        var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonhappy);
                        btnFeliz.SetImageDrawable(imgfeliz);
                        var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonneutral);
                        btnNormal.SetImageDrawable(imgnormal);
                        var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonsaddorado);
                        btnTriste.SetImageDrawable(imgtriste);
                    }
                    txtComentario.Text = ConsultaDiarios[0].descripcion;

                    byte[] imageAsBytesFoto1 = Base64.Decode(ConsultaDiarios[0].foto1, Base64Flags.Default);
                    var bitFoto1 = BitmapFactory.DecodeByteArray(imageAsBytesFoto1, 0, imageAsBytesFoto1.Length);
                    Foto1.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitFoto1, 200));
                    byte[] imageAsBytesFoto2 = Base64.Decode(ConsultaDiarios[0].foto2, Base64Flags.Default);
                    var bitFoto2 = BitmapFactory.DecodeByteArray(imageAsBytesFoto2, 0, imageAsBytesFoto2.Length);
                    Foto2.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitFoto2, 200));
                    byte[] imageAsBytesFoto3 = Base64.Decode(ConsultaDiarios[0].foto3, Base64Flags.Default);
                    var bitFoto3 = BitmapFactory.DecodeByteArray(imageAsBytesFoto3, 0, imageAsBytesFoto3.Length);
                    Foto3.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitFoto3, 200));
                }
                catch (System.Exception)
                {
                    #region Servidor
                    string server = "https://cabasus-mobile.azurewebsites.net/v1/journal/" + id_diario;
                    var uri = new System.Uri(string.Format(server, string.Empty));
                    HttpClient Cliente = new HttpClient();
                    Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                    var consulta = await Cliente.GetAsync(uri);
                    consulta.EnsureSuccessStatusCode();
                    if (consulta.IsSuccessStatusCode)
                    {
                        JsonValue ConsultaJson = await consulta.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<JornadasNube>(ConsultaJson);

                        Fecha.Text = data.date.Substring(0, 10);
                        if (data.horse_status == "1")
                        {
                            EstadoDelCaballo = 1;
                            btnFeliz.Background = null;
                            btnNormal.Background = null;
                            btnTriste.Background = null;
                            var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonhappyDorado);
                            btnFeliz.SetImageDrawable(imgfeliz);
                            var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonneutral);
                            btnNormal.SetImageDrawable(imgnormal);
                            var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonsad);
                            btnTriste.SetImageDrawable(imgtriste);
                        }
                        else if (data.horse_status == "2")
                        {
                            EstadoDelCaballo = 2;
                            btnFeliz.Background = null;
                            btnNormal.Background = null;
                            btnTriste.Background = null;
                            var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonhappy);
                            btnFeliz.SetImageDrawable(imgfeliz);
                            var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonneutraldorado);
                            btnNormal.SetImageDrawable(imgnormal);
                            var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonsad);
                            btnTriste.SetImageDrawable(imgtriste);
                        }
                        else
                        {
                            EstadoDelCaballo = 3;
                            btnFeliz.Background = null;
                            btnNormal.Background = null;
                            btnTriste.Background = null;
                            var imgfeliz = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonhappy);
                            btnFeliz.SetImageDrawable(imgfeliz);
                            var imgnormal = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.emoticonneutral);
                            btnNormal.SetImageDrawable(imgnormal);
                            var imgtriste = ContextCompat.GetDrawable(ApplicationContext, Resource.Drawable.ResumenEmoticonsaddorado);
                            btnTriste.SetImageDrawable(imgtriste);
                        }
                        txtComentario.Text = data.content;
                        txtComentario.SetTextColor(Color.White);

                        byte[] imageAsBytesFoto1 = Base64.Decode(cadena.Split('$')[2], Base64Flags.Default);
                        var bitFoto1 = BitmapFactory.DecodeByteArray(imageAsBytesFoto1, 0, imageAsBytesFoto1.Length);
                        Foto1.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitFoto1, 200));
                        byte[] imageAsBytesFoto2 = Base64.Decode(cadena.Split('$')[3], Base64Flags.Default);
                        var bitFoto2 = BitmapFactory.DecodeByteArray(imageAsBytesFoto2, 0, imageAsBytesFoto2.Length);
                        Foto2.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitFoto2, 200));
                        byte[] imageAsBytesFoto3 = Base64.Decode(cadena.Split('$')[4], Base64Flags.Default);
                        var bitFoto3 = BitmapFactory.DecodeByteArray(imageAsBytesFoto3, 0, imageAsBytesFoto3.Length);
                        Foto3.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bitFoto3, 200));
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.You_need_internet), ToastLength.Short).Show();
                        Intent intent = new Intent(this, (typeof(ActivityDiary)));
                        intent.PutExtra("Date", Fecha.Text);
                        this.StartActivity(intent);
                        Finish();
                    }
                    #endregion
                }
            }

            #region Guardar
            var btnDone = FindViewById<TextView>(Resource.Id.btnDoneDiary);
            btnDone.Click += async delegate {
                #region ProgressBar
                ProgressBar progressBar;
                progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                p.AddRule(LayoutRules.CenterInParent);
                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                FindViewById<RelativeLayout>(Resource.Id.FondoNuevoDiario).AddView(progressBar, p);

                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                #endregion
                if (txtComentario.Text.Trim().Equals(""))
                {
                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    Toast.MakeText(this, GetText(Resource.String.description_saving_diary), ToastLength.Short).Show();
                }
                else
                {
                    #region Codificar fotos
                    using (var stream = new MemoryStream())
                    {
                        Bitmap bite = ((BitmapDrawable)Foto1.Drawable).Bitmap;
                        bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        var bites = stream.ToArray();
                        stringFoto1 = System.Convert.ToBase64String(bites);
                    }
                    using (var stream = new MemoryStream())
                    {
                        Bitmap bite = ((BitmapDrawable)Foto2.Drawable).Bitmap;
                        bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        var bites = stream.ToArray();
                        stringFoto2 = System.Convert.ToBase64String(bites);
                    }
                    using (var stream = new MemoryStream())
                    {
                        Bitmap bite = ((BitmapDrawable)Foto3.Drawable).Bitmap;
                        bite.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        var bites = stream.ToArray();
                        stringFoto3 = System.Convert.ToBase64String(bites);
                    }
                    #endregion
                    try
                    {

                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        if (ActualizarDiario)
                        {
                            try
                            {
                                var Actualizar = con.Query<DiarioLocal>("update diarys set estado_caballo = " + EstadoDelCaballo + ", descripcion = '" + txtComentario.Text + "', foto1 = '" + stringFoto1 + "', foto2 = '" + stringFoto2 + "', foto3 = '" + stringFoto3 + "' where id_diary = " + id_diario + ";", (new DiarioLocal()).id_diary);
                                con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                Intent intent = new Intent(this, (typeof(ActivityDiary)));
                                intent.PutExtra("Date", Fecha.Text);
                                this.StartActivity(intent);
                                Finish();
                            }
                            catch (System.Exception)
                            {
                                string actualizarFoto1 = await ActualizarFoto(cadena.Split('$')[5], stringFoto1);
                                string actualizarFoto2 = await ActualizarFoto(cadena.Split('$')[6], stringFoto2);
                                string actualizarFoto3 = await ActualizarFoto(cadena.Split('$')[7], stringFoto3);

                                string server = "https://cabasus-mobile.azurewebsites.net/v1/journal/" + id_diario;
                                string content = "application/json";
                                HttpClient Cliente = new HttpClient();
                                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                                string Estado = "";
                                if (EstadoDelCaballo == 1)
                                    Estado = "happy";
                                else if (EstadoDelCaballo == 2)
                                    Estado = "normal";
                                else
                                    Estado = "sad";
                                var datos = new ActualizarJornada()
                                {
                                    horse = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                                    content = txtComentario.Text,
                                    horse_status = Estado,
                                    images = new List<string>() { actualizarFoto1, actualizarFoto2, actualizarFoto3 }
                                };

                                var json = JsonConvert.SerializeObject(datos, Formatting.Indented, new JsonSerializerSettings());
                                if (CrossConnectivity.Current.IsConnected)
                                {
                                    var actualizar = await Cliente.PutAsync(server, new StringContent(json.ToString(), Encoding.UTF8, content));
                                    if (actualizar.IsSuccessStatusCode)
                                    {
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        Intent intent = new Intent(this, (typeof(ActivityDiary)));
                                        intent.PutExtra("Date", Fecha.Text);
                                        this.StartActivity(intent);
                                        Finish();
                                    }
                                    else
                                    {
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        Toast.MakeText(this, Resource.String.Failed_to_save_data, ToastLength.Short).Show();
                                    }
                                }
                                else
                                {
                                    Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                }

                            }
                        }
                        else
                        {
                            if (CrossConnectivity.Current.IsConnected)
                            {
                                #region Guardar Imagenes
                                string[] Fotos = new string[3];
                                for (int i = 0; i < 3; i++)
                                {
                                    #region Servidor imagenes
                                    string server = "https://cabasus-mobile.azurewebsites.net/v1/images";
                                    string json = "application/json";
                                    HttpClient cliente = new HttpClient();
                                    #endregion

                                    List<Imagenes> AddImage = new List<Imagenes>();
                                    #region Numero de foto
                                    if (i == 0)
                                    {
                                        string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(stringFoto1);
                                        await GuardarFotoNube(AddImage, CadenaFoto, new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected);
                                    }
                                    else if (i == 1)
                                    {
                                        string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(stringFoto2);
                                        await GuardarFotoNube(AddImage, CadenaFoto, new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected);
                                    }
                                    else
                                    {
                                        string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(stringFoto3);
                                        await GuardarFotoNube(AddImage, CadenaFoto, new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected);
                                    }
                                    #endregion
                                    try
                                    {
                                        #region guardado de imagenes
                                        var jsonConvert = JsonConvert.SerializeObject(AddImage);
                                        string jsonCorrecto = jsonConvert.Substring(1, jsonConvert.Length - 2);
                                        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                                        var respuesta = await cliente.PostAsync(server, new StringContent(jsonCorrecto, Encoding.UTF8, json));
                                        respuesta.EnsureSuccessStatusCode();
                                        if (respuesta.IsSuccessStatusCode)
                                        {
                                            JsonValue content = await respuesta.Content.ReadAsStringAsync();
                                            var data = JsonConvert.DeserializeObject<ImagenBeforeAdd>(content);
                                            Fotos[i] = data.id;
                                        }
                                        #endregion
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Toast.MakeText(this, ex.Message.ToString(), ToastLength.Long).Show();
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                    }
                                }
                                #endregion

                                #region Servidor jornadas
                                string serverJornadas = "https://cabasus-mobile.azurewebsites.net/v1/journals";
                                string jsonJornadas = "application/json";
                                HttpClient clienteJornadas = new HttpClient();
                                #endregion
                                #region Estado del caballo
                                string Estado = "";
                                if (EstadoDelCaballo == 1)
                                    Estado = "happy";
                                else if (EstadoDelCaballo == 2)
                                    Estado = "normal";
                                else
                                    Estado = "sad";
                                #endregion
                                #region Asignar datos de jornada
                                AgregarDiariosNube AddDiario = new AgregarDiariosNube()
                                {
                                    horse = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                                    content = txtComentario.Text,
                                    horse_status = Estado,
                                    date = Fecha.Text + "T20:51:10.467Z",
                                    images = Fotos,
                                    visibility = "private"
                                };
                                #endregion
                                try
                                {
                                    #region Guardado de jornada
                                    var jsonConvertJornadas = JsonConvert.SerializeObject(AddDiario);
                                    clienteJornadas.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                                    var respuestaJornadas = await clienteJornadas.PostAsync(serverJornadas, new StringContent(jsonConvertJornadas.ToString(), Encoding.UTF8, jsonJornadas));
                                    respuestaJornadas.EnsureSuccessStatusCode();
                                    if (respuestaJornadas.IsSuccessStatusCode)
                                    {
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        Intent intent = new Intent(this, (typeof(ActivityDiary)));
                                        intent.PutExtra("Date", Date);
                                        this.StartActivity(intent);
                                        Finish();
                                    }
                                    #endregion
                                }
                                catch (System.Exception ex)
                                {
                                    Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Long).Show();
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                }
                            }
                            else
                            {
                                var InsertarDiario = con.Query<DiarioLocal>(
                                    "insert into diarys values" +
                                    "(null," +
                                    " '" + new ShareInside().ConsultarDatosUsuario()[0].id + "'," +
                                    " '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'," +
                                    " '" + Date + "'," +
                                    " " + EstadoDelCaballo + "," +
                                    " '" + txtComentario.Text + "'," +
                                    " '" + stringFoto1 + "'," +
                                    " '" + stringFoto2 + "'," +
                                    " '" + stringFoto3 + "'," +
                                    "'private');",
                                    (new DiarioLocal()).id_diary);
                                con.Query<HorsesCloud>("UPDATE Horses SET Sync= 1 WHERE id='" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' ;", new HorsesCloud().id);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                Intent intent = new Intent(this, (typeof(ActivityDiary)));
                                intent.PutExtra("Date", Fecha.Text);
                                this.StartActivity(intent);
                                Finish();
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        Toast.MakeText(this, Resource.String.Failed_to_save_data, ToastLength.Short).Show();
                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    }
                }
            };
            #endregion

        }

        public override void OnBackPressed()
        {

        }

        public async Task GuardarFotoNube(List<Imagenes> AddImage, string CadenaFoto, string fk_horse)
        {
            AddImage.Add(new Imagenes()
            {
                owner = new Owner()
                {
                    id = new ShareInside().ConsultarDatosUsuario()[0].id
                },
                name = new ShareInside().ConsultarDatosUsuario()[0].id + "$" + fk_horse,
                data = CadenaFoto,
                content_type = "image/jpeg"
            });
        }
        public async Task<string> ActualizarFoto(string id_foto, string dataFoto)
        {
            string server = "https://cabasus-mobile.azurewebsites.net/v1/images/" + id_foto;
            string content = "application/json";
            HttpClient Cliente = new HttpClient();
            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

            var datos = new ActualizarFoto()
            {
                name = new ShareInside().ConsultarDatosUsuario()[0].id + "$" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected,
                data = dataFoto
            };

            var json = JsonConvert.SerializeObject(datos, Formatting.Indented, new JsonSerializerSettings());
            var actualizar = await Cliente.PutAsync(server, new StringContent(json.ToString(), Encoding.UTF8, content));

            if (actualizar.IsSuccessStatusCode)
            {
                JsonValue ConsultaJson = await actualizar.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ActualizarImagen>(ConsultaJson);
                return data.id;
            }
            else
            {
                return await ActualizarFoto(id_foto, dataFoto);
            }
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
                            AlertDialog.Builder mensajeError = new AlertDialog.Builder(this);
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
            UCrop uCrop = UCrop.Of(uri, Uri.FromFile(new Java.IO.File(CacheDir, destinationFileName))).WithAspectRatio(1, 1).WithMaxResultSize(1200, 1200);
            UCrop.Options options = new UCrop.Options();

            options.SetCompressionQuality(100);
            options.SetShowCropGrid(false);

            options.SetCompressionFormat(Bitmap.CompressFormat.Jpeg);
            options.SetFreeStyleCropEnabled(false);
            options.SetToolbarColor(Color.Black);
            options.SetStatusBarColor(Color.Black);
            options.SetToolbarWidgetColor(Color.White);
            options.SetToolbarTitle(GetString(Resource.String.select_image));
            uCrop.WithOptions(options);

            uCrop.Start(this);
        }

        private void HandleCropResult(Intent result)
        {
            Uri resultUri = UCrop.GetOutput(result);
            if (resultUri != null)
            {
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

                resizedBitmap = (new ShareInside()).RedondearbitmapImagen(resizedBitmap, (new ShareInside().ConvertPixelsToDp(600, Resources.DisplayMetrics.Density)));

                #region Imagen
                if (ContaFoto == 1)
                {
                    Foto1.SetImageBitmap(resizedBitmap);
                    Foto1.SetBackgroundResource(Resource.Drawable.RedondearFotoDiario);
                }
                else if (ContaFoto == 2)
                {
                    Foto2.SetImageBitmap(resizedBitmap);
                    Foto2.SetBackgroundResource(Resource.Drawable.RedondearFotoDiario);
                }
                else
                {
                    Foto3.SetImageBitmap(resizedBitmap);
                    Foto3.SetBackgroundResource(Resource.Drawable.RedondearFotoDiario);
                }
                #endregion
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
    public class DiarioLocal
    {
        public string id_diary { get; set; }
        public string fk_usuario { get; set; }
        public string fk_caballo { get; set; }
        public string dates { get; set; }
        public string estado_caballo { get; set; }
        public string descripcion { get; set; }
        public string foto1 { get; set; }
        public string foto2 { get; set; }
        public string foto3 { get; set; }
    }
}