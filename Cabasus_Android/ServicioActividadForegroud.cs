using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Widget;
using Plugin.Geolocator.Abstractions;
using SQLite;
using static Android.Gms.Common.Apis.GoogleApiClient;
using Android.Gms.Common;
using Android.Locations;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using System;

namespace Cabasus_Android
{
    [Service]
    class ServicioActividadForegroud : Service, IConnectionCallbacks, IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        public const int SERVICE_RUNNING_NOTIFICATION_ID = -123;
        Timer t;
        int tiempo = 0;
        string timernotification = "";

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var pantalla = new Intent(this, typeof(Screens.ActivityRun));
            pantalla.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 1, pantalla, PendingIntentFlags.OneShot);
            Notification.Builder builder;
            var channelId = "";
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                channelId = NotificationChannel("CABASUS Service", "Cabasus Location");
                builder = new Notification.Builder(this, channelId)
            .SetContentTitle("Actividad")
            .SetContentText(timernotification)
            .SetSmallIcon(Resource.Drawable.notificacion)
            .SetContentIntent(pendingIntent)
            .SetOngoing(true);
            }
            else
            {
              builder = new Notification.Builder(this)
            .SetContentTitle("Actividad")
            .SetContentText(timernotification)
            .SetSmallIcon(Resource.Drawable.notificacion)
            .SetContentIntent(pendingIntent)
            .SetOngoing(true);
            }

            
            //.AddAction(DetenerActividad())
            //.AddAction(AbrirApp())

            Notification notification = builder.Build();

            NotificationManager notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            notificationManager.Notify(SERVICE_RUNNING_NOTIFICATION_ID, notification);

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
                && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                StopService(new Intent(this, typeof(ServicioActividadForegroud)));
            }
            else
            {
                if (CheckPlayServices())
                {
                    BuildGoogleApiClient();
                    CreateLocationRequest();
                }
            }

            mRequestingLocationUpdates = true;
            Timer i = new Timer();
            i.Interval = 1000;
            i.Enabled = true;
            var contador = 0;
            i.Elapsed += (s, e) =>
            {
                contador++;
                if (contador >= 5)
                {
                    StartLocationUpdates();
                    i.Stop();
                }
            };
            i.Start();

            double seg = 0, min = 0, horas = 0;
            string sseg = "", smin = ":00", shoras = "00";

            t = new Timer();
            t.Interval = 1000;
            t.Enabled = true;
            t.Elapsed += (s, e) =>
            {
                    tiempo++;
                    seg++;
                    if (seg < 10)
                        sseg = ":0" + seg;
                    else if (seg < 59)
                        sseg = ":" + seg;
                    else if (seg >= 59)
                    {
                        seg = 0;
                        sseg = ":00";
                        min++;

                        if (min < 10)
                            smin = ":0" + min;
                        else if (min < 59)
                            smin = ":" + min;
                        else if (min >= 59)
                        {
                            min = 0;
                            smin = ":00";
                            horas++;

                            if (horas < 10)
                                shoras = "0" + horas;
                            else
                                shoras = horas.ToString();
                        }

                    }
                timernotification = shoras + smin + sseg;
                pantalla = new Intent(this, typeof(Screens.ActivityRun));
                pantalla.AddFlags(ActivityFlags.ClearTop);
                pendingIntent = PendingIntent.GetActivity(this, 1, pantalla, PendingIntentFlags.OneShot);

                Intent myintent = new Intent("IntentActividad");
                myintent.PutExtra("Tiempo", timernotification);
                myintent.PutExtra("RT", tiempo);
                SendBroadcast(myintent);

                builder.SetContentText(timernotification);
                builder.SetContentIntent(pendingIntent);
                notification = builder.Build();
                notificationManager.Notify(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            };
            t.Start();

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            return StartCommandResult.Sticky;
        }

        public string NotificationChannel(string ID, string Name)
        {
            var channelId = ID;
            var channelName = Name;
            var chan = new NotificationChannel(channelId, channelName, NotificationImportance.None);
            chan.LightColor = Color.Blue;
            chan.LockscreenVisibility = NotificationVisibility.Public;
            var service = GetSystemService(Context.NotificationService) as NotificationManager;
            service.CreateNotificationChannel(chan);
            return channelId;
        }

        public override bool StopService(Intent name)
        {
            new ShareInside().GuardarActividadEnProgreso("0", "0");
            mRequestingLocationUpdates = false;
            StopLocationUpdates();
            return base.StopService(name);
        }

        public override void OnDestroy()
        {
            t.Stop();
            tiempo = 0;
            new ShareInside().GuardarActividadEnProgreso("0", "0");
            mRequestingLocationUpdates = false;
            StopLocationUpdates();
            StopForeground(true);
            base.OnDestroy();
        }

        public void GuardarDatos(double lat, double lon, int veloci)
        {
            SQLiteConnection c = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadesLocal.sqlite"));
            c.Insert(new Actividad
            {
                Latitud = lat,
                Longitud = lon,
                Velocidad = veloci
            });
        }

        #region Metodos propios de GoogleAPI;

        private const int MY_PERMISSION_REQUEST_CODE = 7171;
        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 7172;
        private bool mRequestingLocationUpdates = false;
        private LocationRequest mLocationRequest;
        private GoogleApiClient mGoogleApiClient;
        private Location mLastLocation;

        private static int UPDATE_INTERVAL = 1000; // SEC
        private static int FATEST_INTERVAL = 1000; // SEC
        private static int DISPLACEMENT = 1; // METERS

        private void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FATEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
        }

        private void BuildGoogleApiClient()
        {
            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.API).Build();
            mGoogleApiClient.Connect();
        }

        private bool CheckPlayServices()
        {
            int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GooglePlayServicesUtil.IsUserRecoverableError(resultCode))
                {
                    //GooglePlayServicesUtil.GetErrorDialog(resultCode, this, PLAY_SERVICES_RESOLUTION_REQUEST).Show();
                }
                else
                {
                    Toast.MakeText(ApplicationContext, Resource.String.This_device_is_not_support, ToastLength.Long).Show();
                    //Finish();
                }
                return false;
            }
            return true;
        }

        private void DisplayLocation()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
              && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            mLastLocation = LocationServices.FusedLocationApi.GetLastLocation(mGoogleApiClient);
            if (mLastLocation != null)
            {                               
                double KH = ((mLastLocation.Speed / 1000) * 3600);
                var velocidad = Math.Round(KH, 2) + " km/h";
                var intencidad = "";
                int vel = 0;
                if (KH <= 0)
                {
                    vel = 1;
                    intencidad = GetString(Resource.String.Easy);
                }
                if (KH >= 0.3 && KH <= 11.8)
                {
                    vel = 1;
                    intencidad = GetString(Resource.String.Easy);
                }
                else if (KH > 11.8 && KH <= 20.8)
                {
                    vel = 2;
                    intencidad = GetString(Resource.String.Medium);
                }
                else if (KH > 20.8)
                {
                    vel = 3;
                    intencidad = GetString(Resource.String.Hard);
                }
                //vel = 2;
                //velocidad = 3+" km/h";

                Intent myintent = new Intent("IntentActividadMapa");
                myintent.PutExtra("Velocidad", velocidad);
                myintent.PutExtra("intencidad", intencidad);
                myintent.PutExtra("Latitud", mLastLocation.Latitude);
                myintent.PutExtra("Longitud", mLastLocation.Longitude);
                SendBroadcast(myintent);
                if (vel > 0)
                    GuardarDatos(mLastLocation.Latitude, mLastLocation.Longitude, vel);
            }
        }

        private void StartLocationUpdates()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
              && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            LocationServices.FusedLocationApi.RequestLocationUpdates(mGoogleApiClient, mLocationRequest, this);
        }

        private void StopLocationUpdates()
        {
            LocationServices.FusedLocationApi.RemoveLocationUpdates(mGoogleApiClient, this);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {

        }

        public void OnConnected(Bundle connectionHint)
        {
            DisplayLocation();
            if (mRequestingLocationUpdates)
                StartLocationUpdates();
        }

        public void OnConnectionSuspended(int cause)
        {
            mGoogleApiClient.Connect();
        }

        public void OnLocationChanged(Location location)
        {
            mLastLocation = location;
            DisplayLocation();
        }

        #endregion;

    }
}