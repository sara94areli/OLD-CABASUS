using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Support.V7.App;
using Android.Gms.Maps.Model;
using Android.Animation;
using Android.Views.Animations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using Newtonsoft.Json;
using Android.Graphics;
using SQLite;
using Android.Util;
using Android.Support.Design.Widget;
using System.Linq;

namespace Cabasus_Android.Screens
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, NoHistory = true)]
    public class ActivityActivityDetalle : AppCompatActivity, IOnMapReadyCallback, Android.Support.Design.Widget.AppBarLayout.IOnOffsetChangedListener
    {
        double distance_slow, distance_normal, distance_strong, distance_total;
        List<List<double>> latlat;
        List<string> latitud, longitud;

        ImageView imgCaballoDetalle, imgUsuarioDetalle;

        TextView txtUsuarioDetalle, txtDistance, txtTime, txtKm1, txtKm2, txtKm3, btnVerRuta, imgEmotionDetalle;
        
        Android.Support.Design.Widget.AppBarLayout appBar;
        CollapsingToolbarLayout toolBar;
        Android.Support.V7.Widget.Toolbar mToolBar;

        GoogleMap map;
        CameraUpdate camera;
        CircleOptions circleOptions;

        ProgressBar progressBar;

        bool local = true;

        public void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;

            if (!local)
            {
                LatLng latLng = new LatLng(latlat[0][1], latlat[0][0]);
                camera = CameraUpdateFactory.NewLatLngZoom(latLng, 15);
                map.AnimateCamera(camera);

                var polilyneOption = new PolylineOptions();
                polilyneOption.InvokeColor(0x66ff0000);
                polilyneOption.Add(latLng);

                if (latlat.Count <= 1)
                {
                    LatLng cordenadas = new LatLng(latlat[0][1], latlat[0][0]);
                    circleOptions = new CircleOptions();
                    circleOptions.InvokeCenter(cordenadas);
                    circleOptions.InvokeRadius(5);
                    circleOptions.InvokeFillColor(0x66ff0000);
                    var newCircle = map.AddCircle(circleOptions);//  .AddPolyline(polilyneOption);
                    newCircle.Visible = true;
                }
                else
                {
                    for (int i = 0; i < latlat.Count; i++)
                    {
                        try
                        {
                            polilyneOption.Add(new LatLng(latlat[i][1], latlat[i][0]));
                        }
                        catch
                        {

                        }
                    }
                }

                map.AddPolyline(polilyneOption);
            }
            else
            {
                //el primero es negativo

                List<double> nlatitud = new List<double>();
                List<double> nlongitud = new List<double>();
                
                foreach (var item in latitud)
                {
                    if (item == "1" || item == "2" || item == "3")
                    { }
                    else if (item == "")
                    { }
                    else
                        nlatitud.Add(double.Parse(item));

                }

                foreach (var item in longitud)
                {
                    if (item == "1" || item == "2" || item == "3")
                    { }
                    else if (item == "")
                    { }
                    else
                        nlongitud.Add(double.Parse(item));

                }

                LatLng latLng = new LatLng(nlatitud[0], nlongitud[0]);
                camera = CameraUpdateFactory.NewLatLngZoom(latLng, 18);
                map.AnimateCamera(camera);

                var listaLatitud = nlatitud.Distinct().ToList();
                var listaLongitud = nlongitud.Distinct().ToList();

                var polilyneOption = new PolylineOptions();
                polilyneOption.InvokeColor(0x66ff0000);
                polilyneOption.Add(latLng);

                if (listaLatitud.Count == 1)
                {
                    LatLng cordenadas = new LatLng(listaLatitud[0], listaLongitud[0]);
                    circleOptions = new CircleOptions();
                    circleOptions.InvokeCenter(cordenadas);
                    circleOptions.InvokeRadius(5);
                    circleOptions.InvokeFillColor(0x66ff0000);
                    var newCircle = map.AddCircle(circleOptions);//  .AddPolyline(polilyneOption);
                    newCircle.Visible = true;
                }
                else
                {
                    for (int i = 0; i < nlatitud.Count; i ++)
                    {
                        polilyneOption.Add(new LatLng(nlatitud[i], nlongitud[i]));
                        polilyneOption.InvokeWidth(20);
                    }
                    map.AddPolyline(polilyneOption);
                }
            }
        }

        private void setUpMap()
        {
            if (map == null)
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mapDetalle).GetMapAsync(this);
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_activity_detalle);
            Window.SetStatusBarColor(Color.Black);

            var ubicacion = Intent.GetStringExtra("ubicacion");
            var id_caballo = Intent.GetStringExtra("idActividadLocal");

            var caballo = Intent.GetStringExtra("Caballo");
            var usuario = Intent.GetStringExtra("Usuario");

            var imgCaballo = Intent.GetStringExtra("ConsultaPikerIdCaballo");
            
            #region Cazamiento

            imgCaballoDetalle = FindViewById<ImageView>(Resource.Id.imgCaballoDetalle);
            imgUsuarioDetalle = FindViewById<ImageView>(Resource.Id.imgUsuarioDetalle);
            imgEmotionDetalle = FindViewById<TextView>(Resource.Id.imgEmotionDetalle);

            txtUsuarioDetalle = FindViewById<TextView>(Resource.Id.txtUsuarioDetalle);
            txtDistance = FindViewById<TextView>(Resource.Id.txtDistanceDetalle);
            txtTime = FindViewById<TextView>(Resource.Id.txtTimeDetalle);
            txtKm1 = FindViewById<TextView>(Resource.Id.txtKm1);
            txtKm2 = FindViewById<TextView>(Resource.Id.txtKm2);
            txtKm3 = FindViewById<TextView>(Resource.Id.txtKm3);
            btnVerRuta = (TextView)FindViewById(Resource.Id.btnVerRuta);

            appBar = (Android.Support.Design.Widget.AppBarLayout)FindViewById(Resource.Id.appBar);
            appBar.AddOnOffsetChangedListener(this);
            toolBar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);

            mToolBar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbar);

            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
            #endregion

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

            #endregion;

            #region Datos

            progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
            p.AddRule(LayoutRules.CenterInParent);
            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
            FindViewById<RelativeLayout>(Resource.Id.principalDetalle).AddView(progressBar, p);
            progressBar.Visibility = Android.Views.ViewStates.Invisible;
            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

            if (ubicacion != null)
            {
                progressBar.Visibility = Android.Views.ViewStates.Visible;
                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                local = false;
                string server = "https://cabasus-mobile.azurewebsites.net/v1/activities/" + id_caballo;
                var uri = new Uri(string.Format(server, string.Empty));
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                var consulta = await Cliente.GetAsync(uri);
                consulta.EnsureSuccessStatusCode();
                if (consulta.IsSuccessStatusCode)
                {
                    JsonValue ConsultaJson = await consulta.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<ActivityDetalle>(ConsultaJson);

                    //segundos
                    string time = new ShareInside().StoH(data.duration);

                    distance_total = data.distance.slow + data.distance.normal + data.distance.strong;
                    if (distance_total == 0)
                    {
                        txtKm1.Text = "0 % ";
                        txtKm2.Text = "0 % ";
                        txtKm3.Text = "0 % ";
                    }
                    else
                    {
                        distance_slow = (data.distance.slow * 100) / distance_total;
                        distance_normal = (data.distance.normal * 100) / distance_total;
                        distance_strong = (data.distance.strong * 100) / distance_total;

                        txtKm1.Text = Math.Round(distance_slow, 1).ToString() + " % ";
                        txtKm2.Text = Math.Round(distance_normal, 1).ToString() + " % ";
                        txtKm3.Text = Math.Round(distance_strong, 1).ToString() + " % ";
                    }
                    txtTime.Text = time + " hrs";

                    if ((distance_total / 1000) < 1)
                        txtDistance.Text = Math.Round(distance_total, 2).ToString() + " mts";
                    else
                        txtDistance.Text = Math.Round((distance_total / 1000), 2).ToString() + " Km";


                    txtUsuarioDetalle.Text = data.owner.username;
                    toolBar.SetTitle(caballo);
                    
                    toolBar.SetCollapsedTitleTextColor(Color.White);
                    //toolBar.SetExpandedTitleTextColor(Android.Content.Res.ColorStateList.)
                    toolBar.SetCollapsedTitleTextAppearance(Resource.Style.CollapsedAppBar);
                    toolBar.SetExpandedTitleTextAppearance(Resource.Style.CollapsedAppBar);

                    if (data.horse_status.Equals("1"))
                        imgEmotionDetalle.Text = "VW";
                    else if (data.horse_status.Equals("2"))
                        imgEmotionDetalle.Text = "WT";
                    else if (data.horse_status.Equals("3"))
                        imgEmotionDetalle.Text = "N";
                    else if (data.horse_status.Equals("4"))
                        imgEmotionDetalle.Text = "UW";
                    else
                        imgEmotionDetalle.Text = "OW";

                    //imgEmotionDetalle.SetScaleType(ImageView.ScaleType.Center);
                    latlat = new List<List<double>>();
                    latlat.AddRange(data.location.coordinates);

                    var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), imgCaballo + ".png"));

                    Java.IO.File ImagenUri = new Java.IO.File(RutaImage.ToString());
                    Bitmap ImagenBitMap = BitmapFactory.DecodeFile(ImagenUri.AbsolutePath);
                    imgCaballoDetalle.SetImageBitmap(ImagenBitMap);

                    byte[] imageAsBytes = Base64.Decode(data.owner.photo.data, Base64Flags.Default);
                    var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                    imgUsuarioDetalle.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));

                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                }
                else
                {
                    Toast.MakeText(this, Resource.String.error_consulting, ToastLength.Short).Show();
                    StartActivity(typeof(ActivityActivity));
                    Finish();
                }
            }
            else
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                var data = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where ID_ActividadLocal = "+ id_caballo);

                foreach (var item in data)
                {
                    //segundos
                    string time = new ShareInside().StoH(item.Duration);

                    distance_total = item.Slow + item.Normal + item.Strong;

                    if (distance_total == 0)
                    {
                        txtKm1.Text = "0 % ";
                        txtKm2.Text = "0 % ";
                        txtKm3.Text = "0 % ";
                    }
                    else
                    {
                        distance_slow = (item.Slow * 100) / distance_total;
                        distance_normal = (item.Normal * 100) / distance_total;
                        distance_strong = (item.Strong * 100) / distance_total;

                        txtKm1.Text = Math.Round(distance_slow, 1).ToString() + " % ";
                        txtKm2.Text = Math.Round(distance_normal, 1).ToString() + " % ";
                        txtKm3.Text = Math.Round(distance_strong, 1).ToString() + " % ";
                    }

                    txtTime.Text = time + " hrs";

                    if ((distance_total / 1000) < 1)
                        txtDistance.Text = Math.Round(distance_total, 2).ToString() + " mts";
                    else
                        txtDistance.Text = Math.Round((distance_total / 1000), 2).ToString() + " Km";

                    txtUsuarioDetalle.Text = item.ID_Usuario;
                    toolBar.SetTitle(caballo);

                    toolBar.SetCollapsedTitleTextColor(Color.White);
                    //toolBar.SetExpandedTitleTextColor(Android.Content.Res.ColorStateList.)
                    toolBar.SetCollapsedTitleTextAppearance(Resource.Style.CollapsedAppBar);
                    toolBar.SetExpandedTitleTextAppearance(Resource.Style.CollapsedAppBar);

                    if (item.Horse_Status.Equals("1"))
                        imgEmotionDetalle.Text = "VW";
                    else if (item.Horse_Status.Equals("2"))
                        imgEmotionDetalle.Text = "WT";
                    else if (item.Horse_Status.Equals("3"))
                        imgEmotionDetalle.Text = "N";
                    else if (item.Horse_Status.Equals("4"))
                        imgEmotionDetalle.Text = "UW";
                    else
                        imgEmotionDetalle.Text = "OW";
                    
                    latitud = new List<string>();
                    longitud = new List<string>();


                    latitud.AddRange(item.Latitudes.Split('$'));
                    longitud.AddRange(item.Longitudes.Split('$'));
                    

                    var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), imgCaballo + ".png"));

                    Java.IO.File ImagenUri = new Java.IO.File(RutaImage.ToString());
                    Bitmap ImagenBitMap = BitmapFactory.DecodeFile(ImagenUri.AbsolutePath);
                    imgCaballoDetalle.SetImageBitmap(ImagenBitMap);

                    byte[] imageAsBytes = Base64.Decode((new ShareInside().ConsultarDatosUsuario()[0].photo.data), Base64Flags.Default);
                    var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                    imgUsuarioDetalle.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));
                }

                
            }

            #endregion
            Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
            alertar.RequestWindowFeature(1);
            alertar.SetCancelable(true);
            alertar.SetContentView(Resource.Layout.layout_CustomRuta);

            btnVerRuta.Click += delegate
            {
                btnVerRuta.Enabled = false;
                
                alertar.Show();
                setUpMap();
                btnVerRuta.Enabled = true;
            };

            animationProgress();
        }

        public void OnOffsetChanged(Android.Support.Design.Widget.AppBarLayout appBarLayout, int verticalOffset)
        {
            //appBar.Alpha = ((float)verticalOffset - 255) / 255;
            mToolBar.Alpha = (((float)verticalOffset + 255) / -255); // 0 invisible 
            
            
            imgUsuarioDetalle.Alpha = ((float)verticalOffset + 255) / 255; 
            txtUsuarioDetalle.Alpha = ((float)verticalOffset + 255) / 255; // 1 visible 
            
            
        }

        private void animationProgress()
        {
            ProgressBar progressSlow = (ProgressBar)FindViewById(Resource.Id.pgSlow);
            ObjectAnimator pAnimSlow = ObjectAnimator.OfInt(progressSlow, "progress", 0, (int)distance_slow);
            pAnimSlow.SetDuration(1500);
            pAnimSlow.SetInterpolator(new LinearInterpolator());

            ProgressBar progressNormal = (ProgressBar)FindViewById(Resource.Id.pgNormal);
            ObjectAnimator pAnimNormal = ObjectAnimator.OfInt(progressNormal, "progress", 0, (int)distance_normal);
            pAnimNormal.SetDuration(1500);
            pAnimNormal.SetInterpolator(new LinearInterpolator());

            ProgressBar progressFast = (ProgressBar)FindViewById(Resource.Id.pgFast);
            ObjectAnimator pAnimFast = ObjectAnimator.OfInt(progressFast, "progress", 0, (int)distance_strong);
            pAnimFast.SetDuration(1500);
            pAnimFast.SetInterpolator(new LinearInterpolator());

            pAnimSlow.Start();
            pAnimNormal.Start();
            pAnimFast.Start();
        }

        public override void OnBackPressed()
        {
        }
    }
}

public class DistanceDetalle
{
    public double slow { get; set; }
    public double normal { get; set; }
    public double strong { get; set; }
}

public class LocationDetalle
{
    public List<List<double>> coordinates { get; set; }
    public string type { get; set; }
}

public class Owner
{
    public string username { get; set; }
    public Photo photo { get; set; }
}

public class Photo
{
    public string data { get; set; }
    public string content_type { get; set; }
}

public class ActivityDetalle
{
    public DateTime updatedAt { get; set; }
    public DateTime createdAt { get; set; }
    public Owner owner { get; set; }
    public string horse { get; set; }
    public int duration { get; set; }
    public string horse_status { get; set; }
    public bool visible { get; set; }
    public DistanceDetalle distance { get; set; }
    public LocationDetalle location { get; set; }
    public DateTime date { get; set; }
    public string id { get; set; }
}