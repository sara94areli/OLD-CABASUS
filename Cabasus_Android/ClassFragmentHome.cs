using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Cabasus_Android.Screens;
using Newtonsoft.Json;
using Plugin.Connectivity;
using SQLite;

namespace Cabasus_Android
{
    public class ClassFragmentHome : Fragment
    {
        LinearLayout ContentToday, ContentTomorrow;
        ViewPager pagerContentToday, pagerContentTomorrow;
        TabLayout tabLayoutToday, tabLayoutTomorrow;
        LayoutInflater _LayoutInflater;

        //Dieta
        TextView txtEnergyDemandTotal, txtHayDemand, txtHayEnergy, txtEnergyFeed, txtPNM, txtPCM, txtPCP, txtPPM, txtPPP, txtSinDatos;
        //letrero blanco si no hay dato
        TextView txtDemandLetreto, txtHayDemandLetreto, txtHayEnergyLetreto, txtEnergyFeedLetreto, txtFeedingLetreto;
        TextView lblPPP, lblPPM, lblPCP, lblPCM, lblPNM;

        LinearLayout lnUno, lnDos, lnTres;
        public ClassFragmentHome(LayoutInflater _layoutInflater)
        {
            this._LayoutInflater = _layoutInflater;
        }

        public override async void OnActivityCreated(Bundle savedInstanceState)
        {
            try
            {
                base.OnActivityCreated(savedInstanceState);

                ContentToday = View.FindViewById<LinearLayout>(Resource.Id.ContentToday);
                pagerContentToday = View.FindViewById<ViewPager>(Resource.Id.pagerContentToday);
                tabLayoutToday = View.FindViewById<TabLayout>(Resource.Id.tabLayoutToday);

                ContentTomorrow = View.FindViewById<LinearLayout>(Resource.Id.ContentTomorrow);
                pagerContentTomorrow = View.FindViewById<ViewPager>(Resource.Id.pagerContentTomorrow);
                tabLayoutTomorrow = View.FindViewById<TabLayout>(Resource.Id.tabLayoutTomorrow);

                List<DatosReminder> ListaToday = await FiltroToday();
                List<DatosReminder> ListaTomorrow = await FiltroTomorrow();

                AdaptadorContents adapter = new AdaptadorContents(ListaToday, _LayoutInflater, 1);
                pagerContentToday.Adapter = adapter;
                tabLayoutToday.SetupWithViewPager(pagerContentToday, true);


                adapter = null;
                adapter = new AdaptadorContents(ListaTomorrow, _LayoutInflater, 2);
                pagerContentTomorrow.Adapter = adapter;
                tabLayoutTomorrow.SetupWithViewPager(pagerContentTomorrow, true);

                txtEnergyDemandTotal = View.FindViewById<TextView>(Resource.Id.txtEnergyDemandTotal);
                txtHayDemand = View.FindViewById<TextView>(Resource.Id.txtHayDemand);
                txtHayEnergy = View.FindViewById<TextView>(Resource.Id.txtHayEnergy);
                txtEnergyFeed = View.FindViewById<TextView>(Resource.Id.txtEnergyFeed);
                txtPNM = View.FindViewById<TextView>(Resource.Id.txtPNM);
                txtPCP = View.FindViewById<TextView>(Resource.Id.txtPCP);
                txtPPM = View.FindViewById<TextView>(Resource.Id.txtPPM);
                txtPPP = View.FindViewById<TextView>(Resource.Id.txtPPP);
                txtPCM = View.FindViewById<TextView>(Resource.Id.txtPCM);
                txtSinDatos = View.FindViewById<TextView>(Resource.Id.txtSinDatos);

                txtDemandLetreto = View.FindViewById<TextView>(Resource.Id.txtDemandLetreto);
                txtHayDemandLetreto = View.FindViewById<TextView>(Resource.Id.txtHayDemandLetreto);
                txtHayEnergyLetreto = View.FindViewById<TextView>(Resource.Id.txtHayEnergyLetreto);
                txtEnergyFeedLetreto = View.FindViewById<TextView>(Resource.Id.txtEnergyFeedLetreto);
                txtFeedingLetreto = View.FindViewById<TextView>(Resource.Id.txtFeedingLetreto);

                lblPPP = View.FindViewById<TextView>(Resource.Id.lblPPP);
                lblPPM = View.FindViewById<TextView>(Resource.Id.lblPPM);
                lblPCP = View.FindViewById<TextView>(Resource.Id.lblPCP);
                lblPCM = View.FindViewById<TextView>(Resource.Id.lblPCM);
                lblPNM = View.FindViewById<TextView>(Resource.Id.lblPNM);

                lnUno = View.FindViewById<LinearLayout>(Resource.Id.lnlLineUno);
                lnDos = View.FindViewById<LinearLayout>(Resource.Id.lnlLineDos);
                lnTres = View.FindViewById<LinearLayout>(Resource.Id.lnlLineTres);

                HorseFood();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<List<DatosReminder>> FiltroTomorrow()
        {
            List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
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
                        #region Recordatorios nube por fecha
                        foreach (var item in data)
                        {
                            if (item.date.Substring(0, 10) == DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") && item.horse == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected && item.owner == new ShareInside().ConsultarDatosUsuario()[0].id)
                            {
                                var datos = new DatosReminder();
                                datos.Description = item.description;
                                datos.Begin = item.date.Substring(0,10) + " " + item.date.Substring(11, 8);
                                datos.End = item.end_date.Substring(0, 10) + " " + item.end_date.Substring(11, 8);
                                datos.Notification = item.alert_before.ToString();
                                datos.id_reminder = item.id;
                                datos.Tipo = item.type.ToString();
                                datos.fk_usuario = item.owner;
                                datos.fk_caballo = item.horse;
                                ListaRecordatorios.Add(datos);
                            }
                        }
                        #endregion

                        ListaRecordatorios = await FiltroLocalTomorrow(ListaRecordatorios);
                    }
                    else
                    {
                        ListaRecordatorios = await FiltroLocalTomorrow(ListaRecordatorios);
                    }
                }
                else
                {
                    ListaRecordatorios = await FiltroLocalTomorrow(ListaRecordatorios);
                }
            }
            else
            {
                ListaRecordatorios = await FiltroLocalTomorrow(ListaRecordatorios);
            }
            return ListaRecordatorios;
        }
        public async Task<List<DatosReminder>> FiltroLocalTomorrow(List<DatosReminder> ListaRecordatorios)
        {
            #region Recordatorios local por fecha
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
            var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fecha = '" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "';", new RedordatoriosLocales().id_reminder);

            if (ConsultaRecordatorios.Count > 0)
            {
                foreach (var item in ConsultaRecordatorios)
                {
                    var datos = new DatosReminder();
                    datos.Description = item.descripcion;
                    datos.Begin = item.inicio.Substring(0, 10) + " " + item.inicio.Substring(11, 8);
                    datos.End = item.fin.Substring(0, 10) + " " + item.fin.Substring(11, 8);
                    datos.Notification = item.notificacion;
                    datos.id_reminder = item.id_reminder.ToString();
                    datos.Tipo = item.Tipo;
                    datos.fk_usuario = item.fk_Usuario;
                    datos.fk_caballo = item.fk_caballo;
                    datos.fecha = item.fecha;
                    ListaRecordatorios.Add(datos);
                }
            }
            #endregion

            #region Sin recordatorios de la nube ni locales de la fecha seleccionada
            if (ListaRecordatorios.Count == 0)
            {
                var datos = new DatosReminder();
                datos.Description = GetText(Resource.String.There_are_no_reminders);
                ListaRecordatorios.Add(datos);
            }
            #endregion
            return ListaRecordatorios;
        }

        public async Task<List<DatosReminder>> FiltroLocal(List<DatosReminder> ListaRecordatorios)
        {
            #region Recordatorios local por fecha
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
            var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fecha = '" + DateTime.Now.ToString("yyyy-MM-dd") + "';", new RedordatoriosLocales().id_reminder);

            if (ConsultaRecordatorios.Count > 0)
            {
                foreach (var item in ConsultaRecordatorios)
                {
                    var datos = new DatosReminder();
                    datos.Description = item.descripcion;
                    datos.Begin = item.inicio.Substring(0, 10) + " " + item.inicio.Substring(11, 8);
                    datos.End = item.fin.Substring(0, 10) + " " + item.fin.Substring(11, 8);
                    datos.Notification = item.notificacion;
                    datos.id_reminder = item.id_reminder.ToString();
                    datos.Tipo = item.Tipo;
                    datos.fk_usuario = item.fk_Usuario;
                    datos.fk_caballo = item.fk_caballo;
                    datos.fecha = item.fecha;
                    ListaRecordatorios.Add(datos);
                }
            }
            #endregion

            #region Sin recordatorios de la nube ni locales de la fecha seleccionada
            if (ListaRecordatorios.Count == 0)
            {
                var datos = new DatosReminder();
                datos.Description = GetText(Resource.String.There_are_no_reminders);
                ListaRecordatorios.Add(datos);
            }
            #endregion
            return ListaRecordatorios;
        }

        public async Task<List<DatosReminder>> FiltroToday()
        {
            List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
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
                        #region Recordatorios nube por fecha
                        foreach (var item in data)
                        {
                            if (item.date.Substring(0, 10) == DateTime.Now.ToString("yyyy-MM-dd") && item.horse == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected && item.owner == new ShareInside().ConsultarDatosUsuario()[0].id)
                            {
                                var datos = new DatosReminder();
                                datos.Description = item.description;
                                datos.Begin = item.date.Substring(0, 10) + " " + item.date.Substring(11, 8);
                                datos.End = item.end_date.Substring(0, 10) + " " + item.end_date.Substring(11, 8);
                                datos.Notification = item.alert_before.ToString();
                                datos.id_reminder = item.id;
                                datos.Tipo = item.type.ToString();
                                datos.fk_usuario = item.owner;
                                datos.fk_caballo = item.horse;
                                ListaRecordatorios.Add(datos);
                            }
                        }
                        #endregion

                        ListaRecordatorios = await FiltroLocal(ListaRecordatorios);
                    }
                    else
                    {
                        ListaRecordatorios = await FiltroLocal(ListaRecordatorios);
                    }
                }
                else
                {
                    ListaRecordatorios = await FiltroLocal(ListaRecordatorios);
                }
            }
            else
            {
                ListaRecordatorios = await FiltroLocal(ListaRecordatorios);
            }
            return ListaRecordatorios;
        }

        public async void HorseFood()
        {
            var TipoRaza = "";
            var PesoHorse = 1.1;
            var tiempoTrote = 0;
            var tiempoCaminata = 0;
            var tiempoGalope = 0;
            var FactorFitness = "";
            var avena = 1.0;

            var idcaballo = new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected;

            try
            {
                var conn = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                var consul = conn.Query<ConsumoAvena>("select * from ConsumoAvena where id_caballo='" + idcaballo + "'");

                avena = consul[0].kilogramos;

                if (avena <= 0)
                    return;
            }
            catch (Exception)
            {
                avena = 0;
                txtSinDatos.SetText(Resource.String.CampoAvena);
                
                return;
            }

            #region Consultar Datos Caballo
            try
            {
                var url = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + idcaballo;
                HttpClient cliente = new HttpClient();
                if (CrossConnectivity.Current.IsConnected)
                {
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                    var clientePost = await cliente.GetAsync(url);
                    clientePost.EnsureSuccessStatusCode();

                    if (clientePost.IsSuccessStatusCode)
                    {
                        JsonValue ObjJson = await clientePost.Content.ReadAsStringAsync();
                        var datos = JsonConvert.DeserializeObject<InHorse>(ObjJson);
                        #region Obtener tipo de raza
                        var con1 = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var consulta1 = con1.Query<Razas>("Select tipo from razas where Id_Raza=" + datos.breed + "");
                        TipoRaza = consulta1[0].tipo;
                        PesoHorse = datos.weight;

                        if (PesoHorse <= 0)
                            return;

                        #endregion
                    }

                }

            }
            catch (Exception)
            {
                //Llena el campo del peso del caballo o raza
                //Toast.MakeText(this, Resource.String.The_information, ToastLength.Short).Show();
                txtSinDatos.SetText(Resource.String.CampoPesoCaballo);

              

                return;
            }
            #endregion

            #region Consultar actividades

            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
            var consulta = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo desc order by ID_ActividadLocal limit 1");
            if (consulta.Count > 0)
            {
                var tiempototal = consulta[0].Duration;
                var normal = consulta[0].Normal;
                var strong = consulta[0].Strong;
                var slow = consulta[0].Slow;

                var totaldistancias = normal + strong + slow;
                //Toast.MakeText(this, "TiempoTotal: " + tiempototal + " DistanciaTotal: " + totaldistancias, ToastLength.Long).Show();
                //porcentajes;
                normal = (normal * 100) / totaldistancias;
                strong = (strong * 100) / totaldistancias;
                slow = (slow * 100) / totaldistancias;

                //Toast.MakeText(this, "Porcentajes Distancia: " + (normal + strong + slow), ToastLength.Long).Show();
                normal = ((normal * tiempototal) / 100) / 60;
                strong = ((strong * tiempototal) / 100) / 60;
                slow = ((slow * tiempototal) / 100) / 60;

                //Toast.MakeText(this, "Tiemposreal: " + tiempototal + "Tiempos porcentajes: " + (normal + strong + slow), ToastLength.Long).Show();
            }
            else
            {
                txtSinDatos.SetText(Resource.String.do_activity);
                return;
            }

            #endregion

            #region Formula Energia demandada

            txtDemandLetreto.SetText(Resource.String.Demand);
            txtSinDatos.SetText(Resource.String.Hay);
            txtHayDemandLetreto.SetText(Resource.String.HayDemand);
            txtHayEnergyLetreto.SetText(Resource.String.HayEnergy);
            txtEnergyFeedLetreto.SetText(Resource.String.EnergyFeed);
            txtFeedingLetreto.SetText(Resource.String.Feeding);
            lblPNM.SetText(Resource.String.PNM);
            lblPCM.SetText(Resource.String.PCM);
            lblPCP.SetText(Resource.String.PCP);
            lblPPM.SetText(Resource.String.PPM);
            lblPPP.SetText(Resource.String.PPP);

            lnUno.SetBackgroundResource(Resource.Color.cDorado);
            lnDos.SetBackgroundResource(Resource.Color.cDorado);
            lnTres.SetBackgroundResource(Resource.Color.cDorado);

            var Energy_Demand_Maintenance = 0.0;
            var Energy_Demand_Training = 0.0;
            var Energy_Demand_Subtotal = 0.0;
            var The_Fitness_Factor = 0.0;
            var Energy_Demand_Total = 0.0;
            var Hay_Energy = 0.0;
            var Hay_Demand = 0.0;
            var Oat_Energ = 0.0;
            var Energy_Demand_Concentrated_Feed = 0.0;

            var Pegus_Natural_Musli = 8.1;
            var Pegus_Classic_Musli = 11.1;
            var Pegus_Classic_Pellet = 11.0;
            var Pegus_Power_Musli = 10.6;
            var Pegus_Power_Pellet = 11.0;

            var metabolic = Math.Pow(PesoHorse, 0.75);
            switch (TipoRaza)
            {
                case "warmblood":
                    Energy_Demand_Maintenance = metabolic * 0.52;
                    Energy_Demand_Training = ((tiempoCaminata * 0.19) + (tiempoTrote * 0.48) + (tiempoGalope * 1.92)) * (metabolic / 1000);
                    Energy_Demand_Subtotal = Energy_Demand_Maintenance + Energy_Demand_Training;

                    break;
                case "horoughbred":
                    Energy_Demand_Maintenance = metabolic * 0.64;
                    Energy_Demand_Training = ((tiempoCaminata * 0.19) + (tiempoTrote * 0.48) + (tiempoGalope * 1.92)) * (metabolic / 1000);
                    Energy_Demand_Subtotal = Energy_Demand_Maintenance + Energy_Demand_Training;

                    break;
                case "draft horse":
                    Energy_Demand_Maintenance = metabolic * 0.45;
                    Energy_Demand_Training = ((tiempoCaminata * 0.19) + (tiempoTrote * 0.48) + (tiempoGalope * 1.92)) * (metabolic / 1000);
                    Energy_Demand_Subtotal = Energy_Demand_Maintenance + Energy_Demand_Training;

                    break;
                case "pony":
                    Energy_Demand_Maintenance = metabolic * 0.40;
                    Energy_Demand_Training = ((tiempoCaminata * 0.8) + (tiempoTrote * 2) + (tiempoGalope * 8.1)) * (metabolic / 1000);
                    Energy_Demand_Subtotal = Energy_Demand_Maintenance + Energy_Demand_Training;

                    break;
            }
            switch (FactorFitness)
            {
                case "Very Well Trained":
                    The_Fitness_Factor = Energy_Demand_Subtotal * 1.11;
                    break;
                case "Well Trained":
                    The_Fitness_Factor = Energy_Demand_Subtotal * 1.055;
                    break;
                case "Normal":
                    The_Fitness_Factor = Energy_Demand_Subtotal * 1;
                    break;
                case "Underweight":
                    The_Fitness_Factor = Energy_Demand_Subtotal * 1.1;
                    break;
                case "Overweight":
                    The_Fitness_Factor = Energy_Demand_Subtotal * 0.91;
                    break;
            }

            Energy_Demand_Total = Energy_Demand_Subtotal * The_Fitness_Factor;
            Hay_Demand = 1.5 * (PesoHorse / 100);
            Hay_Energy = Hay_Demand * 6.2;
            Oat_Energ = avena * 11.1;
            Energy_Demand_Concentrated_Feed = Energy_Demand_Total - Hay_Energy - Oat_Energ;

            txtEnergyDemandTotal.Text = Energy_Demand_Total.ToString();
            txtHayDemand.Text = Hay_Demand.ToString();
            txtHayEnergy.Text = Hay_Energy.ToString();
            txtEnergyFeed.Text = Energy_Demand_Concentrated_Feed.ToString();
            

            if (Energy_Demand_Concentrated_Feed > 0 && Energy_Demand_Concentrated_Feed <= 13)
            {
                Pegus_Natural_Musli = Energy_Demand_Concentrated_Feed / Pegus_Natural_Musli;
                txtPNM.Text = Math.Round(Pegus_Natural_Musli, 2) + " Kg"; //Energy_Demand_Concentrated_Feed/MJ_Pegus_Natural_Müsli,
                txtPCM.Text = "0.0 Kg";
                txtPCP.Text = "0.0 Kg";
                txtPPM.Text = "0.0 Kg";
                txtPPP.Text = "0.0 Kg";
            }
            if (Energy_Demand_Concentrated_Feed > 13 && Energy_Demand_Concentrated_Feed <= 20)
            {
                Pegus_Classic_Musli = Energy_Demand_Concentrated_Feed / Pegus_Classic_Musli;
                Pegus_Classic_Pellet = Energy_Demand_Concentrated_Feed / Pegus_Classic_Pellet;

                txtPNM.Text = "0.0 Kg";
                txtPCM.Text = Math.Round(Pegus_Classic_Musli, 2) + " Kg"; //Energy_Demand_Concentrated_Feed/MJ_Pegus_Classic_Müsli
                txtPCP.Text = Math.Round(Pegus_Classic_Pellet, 2) + " Kg"; //Energy_Demand_Concentrated_Feed/MJ_Pegus_Classic_Pellet
                txtPPM.Text = "0.0 Kg";
                txtPPP.Text = "0.0 Kg";
            }
            if (Energy_Demand_Concentrated_Feed > 20)
            {
                Pegus_Power_Musli = Energy_Demand_Concentrated_Feed / Pegus_Power_Musli;
                Pegus_Power_Pellet = Energy_Demand_Concentrated_Feed / Pegus_Power_Pellet;

                txtPNM.Text = "0.0 Kg";
                txtPCM.Text = "0.0 Kg";
                txtPCP.Text = "0.0 Kg";
                txtPPM.Text = Math.Round(Pegus_Power_Musli, 2) + " Kg"; //Energy_Demand_Concentrated_Feed/MJ_Pegus_Power_Müsli
                txtPPP.Text = Math.Round(Pegus_Power_Pellet, 2) + " Kg"; //Energy_Demand_Concentrated_Feed/MJ_Pegus_Power_Pellet
            }
            #endregion
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.Layout_FragmentHome, container, false);
        }
    }
}