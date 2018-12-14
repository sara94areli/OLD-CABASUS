using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net.Http;
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
using Plugin.Connectivity;
using idioma = Java.Util;

namespace Cabasus_Android.Screens
{
    [Activity(Label = "ActivityRecovery", NoHistory = true)]
    public class ActivityRecovery : Activity
    {
        EditText email;
        ProgressBar progressBar;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var Idioma = new ShareInside().ConsultarIdioma();
            Java.Util.Locale.Default = new idioma.Locale(Idioma.Idioma, Idioma.Pais);
            Resources.Configuration.Locale = Java.Util.Locale.Default;
            Resources.UpdateConfiguration(Resources.Configuration, Resources.DisplayMetrics);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_Recovery);
            Window.SetStatusBarColor(Color.Black);
            // new ShareInside().GuardarPantallaActual("ActivityRecovery");

            #region Referencia de botones de xml
            var btnCancel = FindViewById<Button>(Resource.Id.RecoveryCancel);
            var btnEnviar = FindViewById<Button>(Resource.Id.btnRecover);
            email = FindViewById<EditText>(Resource.Id.txtRecupererEmail);
            #endregion

            #region botones
            btnCancel.Click += delegate {
                StartActivity(typeof(ActivityLogin));
                Finish();
            };

            btnEnviar.Click += delegate {
                RecuperarContra();
            };
            #endregion
        }

        public override void OnBackPressed()
        {
            StartActivity(typeof(ActivityLogin));
            Finish();
        }

        private async void RecuperarContra()
        {
            if (!string.IsNullOrWhiteSpace(email.Text))
            {
                try
                {
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        string server = "https://cabasus-mobile.azurewebsites.net/v1/auth/recover_password";
                        string json = "application/json";
                        JsonObject jsonObject = new JsonObject();
                        jsonObject.Add("email", email.Text);
                        HttpClient cliente = new HttpClient();

                        #region CargarLoading
                        progressBar = new ProgressBar(this, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                        FindViewById<RelativeLayout>(Resource.Id.PrincipalRecovery).AddView(progressBar, p);
                        progressBar.Visibility = Android.Views.ViewStates.Invisible;
                        Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        #endregion

                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        var respuesta = await cliente.PostAsync(server, new StringContent(jsonObject.ToString(), Encoding.UTF8, json));
                        respuesta.EnsureSuccessStatusCode();
                        if (respuesta.IsSuccessStatusCode)
                        {
                            var content = await respuesta.Content.ReadAsStringAsync();
                            Toast.MakeText(this, Resource.String.Check_your_email, ToastLength.Long).Show();
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        }
                        else
                        {
                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
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
                    Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
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
}