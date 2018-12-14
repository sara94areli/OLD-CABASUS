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
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cabasus_Android.Screens;
using Newtonsoft.Json;
using Plugin.Connectivity;
using SQLite;

namespace Cabasus_Android
{
    class AdaptadorCaballos : BaseAdapter<HorsesCloud>
    {
        List<HorsesCloud> items;
        Activity context;
        ProgressBar progressBar;

        public AdaptadorCaballos(Activity context, List<HorsesCloud> items) : base()
        {
            this.context = context;
            this.items = items;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override HorsesCloud this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
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

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;

            if (item.name == "HC5321876515844562" && item.id == "HC5321876515844562" && item.owner == "HC5321876515844562" && item.owner_name == "HC5321876515844562" && item.photo == "HC5321876515844562")
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.RowCloud_Local, null);
                return view;
            }
            else if (item.name == "HL5321876515844562" && item.id == "HL5321876515844562" && item.owner == "HL5321876515844562" && item.owner_name == "HL5321876515844562" && item.photo == "HL5321876515844562")
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.RowCloud_Local, null);
                view.FindViewById<TextView>(Resource.Id.lbltitulo).Text = context.GetText(Resource.String.Horses_on_my_phone).ToString();
                view.FindViewById<ImageView>(Resource.Id.img).SetImageResource(Resource.Drawable.SettingsTelefono);
                return view;
            }
            else if (item.name == "NH5321876515844562" && item.id == "NH5321876515844562" && item.owner == "NH5321876515844562" && item.owner_name == "NH5321876515844562" && item.photo == "NH5321876515844562")
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.RowNotHorses, null);
                return view;
            }
            else
            {
                if (item.Sync == 0 || item.Sync == 1)
                {
                    item.Sync = 0;
                    view = context.LayoutInflater.Inflate(Resource.Layout.RowHorsesCloud, null);

                    if (item.Sync == 0)
                    {
                        view.FindViewById<ImageView>(Resource.Id.btnDownload).Alpha = 0;
                        view.FindViewById<ImageView>(Resource.Id.btnDownload).Enabled = false;
                    }

                    view.FindViewById<ImageView>(Resource.Id.btnDownload).SetImageResource(Resource.Drawable.SettingsSync);
                    view.FindViewById<ImageView>(Resource.Id.btnOpcion).SetImageResource(Resource.Drawable.SettingsDeleteLocalHorse);
                    
                    view.FindViewById<ImageView>(Resource.Id.btnOpcion).Click += delegate
                    {

                        Dialog alertar = new Dialog(context, Resource.Style.Theme_Dialog_Translucent);
                        alertar.RequestWindowFeature(1);
                        alertar.SetCancelable(false);
                        alertar.SetContentView(Resource.Layout.layout_CustomAlertOpcionesCaballoNoPropio);
                        alertar.FindViewById<TextView>(Resource.Id.txttextotitulo).Text = context.GetText(Resource.String.Do_you_want_to_eliminate_this_horse).ToString();
                        alertar.FindViewById<TextView>(Resource.Id.txtdescrpcion).Text = context.GetText(Resource.String.If_you_eliminate_this_horse).ToString();
                            alertar.Show();

                        alertar.FindViewById<TextView>(Resource.Id.btnno).Click += delegate
                        {
                            alertar.Dismiss();
                        };

                        alertar.FindViewById<TextView>(Resource.Id.btnsi).Click += async delegate
                        {
                            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                            try
                            {
                                #region Eliminar de base de datos local

                                try
                                {
                                    con.Query<Horses>("delete from Horses where id='" + item.id + "'");
                                    con.Query<Horses>("delete  from DiariosNube where fk_usuario='" + item.id + "'");
                                    con.Query<Horses>("delete  from diarys where fk_caballo='" + item.id + "'");
                                    con.Query<Horses>("delete  from reminders where fk_caballo='" + item.id + "'");

                                    var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                                    con2.Query<Horses>("delete  from ActividadesCloudMes where ID_Caballo='" + item.id + "'");
                                    con2.Query<Horses>("delete  from ActividadesCloud where ID_Caballo='" + item.id + "'");
                                    con2.Query<Horses>("delete  from ActividadPorCaballo where ID_Caballo='" + item.id + "'");
                                }
                                catch (Exception)
                                {
                                }

                                #endregion
                                items.Remove(items[position]);
                                var consultaeliminar = con.Query<Horse>("select * from Horses", item.id);
                                if (consultaeliminar.Count == 0)
                                {
                                    items.Insert(1, new HorsesCloud()
                                    {
                                        id = "NH5321876515844562",
                                        name = "NH5321876515844562",
                                        owner = "NH5321876515844562",
                                        owner_name = "NH5321876515844562",
                                        photo = "NH5321876515844562"
                                    });
                                }
                                this.NotifyDataSetChanged();

                            }
                            catch (Exception ex)
                            {
                                Toast.MakeText(context, Resource.String.The_information, ToastLength.Short).Show();
                            }
                            alertar.Dismiss();
                        };
                    };
                }
                else
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.RowHorsesCloud, null);

                    if (item.owner != new ShareInside().ConsultarDatosUsuario()[0].id)
                    {
                        view.FindViewById<ImageView>(Resource.Id.btnOpcion).SetImageResource(Resource.Drawable.PuntosRed);
                        view.FindViewById<ImageView>(Resource.Id.btnOpcion).Click += delegate
                        {
                            Dialog alertar = new Dialog(context, Resource.Style.Theme_Dialog_Translucent);
                            alertar.RequestWindowFeature(1);
                            alertar.SetCancelable(false);
                            alertar.SetContentView(Resource.Layout.layout_CustomAlertOpcionesCaballoNoPropio);
                            alertar.Show();

                            alertar.FindViewById<TextView>(Resource.Id.btnno).Click += delegate
                            {
                                alertar.Dismiss();
                            };

                            alertar.FindViewById<TextView>(Resource.Id.btnsi).Click += async delegate
                            {
                                try
                                {
                                    string servidor = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + item.id + "/unshare";
                                    var uri = new Uri(string.Format(servidor, string.Empty));
                                    string json2 = "application/json";

                                    HttpClient cliente2 = new HttpClient();
                                    cliente2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                                    progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                    RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                                    p.AddRule(LayoutRules.CenterInParent);
                                    progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                                    alertar.FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);
                                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                                    alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                    var clientePost2 = await cliente2.PutAsync(uri, new StringContent("", Encoding.UTF8, json2));
                                    clientePost2.EnsureSuccessStatusCode();

                                    if (clientePost2.IsSuccessStatusCode)
                                    {
                                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));

                                        #region Eliminar de base de datos local
                                        try
                                        {
                                            con.Query<Horses>("delete from Horses where id='" + item.id + "'");
                                            con.Query<Horses>("delete  from DiariosNube where fk_usuario='" + item.id + "'");
                                            con.Query<Horses>("delete  from diarys where fk_caballo='" + item.id + "'");
                                            con.Query<Horses>("delete  from reminders where fk_caballo='" + item.id + "'");

                                            var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                                            con2.Query<Horses>("delete  from ActividadesCloudMes where ID_Caballo='" + item.id + "'");
                                            con2.Query<Horses>("delete  from ActividadesCloud where ID_Caballo='" + item.id + "'");
                                            con2.Query<Horses>("delete  from ActividadPorCaballo where ID_Caballo='" + item.id + "'");
                                        }
                                        catch (Exception)
                                        {
                                        }

                                        #endregion

                                        items.RemoveAll(x => x.id == item.id.ToString());
                                        await new ShareInside().GuardadoFotosYConsulta();
                                        if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml")))
                                        {
                                            var consultaeliminar = con.Query<Horse>("select * from Horses", item.id);
                                            List<HorsesCloud> lista =items.Where(x=> x.id== "NH5321876515844562").ToList();
                                            if (lista.Count>0)
                                            {

                                            }
                                            else if (consultaeliminar.Count == 0) 
                                            {
                                                items.Insert(1, new HorsesCloud()
                                                {
                                                    id = "NH5321876515844562",
                                                    name = "NH5321876515844562",
                                                    owner = "NH5321876515844562",
                                                    owner_name = "NH5321876515844562",
                                                    photo = "NH5321876515844562"
                                                });
                                            }
                                            this.NotifyDataSetChanged();
                                            alertar.Dismiss();
                                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                            alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                        }
                                        else
                                        {
                                            context.StartActivity(typeof(ActivitySettings));
                                            context.Finish();
                                        }
                                    }
                                    else
                                    {
                                        Toast.MakeText(context, Resource.String.The_information, ToastLength.Short).Show();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Toast.MakeText(context,Resource.String.The_information, ToastLength.Short).Show();
                                    alertar.Dismiss();

                                }
                            };
                        };
                    }
                    else
                    {
                        view.FindViewById<ImageView>(Resource.Id.btnOpcion).Click += delegate
                        {
                            float aparicion = 0.0f;
                            int crecerlinea = 0;
                            Dialog alertar = new Dialog(context, Resource.Style.Theme_Dialog_Translucent);
                            alertar.RequestWindowFeature(1);
                            alertar.SetCancelable(true);
                            alertar.SetContentView(Resource.Layout.layout_CustomAlertOpcionesCaballo);
                            alertar.Show();

                            alertar.FindViewById<TextView>(Resource.Id.btnManage).Click += delegate
                            {
                                alertar.Dismiss();
                                Intent intent = new Intent(context, (typeof(ActivityShares)));
                               // List<string> s = new List<string>();
                                //foreach (var lol in item.shares)
                                //{
                                //    s.Add(lol.user);
                                //}
                                intent.PutExtra("idhorse", item.id);
                                intent.PutExtra("nombre", item.name);
                                intent.PutExtra("foto", item.photo);

                               // intent.PutStringArrayListExtra("share", s);
                                context.StartActivity(intent);
                                context.Finish();
                            };
                            alertar.FindViewById<TextView>(Resource.Id.btnEdit).Click += delegate
                            {
                                alertar.Dismiss();
                                Intent intent = new Intent(context, (typeof(ActivityRegistroCaballos)));
                                intent.PutExtra("idhorse", item.id);
                                intent.PutExtra("opcion", "2");
                                context.StartActivity(intent);
                                context.Finish();
                            };
                            alertar.FindViewById<TextView>(Resource.Id.btnDelete).Click += delegate
                            {
                                Timer t = new Timer();
                                t.Interval = 5;
                                t.Enabled = true;
                                t.Elapsed += (s, e) =>
                                {
                                    context.RunOnUiThread(() =>
                                    {
                                        aparicion += 0.01f;
                                        crecerlinea += 6;
                                        if (aparicion >= 1.0f)
                                        {
                                            t.Stop();
                                        }
                                        alertar.FindViewById<LinearLayout>(Resource.Id.SeccionEliminar).LayoutParameters = new LinearLayout.LayoutParams(-1, 0, aparicion);
                                        alertar.FindViewById<LinearLayout>(Resource.Id.lineasuperior).LayoutParameters = new LinearLayout.LayoutParams(crecerlinea, 2);
                                    });
                                };
                                t.Start();
                            };

                            alertar.FindViewById<TextView>(Resource.Id.btnno).Click += delegate
                            {
                                Timer t = new Timer();
                                t.Interval = 5;
                                t.Enabled = true;
                                t.Elapsed += (s, e) =>
                                {
                                    context.RunOnUiThread(() =>
                                    {
                                        aparicion -= 0.01f;
                                        crecerlinea -= 6;
                                        if (aparicion <= 0.0f)
                                        {
                                            t.Stop();
                                        }
                                        alertar.FindViewById<LinearLayout>(Resource.Id.SeccionEliminar).LayoutParameters = new LinearLayout.LayoutParams(-1, 0, aparicion);
                                        alertar.FindViewById<LinearLayout>(Resource.Id.lineasuperior).LayoutParameters = new LinearLayout.LayoutParams(crecerlinea, 2);
                                    });
                                };
                                t.Start();
                            };

                            alertar.FindViewById<TextView>(Resource.Id.btnsi).Click += async delegate
                            {
                                alertar.SetCancelable(false);
                                var consulta = new List<HorsesCloud>();
                                //Eliminar caballo
                                try
                                {
                                    if (CrossConnectivity.Current.IsConnected)
                                    {
                                        string server2 = "https://cabasus-mobile.azurewebsites.net/v1/horses/";
                                        var uri = new Uri(string.Format(server2, string.Empty));
                                        var tex = new ShareInside().ConsultToken();
                                        HttpClient cliente2 = new HttpClient();
                                        cliente2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                                        progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(200, 200);
                                        p.AddRule(LayoutRules.CenterInParent);
                                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                                        alertar.FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);
                                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                                        alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                        var clientePost2 = await cliente2.DeleteAsync(uri + item.id);
                                      
                                        clientePost2.EnsureSuccessStatusCode();

                                        if (clientePost2.IsSuccessStatusCode)
                                        {
                                            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));

                                            #region Eliminar de base de datos local
                                            try
                                            {
                                                con.Query<Horses>("delete from Horses where id='" + item.id + "'");
                                                con.Query<Horses>("delete  from DiariosNube where fk_usuario='" + item.id + "'");
                                                con.Query<Horses>("delete  from diarys where fk_caballo='" + item.id + "'");
                                                con.Query<Horses>("delete  from reminders where fk_caballo='" + item.id + "'");

                                                var con2 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                                                con2.Query<Horses>("delete  from ActividadesCloudMes where ID_Caballo='" + item.id + "'");
                                                con2.Query<Horses>("delete  from ActividadesCloud where ID_Caballo='" + item.id + "'");
                                                con2.Query<Horses>("delete  from ActividadPorCaballo where ID_Caballo='" + item.id + "'");
                                            }
                                            catch (Exception)
                                            {
                                            }

                                            #endregion

                                            items.RemoveAll(x => x.id == item.id.ToString());
                                            await new ShareInside().GuardadoFotosYConsulta();
                                            if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml")))
                                            {
                                                var consultaeliminar = con.Query<Horse>("select * from Horses", item.id);
                                                List<HorsesCloud> lista = items.Where(x => x.id == "NH5321876515844562").ToList();
                                                if (lista.Count > 0)
                                                {

                                                }
                                                else if (consultaeliminar.Count == 0)
                                                {
                                                    items.Insert(1, new HorsesCloud()
                                                    {
                                                        id = "NH5321876515844562",
                                                        name = "NH5321876515844562",
                                                        owner = "NH5321876515844562",
                                                        owner_name = "NH5321876515844562",
                                                        photo = "NH5321876515844562"
                                                    });
                                                }
                                                this.NotifyDataSetChanged();
                                                alertar.Dismiss();
                                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                            }
                                            else
                                            {
                                                context.StartActivity(typeof(ActivitySettings));
                                                context.Finish();
                                            }
                                        }
                                        else
                                        {
                                            Toast.MakeText(context, Resource.String.The_horse_couldnt_be_eliminated, ToastLength.Short).Show();
                                        }
                                    }
                                    else
                                    {
                                        Toast.MakeText(context, Resource.String.You_need_internet, ToastLength.Short).Show();
                                    }
                                    alertar.SetCancelable(true);
                                    alertar.Dismiss();
                                }
                                catch (Exception ex)
                                {
                                    Toast.MakeText(context, Resource.String.You_need_internet, ToastLength.Short).Show();
                                }
                            };
                        };
                        view.FindViewById<ImageView>(Resource.Id.imgCaballo).Click += delegate {

                            Intent intent = new Intent(context, (typeof(ActivityRegistroCaballos)));
                            intent.PutExtra("idhorse", item.id);
                            intent.PutExtra("opcion", "2");
                            context.StartActivity(intent);
                        };
                    }

                    var conexion = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var datos = conexion.Query<Horses>("select id from Horses where id = '" + item.id + "'");

                    if (datos.Count > 0)
                    {
                        var imagen = view.FindViewById<ImageView>(Resource.Id.btnDownload);
                        imagen.Enabled = false;
                        imagen.Alpha = 0;
                    }
                    else
                    {
                        view.FindViewById<ImageView>(Resource.Id.btnDownload).Click += delegate
                        {
                            try
                            {

                                conexion.Insert(new Horses
                                {
                                    id = item.id,
                                    name = item.name,
                                    owner = item.owner,
                                    owner_name = item.owner_name,
                                    photo = item.photo,
                                    Sync = 0
                                });
                                context.StartActivity(typeof(Screens.ActivitySettings));

                            }
                            catch (Exception ex)
                            {
                              //  Toast.MakeText(context, "Ya descargaste este caballo", ToastLength.Short).Show();
                            }
                        };
                    }
                }
            }

            view.FindViewById<TextView>(Resource.Id.lblNombreCaballo).Text = item.name;
            view.FindViewById<TextView>(Resource.Id.lblDuenio).Text = context.GetText(Resource.String.Owner).ToString() + item.owner_name;
            
            byte[] imageAsBytes = Base64.Decode(item.photo, Base64Flags.Default);
            var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);

            view.FindViewById<ImageView>(Resource.Id.imgCaballo).SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));

            return view;
        }
        
        public bool ConsultarLista()
        {

            var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
            var Datos = JsonConvert.DeserializeObject<List<HorsesComplete>>(((IdNameHorse)(new XmlSerializer(typeof(IdNameHorse))).Deserialize(Lectura)).DatosCaballo);
            Lectura.Close();
            if (Datos.Count == 0)
            {
                System.IO.File.Delete(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                return true;
            }
            else
                return false;
        }
    }
    public class Owner
    {
        public string id { get; set; }
    }
    public class Imagenes
    {
        public Owner owner { get; set; }
        public string name { get; set; }
        public string data { get; set; }
        public string content_type { get; set; }
    }
    public class Data
    {
        public string type { get; set; }
        public List<int> data { get; set; }
    }
    public class ImagenBeforeAdd
    {
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public Data data { get; set; }
        public string contentType { get; set; }
        public string id { get; set; }
    }
    public class AgregarDiariosNube
    {
        public string horse { get; set; }
        public string content { get; set; }
        public string horse_status { get; set; }
        public string date { get; set; }
        public string[] images { get; set; }
        public string visibility { get; set; }
    }
    public class Horses
    {
        public string id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string owner_name { get; set; }
        public string photo { get; set; }
        public int Sync { get; set; }
    }
    public class AgregarRecordatoriosNube
    {
        public string horse { get; set; }
        public string description { get; set; }
        public string date { get; set; }
        public string end_date { get; set; }
        public int type { get; set; }
        public int alert_before { get; set; }
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
    public class RedordatoriosLocales
    {
        public int id_reminder { get; set; }
        public string fk_Usuario { get; set; }
        public string fk_caballo { get; set; }
        public string descripcion { get; set; }
        public string inicio { get; set; }
        public string fin { get; set; }
        public string notificacion { get; set; }
        public string Tipo { get; set; }
        public string fecha { get; set; }
    }
}