using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Android.App;
using Android.Graphics;
using Android.Widget;
using static Android.App.DatePickerDialog;
using Android.Support.V4.View;
using Android.Content;
using Android.Views;
using Com.Gigamole.Infinitecycleviewpager;
using Android.Support.V4.App;
using Java.Lang;
using Cabasus_Android.Screens;
using System.Net.Http;
using System.Json;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Android.Util;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using System.Timers;

using Android.OS;
using Cabasus_Android.Model;
using Cabasus_Android.Common;
using SQLite;
using Plugin.Connectivity;
using System.Text;
using System.Linq;
using Android.Media;
using Android.Support.Design.Widget;

namespace Cabasus_Android
{
    public class clasesJSON
    {

    }

    public class MyAdapterFp : FragmentPagerAdapter
    {
        int tabCount = 2;
        Android.App.Activity _Context;
        string Date;
        public MyAdapterFp(Android.Support.V4.App.FragmentManager fm, Android.App.Activity context, string _Date) : base(fm)
        {
            this._Context = context;
            this.Date = _Date;
        }

        public override int Count
        {
            get
            {
                return tabCount;
            }
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            ICharSequence charSequence;
            if (position == 0)
            {
                charSequence = new Java.Lang.String(_Context.GetString(Resource.String.Activities));
            }
            //else if (position == 1)
            //{
            //    charSequence = new Java.Lang.String(_Context.GetString(Resource.String.Reminder));
            ////}
            //else
            //    charSequence = new Java.Lang.String(_Context.GetString(Resource.String.Team_Journal));
            else
                charSequence = new Java.Lang.String(_Context.GetString(Resource.String.Reminder));
            return charSequence;
        }

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            return ContentFragmentFiltrosRecordatorios.NewInstance(position, _Context, Date);
        }

    }

    public class MyAdapter : FragmentPagerAdapter
    {
        int tabCount = 2;
        ActivityActivity activity;

        public MyAdapter(Android.Support.V4.App.FragmentManager fm, ActivityActivity a) : base(fm) { activity = a; }

        public override int Count
        {
            get
            {
                return tabCount;
            }
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            ICharSequence charSequence;
            if (position == 0)
            {
                charSequence = new Java.Lang.String(activity.GetText(Resource.String.Seven_days_ago).ToString());
            }
            else
            {
                charSequence = new Java.Lang.String(activity.GetText(Resource.String.Per_month).ToString());
            }

            return charSequence;
        }

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            return ContentFragment.NewInstance(position, activity);
        }

    }

    [BroadcastReceiver(Enabled = true)]
    public class AlarmNotificationReceiver : BroadcastReceiver
    {
        public override async void OnReceive(Context context, Intent intent)
        {
            var datosModelo = intent.GetStringExtra("datos");

            DatosReminder Datos = JsonConvert.DeserializeObject<DatosReminder>(datosModelo);

            int contador = intent.GetIntExtra("ids", 0);

            Intent intentFiltro = new Intent(context, (typeof(Activity_FiltrosPrincipalesCalendar)));
            intentFiltro.PutExtra("Date", Convert.ToDateTime(Datos.Begin).ToString("yyyy-MM-dd"));
            intentFiltro.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(context, contador, intentFiltro, PendingIntentFlags.OneShot);
            var notificationBuilder = new Notification.Builder(context)
                        .SetContentTitle(context.GetText(Resource.String.Begin)+": " + Datos.Begin)
                        .SetSmallIcon(Resource.Drawable.notificacion)
                        .SetContentText(Datos.Description)
                        .SetAutoCancel(true)
                        .SetGroup("Recordatorios")
                        .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                        .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(context);
            notificationManager.Notify(contador, notificationBuilder.Build());

            new ShareInside().AlarmNotifiation(context, contador);
        }
    }

    public class ShareInside
    {
        public class DensityDevice
        {
            public string dencity { get; set; }
        }
        public void xmlDencity(string dpi)
        {
            var DC = new DensityDevice();
            DC.dencity = dpi;
            var serializador = new XmlSerializer(typeof(DensityDevice));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "dencityDevice.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }
        public string xmlDencityConsult()
        {
            var serializador = new XmlSerializer(typeof(DensityDevice));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "dencityDevice.xml"));
            var datos = (DensityDevice)serializador.Deserialize(Lectura);
            Lectura.Close();
            return datos.dencity;
        }

      
        public string getDensityDpi(Context _context)
        {
            float density = (float)_context.Resources.DisplayMetrics.Density;

            if (density >= 4.0)
            {
                xmlDencity("xxxhdpi");
                return xmlDencityConsult();
            }
            else if (density >= 3.0)
            {
                xmlDencity("xxhdpi");
                return xmlDencityConsult();
            }
            else if (density >= 2.0)
            {
                xmlDencity("xhdpi");
                return xmlDencityConsult();
            }
            else if (density >= 1.5)
            {
                xmlDencity("hdpi");
                return xmlDencityConsult();
            }
            else if (density >= 1.0)
            {
                xmlDencity("mdpi");
                return xmlDencityConsult();
            }
            else
            {
                xmlDencity("ldpi");
                return xmlDencityConsult();
            }
        }

        public void GuardarActividadEnProgreso(string id, string nombre)
        {
            var DC = new PositionCicleView();
            DC.Id_HorseSelected = id;
            DC.Name_HoserSelected = nombre;
            DC.Position_HorseSelected = 0;

            var serializador = new XmlSerializer(typeof(PositionCicleView));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadEnProgreso.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }
        public PositionCicleView ConsultarActividadEnProgreso()
        {
            var serializador = new XmlSerializer(typeof(PositionCicleView));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ActividadEnProgreso.xml"));
            var datos = (PositionCicleView)serializador.Deserialize(Lectura);
            Lectura.Close();
            return datos;
        }

        public async Task<string> CadenaFotoNubeAsync(string foto)
        {
            byte[] imageAsBytes = Base64.Decode(foto, Base64Flags.Default);
            var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
            var Foto = new ShareInside().getResizedBitmap(bit, 100, 100);

            MemoryStream stream = new MemoryStream();
            Foto.Compress(Bitmap.CompressFormat.Png, 0, stream);
            byte[] bitmapData = stream.ToArray();

            string FotoCadena = Convert.ToBase64String(bitmapData);
            return FotoCadena;
        }

        public Bitmap getResizedBitmap(Bitmap bm, int newWidth, int newHeight)
        {
            int width = bm.Width;
            int height = bm.Height;
            float scaleWidth = ((float)newWidth) / width;
            float scaleHeight = ((float)newHeight) / height;
            Matrix matrix = new Matrix();
            matrix.PostScale(scaleWidth, scaleHeight);
            Bitmap resizedBitmap = Bitmap.CreateBitmap(
                bm, 0, 0, width, height, matrix, false);
            bm.Recycle();
            return resizedBitmap;
        }
        public void CopyDocuments(string FileName,string AssetsFileName)
        {

            //string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            //string path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(path, FileName);

            try
            {
                if (!File.Exists(dbPath))
                {
                    using (var br = new BinaryReader(Application.Context.Assets.Open(AssetsFileName)))
                    {
                        using (var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                        {
                            byte[] buffer = new byte[2048];
                            int length = 0;
                            while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bw.Write(buffer, 0, length);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        public Bitmap RedondearbitmapImagen(Bitmap bitmap, int px)
        {
            Bitmap output = Bitmap.CreateBitmap(bitmap.Width, bitmap
                    .Height, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(output);

            Paint paint = new Paint();
            Rect rect = new Rect(0, 0, bitmap.Width,
                bitmap.Height);
            RectF rectF = new RectF(rect);

            paint.AntiAlias = true;
            canvas.DrawARGB(0, 0, 0, 0);
            paint.Color = Color.Black;
            canvas.DrawRoundRect(rectF, (float)px, (float)px, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, rect, rect, paint);

            return output;

        }

        public void GuardarCaballos(string ConsultaJson)
        {
            var DC = new IdNameHorse();
            DC.DatosCaballo = ConsultaJson;
            var serializador = new XmlSerializer(typeof(IdNameHorse));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }



        public HorsesComplete ConsultarShares(string id)
        {
            try
            {
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                var Datos = JsonConvert.DeserializeObject<List<HorsesComplete>>(((IdNameHorse)(new XmlSerializer(typeof(IdNameHorse))).Deserialize(Lectura)).DatosCaballo);
                Lectura.Close();
                foreach (var item in Datos)
                {
                    if (item.id==id)
                    {
                        return item; 
                    }
                }
                return null;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public async Task GuardadoFotosYConsulta()
        {
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
                    GuardarCaballos(ConsultaJson);
                    foreach (var item in data)
                    {
                        var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), item.id + ".png"));
                        if (!File.Exists(RutaImage.ToString()))
                        {
                            byte[] ArregloImagen = Base64.Decode(item.photo.data, Base64Flags.Default);
                            var biteImage = BitmapFactory.DecodeByteArray(ArregloImagen, 0, ArregloImagen.Length);

                            var biteMapRedondoImage = new ShareInside().RedondearbitmapImagen(biteImage, 200);

                            byte[] bitmapData;
                            //byte[] bitmapImagenCuadrada;
                            using (var stream = new MemoryStream())
                            {
                                biteMapRedondoImage.Compress(Bitmap.CompressFormat.Png, 0, stream);
                                biteImage.Compress(Bitmap.CompressFormat.Png, 0, stream);

                                bitmapData = stream.ToArray();
                                //bitmapImagenCuadrada = stream.ToArray();
                            }
                            File.WriteAllBytes(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/" + item.id + ".png"), bitmapData);
                            //File.WriteAllBytes(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/" + item.id + "D.png"), bitmapImagenCuadrada);
                        }
                    }
                    GuardarPosicionPiker(data[0].id, data[0].name, 0);
                }
                else
                {
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                }
            }
        }

        public async Task GuardarFotosEditaddas(string IdFoto)
        {
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
                    GuardarCaballos(ConsultaJson);
                    foreach (var item in data)
                    {
                        var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), item.id + ".png"));
                        if (IdFoto==item.id)
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
                    GuardarPosicionPiker(data[0].id, data[0].name, 0);
                }
                else
                {
                    System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                }
            }
        }

        public int ConvertPixelsToDp(float pixelValue, float DisplayMetrics)
        {
            var dp = (int)((pixelValue) / DisplayMetrics);
            return dp;
        }

        public float ConvertDPToPixels(int DP, float DisplayMetrics)
        {
            var Pix = ((DP) * DisplayMetrics);
            return (float)System.Math.Round(Pix);
        }

        public void SaveToken(validartoken token)
        {
            var DC = new validartoken();
            try
            {
                DC.token = token.token;
                DC.expiration = token.expiration;
                var serializador = new XmlSerializer(typeof(validartoken));
                var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "token.xml"));
                serializador.Serialize(Escritura, DC);
                Escritura.Close();
            }
            catch (System.Exception) { }
        }

        public void SaveTokenPhone(string token)
        {
            var DC = new Token();
            try
            {
                DC.token = token;
                var serializador = new XmlSerializer(typeof(Token));
                var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "tokenphone.xml"));
                serializador.Serialize(Escritura, DC);
                Escritura.Close();
            }
            catch (System.Exception) { }
        }

        public string ConsultTokenPhone()
        {
            try
            {
                var serializador = new XmlSerializer(typeof(Token));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "tokenphone.xml"));
                var datos = (Token)serializador.Deserialize(Lectura);
                Lectura.Close();
                return datos.token;
            }
            catch (System.Exception) { return ""; }
        }
        private DateTime ConsultarExpiracion()
        {
            var serializador = new XmlSerializer(typeof(validartoken));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "token.xml"));
            var datos = (validartoken)serializador.Deserialize(Lectura);
            Lectura.Close();
            return datos.expiration;
        }
        public string ConsultToken()
        {
            try
            {
                var expiracion = ConsultarExpiracion();
                var fechaactual = DateTime.Now;
                if (fechaactual >= expiracion)
                {
                     GenerarToken();
                }
                var serializador = new XmlSerializer(typeof(validartoken));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "token.xml"));
                var datos = (validartoken)serializador.Deserialize(Lectura);
                Lectura.Close();
                return datos.token;
            }
            catch (System.Exception) { return ""; }
        }

        public async void GenerarToken()
        {
            string server = "https://cabasus-mobile.azurewebsites.net/v1/auth/login";
            string json = "application/json";
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("email", ConsultarDatosDeLogueo()[0].User);
            jsonObject.Add("password", ConsultarDatosDeLogueo()[0].Password);
            HttpClient cliente = new HttpClient();
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    var respuesta = await cliente.PostAsync(server, new StringContent(jsonObject.ToString(), System.Text.Encoding.UTF8, json));
                    if (respuesta.IsSuccessStatusCode)
                    {
                        var content = await respuesta.Content.ReadAsStringAsync();
                        Token deserializedProduct = JsonConvert.DeserializeObject<Token>(content);
                        var tokenvalidar = new validartoken()
                        {
                            token = deserializedProduct.token,
                            expiration = DateTime.Now.AddDays(1)
                        };
                        SaveToken(tokenvalidar);
                       // Toast.MakeText(context, deserializedProduct.token, ToastLength.Short).Show();
                    }
                }
            }
            catch
            {
            }
        }
      
        public void GuardarPantallaActual(string ID_Pantalla)
        {
            var DC = new Token();
            try
            {
                DC.token = ID_Pantalla;
                var serializador = new XmlSerializer(typeof(Token));
                var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "PantallaActual.xml"));
                serializador.Serialize(Escritura, DC);
                Escritura.Close();
            }
            catch (System.Exception) { }
        }

        public string ConsultarPantallaActual()
        {
            try
            {
                var serializador = new XmlSerializer(typeof(Token));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "PantallaActual.xml"));
                var datos = (Token)serializador.Deserialize(Lectura);
                Lectura.Close();
                return datos.token;
            }
            catch (System.Exception) { return ""; }
        }

        public void GuardarEstadoParaLaActividad(string e)
        {
            var DC = new EstadoActividad();
            try
            {
                DC.Estado = e;
                var serializador = new XmlSerializer(typeof(EstadoActividad));
                var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoParaLaActividad.xml"));
                serializador.Serialize(Escritura, DC);
                Escritura.Close();
            }
            catch (System.Exception) { }
        }

        public string ConsultarEstadoParaLaActividad()
        {
            try
            {
                var serializador = new XmlSerializer(typeof(EstadoActividad));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoParaLaActividad.xml"));
                var datos = (EstadoActividad)serializador.Deserialize(Lectura);
                Lectura.Close();
                return datos.Estado;
            }
            catch (System.Exception) { return ""; }
        }
        
        public string StoH(int segundos)
        {
            int hora = 0, min = 0, seg = 0;
            hora = segundos / 3600;
            min = (segundos - hora * 3600) / 60;
            seg = segundos - (hora * 3600 + min * 60);

            if (hora < 10 && min < 10 && seg < 10)
            {
                return "0" + hora + ":0" + min + ":0" + seg;
            }
            else if (hora >= 10 && min < 10 && seg < 10)
            {
                return hora + ":0" + min + ":0" + seg;
            }
            else if (hora >= 10 && min >= 10 && seg < 10)
            {
                return hora + ":" + min + ":0" + seg;
            }
            else if (hora < 10 && min < 10 && seg > 10)
            {
                return "0" + hora + ":0" + min + ":" + seg;
            }
            else
            {
                return hora + ":" + min + ":" + seg;
            }
        }
        
        public async Task GuardarDatosUsuario()
        {
            string ServerConsultarUser = "https://cabasus-mobile.azurewebsites.net/v1/profile";
            var URIConsultaUser = new Uri(string.Format(ServerConsultarUser, string.Empty));
            HttpClient ClienteConsultaUser = new HttpClient();
            ClienteConsultaUser.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ConsultToken());
            var SaveConsultUser = await ClienteConsultaUser.GetAsync(URIConsultaUser);
            SaveConsultUser.EnsureSuccessStatusCode();

            if (SaveConsultUser.IsSuccessStatusCode)
            {
                JsonValue ConsultaJson = await SaveConsultUser.Content.ReadAsStringAsync();

                var Datos = JsonConvert.DeserializeObject<GetUser>(ConsultaJson);

                var DC = new GetUser();
                try
                {
                    DC.id = Datos.id;
                    DC.createdAt = Datos.createdAt;
                    DC.updatedAt = Datos.updatedAt;
                    DC.email = Datos.email;
                    DC.weight = Datos.weight;
                    DC.height = Datos.height;
                    DC.birthday = Datos.birthday;
                    DC.username = Datos.username;
                    DC.phones = Datos.phones;
                    DC.language = Datos.language;
                    DC.privacy = Datos.privacy;
                    //try
                    //{
                    DC.photo = Datos.photo;
                    //}
                    //catch (System.Exception)
                    //{

                    //    Java.IO.File ImagenUri = new Java.IO.File((Resource.Drawable.Foto).ToString());
                    //    Bitmap ImagenBitMap = BitmapFactory.DecodeFile(ImagenUri.AbsolutePath);
                    //    string foto2 = "";
                    //}

                    DC.permissions = Datos.permissions;

                    var serializador = new XmlSerializer(typeof(GetUser));
                    var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ConsultDataUsers.xml"));
                    serializador.Serialize(Escritura, DC);
                    Escritura.Close();
                }
                catch 
                {

                }
            }
        }
        public List<GetUser> ConsultarDatosUsuario()
        {
            try
            {
                var serializador = new XmlSerializer(typeof(GetUser));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ConsultDataUsers.xml"));
                var datos = (GetUser)serializador.Deserialize(Lectura);
                Lectura.Close();
                List<GetUser> Lista = new List<GetUser>();
                Lista.Add(datos);
                return Lista;
            }
            catch(System.Exception ex)
            {
                return null;
            }
        }

        public string consultarPickerNotificacionIdCaballo()
        {
            try
            {
                var serializador = new XmlSerializer(typeof(PositionCicleView));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "pickerNotificacionIdCaballo.xml"));
                var datos = (PositionCicleView)serializador.Deserialize(Lectura);
                Lectura.Close();
                return datos.Id_HorseSelected;
            }
            catch (System.Exception ex)
            { return ex.Message; }
            
        }

        public void pickerNotificacionIdCaballo(string id)
        {
            try
            {
                var DC = new PositionCicleView();
                DC.Id_HorseSelected = id;

                var serializador = new XmlSerializer(typeof(PositionCicleView));
                var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "pickerNotificacionIdCaballo.xml"));
                serializador.Serialize(Escritura, DC);
                Escritura.Close();
            }
            catch (System.Exception)
            {
            }
        }

        public void GuardarPosicionPiker(string id, string name, int position)
        {
            var DC = new PositionCicleView();
            DC.Id_HorseSelected = id;
            DC.Name_HoserSelected = name;
            DC.Position_HorseSelected = position;

            var serializador = new XmlSerializer(typeof(PositionCicleView));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "PositionCicleView.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }
        public List<PositionCicleView> ConsultarPosicionPiker()
        {
            var serializador = new XmlSerializer(typeof(PositionCicleView));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "PositionCicleView.xml"));
            var datos = (PositionCicleView)serializador.Deserialize(Lectura);
            Lectura.Close();
            List<PositionCicleView> Lista = new List<PositionCicleView>();
            Lista.Add(datos);
            return Lista;
        }

        public void GuardarDatosDeLogueo(string user, string password, string languaje)
        {
            var DC = new DatosDeLogeo();
            DC.User = user;
            DC.Password = password;
            DC.Language = languaje;

            var serializador = new XmlSerializer(typeof(DatosDeLogeo));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DatosDeLogeo.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }
        public List<DatosDeLogeo> ConsultarDatosDeLogueo()
        {
            var serializador = new XmlSerializer(typeof(DatosDeLogeo));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DatosDeLogeo.xml"));
            var datos = (DatosDeLogeo)serializador.Deserialize(Lectura);
            Lectura.Close();
            List<DatosDeLogeo> Lista = new List<DatosDeLogeo>();
            Lista.Add(datos);
            return Lista;
        }

        public void GuardarRecordarEmail(string email)
        {
            var DC = new RecordarEmail();
            DC.Email = email;

            var serializador = new XmlSerializer(typeof(RecordarEmail));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RecordarEmail.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }
        public string ConsultarRecordarEmail()
        {
            var serializador = new XmlSerializer(typeof(RecordarEmail));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RecordarEmail.xml"));
            var datos = (RecordarEmail)serializador.Deserialize(Lectura);
            Lectura.Close();
            return datos.Email;
        }

        public void GuardarEstadoNotificacion(int opcion)
        {

            var DC = new NumeroNoti();
            int valor;
            #region consultar xml
            try
            {
                var serializadorc = new XmlSerializer(typeof(NumeroNoti));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));
                var datos = (NumeroNoti)serializadorc.Deserialize(Lectura);
                Lectura.Close();
                valor = datos.numero;
            }
            catch (System.Exception)
            {
                valor = 0;
            }
            #endregion
            try
            {
                if (opcion == 1)
                {
                    DC.numero = valor + 1;
                    var serializador = new XmlSerializer(typeof(NumeroNoti));
                    var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));
                    serializador.Serialize(Escritura, DC);
                    Escritura.Close();
                }
                else if (opcion == 0)
                {
                    DC.numero = 0;
                    var serializador = new XmlSerializer(typeof(NumeroNoti));
                    var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));
                    serializador.Serialize(Escritura, DC);
                    Escritura.Close();
                }
                else
                {
                    DC.numero = opcion;
                    var serializador = new XmlSerializer(typeof(NumeroNoti));
                    var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));
                    serializador.Serialize(Escritura, DC);
                    Escritura.Close();
                }

            }
            catch (System.Exception) { }
        }

        public int ConsultarEstaodNotificacion()
        {
            try
            {
                var serializador = new XmlSerializer(typeof(NumeroNoti));
                var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "EstadoNotificacion.xml"));
                var datos = (NumeroNoti)serializador.Deserialize(Lectura);
                Lectura.Close();
                return datos.numero;
            }
            catch (System.Exception ex) { return 0; }
        }

        public async Task<int> consultaparanotiicacionAsync()
        {
            int cantidad = 0;
            #region Consulta para notificaicones
            if (CrossConnectivity.Current.IsConnected)
            {
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                string servidor = "https://cabasus-mobile.azurewebsites.net/v1/profile/pending_shares";
                var Notificacion = await Cliente.GetAsync(servidor);
                Notificacion.EnsureSuccessStatusCode();

                if (Notificacion.IsSuccessStatusCode)
                {
                    JsonValue Consultanotificacion = await Notificacion.Content.ReadAsStringAsync();
                    var notificaciones = JsonConvert.DeserializeObject<List<Notificaciones>>(Consultanotificacion);

                    foreach (var item in notificaciones)
                    {
                        for (int i = 0; i < item.shares.Count; i++)
                        {
                            cantidad++;
                        }
                    }


                }
                return cantidad;
            }
            else
            {
                cantidad = 0;
                return cantidad;
            }

            #endregion
        }

        public void alarm(DatosReminder item, int contador, Context _context)
        {
            contador++;
            AlarmManager manager = (AlarmManager)_context.GetSystemService(Context.AlarmService);
            Intent myIntent = new Intent(_context, typeof(AlarmNotificationReceiver));
            myIntent.PutExtra("datos", JsonConvert.SerializeObject(item));
            myIntent.PutExtra("ids", contador);
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(_context, contador, myIntent, 0);

            TimeSpan Tiempo = Convert.ToDateTime(item.Begin) - DateTime.Now;
            long lapso = (long)(double.Parse(item.Notification) * 1000);

            long MiliSec = (long)Tiempo.TotalMilliseconds;

            long LapsoTotal = MiliSec - lapso;

            manager.Set(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + LapsoTotal, pendingIntent);
        }
        public DatosReminder LocalFiltro(List<DatosReminder> ListaRecordatorios)
        {
            DatosReminder Datos = new DatosReminder();
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
            var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders;", new RedordatoriosLocales().id_reminder);

            if (ConsultaRecordatorios.Count > 0)
            {
                foreach (var item in ConsultaRecordatorios)
                {
                    DateTime Fecha = Convert.ToDateTime(Convert.ToDateTime(item.inicio).ToString("MM/dd/yyyy HH:mm:ss"));
                    DateTime FechaLocal = Convert.ToDateTime(item.inicio);
                    if (FechaLocal >= DateTime.Now)
                    {
                        var datos = new DatosReminder();
                        datos.Description = item.descripcion;
                        datos.Begin = Fecha.ToString("MM/dd/yyyy HH:mm:ss");
                        datos.End = item.fin;
                        datos.Notification = item.notificacion.ToString();
                        datos.id_reminder = item.id_reminder.ToString();
                        datos.Tipo = item.Tipo.ToString();
                        datos.fk_usuario = item.fk_Usuario;
                        datos.fk_caballo = item.fk_caballo;
                        ListaRecordatorios.Add(datos);
                    }
                }
            }

            int contador = 0;
            if (ListaRecordatorios.Count > 0)
            {
                ListaRecordatorios = ListaRecordatorios.OrderBy(d => d.Begin).ToList();
                foreach (var item in ListaRecordatorios)
                {
                    if (contador == 0)
                    {
                        contador++;
                        Datos = item;
                        break;
                    }
                }
            }
            return Datos;
        }
        public async Task<DatosReminder> NotificationRemindersFiltro()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                #region Servidor
                string server = "https://cabasus-mobile.azurewebsites.net/v1/events";
                var uri = new System.Uri(string.Format(server, string.Empty));
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                var consulta = await Cliente.GetAsync(uri);
                consulta.EnsureSuccessStatusCode();
                #endregion

                if (consulta.IsSuccessStatusCode)
                {
                    JsonValue ConsultaJson = await consulta.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<RecordatoriosNube>>(ConsultaJson);

                    if (data.Count > 0)
                    {
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        foreach (var item in data)
                        {
                            DateTime fechaNube = Convert.ToDateTime(item.date.Substring(0, 10) + " " + item.date.Substring(11, 8));
                            if (fechaNube >= DateTime.Now && item.owner == new ShareInside().ConsultarDatosUsuario()[0].id)
                            {
                                var datos = new DatosReminder();
                                datos.Description = item.description;
                                datos.Begin = fechaNube.ToString();
                                datos.End = item.end_date;
                                datos.Notification = item.alert_before.ToString();
                                datos.id_reminder = item.id;
                                datos.Tipo = item.type.ToString();
                                datos.fk_usuario = item.owner;
                                datos.fk_caballo = item.horse;
                                ListaRecordatorios.Add(datos);
                            }
                        }

                        DatosReminder Datos = LocalFiltro(ListaRecordatorios);
                        return Datos;
                    }
                    else
                    {
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        DatosReminder Datos = LocalFiltro(ListaRecordatorios);
                        return Datos;
                    }
                }
                else
                {
                    List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                    DatosReminder Datos = LocalFiltro(ListaRecordatorios);
                    return Datos;
                }
            }
            else
            {
                List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                DatosReminder Datos = LocalFiltro(ListaRecordatorios);
                return Datos;
            }
        }
        public async void AlarmNotifiation(Context context, int contador)
        {
            DatosReminder ModeloAlarm = await NotificationRemindersFiltro();
            if (ModeloAlarm.id_reminder != null)
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var ConsultaRecoedatorios = con.Query<DatosReminder>("select * from AlarmaRecordatorios;", (new DatosReminder()).id_reminder);
                if (ConsultaRecoedatorios.Count > 0)
                {
                    bool ok = false;

                    foreach (var item in ConsultaRecoedatorios)
                    {
                        if (item.id_reminder == ModeloAlarm.id_reminder && item.Begin == ModeloAlarm.Begin && item.Description == ModeloAlarm.Description)
                        {
                            ok = false;
                            if (Convert.ToDateTime(item.Begin) < DateTime.Now)
                            {
                                var insertar = con.Query<DatosReminder>("delete from AlarmaRecordatorios where Begin = '" + item.Begin + "';", (new DatosReminder()).id_reminder);
                            }
                        }
                        else
                        {
                            ok = true;
                            if (Convert.ToDateTime(item.Begin) < DateTime.Now)
                            {
                                var insertar = con.Query<DatosReminder>("delete from AlarmaRecordatorios where Begin = '" + item.Begin + "';", (new DatosReminder()).id_reminder);
                            }
                        }
                    }
                    if (ok)
                    {
                        var insertar = con.Query<DatosReminder>("insert into AlarmaRecordatorios values('" + ModeloAlarm.Description + "', '" + ModeloAlarm.Begin + "', '" + ModeloAlarm.End + "', '" + ModeloAlarm.Notification + "', '" + ModeloAlarm.Tipo + "', '" + ModeloAlarm.id_reminder + "', '" + ModeloAlarm.fk_usuario + "', '" + ModeloAlarm.fk_caballo + "', '" + ModeloAlarm.fecha + "');", (new DatosReminder()).id_reminder);
                        new ShareInside().alarm(ModeloAlarm, contador++, context);
                    }
                }
                else
                {
                    var insertar = con.Query<DatosReminder>("insert into AlarmaRecordatorios values('" + ModeloAlarm.Description + "', '" + ModeloAlarm.Begin + "', '" + ModeloAlarm.End + "', '" + ModeloAlarm.Notification + "', '" + ModeloAlarm.Tipo + "', '" + ModeloAlarm.id_reminder + "', '" + ModeloAlarm.fk_usuario + "', '" + ModeloAlarm.fk_caballo + "', '" + ModeloAlarm.fecha + "');", (new DatosReminder()).id_reminder);
                    new ShareInside().alarm(ModeloAlarm, contador++, context);
                }
            }
        }

        public void GuardarIdioma(string Idioma, string Pais)
        {
            var DC = new Idiomas();
            DC.Idioma = Idioma;
            DC.Pais = Pais;

            var serializador = new XmlSerializer(typeof(Idiomas));
            var Escritura = new StreamWriter(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdiomaApp.xml"));
            serializador.Serialize(Escritura, DC);
            Escritura.Close();
        }
        public Idiomas ConsultarIdioma()
        {
            var serializador = new XmlSerializer(typeof(Idiomas));
            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdiomaApp.xml"));
            var datos = (Idiomas)serializador.Deserialize(Lectura);
            Lectura.Close();
            return datos;
        }
        public class Idiomas
        {
            public string Idioma { get; set; }
            public string Pais { get; set; }
        }
    }
    
    public class PickerDate : Java.Lang.Object, IOnDateSetListener
    {
        Button textoDate;
        TextView textoDateDiario;

        public PickerDate(Button textoDate)
        {
            this.textoDate = textoDate;
        }

        public PickerDate(TextView textoDateDiario)
        {
            this.textoDateDiario = textoDateDiario;
        }

        void IOnDateSetListener.OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            string m = (month + 1).ToString(), d = dayOfMonth.ToString();

            if (m.Length <= 1)
                m = "0" + (month + 1).ToString();
            if (d.Length <= 1)
                d = "0" + dayOfMonth.ToString();

            try { textoDate.Text = year.ToString() + "/" + m + "/" + d; } catch (System.Exception) { }
            try
            {
                double Mes = (double.Parse(m) + 1);
                if (Mes < 10)
                    m = "0" + Mes.ToString(); 
                textoDateDiario.Text = d + "-" + m + "-" + year.ToString();
            } catch (System.Exception) { }
        }
    }

    public class Constants
    {
        public const string ListenConnectionString = "Endpoint=sb://cabasus-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=Wk9SpMer9RaoKka4P3W5aqqqCwTbyB4TVnRES465ols=";
        public const string NotificationHubName = "cabasus-notification";
    }

    public class HorsesComplete
    {
        public string name { get; set; }
        public string breed { get; set; }
        public float weight { get; set; }
        public float height { get; set; }
        public string birthday { get; set; }
        public bool visible { get; set; }
        public string gender { get; set; }
        public Photo photo { get; set; }
        public List<ShareSettings> shares { get; set; }
        public string id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public OwnerSettings owner { get; set; }
    }

    public class OwnerSettings
    {
        public string id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string email { get; set; }
        public int weight { get; set; }
        public int height { get; set; }
        public string birthday { get; set; }
        public string username { get; set; }
        public List<Phone> phones { get; set; }
        public string language { get; set; }
        public Privacy privacy { get; set; }
        public Photo photo { get; set; }
        public List<string> permissions { get; set; }
    }

    public class Photo
    {
        public string data { get; set; }
        public string content_type { get; set; }
    }

    public class ShareSettings
    {
        public string user { get; set; }
    }

    public class HorsesCloud
    {
        public string id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string owner_name { get; set; }
        public string photo { get; set; }
        public int Sync { get; set; }
        public List<ShareSettings> shares { get; set; }
    }

    public class ActividadPorCaballo
    {
        [PrimaryKey, AutoIncrement]
        public int ID_ActividadLocal { get; set; }
        public string ID_Caballo { get; set; }
        public string ID_Usuario { get; set; }
        public int Duration { get; set; }
        public double Slow { get; set; }
        public double Normal { get; set; }
        public double Strong { get; set; }
        public string Horse_Status { get; set; }
        public string Dates { get; set; }
        public string Latitudes { get; set; }
        public string Longitudes { get; set; }
    }

    public class ActividadesCloud
    {
        public string ID_ActividadLocal { get; set; }
        public string ID_Caballo { get; set; }
        public string ID_Usuario { get; set; }
        public int Duration { get; set; }
        public double Slow { get; set; }
        public double Normal { get; set; }
        public double Strong { get; set; }
        public string Horse_Status { get; set; }
        public string Dates { get; set; }
        public string Latitudes { get; set; }
        public string Longitudes { get; set; }
        public string User { get; set; }
    }

    public class ActividadesCloudMes
    {
        public string ID_ActividadLocal { get; set; }
        public string ID_Caballo { get; set; }
        public string ID_Usuario { get; set; }
        public int Duration { get; set; }
        public double Slow { get; set; }
        public double Normal { get; set; }
        public double Strong { get; set; }
        public string Horse_Status { get; set; }
        public string Dates { get; set; }
        public string Latitudes { get; set; }
        public string Longitudes { get; set; }
        public string User { get; set; }
    }

    public class Actividad
    {
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public int Velocidad { get; set; }
    }

    public class CreateUser
    {
        public string email { get; set; }
        public string password { get; set; }
        public string language { get; set; }
    }

    public class Phone
    {
        public string number { get; set; }
        public string os { get; set; }
        public string key { get; set; }
        public string id { get; set; }
        public string registrationId { get; set; }
    }

    public class Privacy
    {
        public bool searchable { get; set; }
    }

    public class UpdateUser
    {
        public double weight { get; set; }
        public double height { get; set; }
        public string birthday { get; set; }
        public string username { get; set; }
        public List<Phone> phones { get; set; }
        public string language { get; set; }
        public Privacy privacy { get; set; }
        public Photo photo { get; set; }
    }

    public class validartoken
    {
        public string token { get; set; }
        public DateTime expiration
        {
            get;
            set;
        }
    }

    public class Token
    {
        public string token { get; set; }
    }

    public class TokenPhone
    {
        public string id { get; set; }
    }

    public class GetPhone
    {
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public string registrationId { get; set; }
        public string owner { get; set; }
        public string number { get; set; }
        public string os { get; set; }
        public string token { get; set; }
        public string id { get; set; }
    }

    public class ConsumoAvena
    {
        public string id_caballo { get; set; }
        public double kilogramos { get; set; }
    }
    public class EstadoActividad
    {
        public string Estado { get; set; }
    }

    public class LoginUser
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class GetUser
    {
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public string email { get; set; }
        public DateTime verifiedAt { get; set; }
        public DateTime birthday { get; set; }
        public double height { get; set; }
        public string language { get; set; }
        public string username { get; set; }
        public double weight { get; set; }
        public List<object> permissions { get; set; }
        public Photo photo { get; set; }
        public Privacy privacy { get; set; }
        public List<Phone> phones { get; set; }
        public string id { get; set; }
    }

    public class CaballoId
    {
        public List<int> idedo = new List<int>();

        public List<int> getCaballoId(int id)
        {
            this.idedo.Add(id);
            return this.idedo;
        }

    }

    public class Photo2
    {
        public string data { get; set; }
    }

    public class Privacy2
    {
        public bool searchable { get; set; }
    }

    public class UserPhone
    {
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public string email { get; set; }
        public string language { get; set; }
        public DateTime birthday { get; set; }
        public int height { get; set; }
        public string username { get; set; }
        public int weight { get; set; }
        public List<object> permissions { get; set; }
        public Photo2 photo { get; set; }
        public Privacy2 privacy { get; set; }
        public List<string> phones { get; set; }
        public string id { get; set; }
    }

    public class CaballosUpFormat : PagerAdapter
    {
        List<string> listNombreC;
        List<string> listId;
        Context context;
        LayoutInflater layoutInflater;
        HorizontalInfiniteCycleViewPager cycleViewPager;

        public CaballosUpFormat(List<string> listNombreC, List<string> listId, Context context, HorizontalInfiniteCycleViewPager cycleViewPager)
        {
            this.listNombreC = listNombreC;
            this.listId = listId;
            this.context = context;
            this.cycleViewPager = cycleViewPager;
            layoutInflater = LayoutInflater.From(context);
        }

        ImageView imgView;
        TextView lblNombreCaballoUp;
        string Id_Horse;
        
        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            View view = layoutInflater.Inflate(Resource.Layout.CaballosUP, container, false);
            imgView = view.FindViewById<ImageView>(Resource.Id.imgCaballo);

            var RutaImage = Android.Net.Uri.Parse(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), listId[position] + ".png"));

            Java.IO.File ImagenUri = new Java.IO.File(RutaImage.ToString());
            Bitmap ImagenBitMap = BitmapFactory.DecodeFile(ImagenUri.AbsolutePath);
            imgView.SetImageBitmap(ImagenBitMap);
            
            Id_Horse = listId[position];
            container.AddView(view);

            return view;
        }

        public override int Count => listNombreC.Count;

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view.Equals(@object);
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            container.RemoveView((View)@object);
        }
        
    }
    public class IdNameHorse
    {
        public string DatosCaballo { get; set; }
    }
    public class PositionCicleView
    {
        public string Id_HorseSelected { get; set; }
        public string Name_HoserSelected { get; set; }
        public int Position_HorseSelected { get; set; }
    }
    public class DatosDeLogeo
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Language { get; set; }
    }

    public class MensajeErrorLogin
    {
        public string status { get; set; }
    }

    public class ListaErroresLogin
    {
        public List<string> messages { get; set; }
    }

    public class RecordarEmail
    {
        public string Email { get; set; }
    }
    public class Razas
    {
        public int Id_Raza { get; set; }
        public int id_gender { get; set; }
        public string tipo { get; set; }
        public string en { get; set; }
        public string de { get; set; }
        public string fr { get; set; }
        public string it { get; set; }
        public string no { get; set; }
        public string ar { get; set; }
        public string tr { get; set; }
        public string pt { get; set; }
        public string zu { get; set; }
        public string ru { get; set; }
        public string es { get; set; }

    }

    public class Distance
    {
        public double slow { get; set; }
        public double normal { get; set; }
        public double strong { get; set; }
    }

    public class LocationCloud
    {
        public List<double[]> coordinates { get; set; }
    }

    public class ActividadCluod
    {
        public string id { get; set; }
        public string horse { get; set; }
        public double duration { get; set; }
        public Distance distance { get; set; }
        public string horse_status { get; set; }
        public string date { get; set; }
        public OwnerActivity owner { get; set; }
        public LocationCloud location { get; set; }
        public bool visible { get; set; }
    }

    public class SubirActividadCluod
    {
        public string horse { get; set; }
        public double duration { get; set; }
        public Distance distance { get; set; }
        public string horse_status { get; set; }
        public string date { get; set; }
        public LocationCloud location { get; set; }
        public bool visible { get; set; }
    }

    public class OwnerActivity
    {
        public string username { get; set; }
        public string id { get; set; }
    }

    public class NewPhones
    {
        public GetUser owner { get; set; }
        public string number { get; set; }
        public string os { get; set; }
        public string token { get; set; }
    }

    public class NotificationsData
    {
        public string user { get; set; }
        public string horse { get; set; }
        public string message { get; set; }
    }

    /*class LoadDataAsync : AsyncTask<string, string, string>
    {
        ActivityHome homeActivity;
        int position = 0;
        LinearLayout linear;
        bool internet;

        public LoadDataAsync(ActivityHome homeActivity, LinearLayout linear, bool internet)
        {
            this.homeActivity = homeActivity;
            this.linear = linear;
            this.internet = internet;
        }

        protected override void OnPreExecute()
        {

        }

        protected override string RunInBackground(params string[] @params)
        {
            string result = new HTTPDataHandler().GetHTTPData(@params[0]);
            return result;
        }

        protected override void OnPostExecute(string result)
        {
            if (internet)
            {
                try
                {
                    RssObject data = JsonConvert.DeserializeObject<RssObject>(result);
                    while (data.items.Count > position)
                    {
                        var rowhottips = homeActivity.LayoutInflater.Inflate(Resource.Layout.layout_rss, null);
                        var title = rowhottips.FindViewById<TextView>(Resource.Id.txtTitleRss);
                        title.Text = data.items[position].title;
                        var pubDate = rowhottips.FindViewById<TextView>(Resource.Id.txtPubDateRss);
                        pubDate.Text = data.items[position].pubDate;
                        var description = rowhottips.FindViewById<TextView>(Resource.Id.txtDescriptionRss);
                        description.Text = data.items[position].content;

                        position++;
                        linear.AddView(rowhottips);
                    }
                }
                catch
                { }
            }
            else
            {
                var rowhottips = homeActivity.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                rowhottips.FindViewById<TextView>(Resource.Id.lblNoData).Text = "Necesita internet para visualizar Hot-Tips";
                linear.AddView(rowhottips);
            }
        }
    }*/

    //public class myItem {
    //    public string title { get; set; }
    //    public string pubDate { get; set; }
    //    public string link { get; set; }
    //    public string guid { get; set; }
    //    public string author { get; set; }
    //    public string thumbnail { get; set; }
    //    public string description { get; set; }
    //    public string content { get; set; }
    //    public Enclosure enclosure { get; set; }
    //    public List<string> categories { get; set; }
    //}

    public class AdaptadorContentsHome : PagerAdapter
    {
        Context _context;
        private List<Item> _item;
        private List<DatosReminder> _reminder;
        LayoutInflater _layoutInflater;
        int _content;

        public AdaptadorContentsHome(Context context, List<Item> item, List<DatosReminder> reminder, LayoutInflater layout, int content)
        {
            _context = context;
            _item = item;
            _reminder = reminder;
            _layoutInflater = layout;
            _content = content;
        }

        public override int Count
        {
            get
            {
                if (_content == 0)
                    return _item.Count;
                else if (_content == 1)
                    return _reminder.Count;
                else
                    return _reminder.Count;
            }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view == ((View)@object);
        }

        public override Java.Lang.Object InstantiateItem(View container, int position)
        {
            var raiz = _layoutInflater.Inflate(Resource.Layout.layout_rss, null);
            var txtTitulo = raiz.FindViewById<TextView>(Resource.Id.txtTituloRSS);
            var txtCuerpo = raiz.FindViewById<TextView>(Resource.Id.txtCuerpoRSS);

            txtTitulo.Text = _item[position].title;
            txtCuerpo.Text = _item[position].content;

            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(_item[position].link));

            txtTitulo.Click += delegate {
                _context.StartActivity(intent);
            };
            txtCuerpo.Click += delegate {
                _context.StartActivity(intent);
            };
            ((ViewPager)container).AddView(raiz, 0);
            return raiz;
        }

        public override void DestroyItem(View container, int position, Java.Lang.Object @object)
        {
            ((ViewPager)container).RemoveView((View)@object);
        }
    }

    public class AdaptadorContents : PagerAdapter
    {
        private List<DatosReminder> _reminder;
        LayoutInflater _layoutInflater;
        int _content;

        public AdaptadorContents(List<DatosReminder> reminder, LayoutInflater layout, int content)
        {
            _reminder = reminder;
            _layoutInflater = layout;
            _content = content;
        }

        public override int Count
        {
            get
            {
                if (_content == 1)
                    return _reminder.Count;
                else
                    return _reminder.Count;
            }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view == ((View)@object);
        }

        public override Java.Lang.Object InstantiateItem(View container, int position)
        {
            if (_content == 1)
            {
                if (_reminder[position].Description.Equals(Resource.String.There_are_no_reminders))
                {
                    var raiz = _layoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                    raiz.FindViewById<TextView>(Resource.Id.lblNoData).Text = _reminder[position].Description;    //SetText(Resource.String.No_data);
                    ((ViewPager)container).AddView(raiz, 0);
                    return raiz;
                }
                else
                {
                    var raiz = _layoutInflater.Inflate(Resource.Layout.layout_ContenidoRecordatorios, null);

                    raiz.FindViewById<TextView>(Resource.Id.txtDescripcionReminder).Text = _reminder[position].Description;
                    raiz.FindViewById<TextView>(Resource.Id.lblBegin).Text = _reminder[position].Begin;
                    raiz.FindViewById<TextView>(Resource.Id.lblEnd).Text = _reminder[position].End;
                    raiz.FindViewById<TextView>(Resource.Id.lblNotification).Text = _reminder[position].Notification;
                    raiz.FindViewById<LinearLayout>(Resource.Id.layoutFondo).SetBackgroundColor(Color.Argb(127, 51, 51, 51));
                    raiz.FindViewById<ImageView>(Resource.Id.btnOpcionesRecordatorio).Alpha = 0f;
                    ((ViewPager)container).AddView(raiz, 0);
                    return raiz;
                }
            }
            else
            {
                if (_reminder[position].Description.Equals(Resource.String.There_are_no_reminders))
                {
                    var raiz = _layoutInflater.Inflate(Resource.Layout.Layout_NoData, null);

                    raiz.FindViewById<TextView>(Resource.Id.lblNoData).Text = _reminder[position].Description;   ///SetText(Resource.String.No_data);
                    ((ViewPager)container).AddView(raiz, 0);
                    return raiz;
                }
                else
                {
                    var raiz = _layoutInflater.Inflate(Resource.Layout.layout_ContenidoRecordatorios, null);

                    raiz.FindViewById<TextView>(Resource.Id.txtDescripcionReminder).Text = _reminder[position].Description;
                    raiz.FindViewById<TextView>(Resource.Id.lblBegin).Text = _reminder[position].Begin;
                    raiz.FindViewById<TextView>(Resource.Id.lblEnd).Text = _reminder[position].End;
                    raiz.FindViewById<TextView>(Resource.Id.lblNotification).Text = _reminder[position].Notification;
                    raiz.FindViewById<LinearLayout>(Resource.Id.layoutFondo).SetBackgroundColor(Color.Argb(127, 51, 51, 51));
                    raiz.FindViewById<ImageView>(Resource.Id.btnOpcionesRecordatorio).Alpha = 0f;
                    ((ViewPager)container).AddView(raiz, 0);
                    return raiz;
                }
            }

        }

        public override void DestroyItem(View container, int position, Java.Lang.Object @object)
        {
            ((ViewPager)container).RemoveView((View)@object);
        }
    }

    class LoadDataAsync : AsyncTask<string, string, string> 
    {
        ActivityHome homeActivity;
        int position = 0;
        ViewPager _pager;
        bool internet;
        LayoutInflater _layout;
        LinearLayout linear;
        TabLayout _tabLayoutRss;

        public LoadDataAsync(ActivityHome homeActivity, TabLayout tabLayout, ViewPager pager, bool internet, LayoutInflater layout, LinearLayout linear)
        {
            this.homeActivity = homeActivity;
            _pager = pager;
            this.internet = internet;
            _layout = layout;
            this.linear = linear;
            _tabLayoutRss = tabLayout;
        }

        protected override void OnPreExecute()
        { }

        protected override string RunInBackground(params string[] @params)
        {
            string result = new HTTPDataHandler().GetHTTPData(@params[0]);
            return result;
        }

        protected override void OnPostExecute(string result)
        {
            try
            {
                if (internet)
                {
                    //PagerChuy
                    RssObject data = JsonConvert.DeserializeObject<RssObject>(result);
                    
                    if (data.items.Count > 1)
                    {
                        data.items.Insert(0, data.items[data.items.Count - 1]);
                        data.items.Add(data.items[1]);
                    }

                    AdaptadorContentsHome adapter = new AdaptadorContentsHome(homeActivity, data.items, null, _layout, 0);
                    _pager.Adapter = adapter;
                    _tabLayoutRss.SetupWithViewPager(_pager, true);

                    if (_pager.Adapter.Count > 1)
                    {
                        var contador = 0;
                        var valor = _pager.Adapter.Count - 2;
                        ((ViewGroup)_tabLayoutRss.GetChildAt(0)).GetChildAt(0).Visibility = ViewStates.Invisible;
                        _tabLayoutRss.RemoveTabAt(valor);

                        _pager.PageSelected += delegate
                        {
                            contador = _pager.CurrentItem;
                        };

                        _pager.PageScrolled += delegate
                        {
                            if (contador == 0)
                            {
                                var bandera = true;

                                Timer t = new Timer();
                                t.Interval = 400;
                                t.Enabled = true;
                                t.Elapsed += (s, e) => {
                                    homeActivity.RunOnUiThread(() =>
                                    {
                                        bandera = false;

                                        if (!bandera)
                                        {
                                            _pager.SetCurrentItem(_pager.Adapter.Count - 2, false);
                                            t.Stop();
                                        }
                                    });
                                };
                                t.Start();
                            }

                            if (contador == _pager.Adapter.Count - 1)
                            {
                                Timer t = new Timer();
                                t.Interval = 400;
                                t.Enabled = true;
                                t.Elapsed += (s, e) =>
                                {
                                    var bandera = true;
                                    homeActivity.RunOnUiThread(() =>
                                    {
                                        bandera = false;
                                        if (!bandera)
                                        {
                                            _pager.SetCurrentItem(1, false);
                                            t.Stop();
                                        }
                                    });
                                };
                                t.Start();
                            }

                        };

                        _pager.SetCurrentItem(1, false);
                    }

                }
                else
                {
                    var rowhottips = homeActivity.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                    rowhottips.FindViewById<LinearLayout>(Resource.Id.lnBack).SetBackgroundColor(Color.Argb(0, 0, 0, 0));
                    var txtNoData = rowhottips.FindViewById<TextView>(Resource.Id.lblNoData).Text = homeActivity.GetText(Resource.String.You_need_internet_to_visualize_the_HOT);
                    linear.AddView(rowhottips);
                }
            }
            catch (System.Exception)
            {
                var rowhottips = homeActivity.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                var txtNoData = rowhottips.FindViewById<TextView>(Resource.Id.lblNoData).Text = homeActivity.GetText(Resource.String.You_need_internet_to_visualize_the_HOT);
                linear.AddView(rowhottips);
            }
        }
    }

    public class Reminders
    {
        public string Description { get; set; }
        public string Begin { get; set; }
        public string End { get; set; }
        public string Notification { get; set; }
    }

    public class NumeroNoti
    {
        public int numero { get; set; }
    }
}