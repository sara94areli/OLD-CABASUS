using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Plugin.Connectivity;
using SQLite;

namespace Cabasus_Android
{
    [Service]
    class ServicioForegroudSincronizar : Service
    {
        public const int SERVICE_RUNNING_NOTIFICATION_ID = -12;
        bool YaSeActualizo = false;
        Notification notification;
        NotificationManager notificationManager;
        Notification.Builder builder;

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
            var channelId = "";
                        
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                channelId = NotificationChannel("CABASUS Service", "Cabasus Location");
                builder = new Notification.Builder(this, channelId)
            .SetContentTitle(GetText(Resource.String.Synchronizing_data))
            .SetContentText("")
            .SetProgress(0, 0, true).SetContentText(GetText(Resource.String.Uploading))
            .SetSmallIcon(Resource.Drawable.notificacion)
            .SetContentIntent(pendingIntent)
            .SetOngoing(true);
            }
            else
            {
                builder = new Notification.Builder(this)
              .SetContentTitle(GetText(Resource.String.Synchronizing_data))
              .SetContentText("")
              .SetProgress(0, 0, true).SetContentText(GetText(Resource.String.Uploading))
              .SetSmallIcon(Resource.Drawable.notificacion)
              .SetContentIntent(pendingIntent)
              .SetOngoing(true);
            }

            notification = builder.Build();

            notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);

            notificationManager.Notify(SERVICE_RUNNING_NOTIFICATION_ID, notification);

            SincronizacionAsync();

            return StartCommandResult.Sticky;
        }

        public async void SincronizacionAsync()
        {
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
            var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and visible = 'private' ;", (new DiarioLocal()).id_diary);
            var ConsultaJornadas = con.Query<DiarioLocal>("select * from diarys where fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and visible = 'public' ;", (new DiarioLocal()).id_diary);
            var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "';", (new RedordatoriosLocales()).id_reminder);
            var conA = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
            var ConsultaActividades = conA.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo", (new ActividadPorCaballo()).ID_Caballo);

            while (!YaSeActualizo)
            {
                var Mensaje = "";
                //int como = 0;
                //var progres = Java.Lang.Runtime.GetRuntime().Exec("ping -c 1 www.google.com");
                //como = await progres.WaitForAsync();

                ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
                NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
                bool isOnline = (activeConnection != null) && activeConnection.IsConnected;                   

                if (isOnline)
                {
                    try
                    {
                        //await Task.Delay(20000);
                        Mensaje = "Subiendo";
                        builder.SetContentText(Mensaje);
                        notification = builder.Build();
                        notificationManager.Notify(SERVICE_RUNNING_NOTIFICATION_ID, notification);
                        if (ConsultaDiarios.Count > 0)
                            await SincronizarDiarios(ConsultaDiarios, con);
                        if (ConsultaJornadas.Count > 0)
                            await SincronizarJornadasAsync(ConsultaJornadas, con);
                        if (ConsultaRecordatorios.Count > 0)
                            await SincronizarRecordatorioAsync(ConsultaRecordatorios, con);
                        if (ConsultaActividades.Count > 0)
                            await SincronizarActividadesAsync(ConsultaActividades, conA);
                        YaSeActualizo = true;
                    }
                    catch (Exception)
                    {
                        YaSeActualizo = false;
                        Mensaje = GetText(Resource.String.Connect_to_the_internet);
                    }
                }
                else
                {
                    YaSeActualizo = false;
                    Mensaje = GetText(Resource.String.Connect_to_the_internet);
                }
                builder.SetContentText(Mensaje);
                notification = builder.Build();
                notificationManager.Notify(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            }

            var ii = new Intent(this, typeof(ServicioForegroudSincronizar));
            StopService(ii);
        }

        private async Task SincronizarActividadesAsync(List<ActividadPorCaballo> consulta, SQLiteConnection con)
        {
            #region Sincronizar Actividades
            
            try
            {
                foreach (var actividad in consulta)
                {
                    var latitudes = actividad.Latitudes.Split('$');
                    var longitudes = actividad.Longitudes.Split('$');

                    List<double[]> JuntarLAyLO = new List<double[]>();

                    for (int i = 1; i < latitudes.Length; i += 2)
                    {
                        JuntarLAyLO.Add(new double[2] { double.Parse(longitudes[i]), double.Parse(latitudes[i]) });
                    }

                    var ActividadParaInsertar = new SubirActividadCluod()
                    {
                        horse = actividad.ID_Caballo,
                        duration = actividad.Duration,
                        distance = new Distance()
                        {
                            slow = actividad.Slow,
                            normal = actividad.Normal,
                            strong = actividad.Strong
                        },
                        horse_status = actividad.Horse_Status,
                        date = Convert.ToDateTime(actividad.Dates).ToString("yyyy-MM-dd") + "T00:00:00.000Z",
                        //date = Convert.ToDateTime(actividad.Dates).Kind.ToString("{0:O}"),
                        location = new LocationCloud()
                        {
                            coordinates = JuntarLAyLO
                        },
                        visible = true
                    };

                    var JsonPInsertar = JsonConvert.SerializeObject(ActividadParaInsertar, Formatting.Indented, new JsonSerializerSettings());

                    HttpClient ClienteActividad = new HttpClient();

                    if (CrossConnectivity.Current.IsConnected)
                    {
                        ClienteActividad.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                                                
                        string ServerActividad = "https://cabasus-mobile.azurewebsites.net/v1/activities";
                        string Formato = "application/json";

                        var ClientePostActividad = await ClienteActividad.PostAsync(ServerActividad, new StringContent(JsonPInsertar, Encoding.UTF8, Formato));
                        
                        ClientePostActividad.EnsureSuccessStatusCode();

                        if (ClientePostActividad.IsSuccessStatusCode)
                        {
                            con.Query<ActividadPorCaballo>("Delete from ActividadPorCaballo WHERE ID_ActividadLocal=" + actividad.ID_ActividadLocal, (new ActividadPorCaballo()).ID_Caballo);
                        }
                        else
                        {
                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
                    }
                }

                var conCaballoDescargado = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                //conCaballoDescargado.Query<Horses>("UPDATE Horses SET Sync= 0 WHERE id='" + id + "'", new Horses().id);
            }
            catch
            {
                //var conCaballoDescargado = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                //conCaballoDescargado.Query<Horses>("UPDATE Horses SET Sync= 1 WHERE id='" + id + "'", new Horses().id);
            }
            #endregion
        }

        private async Task SincronizarRecordatorioAsync(List<RedordatoriosLocales> ConsultaRecordatorios, SQLiteConnection con)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                if (ConsultaRecordatorios.Count > 0)
                {
                    foreach (var item in ConsultaRecordatorios)
                    {
                        #region modelo 
                        string FechaInicio = DateTime.ParseExact(item.inicio.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        string TiempoInicio = item.inicio.Substring(11, 8);
                        string Inicio = FechaInicio + "T" + TiempoInicio + ".456Z";

                        string FechaFin = DateTime.ParseExact(item.fin.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        string TiempoFin = item.fin.Substring(11, 8);
                        string Fin = FechaFin + "T" + TiempoFin + ".456Z";

                        AgregarRecordatoriosNube RecordatorioNube = new AgregarRecordatoriosNube()
                        {
                            horse = item.fk_caballo,
                            description = item.descripcion,
                            date = Inicio,
                            end_date = Fin,
                            type = int.Parse(item.Tipo),
                            alert_before = int.Parse(item.notificacion)
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
                                var EliminarRecordatorios = con.Query<RedordatoriosLocales>("delete from reminders where id_reminder =" + item.id_reminder + ";", (new RedordatoriosLocales()).id_reminder);
                                Toast.MakeText(this, Resource.String.Complete_synchronization, ToastLength.Short).Show();
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                        }
                    }
                }
                else
                    Toast.MakeText(this, Resource.String.No_reminders_to_synchronize, ToastLength.Short).Show();
            }
            else
                Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
        }

        private async Task SincronizarJornadasAsync(List<DiarioLocal> ConsultaDiarios, SQLiteConnection con)
        {
            #region Sincronizar Jornadas

            if (CrossConnectivity.Current.IsConnected)
            {
                if (ConsultaDiarios.Count > 0)
                {
                    List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                    foreach (var itemDiarios in ConsultaDiarios)
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
                                string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(itemDiarios.foto1);
                                await GuardarFotoNube(AddImage, CadenaFoto, itemDiarios.fk_caballo);
                            }
                            else if (i == 1)
                            {
                                string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(itemDiarios.foto2);
                                await GuardarFotoNube(AddImage, CadenaFoto, itemDiarios.fk_caballo);
                            }
                            else
                            {
                                string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(itemDiarios.foto3);
                                await GuardarFotoNube(AddImage, CadenaFoto, itemDiarios.fk_caballo);
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
                            catch (Exception ex)
                            {
                                Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
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
                        if (itemDiarios.estado_caballo == "1")
                            Estado = "happy";
                        else if (itemDiarios.estado_caballo == "2")
                            Estado = "normal";
                        else
                            Estado = "sad";
                        #endregion
                        #region Asignar datos de jornada
                        AgregarDiariosNube AddDiario = new AgregarDiariosNube()
                        {
                            horse = itemDiarios.fk_caballo,
                            content = itemDiarios.descripcion,
                            horse_status = Estado,
                            date = Convert.ToDateTime(itemDiarios.dates).ToString("yyyy-MM-dd") + "T20:51:10.467Z",
                            images = Fotos,
                            visibility = "public"
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
                                var EliminarDiario = con.Query<DiarioLocal>("delete from diarys where id_diary =" + itemDiarios.id_diary + " and visible = 'public' ;", (new DiarioLocal()).id_diary);
                                Toast.MakeText(this, Resource.String.Complete_synchronization, ToastLength.Short).Show();
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Long).Show();
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, Resource.String.No_journals_to_synchronize, ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
            }
            #endregion
        }

        private async Task SincronizarDiarios(List<DiarioLocal> ConsultaDiarios, SQLiteConnection con)
        {
            #region Sincronizar Diarios
            
            if (CrossConnectivity.Current.IsConnected)
            {
                if (ConsultaDiarios.Count > 0)
                {
                    List<DiarioLocal> ListaDiario = new List<DiarioLocal>();
                    foreach (var item in ConsultaDiarios)
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
                                string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(item.foto1);
                                await GuardarFotoNube(AddImage, CadenaFoto, item.fk_caballo);
                            }
                            else if (i == 1)
                            {
                                string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(item.foto2);
                                await GuardarFotoNube(AddImage, CadenaFoto, item.fk_caballo);
                            }
                            else
                            {
                                string CadenaFoto = await new ShareInside().CadenaFotoNubeAsync(item.foto3);
                                await GuardarFotoNube(AddImage, CadenaFoto, item.fk_caballo);
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
                            catch (Exception ex)
                            {
                                Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
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
                        if (item.estado_caballo == "1")
                            Estado = "happy";
                        else if (item.estado_caballo == "2")
                            Estado = "normal";
                        else
                            Estado = "sad";
                        #endregion
                        #region Asignar datos de jornada
                        AgregarDiariosNube AddDiario = new AgregarDiariosNube()
                        {
                            horse = item.fk_caballo,
                            content = item.descripcion,
                            horse_status = Estado,
                            date = Convert.ToDateTime(item.dates).ToString("yyyy-MM-dd") + "T20:51:10.467Z",
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
                                var EliminarDiario = con.Query<DiarioLocal>("delete from diarys where id_diary =" + item.id_diary + " and visible = 'private' ;", (new DiarioLocal()).id_diary);
                                Toast.MakeText(this, Resource.String.Complete_synchronization, ToastLength.Short).Show();
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(this, Resource.String.The_information, ToastLength.Long).Show();
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, Resource.String.No_diary_to_synchronize, ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this, Resource.String.You_need_internet, ToastLength.Short).Show();
            }
            #endregion
        }

        public async Task Sincronizar()
        {
            int como = 0;
            var progres = Java.Lang.Runtime.GetRuntime().Exec("ping -c 1 www.google.com");
            como = await progres.WaitForAsync();
            int contador = 0;

            while (como == 1 && contador != 10)
            {
                como = await progres.WaitForAsync();
                contador++;
            }
            if(contador != 10)
                int.Parse("l");            
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
            return base.StopService(name);
        }

        public override void OnDestroy()
        {           
            StopForeground(true);
            base.OnDestroy();
        }
    }
}