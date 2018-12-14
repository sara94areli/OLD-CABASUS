using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Plugin.Connectivity;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityShares", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    
    public class ActivityShares : Activity
    {
        public int refresh;
        ImageView horsefoto;
        private ViewGroup FondoParaBotonFlotante;
        private int MX, MY;
        private RelativeLayout btnaddrider;
        private float ScreenWith = 0;
        private float ScrenHeight = 0;
        public ListView ListaCaballos1;
        int contador = 0;
        ListView textListView;
        EditText buscarrider;
        ProgressBar progressBar;
        TextView nombrehorse;
        ImageView fotohorse;
        ShareInside s = new ShareInside();
        string idhorses,foto;
        ListView listasegura;
        SwipeRefreshLayout refreshing;

        public override void OnBackPressed()
        {
            // base.OnBackPressed();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OverridePendingTransition(Resource.Animation.anim1, Resource.Animation.anim1);
            SetContentView(Resource.Layout.layout_share);
            Window.SetStatusBarColor(Color.Black);
            new ShareInside().GuardarPantallaActual("ActivityShares");
            #region Menu botones
            var btnhome = FindViewById<LinearLayout>(Resource.Id.btn_home);
            var btnactivity = FindViewById<LinearLayout>(Resource.Id.btn_activity);
            var btndiary = FindViewById<LinearLayout>(Resource.Id.btn_diary);
            var btncalendar = FindViewById<LinearLayout>(Resource.Id.btn_calendar);
            var btn_settings = FindViewById<LinearLayout>(Resource.Id.btn_settings);
             refreshing = FindViewById<SwipeRefreshLayout>(Resource.Id.swiperrefresh);

            refreshing.SetColorSchemeColors(Color.Rgb(197, 152, 5));
            refreshing.SetProgressBackgroundColorSchemeColor(Color.Rgb(255, 255, 255));
            refreshing.Refresh += async delegate {
                if (CrossConnectivity.Current.IsConnected)
                {
                    LLenarRiders();
                }
                else {
                    Toast.MakeText(this,Resource.String.You_need_internet, ToastLength.Short).Show();
                }
                refreshing.Refreshing = false;
            };

            btnhome.Click += delegate
            {
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

            #endregion
            
            horsefoto = FindViewById<ImageView>(Resource.Id.fotohorse);
            horsefoto.SetBackgroundResource(Resource.Drawable.cornerImageButton);

            FondoParaBotonFlotante = FindViewById<RelativeLayout>(Resource.Id.FondoBoton);
            btnaddrider = FondoParaBotonFlotante.FindViewById<RelativeLayout>(Resource.Id.btnaddshare);

            nombrehorse = FindViewById<TextView>(Resource.Id.txtnombrehorse);
            fotohorse = FindViewById<ImageView>(Resource.Id.fotohorse);

            #region BotonFlotante;
            
            var metrics = Resources.DisplayMetrics;
            ScreenWith = metrics.WidthPixels;
            ScrenHeight = metrics.HeightPixels;
            btnaddrider.SetX(ScreenWith - ConvertDPToPixels(70));
            btnaddrider.SetY(ScrenHeight - ConvertDPToPixels(150));

            #endregion;
            
            LLenarRiders();
          
            FindViewById<LinearLayout>(Resource.Id.btnbackhorse).Click +=async  delegate {
               StartActivity(typeof(ActivitySettings));
                Finish();
            };

            listasegura=FindViewById<ListView>(Resource.Id.ListaShare);
            listasegura.ScrollStateChanged += delegate {

                if (listasegura.LastVisiblePosition == listasegura.Adapter.Count - 1 && listasegura.GetChildAt(listasegura.ChildCount - 1).Bottom <= listasegura.Height)
                {
                    #region Mover el boton izquierda;

                    ObjectAnimator animX = ObjectAnimator.OfFloat(btnaddrider, "X", new ShareInside().ConvertDPToPixels(10, Resources.DisplayMetrics.Density));
                    ObjectAnimator animY = ObjectAnimator.OfFloat(btnaddrider, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                    ObjectAnimator animSX = ObjectAnimator.OfFloat(btnaddrider, "scaleX", 1f);
                    ObjectAnimator animSY = ObjectAnimator.OfFloat(btnaddrider, "scaleY", 1f);

                    animX.SetDuration(500);
                    animY.SetDuration(500);
                    animSX.SetDuration(500);
                    animSY.SetDuration(500);

                    AnimatorSet animator = new AnimatorSet();
                    animator.Play(animX).With(animY).With(animSX).With(animSY);
                    animator.Start();
                    #endregion
                }
                else
                {
                    #region Mover el boton derecha;
                    ObjectAnimator animX = ObjectAnimator.OfFloat(btnaddrider, "X", ScreenWith - new ShareInside().ConvertDPToPixels(70, Resources.DisplayMetrics.Density));
                    ObjectAnimator animY = ObjectAnimator.OfFloat(btnaddrider, "Y", ScrenHeight - new ShareInside().ConvertDPToPixels(160, Resources.DisplayMetrics.Density));
                    ObjectAnimator animSX = ObjectAnimator.OfFloat(btnaddrider, "scaleX", 1f);
                    ObjectAnimator animSY = ObjectAnimator.OfFloat(btnaddrider, "scaleY", 1f);

                    animX.SetDuration(500);
                    animY.SetDuration(500);
                    animSX.SetDuration(500);
                    animSY.SetDuration(500);

                    AnimatorSet animator = new AnimatorSet();
                    animator.Play(animX).With(animY).With(animSX).With(animSY);
                    animator.Start();
                    #endregion;
                }
                
            };

            btnaddrider.Click += delegate {
                try
                {
                    Dialog alertar = new Dialog(this, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.DialogoRider);
                    
                    buscarrider = alertar.FindViewById<EditText>(Resource.Id.buscarrider);

                    progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                    RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                    p.AddRule(LayoutRules.CenterInParent);
                    progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                    alertar.FindViewById<RelativeLayout>(Resource.Id.dialogorider).AddView(progressBar, p);

                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                    Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                    buscarrider.TextChanged += async (object sender, TextChangedEventArgs g) =>
                    {
                        if (buscarrider.Text.Length > 3)
                        {
                            string server = "https://cabasus-mobile.azurewebsites.net/v1/users/search";
                            string json = "application/json";
                            JsonObject jsonObject2 = new JsonObject();
                            jsonObject2.Add("username",  buscarrider.Text);

                            HttpClient cliente = new HttpClient();
                            if (CrossConnectivity.Current.IsConnected)
                            {
                                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(s.ConsultToken());

                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                var clientePost2 = await cliente.PostAsync(server, new StringContent(jsonObject2.ToString(), Encoding.UTF8, json));


                                clientePost2.EnsureSuccessStatusCode();
                                if (clientePost2.IsSuccessStatusCode)
                                {
                                    JsonValue ObjJson = await clientePost2.Content.ReadAsStringAsync();
                                    try
                                    {
                                        var data = JsonConvert.DeserializeObject<List<UserPhone>>(ObjJson);
                                        textListView = alertar.FindViewById<ListView>(Resource.Id.Listarider);
                                        textListView.Adapter = new AdaptadorBuscarRiders(this, data, idhorses, nombrehorse.Text, alertar);
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                    }
                                    catch (Exception ex)
                                    {
                                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                    }
                                }

                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            await
                            Task.Delay(1500);
                            try
                            {
                                List<UserPhone> data = new List<UserPhone>();
                                textListView.Adapter = new AdaptadorBuscarRiders(this, data, idhorses, nombrehorse.Text, alertar);
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            }
                            catch (Exception)
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            }

                        }
                    };

                    alertar.SetOnDismissListener(new OnDismissListener(() =>
                    {
                        LLenarRiders();
                    }));

                    alertar.Show();


                }
                catch (Exception ex)
                {

                    Toast.MakeText(this,Resource.String.The_information, ToastLength.Short).Show();
                }
            };
        }
        
        public async void LLenarRiders()
        {
            try
            {
                ListView  ListaCaballos = FindViewById<ListView>(Resource.Id.ListaShare);

                idhorses = this.Intent.GetStringExtra("idhorse");
                nombrehorse.Text = this.Intent.GetStringExtra("nombre");
              //  compartir = Intent.GetStringArrayListExtra("share");
                foto = Intent.GetStringExtra("foto");
                byte[] imageAsBytes = Base64.Decode(foto, Base64Flags.Default);
                var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                fotohorse.SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));

                #region Llenar lista Riders

                string serverConsulHorse = "https://cabasus-mobile.azurewebsites.net/v1/horses/"+idhorses+"/shares";
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                refreshing.Refreshing = true;

                var SaveConsultHorse = await Cliente.GetAsync(serverConsulHorse);
                refreshing.Refreshing = false;

                SaveConsultHorse.EnsureSuccessStatusCode();
                if (SaveConsultHorse.IsSuccessStatusCode)
                {
                    JsonValue ConsultaJson = await SaveConsultHorse.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<UserPhone>>(ConsultaJson);
                    if (data.Count == 0)
                    {
                        data.Add(new UserPhone()
                        {
                            id = "123456789"
                        });
                    }
                    ListaCaballos.Adapter = new AdaptadorRiders(this, data,idhorses, nombrehorse.Text);
                }
                else
                {
                    Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                }
                #endregion
            }
            catch (Exception ex )
            {
                Toast.MakeText(this,Resource.String.The_information, ToastLength.Short).Show();
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

       
        private sealed class OnDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
        {
            private readonly Action action;

            public OnDismissListener(Action action)
            {
                this.action = action;
            }

            public void OnDismiss(IDialogInterface dialog)
            {
                this.action();
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
    }
   
}