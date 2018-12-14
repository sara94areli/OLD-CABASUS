using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cabasus_Android;
using Cabasus_Android.Screens;
using Newtonsoft.Json;
using Plugin.Connectivity;
using idioma = Java.Util;
using System.Globalization;

namespace Cabasus_Android
{
    class AdaptadorRazas : BaseAdapter<Razas>
    {
        List<Razas> items;
        Activity context;
        Dialog dlg;
        TextView bds;
        public AdaptadorRazas(Activity context, List<Razas> items,Dialog d,TextView bre) : base()
        {
            this.context = context;
            this.items = items;
            dlg = d;
            bds=bre;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Razas this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            
            var item = items[position];
            View view = convertView;
            view = context.LayoutInflater.Inflate(Resource.Layout.ItemRazas, null);
            //if (idio == "es")
            //{
            //    view.FindViewById<TextView>(Resource.Id.lblraza).Text = item.es;
            //    view.FindViewById<LinearLayout>(Resource.Id.linearraza).Click += delegate
            //    {
            //        dlg.Dismiss();
            //        bds.Text = item.es;
            //        bds.Tag = item.Id_Raza;
            //    };
            //}
            //else
            //{
             view.FindViewById<TextView>(Resource.Id.lblraza).Text = item.en;
                view.FindViewById<LinearLayout>(Resource.Id.linearraza).Click += delegate
               {
                    dlg.Dismiss();
                    bds.Text = item.en;
                    bds.Tag = item.Id_Raza;
                };
            //}
            return view;
        }
    }

    class AdaptadorGender : BaseAdapter<Razas>
    {
        List<Razas> items;
        Activity context;
        LayoutInflater li;
        Dialog dlg;
        TextView Gender;
        string idio;

        public AdaptadorGender(Activity context, List<Razas> items, Dialog d, TextView gender,string idio) : base()
        {
            this.context = context;
            this.items = items;
            dlg = d;
            this.idio = idio;
            Gender = gender;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Razas this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            view = context.LayoutInflater.Inflate(Resource.Layout.ItemGender, null);
            //Cambiar idioma cuado cambie el lenguaje
            if (idio == "es")
            {
                view.FindViewById<TextView>(Resource.Id.lblgender).Text = item.es;

                view.FindViewById<LinearLayout>(Resource.Id.leneargender).Click += delegate
                {
                    dlg.Dismiss();
                    Gender.Text = item.es;
                    Gender.Tag = item.id_gender;

                };
            }
            else
            {
                view.FindViewById<TextView>(Resource.Id.lblgender).Text = item.en;

                view.FindViewById<LinearLayout>(Resource.Id.leneargender).Click += delegate
                {
                    dlg.Dismiss();
                    Gender.Text = item.en;
                    Gender.Tag = item.id_gender;

                };
            }

            
            return view;
        }
    }

    class AdaptadorRiders : BaseAdapter<UserPhone>
    {
        string Idhorse, nombrehorse;
        List<UserPhone> items;
        Activity context;
        ProgressBar progressBar;

        public AdaptadorRiders(Activity context, List<UserPhone> items,string id,string nombre) : base()
        {
            this.context = context;
            this.items = items;
            this.Idhorse = id;
            nombrehorse = nombre;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override UserPhone this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (item.id=="123456789")
            {
                
                view = context.LayoutInflater.Inflate(Resource.Layout.RowNotHorses, null);
                view.FindViewById<TextView>(Resource.Id.lbltitulo).Text = context.GetText(Resource.String.You_haven).ToString();
                return view;
            }
            else
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.ItemShare, null);
                view.FindViewById<TextView>(Resource.Id.txtNombrerider).Text = item.username;
                view.FindViewById<TextView>(Resource.Id.txtemailrider).Text = item.email;
                view.FindViewById<TextView>(Resource.Id.txtNombrerider).Text = item.username;
                try
                {
                    byte[] imageAsBytes = Base64.Decode(item.photo.data, Base64Flags.Default);
                    var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);

                    view.FindViewById<ImageView>(Resource.Id.imgrider).SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));

                }
                catch (Exception)
                {

                    view.FindViewById<ImageView>(Resource.Id.imgrider).SetImageResource(Resource.Drawable.Foto);

                }

                view.FindViewById<ImageView>(Resource.Id.imgborrar).Click += delegate
                {

                    Dialog alertar = new Dialog(context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.layout_customAlertDeleteRider);
                    alertar.Show();
                    alertar.FindViewById<TextView>(Resource.Id.btnno).Click += delegate
                    {
                        alertar.Dismiss();
                    };

                    alertar.FindViewById<TextView>(Resource.Id.btnsi).Click += async delegate
                    {
                        try
                        {
                            string servidor = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + Idhorse + "/shares/" + item.id;
                            string json = "application/json";
                            HttpClient cliente = new HttpClient();

                            if (CrossConnectivity.Current.IsConnected)
                            {
                                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                                progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                                p.AddRule(LayoutRules.CenterInParent);
                                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                                alertar.FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);
                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                var resul = await cliente.DeleteAsync(servidor);

                                resul.EnsureSuccessStatusCode();
                                if (resul.IsSuccessStatusCode)
                                {
                                    items.Remove(items[position]);
                                    if (items.Count <= 0)
                                    {
                                        items.Add(new UserPhone() { id = "123456789" });
                                    }
                                    this.NotifyDataSetChanged();
                                    alertar.Dismiss();
                                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                    alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                                }
                                else
                                {
                                    Toast.MakeText(context,Resource.String.You_haven_downloaded, ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                Toast.MakeText(context, Resource.String.You_need_internet, ToastLength.Short).Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(context,Resource.String.The_information, ToastLength.Short).Show();
                        }
                    };

                };
                return view;
            }
        }
    }

    class AdaptadorBuscarRiders : BaseAdapter<UserPhone>
    {
        List<UserPhone> items;
        Activity context;
        string Idhorse,nombrehorse;
        bool BanderaLocal = false;
        ShareInside s = new ShareInside();
        Dialog D;
        ProgressBar progressBar;
        public AdaptadorBuscarRiders(Activity context, List<UserPhone> items,string id,string nombre, Dialog d) : base()
        {
            this.context = context;
            this.items = items;
            this.Idhorse = id;
            nombrehorse = nombre;
            D = d;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override UserPhone this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            view = context.LayoutInflater.Inflate(Resource.Layout.ItemShare, null);

            view.FindViewById<TextView>(Resource.Id.txtNombrerider).Text = item.username;
            view.FindViewById<TextView>(Resource.Id.txtemailrider).Text = item.email;
            try
            {
                byte[] imageAsBytes = Base64.Decode(item.photo.data, Base64Flags.Default);
                var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                view.FindViewById<ImageView>(Resource.Id.imgrider).SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));
            }
            catch (Exception)
            {
                view.FindViewById<ImageView>(Resource.Id.imgrider).SetImageResource(Resource.Drawable.Foto);
            }

            view.FindViewById<ImageView>(Resource.Id.imgborrar).SetImageResource(Resource.Drawable.ResumenMas1);
            view.FindViewById<ImageView>(Resource.Id.imgborrar).Click +=async delegate {
                #region Alerta para enviar notificacion

                Dialog alertar = new Dialog(context, Resource.Style.Theme_Dialog_Translucent);
                alertar.RequestWindowFeature(1);
                alertar.SetCancelable(true);
                alertar.SetContentView(Resource.Layout.layout_customAlertDeleteRider);
                alertar.FindViewById<TextView>(Resource.Id.txtmensaje).Text =context.GetText(Resource.String.Do_you_want_to).ToString()+ " "+item.username+"?";
                alertar.Show();
                alertar.FindViewById<TextView>(Resource.Id.btnno).Click += delegate
                {
                    alertar.Dismiss();
                };

                alertar.FindViewById<TextView>(Resource.Id.btnsi).Click += async delegate
                {
                    try
                    {
                        
                            string Servidor = " https://cabasus-mobile.azurewebsites.net/v1/horses/" + Idhorse + "/share_with/" + item.id;
                            string json = "application/json";
                            HttpClient client = new HttpClient();
                            if (CrossConnectivity.Current.IsConnected)
                            {

                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                            progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                            p.AddRule(LayoutRules.CenterInParent);
                            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                            alertar.FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);
                            progressBar.Visibility = Android.Views.ViewStates.Visible;
                            alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                            var cliput = await client.PostAsync(Servidor, new StringContent("", Encoding.UTF8, json));

                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            cliput.EnsureSuccessStatusCode();
                                if (cliput.IsSuccessStatusCode)
                                {
                                    Toast.MakeText(context, Resource.String.The_horse_has_been, ToastLength.Short).Show();
                                    D.Dismiss();
                                }
                                else
                                {
                                    Toast.MakeText(context, Resource.String.The_horse_couldn, ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                Toast.MakeText(context, Resource.String.You_need_internet, ToastLength.Short).Show();
                            }
                        
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, Resource.String.The_horse_has_already, ToastLength.Short).Show();
                    }
                    alertar.Dismiss();
                };
                #endregion
            };
            
            return view;
        }
    }

    class AdaptadorHorses : BaseAdapter<HorseSearch>
    {
        List<HorseSearch> items;
        Activity context;
        IList<string> Share;
        ShareInside s = new ShareInside();
        Dialog D;
        ProgressBar progressBar;
        public AdaptadorHorses(Activity context, List<HorseSearch> items,  Dialog d) : base()
        {
            this.context = context;
            this.items = items;
            D = d;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override HorseSearch this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            view = context.LayoutInflater.Inflate(Resource.Layout.ItemShare, null);

            view.FindViewById<TextView>(Resource.Id.txtNombrerider).Text = item.name;
            view.FindViewById<TextView>(Resource.Id.txtemailrider).Text = item.owner.username;
            try
            {
                byte[] imageAsBytes = Base64.Decode(item.photo.data, Base64Flags.Default);
                var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                view.FindViewById<ImageView>(Resource.Id.imgrider).SetImageBitmap(new ShareInside().RedondearbitmapImagen(bit, 200));
            }
            catch (Exception)
            {
                view.FindViewById<ImageView>(Resource.Id.imgrider).SetImageResource(Resource.Drawable.Foto);
            }

            view.FindViewById<ImageView>(Resource.Id.imgborrar).SetImageResource(Resource.Drawable.ResumenMas1);
            view.FindViewById<ImageView>(Resource.Id.imgborrar).Click += async delegate {
                try
                {
                    #region Alerta para enviar notificacion

                    Dialog alertar = new Dialog(context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.layout_customAlertDeleteRider);
                    alertar.FindViewById<TextView>(Resource.Id.txtmensaje).Text = context.GetText(Resource.String.Do_you_want).ToString() +" \""+ item.owner.username + "\" " + context.GetText(Resource.String.to_share_this_horse).ToString();
                    alertar.Show();
                    alertar.FindViewById<TextView>(Resource.Id.btnno).Click += delegate
                    {
                        alertar.Dismiss();
                    };

                    alertar.FindViewById<TextView>(Resource.Id.btnsi).Click += async delegate
                    {
                        try
                        {
                            #region Request API Compartir por caballo

                            string servidor = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + item.id + "/shares";
                            string json = "application/json";
                            HttpClient client = new HttpClient();
                            if (CrossConnectivity.Current.IsConnected)
                            {
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(s.ConsultToken());

                                progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                                p.AddRule(LayoutRules.CenterInParent);
                                progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                                alertar.FindViewById<RelativeLayout>(Resource.Id.principal).AddView(progressBar, p);

                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                var cliput = await client.PostAsync(servidor, new StringContent("", Encoding.UTF8, json));

                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                cliput.EnsureSuccessStatusCode();
                                if (cliput.IsSuccessStatusCode)
                                {
                                    Toast.MakeText(context,Resource.String.Notification_sent_to_the_owner, ToastLength.Short).Show();

                                }
                                else
                                {
                                    Toast.MakeText(context, Resource.String.The_user_couldn, ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                Toast.MakeText(context,Resource.String.You_need_internet, ToastLength.Short).Show();
                            }
                            #endregion
                            alertar.Dismiss();
                            D.Dismiss();
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(context, Resource.String.The_notification_has, ToastLength.Short).Show();

                            alertar.Dismiss();
                            D.Dismiss();
                        }
                    };


                    #endregion
                }
                catch (Exception ex)
                {
                    Toast.MakeText(context, Resource.String.The_information, ToastLength.Short).Show();
                }
            };

            return view;
        }
    }

    class AdaptadorNotificaciones : BaseAdapter<Notificaciones>
    {
        List<Notificaciones> items;
        Activity context;
        ProgressBar progressBar;
        string texto;

        public AdaptadorNotificaciones(Activity context, List<Notificaciones> items) : base()
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

        public override Notificaciones this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            var item = items[position];
            View view = convertView;
            
            if (item.id == "123456789")
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.RowNotHorses, null);
                view.FindViewById<TextView>(Resource.Id.lbltitulo).Text = context.GetText(Resource.String.You_dont_have_pending_notifications).ToString();
                return view;
            }
            else
            {
                #region Resta de horas
                DateTime dt = new DateTime();
                
                DateTime fechaactual = DateTime.Now;
                var fecha = Convert.ToDateTime( item.shares[0].updatedAt.Substring(0,10));
                var totaldias = fechaactual - fecha;
                if (totaldias.Days<=0)
                {
                    texto = context.GetText(Resource.String.Today).ToString();
                }
                else if(totaldias.Days>0 && totaldias.Days<=1.1)
                {
                    texto = context.GetText(Resource.String.Yesterday).ToString();
                }
                else 
                {
                    if (new ShareInside().ConsultarIdioma().Idioma == "es")
                    {
                        var arreglotexto = context.GetText(Resource.String.days_ago).ToString().Split('x');
                        texto = arreglotexto[0] +" " + totaldias.Days + " " + arreglotexto[1];
                    }
                    else
                        texto = totaldias.Days + context.GetText(Resource.String.days_ago).ToString();
                }
                
                #endregion
                
                view = context.LayoutInflater.Inflate(Resource.Layout.itemNotificaciones, null);
                view.FindViewById<TextView>(Resource.Id.txtNombredueño).Text = item.shares[0].user.username;
                view.FindViewById<TextView>(Resource.Id.txtmensaje).Text = context.GetText(Resource.String.wants_you_to_share).ToString() + "\"" + item.name + "\"";
                view.FindViewById<TextView>(Resource.Id.txthoras).Text = texto;

                
                view.FindViewById<LinearLayout>(Resource.Id.btneliminar).Click += async delegate
                {

                    try
                    {
                        string Servidor = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + item.id + "/shares/" + item.shares[0].user.id + "/reject";
                        string json = "application/json";
                        HttpClient client = new HttpClient();
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                            progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                            p.AddRule(LayoutRules.CenterInParent);
                            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                            view.FindViewById<RelativeLayout>(Resource.Id.itemnotificacion).AddView(progressBar, p); progressBar.Visibility = Android.Views.ViewStates.Visible;
                            context.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                            var cliput = await client.PostAsync(Servidor, new StringContent("", Encoding.UTF8, json));

                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            context.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            cliput.EnsureSuccessStatusCode();
                            if (cliput.IsSuccessStatusCode)
                            {
                                items.Remove(items[position]);
                                if (items.Count <= 0)
                                {
                                    items.Add(new Notificaciones() { id = "123456789" });
                                }
                                this.NotifyDataSetChanged();
                                Toast.MakeText(context, Resource.String.You_rejected_the_invitation, ToastLength.Short).Show();

                            }
                            else
                            {
                                Toast.MakeText(context, Resource.String.The_invitation_could_not_be_sent, ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(context, Resource.String.You_need_internet, ToastLength.Short).Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context,Resource.String.The_information, ToastLength.Short).Show();
                    }
                };
                view.FindViewById<LinearLayout>(Resource.Id.btnaceptar).Click += async delegate
                {
                    try
                    {
                        string Servidor = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + item.id + "/shares/" + item.shares[0].user.id + "/accept";
                        string json = "application/json";
                        HttpClient client = new HttpClient();
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                            progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                            p.AddRule(LayoutRules.CenterInParent);
                            progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);
                            view.FindViewById<RelativeLayout>(Resource.Id.itemnotificacion).AddView(progressBar, p);
                            progressBar.Visibility = Android.Views.ViewStates.Visible;
                            context.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                            var cliput = await client.PostAsync(Servidor, new StringContent("", Encoding.UTF8, json));

                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            context.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                            cliput.EnsureSuccessStatusCode();
                            if (cliput.IsSuccessStatusCode)
                            {
                                items.Remove(items[position]);
                                if (items.Count <= 0)
                                {
                                    items.Add(new Notificaciones() { id = "123456789" });
                                }
                                this.NotifyDataSetChanged();
                               // Toast.MakeText(context, "Se ha notificado al dueño que has aceptado", ToastLength.Short).Show();
                            }
                            else
                            {
                                Toast.MakeText(context, Resource.String.The_invitation_could_not_be_sent, ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(context, Resource.String.You_need_internet, ToastLength.Short).Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, Resource.String.The_information, ToastLength.Short).Show();
                    }
                };
                return view;
            }
        }
    }

    class AdaptadorLenguaje : BaseAdapter<Lenguaje>
    {
        List<Lenguaje> items;
        Activity context;
        LayoutInflater li;
        Dialog dlg;
        TextView Gender;

        public AdaptadorLenguaje(Activity context, List<Lenguaje> items) : base()
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

        public override Lenguaje this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            view = context.LayoutInflater.Inflate(Resource.Layout.ItemLenguaje, null);
            view.FindViewById<TextView>(Resource.Id.txtxlenguaje).Text = item.idioma;
            view.FindViewById<TextView>(Resource.Id.txtxlenguaje).Click += delegate {
                if (item.idioma == context.GetText(Resource.String.Spanish))
                {
                    new ShareInside().GuardarIdioma("es", "ES");
                    Java.Util.Locale.Default = new idioma.Locale("es", "ES");
                    context.Resources.Configuration.Locale = Java.Util.Locale.Default;
                    context.Resources.UpdateConfiguration(context.Resources.Configuration, context.Resources.DisplayMetrics);
                    context.StartActivity(typeof(ActivityAjustes));
                    context.Finish();
                }
                else if (item.idioma == context.GetText(Resource.String.English))
                {
                    new ShareInside().GuardarIdioma("en", "US");
                    Java.Util.Locale.Default = new idioma.Locale("en", "US");
                    context.Resources.Configuration.Locale = Java.Util.Locale.Default;
                    context.Resources.UpdateConfiguration(context.Resources.Configuration, context.Resources.DisplayMetrics);
                    context.StartActivity(typeof(ActivityAjustes));
                    context.Finish();
                }
                else
                {
                    new ShareInside().GuardarIdioma("de", "GE");
                    Java.Util.Locale.Default = new idioma.Locale("de", "GE");
                    context.Resources.Configuration.Locale = Java.Util.Locale.Default;
                    context.Resources.UpdateConfiguration(context.Resources.Configuration, context.Resources.DisplayMetrics);
                    context.StartActivity(typeof(ActivityAjustes));
                    context.Finish();
                }
            };
            return view;
        }
    }

    public class Lenguaje
    {
        public string idioma { get; set; }
    }

    public class Horse
    {
        public string name { get; set; }
        public List<ShareSettings> shares { get; set; }
    }

    public class Notificaciones
    {
        public string id { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public Photo photo { get; set; }
        public List<shares> shares { get; set; }
    }

    public class shares
    {
        public string updatedAt { get; set; }
        public string _id { get; set; }
        public user user { get; set; }
    }

    public class user
    {
        public string username { get; set; }
        public string id { get; set; }
    }
}