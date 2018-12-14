using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Util;
using Firebase.Messaging;
using Newtonsoft.Json;
using Cabasus_Android.Screens;
using SQLite;

namespace Cabasus_Android
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";
        ActivitySettings se = new Screens.ActivitySettings();
        ActivityHome ho = new ActivityHome();
        public override async void OnMessageReceived(RemoteMessage message)
        {
            Log.Debug(TAG, "From: " + message.From);
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
                Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
                SendNotification(message.GetNotification().Body);
            }
            else
            {
                //Only used for debugging payloads sent from the Azure portal
                Dictionary<string, string> DataNotificaciones = new Dictionary<string, string>();
                foreach (var item in message.Data)
                {
                    DataNotificaciones.Add(item.Key, item.Value);
                }
                string json = JsonConvert.SerializeObject(DataNotificaciones, Formatting.Indented);
                //SendNotification(message.Data.Values.First());
                SendNotification(json);
                //await new ShareInside().GuardadoFotosYConsulta();
                //if (DataNotificaciones["message"] == "horse_sharing_accepted" || DataNotificaciones["message"] == "horse_sharing_invited" || DataNotificaciones["message"] == "horse_sharing_deleted" || DataNotificaciones["message"] == "horse_deleted")
                //{
                //    if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings")
                //    {
                //        StartActivity(typeof(Screens.ActivitySettings));
                //    }
                //}
                //else if (new ShareInside().ConsultarPantallaActual() == "ActivityNotificaciones")
                //{
                //    StartActivity(typeof(Screens.ActivityNotificaciones));
                //}
            }
        }

        async void SendNotification(string messageBody)
        {
            await new ShareInside().GuardadoFotosYConsulta();
            var DatosNotificaciones = JsonConvert.DeserializeObject<NotificationsData>(messageBody);
            var HorseData = DatosNotificaciones.horse.Substring(1, DatosNotificaciones.horse.Length - 2);
            var HorseDataSeparado = HorseData.Split(',');
            var name = HorseDataSeparado[0].Split(':');
            var id= HorseDataSeparado[1].Split(':');
            string idcaballo = id[1].Substring(1, id[1].Length - 2);
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Notification.sqlite"));
            if (DatosNotificaciones.message == "horse_sharing_request")
            {
                //Cuando un corredor solicita al dueño que le compartan un caballo
                var intent = new Intent(this, typeof(Screens.ActivityNotificaciones));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.A_user_wants_you_to_share)+" " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Horse_shared))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse Sharing Request")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());
                new ShareInside().GuardarEstadoNotificacion(1);
                if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings" || new ShareInside().ConsultarPantallaActual() == "ActivityHome" || new ShareInside().ConsultarPantallaActual() == "ActivityActivity" || new ShareInside().ConsultarPantallaActual() == "ActivityDiary" || new ShareInside().ConsultarPantallaActual() == "ActivityCalendar")
                {
                    Intent i = new Intent();
                    i.SetAction("Campana");
                    this.SendBroadcast(i);
                }

            }
            else if (DatosNotificaciones.message == "horse_sharing_accepted")
            {
                //Cuando el dueño acepta compartir al corredor un caballo solicitado
                var intent = new Intent(this, typeof(Screens.ActivitySettings));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 1, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.Accepted_to_share_the_horse) + " " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Invitation_accepted))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse Sharing Acepted")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());
                this.SendBroadcast(intent);
                if ( new ShareInside().ConsultarPantallaActual() == "ActivityHome" || new ShareInside().ConsultarPantallaActual() == "ActivityActivity" || new ShareInside().ConsultarPantallaActual() == "ActivityDiary")
                {
                    Intent i = new Intent();
                    i.SetAction("Campana");
                    this.SendBroadcast(i);
                }
                if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings")
                {
                    StartActivity(typeof(ActivitySettings));
                }
            }
            else if (DatosNotificaciones.message == "horse_sharing_rejected")
            {
                //El dueño del caballo rechazo compartir el caballo con el corredor
                var intent = new Intent(this, typeof(Screens.ActivityNotificaciones));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 2, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.Rejected_to_share_the_horse) + " " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Invitation_rejected))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse Sharing Rejected")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());
            }
            else if (DatosNotificaciones.message == "horse_sharing_deleted")
            {
                //Cuando el dueño te elimina desde manage riders
                var intent = new Intent(this, typeof(Screens.ActivitySettings));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 2, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.The_owner_stopped_sharing_the_horse) + " " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Unshare_horse))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse Sharing deleted")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());

                #region Eliminar de base de datos local
                try
                {
                    var idhorses = id[1].Substring(1,id[1].Length-2);
                    var con1 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    con1.Query<Horses>("delete from Horses where id='" + idhorses + "'");
                    con1.Query<Horses>("delete  from DiariosNube where fk_usuario='" + idhorses + "'");
                    con1.Query<Horses>("delete  from diarys where fk_caballo='" + idhorses + "'");
                    con1.Query<Horses>("delete  from reminders where fk_caballo='" + idhorses + "'");

                    var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                    con2.Query<Horses>("delete  from ActividadesCloudMes where ID_Caballo='" + idhorses + "'");
                    con2.Query<Horses>("delete  from ActividadesCloud where ID_Caballo='" + idhorses + "'");
                    con2.Query<Horses>("delete  from ActividadPorCaballo where ID_Caballo='" + idhorses + "'");
                }
                catch (Exception) { }
                #endregion

                if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings")
                {
                    StartActivity(typeof(Screens.ActivitySettings));
                }
                else if (new ShareInside().ConsultarPantallaActual() == "ActivityHome" || new ShareInside().ConsultarPantallaActual() == "ActivityActivity" || new ShareInside().ConsultarPantallaActual() == "ActivityDiary" || new ShareInside().ConsultarPantallaActual() == "ActivityCalendar")
                {
                    Intent i = new Intent();
                    i.SetAction("Campana");
                    this.SendBroadcast(i);
                }
            }
            else if (DatosNotificaciones.message == "horse_deleted")
            {
                //El dueño elimino el caballo que el corredor tiene compartido
                var intent = new Intent(this, typeof(Screens.ActivitySettings));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 3, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.The_owner_eliminated_the_horse) + " " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Horse_Eliminated))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse deleted")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());
                #region Eliminar de base de datos local
                try
                {
                     var idhorses = id[1].Substring(1, id[1].Length - 2);
                    var con1 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    con1.Query<Horses>("delete from Horses where id='" + idhorses + "'");
                    con1.Query<Horses>("delete  from DiariosNube where fk_usuario='" + idhorses + "'");
                    con1.Query<Horses>("delete  from diarys where fk_caballo='" + idhorses + "'");
                    con1.Query<Horses>("delete  from reminders where fk_caballo='" + idhorses + "'");

                    var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                    con2.Query<Horses>("delete  from ActividadesCloudMes where ID_Caballo='" + idhorses + "'");
                    con2.Query<Horses>("delete  from ActividadesCloud where ID_Caballo='" + idhorses + "'");
                    con2.Query<Horses>("delete  from ActividadPorCaballo where ID_Caballo='" + idhorses + "'");
                }
                catch (Exception) { }
                #endregion
                if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings")
                {
                    StartActivity(typeof(Screens.ActivitySettings));
                }
                else if (new ShareInside().ConsultarPantallaActual() == "ActivityHome" || new ShareInside().ConsultarPantallaActual() == "ActivityActivity" || new ShareInside().ConsultarPantallaActual() == "ActivityDiary" || new ShareInside().ConsultarPantallaActual() == "ActivityCalendar")
                {
                    Intent i = new Intent();
                    i.SetAction("Campana");
                    this.SendBroadcast(i);
                }
            }
            else if (DatosNotificaciones.message == "horse_sharing_invited")
            {
                //El dueño comparte el caballo desde Manage Rider
                var intent = new Intent(this, typeof(Screens.ActivitySettings));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.A_user_has_shared_the_horse) + " " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Horse_shared))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse Sharing Request")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());
                if (new ShareInside().ConsultarPantallaActual() == "ActivityHome" || new ShareInside().ConsultarPantallaActual() == "ActivityActivity" || new ShareInside().ConsultarPantallaActual() == "ActivityDiary" || new ShareInside().ConsultarPantallaActual() == "ActivityCalendar")
                {
                    Intent i = new Intent();
                    i.SetAction("Campana");
                    this.SendBroadcast(i);
                }
                else if (new ShareInside().ConsultarPantallaActual() == "ActivitySettings")
                {
                    StartActivity(typeof(ActivitySettings));
                }
            }
            else if (DatosNotificaciones.message == "horse_sharing_unshared")
            {
                //Cuando el dueño le compartio un caballo al corredor pero este lo elimino
                var intent = new Intent(this, typeof(Screens.ActivitySettings));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 2, intent, PendingIntentFlags.OneShot);
                string cuerpo = GetText(Resource.String.The_rider_stopped_sharing) + " " + name[1];
                var notificationBuilder = new Notification.Builder(this)
                            .SetContentTitle(GetText(Resource.String.Unshare_horse))
                            .SetSmallIcon(Resource.Drawable.notificacion)
                            .SetContentText(cuerpo)
                            .SetAutoCancel(true)
                            .SetGroup("Horse Sharing Umshared")
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                            .SetContentIntent(pendingIntent);
                con.Insert(new Notificar()
                {
                    Titulo = DatosNotificaciones.message,
                    Cuerpo = cuerpo
                });
                var consulta = con.Query<Notificar>("select * from notificar");
                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify((consulta.Count + 1), notificationBuilder.Build());
                if (new ShareInside().ConsultarPantallaActual()=="ActivityShares")
                {
                    try
                    {
                        
                        var datos = new ShareInside().ConsultarShares(idcaballo);

                        Intent inten = new Intent(this, (typeof(ActivityShares)));

                        inten.PutExtra("idhorse", datos.id);
                        inten.PutExtra("nombre", datos.name);
                        inten.PutExtra("foto", datos.photo.data);
                        this.StartActivity(inten);
                    }
                    catch (Exception)
                    {
                        
                    }

                }
            }
        }
    }
}
