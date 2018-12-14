using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using System;
using SQLite;
using System.Collections.Generic;
using Android.Util;
using Android.Graphics;
using System.Threading.Tasks;
using Plugin.Connectivity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using Newtonsoft.Json;
using System.Globalization;

namespace Cabasus_Android.Screens
{
    public class ContentFragment : Fragment
    {
        //Creamos nuestra clase a inflantar
        public int position;
        ActivityActivity a;

        public static ContentFragment NewInstance(int position, ActivityActivity activity)
        {
            var f = new ContentFragment();
            var b = new Bundle();
            b.PutInt("position", position);
            f.a = activity;
            f.Arguments = b;
            return f;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            position = Arguments.GetInt("position");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return llenarListadeActividades(inflater, container, savedInstanceState);
        }

        public View llenarListadeActividades(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CultureInfo cultureInfo = new CultureInfo(new ShareInside().ConsultarIdioma().Idioma);

            if (position == 0)
            {
                var ll = new ShareInside().ConsultarEstadoParaLaActividad();
                var root = inflater.Inflate(Resource.Layout.fragment_activity7dias, container, false);

                //switch/internet
                if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "1,0" || new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "0,0")
                {
                    LinearLayout general = root.FindViewById<LinearLayout>(Resource.Id.Resumen7Dias);
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));

                    #region llenar el primer dia;
                    var ConsultaDia1 = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where Dates = '" + DateTime.Now.ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes1).Text = DateTime.Now.ToString("MMMM dd",cultureInfo);

                    if (ConsultaDia1.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                {
                                    irDetalleActividad(item.ID_ActividadLocal);
                                }
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el segundo dia;
                    var ConsultaDia2 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes2).Text = DateTime.Now.AddDays(-1).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle

                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el tercer dia;
                    var ConsultaDia3 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes3).Text = DateTime.Now.AddDays(-2).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia3.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia3)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle

                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el cuarto dia;
                    var ConsultaDia4 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes4).Text = DateTime.Now.AddDays(-3).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia4.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia4)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle

                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el quinto dia;
                    var ConsultaDia5 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-4).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes5).Text = DateTime.Now.AddDays(-4).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia5.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia5)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el sexto dia;
                    var ConsultaDia6 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-5).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes6).Text = DateTime.Now.AddDays(-5).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia6.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia6)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle

                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el septimo dia;
                    var ConsultaDia7 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-6).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    general.FindViewById<TextView>(Resource.Id.txtNombreMes7).Text = DateTime.Now.AddDays(-6).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia7.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    else
                    {
                        foreach (var item in ConsultaDia7)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                }

                else if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "1,1")
                {
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                    LinearLayout general = root.FindViewById<LinearLayout>(Resource.Id.Resumen7Dias);

                    #region llenar el primer dia;
                    var ConsultaDia1_1 = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where Dates = '" + DateTime.Now.ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia1_2 = con.Query<ActividadesCloud>("select * from ActividadesCloud where Dates = '" + DateTime.Now.ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes1).Text = DateTime.Now.ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia1_1.Count <= 0 && ConsultaDia1_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia1_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia1_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia1_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia1_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el segundo dia;
                    var ConsultaDia2_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia2_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes2).Text = DateTime.Now.AddDays(-1).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia2_1.Count <= 0 && ConsultaDia2_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia2_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia2_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia2_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia2_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el tercer dia;
                    var ConsultaDia3_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia3_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes3).Text = DateTime.Now.AddDays(-2).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia3_1.Count <= 0 && ConsultaDia3_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia3_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia3_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia3_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia3_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el cuarto dia;
                    var ConsultaDia4_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia4_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes4).Text = DateTime.Now.AddDays(-3).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia4_1.Count <= 0 && ConsultaDia4_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia4_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia4_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia4_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia4_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el quinto dia;
                    var ConsultaDia5_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-4).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia5_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-4).ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes5).Text = DateTime.Now.AddDays(-4).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia5_1.Count <= 0 && ConsultaDia5_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia5_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia5_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia5_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia5_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el sexto dia;
                    var ConsultaDia6_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-5).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia6_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-5).ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes6).Text = DateTime.Now.AddDays(-5).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia6_1.Count <= 0 && ConsultaDia6_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia6_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia6_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia6_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia6_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el septimo dia;
                    var ConsultaDia7_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-6).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia7_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-6).ToString("yyyy/MM/dd") + "' AND ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes7).Text = DateTime.Now.AddDays(-6).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia7_1.Count <= 0 && ConsultaDia7_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia7_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia7_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia7_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia7_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(RowStyle1);
                        }
                    }
                    #endregion;
                }

                else if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "0,1")
                {
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                    LinearLayout general = root.FindViewById<LinearLayout>(Resource.Id.Resumen7Dias);

                    #region llenar el primer dia;
                    var ConsultaDia1_1 = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where Dates = '" + DateTime.Now.ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia1_2 = con.Query<ActividadesCloud>("select * from ActividadesCloud where Dates = '" + DateTime.Now.ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes1).Text = DateTime.Now.ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia1_1.Count <= 0 && ConsultaDia1_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia1_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia1_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";
                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia1_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia1_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia1).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el segundo dia;
                    var ConsultaDia2_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia2_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes2).Text = DateTime.Now.AddDays(-1).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia2_1.Count <= 0 && ConsultaDia2_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia2_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia2_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia2_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia2_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia2).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el tercer dia;
                    var ConsultaDia3_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia3_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes3).Text = DateTime.Now.AddDays(-2).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia3_1.Count <= 0 && ConsultaDia3_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia3_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia3_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia3_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia3_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia3).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el cuarto dia;
                    var ConsultaDia4_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia4_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes4).Text = DateTime.Now.AddDays(-3).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia4_1.Count <= 0 && ConsultaDia4_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia4_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia4_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<ImageView>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia4_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia4_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia4).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el quinto dia;
                    var ConsultaDia5_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-4).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia5_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-4).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes5).Text = DateTime.Now.AddDays(-4).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia5_1.Count <= 0 && ConsultaDia5_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia5_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia5_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<ImageView>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia5_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia5_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia5).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el sexto dia;
                    var ConsultaDia6_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-5).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia6_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-5).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes6).Text = DateTime.Now.AddDays(-5).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia6_1.Count <= 0 && ConsultaDia6_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia6_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia6_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<ImageView>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia6_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia6_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia6).AddView(RowStyle1);
                        }
                    }
                    #endregion;

                    #region llenar el septimo dia;
                    var ConsultaDia7_1 = con.Query<ActividadPorCaballo>("Select * from ActividadPorCaballo where Dates='" + DateTime.Now.AddDays(-6).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadPorCaballo()).Dates);
                    var ConsultaDia7_2 = con.Query<ActividadesCloud>("Select * from ActividadesCloud where Dates='" + DateTime.Now.AddDays(-6).ToString("yyyy/MM/dd") + "' and ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", (new ActividadesCloud()).Dates);

                    general.FindViewById<TextView>(Resource.Id.txtNombreMes7).Text = DateTime.Now.AddDays(-6).ToString("MMMM dd", cultureInfo);
                    if (ConsultaDia7_1.Count <= 0 && ConsultaDia7_2.Count <= 0)
                    {
                        general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                    }
                    if (ConsultaDia7_1.Count > 0)
                    {
                        foreach (var item in ConsultaDia7_1)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<ImageView>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad(item.ID_ActividadLocal);
                                else
                                    irDetalleActividad(item.ID_ActividadLocal);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(RowStyle1);
                        }
                    }
                    if (ConsultaDia7_2.Count > 0)
                    {
                        foreach (var item in ConsultaDia7_2)
                        {
                            var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                            var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                            if (TotalRecorrido < 1000)
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                            }
                            else
                            {
                                RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                            }

                            RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                            if (item.Horse_Status == "1")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                            else if (item.Horse_Status == "2")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                            else if (item.Horse_Status == "3")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                            else if (item.Horse_Status == "4")
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                            else
                                RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                            //Redireccionar pantalla detalle
                            RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                            {
                                if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                                else
                                    irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            };

                            general.FindViewById<LinearLayout>(Resource.Id.Dia7).AddView(RowStyle1);
                        }
                    }
                    #endregion;
                }

                return root;
            }
            else
            {
                var root = inflater.Inflate(Resource.Layout.fragment_activitymes, container, false);
                LinearLayout general = root.FindViewById<LinearLayout>(Resource.Id.LLenadodeActividadesPorMes);

                DateTime MesAndMonth = DateTime.Now;

                var leftarrow = root.FindViewById<ImageView>(Resource.Id.btnflechaatras);

                var rightarrow = root.FindViewById<ImageView>(Resource.Id.btnflechaadelante);

                leftarrow.Click += delegate
                {
                    MesAndMonth = MesAndMonth.AddMonths(-1);

                    if (MesAndMonth.ToString("yyyy/MM") == DateTime.Now.ToString("yyyy/MM"))
                    {
                        rightarrow.Enabled = false;
                        rightarrow.Alpha = 0.5f;
                    }
                    else
                    {
                        rightarrow.Enabled = true;
                        rightarrow.Alpha = 1f;
                    }

                    root.FindViewById<TextView>(Resource.Id.txtmesyanio).Text = MesAndMonth.ToString("MMMM yyyy",cultureInfo);
                    general.RemoveAllViews();

                    a.RunOnUiThread(async () =>
                    {
                        await a.ActividadesInternetAsyncMes(MesAndMonth, a);
                        ConsultaPorFecha(MesAndMonth, general);
                    });


                };

                rightarrow.Click += delegate
                {
                    MesAndMonth = MesAndMonth.AddMonths(1);

                    if (MesAndMonth.ToString("yyyy/MM") == DateTime.Now.ToString("yyyy/MM"))
                    {
                        rightarrow.Enabled = false;
                        rightarrow.Alpha = 0.5f;
                    }
                    else
                    {
                        rightarrow.Enabled = true;
                        rightarrow.Alpha = 1f;
                    }

                    root.FindViewById<TextView>(Resource.Id.txtmesyanio).Text = MesAndMonth.ToString("MMMM yyyy",cultureInfo);
                    general.RemoveAllViews();

                    a.RunOnUiThread(async () =>
                    {
                        await a.ActividadesInternetAsyncMes(MesAndMonth, a);
                        ConsultaPorFecha(MesAndMonth, general);
                    });


                };

                if (MesAndMonth.ToString("yyyy/MM") == DateTime.Now.ToString("yyyy/MM"))
                {
                    rightarrow.Enabled = false;
                    rightarrow.Alpha = 0.5f;
                }
                else
                {
                    rightarrow.Enabled = true;
                }

                general.RemoveAllViews();

                if (CrossConnectivity.Current.IsConnected)
                {
                    root.FindViewById<TextView>(Resource.Id.txtmesyanio).Text = MesAndMonth.ToString("MMMM yyyy");
                    a.RunOnUiThread(async () =>
                    {
                        await a.ActividadesInternetAsyncMes(MesAndMonth, a);
                        ConsultaPorFecha(MesAndMonth, general);
                    });
                }
                else
                {
                    root.FindViewById<TextView>(Resource.Id.txtmesyanio).Text = MesAndMonth.ToString("MMMM yyyy");
                    ConsultaPorFecha(MesAndMonth, general);
                }

                return root;
            }

        }

        public void ConsultaPorFecha(DateTime MesAndMonth, LinearLayout general)
        {
            if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "1,0" || new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "0,0")
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                var ConsultaDia1 = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and Dates between '" + MesAndMonth.ToString("yyyy") + "/" + MesAndMonth.ToString("MM") + "/01' and '" + MesAndMonth.ToString("yyyy") + "/" + MesAndMonth.ToString("MM") + "/31'  ORDER BY Dates DESC", (new ActividadPorCaballo()).Dates);

                if (ConsultaDia1.Count <= 0)
                {
                    LinearLayout principal = new LinearLayout(general.Context);
                    principal.LayoutParameters = new LinearLayout.LayoutParams(-1, -1);
                    principal.Orientation = Orientation.Horizontal;
                    principal.SetBackgroundColor(Color.Argb(255 / 2, 0, 0, 0));

                    TextView t = new TextView(general.Context);
                    t.LayoutParameters = new LinearLayout.LayoutParams(-1, (int)System.Math.Round(50 * (Context.Resources.DisplayMetrics.Xdpi / (float)DisplayMetrics.DensityDefault)));
                    t.Text = GetText(Resource.String.No_activity_for_this_month);
                    t.Gravity = GravityFlags.Center;
                    t.SetTextColor(Color.White);
                    //t.TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 11, Context.Resources.DisplayMetrics);
                    t.TextSize = 11;

                    principal.AddView(t);

                    general.RemoveAllViews();
                    general.AddView(principal);
                }
                else
                {
                    foreach (var item in ConsultaDia1)
                    {
                        var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                        var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                        if (TotalRecorrido < 1000)
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                        }
                        else
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                        }

                        RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                        if (item.Horse_Status == "1")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                        else if (item.Horse_Status == "2")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                        else if (item.Horse_Status == "3")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                        else if (item.Horse_Status == "4")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                        else
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                        //Redireccionar pantalla detalle
                        RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                        {
                            if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                irDetalleActividad(item.ID_ActividadLocal);
                            else
                                irDetalleActividad(item.ID_ActividadLocal);
                        };

                        general.AddView(RowStyle1);
                    }
                }
            }

            else if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "1,1")
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));

                #region llenar el mes solo yo online;
                var ConsultaDia1_1 = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and Dates between '" + MesAndMonth.ToString("yyyy") + "/" + MesAndMonth.ToString("MM") + "/01' and '" + MesAndMonth.ToString("yyyy") + "/" + MesAndMonth.ToString("MM") + "/31'  ORDER BY Dates DESC", (new ActividadPorCaballo()).Dates);
                var ConsultaDia1_2 = con.Query<ActividadesCloudMes>("select * from ActividadesCloudMes where ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and ID_Usuario='" + new ShareInside().ConsultarDatosUsuario()[0].id + "'", (new ActividadesCloudMes()).Dates);

                if (ConsultaDia1_1.Count <= 0 && ConsultaDia1_2.Count <= 0)
                {
                    general.AddView(a.LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                }

                if (ConsultaDia1_1.Count > 0)
                {
                    foreach (var item in ConsultaDia1_1)
                    {
                        var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                        var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                        if (TotalRecorrido < 1000)
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                        }
                        else
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                        }

                        RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                        if (item.Horse_Status == "1")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                        else if (item.Horse_Status == "2")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                        else if (item.Horse_Status == "3")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                        else if (item.Horse_Status == "4")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                        else
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                        //Redireccionar pantalla detalle
                        RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                        {
                            if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                irDetalleActividad(item.ID_ActividadLocal);
                            else
                                irDetalleActividad(item.ID_ActividadLocal);
                        };

                        general.AddView(RowStyle1);
                    }
                }

                if (ConsultaDia1_2.Count > 0)
                {
                    foreach (var item in ConsultaDia1_2)
                    {
                        var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                        var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                        if (TotalRecorrido < 1000)
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                        }
                        else
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                        }

                        RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                        if (item.Horse_Status == "1")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                        else if (item.Horse_Status == "2")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                        else if (item.Horse_Status == "3")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                        else if (item.Horse_Status == "4")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                        else
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                        //Redireccionar pantalla detalle
                        RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                        {
                            if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            else
                                irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                        };

                        general.AddView(RowStyle1);
                    }
                }
                #endregion;

            }

            else if (new ShareInside().ConsultarEstadoParaLaActividad().Split('$')[0] == "0,1")
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));

                #region llenar el mes solo yo online;
                var ConsultaDia1_1 = con.Query<ActividadPorCaballo>("select * from ActividadPorCaballo where ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and Dates between '" + MesAndMonth.ToString("yyyy") + "/" + MesAndMonth.ToString("MM") + "/01' and '" + MesAndMonth.ToString("yyyy") + "/" + MesAndMonth.ToString("MM") + "/31'  ORDER BY Dates DESC", (new ActividadPorCaballo()).Dates);
                var ConsultaDia1_2 = con.Query<ActividadesCloudMes>("select * from ActividadesCloudMes", (new ActividadesCloudMes()).Dates);

                if (ConsultaDia1_1.Count <= 0 && ConsultaDia1_2.Count <= 0)
                {
                    general.AddView(a.LayoutInflater.Inflate(Resource.Layout.RowActividaNoActividad, null));
                }

                if (ConsultaDia1_1.Count > 0)
                {
                    foreach (var item in ConsultaDia1_1)
                    {
                        var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                        var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                        if (TotalRecorrido < 1000)
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                        }
                        else
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                        }

                        RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.ID_Usuario;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;

                        if (item.Horse_Status == "1")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                        else if (item.Horse_Status == "2")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                        else if (item.Horse_Status == "3")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                        else if (item.Horse_Status == "4")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                        else
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                        //Redireccionar pantalla detalle
                        RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                        {
                            if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                irDetalleActividad(item.ID_ActividadLocal);
                            else
                                irDetalleActividad(item.ID_ActividadLocal);
                        };

                        general.AddView(RowStyle1);
                    }
                }

                if (ConsultaDia1_2.Count > 0)
                {
                    foreach (var item in ConsultaDia1_2)
                    {
                        var RowStyle1 = LayoutInflater.Inflate(Resource.Layout.RowActivida, null);
                        var TotalRecorrido = item.Normal + item.Slow + item.Strong;

                        if (TotalRecorrido < 1000)
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round(TotalRecorrido, 2) + " mts.";
                        }
                        else
                        {
                            RowStyle1.FindViewById<TextView>(Resource.Id.txtDistancia).Text = Math.Round((TotalRecorrido / 1000), 2) + " km.";
                        }

                        RowStyle1.FindViewById<TextView>(Resource.Id.txtTiempo).Text = new ShareInside().StoH(int.Parse((item.Duration).ToString())) + " hrs.";
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtCorredor).Text = item.User;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtFecha).Text = item.Dates;
                        RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text = "";

                        if (item.Horse_Status == "1")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "VW";
                        else if (item.Horse_Status == "2")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "WT";
                        else if (item.Horse_Status == "3")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "N";
                        else if (item.Horse_Status == "4")
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "UW";
                        else
                            RowStyle1.FindViewById<TextView>(Resource.Id.imgHorseStatus).Text = "OW";

                        //Redireccionar pantalla detalle
                        RowStyle1.FindViewById<LinearLayout>(Resource.Id.lnIrActividad).Click += delegate
                        {
                            if (RowStyle1.FindViewById<TextView>(Resource.Id.txtLocal).Text.ToLower() == "local")
                                irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                            else
                                irDetalleActividad_Cloud(item.ID_ActividadLocal, item.ID_Usuario, item.ID_Caballo);
                        };

                        general.AddView(RowStyle1);
                    }
                }
                #endregion;
            }
        }

        public void irDetalleActividad_Cloud(string idActividadLocal, string idUsuario, string idCaballo)
        {
            Android.Content.Intent intent = new Android.Content.Intent(a, typeof(ActivityActivityDetalle));


            intent.PutExtra("ubicacion", "n");
            intent.PutExtra("idActividadLocal", idActividadLocal);
            intent.PutExtra("Caballo", new ShareInside().ConsultarPosicionPiker()[0].Name_HoserSelected);
            intent.PutExtra("Usuario", idUsuario);
            intent.PutExtra("ConsultaPikerIdCaballo", new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected);

            StartActivity(intent);
        }

        public void irDetalleActividad(int idActividadLocal)
        {
            Android.Content.Intent intent = new Android.Content.Intent(a, typeof(ActivityActivityDetalle));

            intent.PutExtra("idActividadLocal", idActividadLocal.ToString());
            intent.PutExtra("Caballo", new ShareInside().ConsultarPosicionPiker()[0].Name_HoserSelected);
            intent.PutExtra("ConsultaPikerIdCaballo", new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected);

            StartActivity(intent);
        }
    }

    public class ContentFragmentFiltrosRecordatorios : Fragment
    {
        public int position;
        string Date;
        public ListView JornadaLista, RecordatoriosLista, ActividadesLista;
        public static ContentFragmentFiltrosRecordatorios f;
        public static Android.App.Activity _Context;
        
        public static ContentFragmentFiltrosRecordatorios NewInstance(int position, Android.App.Activity context, string _Date)
        {
            f = new ContentFragmentFiltrosRecordatorios();
            var b = new Bundle();
            b.PutInt("position", position);
            b.PutString("Date", _Date);
            f.Arguments = b;
            _Context = context;
            return f;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            position = Arguments.GetInt("position");
            Date = Arguments.GetString("Date");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (position == 0)
            {
                var root = inflater.Inflate(Resource.Layout.layout_ListaContenidoActividades, container, false);
                ActividadesLista = root.FindViewById<ListView>(Resource.Id.List);
                FiltroActividadesAsync(ActividadesLista, Date);
                return root;
            }
            //else if (position == 1)
            //{
            //    var root = inflater.Inflate(Resource.Layout.layout_ListaContenidoActividades, container, false);
            //    RecordatoriosLista = root.FindViewById<ListView>(Resource.Id.List);
            //    FiltroRecordatoriosAsync(RecordatoriosLista, Date);
            //    return root;
            //}
            //else
            //{
            //    var root = inflater.Inflate(Resource.Layout.layout_ListaContenidoActividades, container, false);
            //    JornadaLista = root.FindViewById<ListView>(Resource.Id.List);
            //    FiltroJornadasAsync(JornadaLista, Date);
            //    return root;
            //}
            else
            {
                var root = inflater.Inflate(Resource.Layout.layout_ListaContenidoActividades, container, false);
                RecordatoriosLista = root.FindViewById<ListView>(Resource.Id.List);
                FiltroRecordatoriosAsync(RecordatoriosLista, Date);
                return root;
            }
        }

        public async Task FiltroRecordatoriosAsync(ListView recordatoriosLista, string date)
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
                        #region Recordatorios nube por fecha
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        foreach (var item in data)
                        {
                            if (item.date.Substring(0, 10) == date && item.horse == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected && item.owner == new ShareInside().ConsultarDatosUsuario()[0].id)
                            {
                                var datos = new DatosReminder();
                                datos.Description = item.description;
                                datos.Begin = item.date;
                                datos.End = item.end_date;
                                datos.Notification = item.alert_before.ToString();
                                datos.id_reminder = item.id;
                                datos.Tipo = item.type.ToString();
                                datos.fk_usuario = item.owner;
                                datos.fk_caballo = item.horse;
                                ListaRecordatorios.Add(datos);
                            }
                        }
                        #endregion

                        #region Recordatorios local por fecha
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fecha = '" + Date + "';", new RedordatoriosLocales().id_reminder);

                        if (ConsultaRecordatorios.Count > 0)
                        {
                            foreach (var item in ConsultaRecordatorios)
                            {
                                var datos = new DatosReminder();
                                datos.Description = item.descripcion;
                                datos.Begin = item.inicio;
                                datos.End = item.fin;
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
                            datos.Description = _Context.GetText(Resource.String.There_are_no_reminders);
                            ListaRecordatorios.Add(datos);
                            recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, false);
                        }
                        #endregion
                        else
                            recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, true);
                    }
                    else
                    {
                        #region Sin recordatorios en la nube
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fecha = '" + Date + "';", new RedordatoriosLocales().id_reminder);

                        #region Con recordatorios locales
                        if (ConsultaRecordatorios.Count > 0)
                        {
                            List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                            foreach (var item in ConsultaRecordatorios)
                            {
                                var datos = new DatosReminder();
                                datos.Description = item.descripcion;
                                datos.Begin = item.inicio;
                                datos.End = item.fin;
                                datos.Notification = item.notificacion;
                                datos.id_reminder = item.id_reminder.ToString();
                                datos.Tipo = item.Tipo;
                                datos.fk_usuario = item.fk_Usuario;
                                datos.fk_caballo = item.fk_caballo;
                                datos.fecha = item.fecha;
                                ListaRecordatorios.Add(datos);
                            }
                            recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, true);
                        }
                        #endregion
                        #region sin recordatorios
                        else
                        {
                            List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                            var datos = new DatosReminder();
                            datos.Description = _Context.GetText(Resource.String.There_are_no_reminders);
                            ListaRecordatorios.Add(datos);
                            recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, false);
                        }
                        #endregion

                        #endregion
                    }
                }
                else
                {
                    #region Sin recordatorios en la nube
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fecha = '" + Date + "';", new RedordatoriosLocales().id_reminder);

                    #region Con recordatorios locales
                    if (ConsultaRecordatorios.Count > 0)
                    {
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        foreach (var item in ConsultaRecordatorios)
                        {
                            var datos = new DatosReminder();
                            datos.Description = item.descripcion;
                            datos.Begin = item.inicio;
                            datos.End = item.fin;
                            datos.Notification = item.notificacion;
                            datos.id_reminder = item.id_reminder.ToString();
                            datos.Tipo = item.Tipo;
                            datos.fk_usuario = item.fk_Usuario;
                            datos.fk_caballo = item.fk_caballo;
                            datos.fecha = item.fecha;
                            ListaRecordatorios.Add(datos);
                        }
                        recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, true);
                    }
                    #endregion
                    #region sin recordatorios
                    else
                    {
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        var datos = new DatosReminder();
                        datos.Description = _Context.GetText(Resource.String.There_are_no_reminders);
                        ListaRecordatorios.Add(datos);
                        recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, false);
                    }
                    #endregion

                    #endregion
                }
            }
            else
            {
                try
                {
                    #region con recordatorios locales
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("select * from reminders where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and fk_usuario = '" + new ShareInside().ConsultarDatosUsuario()[0].id + "' and fecha = '" + Date + "';", new RedordatoriosLocales().id_reminder);
                    if (ConsultaRecordatorios.Count > 0)
                    {
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        foreach (var item in ConsultaRecordatorios)
                        {
                            var datos = new DatosReminder();
                            datos.Description = item.descripcion;
                            datos.Begin = item.inicio;
                            datos.End = item.fin;
                            datos.Notification = item.notificacion;
                            datos.id_reminder = item.id_reminder.ToString();
                            datos.Tipo = item.Tipo;
                            datos.fk_usuario = item.fk_Usuario;
                            datos.fk_caballo = item.fk_caballo;
                            datos.fecha = item.fecha;
                            ListaRecordatorios.Add(datos);
                        }
                        recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, true);
                    }
                    #endregion
                    #region sin recordatorios 
                    else
                    {
                        List<DatosReminder> ListaRecordatorios = new List<DatosReminder>();
                        var datos = new DatosReminder();
                        datos.Description = _Context.GetText(Resource.String.There_are_no_reminders);
                        ListaRecordatorios.Add(datos);
                        recordatoriosLista.Adapter = new AdapterRecordatorios(_Context, ListaRecordatorios, false);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Toast.MakeText(_Context, Resource.String.The_information, ToastLength.Short).Show();
                }
            }
        }

        public async Task<ImagenesNube> FiltroImagenes(string foto1)
        {
            string serverImagenes = "https://cabasus-mobile.azurewebsites.net/v1/images/" + foto1;
            var uriImagenes = new System.Uri(string.Format(serverImagenes, string.Empty));
            HttpClient ClienteImagenes = new HttpClient();
            ClienteImagenes.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

            var consultaImagenes = await ClienteImagenes.GetAsync(uriImagenes);
            consultaImagenes.EnsureSuccessStatusCode();

            if (consultaImagenes.IsSuccessStatusCode)
            {
                JsonValue ConsultaJson = await consultaImagenes.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ImagenesNube>(ConsultaJson);
                return data;
            }
            else
            {
                return await FiltroImagenes(foto1);
            }
        }

        public async Task FiltroJornadasAsync(ListView JornadaLista, string Date)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                #region Servidor
                string server = "https://cabasus-mobile.azurewebsites.net/v1/journals";
                var uri = new System.Uri(string.Format(server, string.Empty));
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                var consulta = await Cliente.GetAsync(uri);
                consulta.EnsureSuccessStatusCode();
                #endregion

                if (consulta.IsSuccessStatusCode)
                {
                    JsonValue ConsultaJson = await consulta.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<JornadasNube>>(ConsultaJson);
                    if (data.Count > 0)
                    {
                        #region Jornadas nube por fecha
                        List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                        ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                        foreach (var item in data)
                        {
                            if (item.date.Substring(0, 10) == Date && item.horse == new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected && item.visibility == "public")
                            {
                                #region Servidor Imagenes
                                ModeloFoto1 = await FiltroImagenes(item.images[0]);
                                string Foto1 = System.Text.Encoding.UTF8.GetString(ModeloFoto1.data.data);

                                ModeloFoto2 = await FiltroImagenes(item.images[1]);
                                string Foto2 = System.Text.Encoding.UTF8.GetString(ModeloFoto2.data.data);

                                ModeloFoto3 = await FiltroImagenes(item.images[2]);
                                string Foto3 = System.Text.Encoding.UTF8.GetString(ModeloFoto3.data.data);
                                #endregion

                                var datos = new DatosJournals();
                                datos.descripcion = item.content;
                                datos.dates = item.date;
                                datos.estado_caballo = item.horse_status;
                                datos.id_diary = item.id;
                                datos.foto1 = Foto1;
                                datos.foto2 = Foto2;
                                datos.foto3 = Foto3;
                                datos.fk_usuario = item.owner;
                                datos.fk_caballo = item.horse;
                                ListaJornadas.Add(datos);
                            }
                        }
                        #endregion

                        #region jornadas locales por fecha
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'public' and dates = '" + Date + "';", (new DiarioLocal()).id_diary);
                        if (ConsultaDiarios.Count > 0)
                        {
                            foreach (var item in ConsultaDiarios)
                            {
                                var datos = new DatosJournals();
                                datos.id_diary = item.id_diary.ToString();
                                datos.fk_usuario = item.fk_usuario;
                                datos.fk_caballo = item.fk_caballo;
                                datos.dates = item.dates;
                                datos.estado_caballo = item.estado_caballo.ToString();
                                datos.descripcion = item.descripcion;
                                datos.foto1 = item.foto1;
                                datos.foto2 = item.foto2;
                                datos.foto3 = item.foto3;
                                ListaJornadas.Add(datos);
                            }
                        }
                        #endregion

                        #region Sin jornadas de la nube ni locales de la fecha seleccionada
                        if (ListaJornadas.Count == 0)
                        {
                            var datos = new DatosJournals();
                            datos.descripcion = _Context.GetText(Resource.String.There_are_no_journals);
                            ListaJornadas.Add(datos);
                            JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                        }
                        else
                            JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                        #endregion
                    }
                    else
                    {
                        try
                        {
                            #region sin jornadas en la nube
                            #region con jornadas locales
                            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                            var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'public' and dates = '" + Date + "';", (new DiarioLocal()).id_diary);
                            ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                            if (ConsultaDiarios.Count > 0)
                            {
                                List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                                foreach (var item in ConsultaDiarios)
                                {
                                    var datos = new DatosJournals();
                                    datos.id_diary = item.id_diary.ToString();
                                    datos.fk_usuario = item.fk_usuario;
                                    datos.fk_caballo = item.fk_caballo;
                                    datos.dates = item.dates;
                                    datos.estado_caballo = item.estado_caballo.ToString();
                                    datos.descripcion = item.descripcion;
                                    datos.foto1 = item.foto1;
                                    datos.foto2 = item.foto2;
                                    datos.foto3 = item.foto3;
                                    ListaJornadas.Add(datos);
                                }
                                JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                            }
                            #endregion
                            #region sin jornadas locales
                            else
                            {
                                List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                                var datos = new DatosJournals();
                                datos.descripcion = _Context.GetText(Resource.String.There_are_no_journals);
                                ListaJornadas.Add(datos);
                                JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                            }
                            #endregion
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(_Context, Resource.String.The_information, ToastLength.Short).Show();
                        }
                    }
                }
                else
                {
                    try
                    {
                        #region con jornadas locales
                        
                        var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                        var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'public' and dates = '" + Date + "';", (new DiarioLocal()).id_diary);
                        ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                        if (ConsultaDiarios.Count > 0)
                        {
                            List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                            foreach (var item in ConsultaDiarios)
                            {
                                var datos = new DatosJournals();
                                datos.id_diary = item.id_diary.ToString();
                                datos.fk_usuario = item.fk_usuario;
                                datos.fk_caballo = item.fk_caballo;
                                datos.dates = item.dates;
                                datos.estado_caballo = item.estado_caballo.ToString();
                                datos.descripcion = item.descripcion;
                                datos.foto1 = item.foto1;
                                datos.foto2 = item.foto2;
                                datos.foto3 = item.foto3;
                                ListaJornadas.Add(datos);
                            }
                            JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                        }
                        #endregion
                        #region sin jornadas locales
                        else
                        {
                            List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                            var datos = new DatosJournals();
                            datos.descripcion = _Context.GetText(Resource.String.There_are_no_journals);
                            ListaJornadas.Add(datos);
                            JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(_Context, Resource.String.The_information, ToastLength.Short).Show();
                    }
                }
            }
            else
            {
                try
                {
                    #region con jornadas locales
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var ConsultaDiarios = con.Query<DiarioLocal>("select * from diarys where fk_caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "' and visible = 'public' and dates = '" + Date + "';", (new DiarioLocal()).id_diary);
                    ImagenesNube ModeloFoto1 = new ImagenesNube(), ModeloFoto2 = new ImagenesNube(), ModeloFoto3 = new ImagenesNube();
                    if (ConsultaDiarios.Count > 0)
                    {
                        List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                        foreach (var item in ConsultaDiarios)
                        {
                            var datos = new DatosJournals();
                            datos.id_diary = item.id_diary.ToString();
                            datos.fk_usuario = item.fk_usuario;
                            datos.fk_caballo = item.fk_caballo;
                            datos.dates = item.dates;
                            datos.estado_caballo = item.estado_caballo.ToString();
                            datos.descripcion = item.descripcion;
                            datos.foto1 = item.foto1;
                            datos.foto2 = item.foto2;
                            datos.foto3 = item.foto3;
                            ListaJornadas.Add(datos);
                        }
                        JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, true, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                    }
                    #endregion
                    #region sin jornadas locales
                    else
                    {
                        List<DatosJournals> ListaJornadas = new List<DatosJournals>();
                        var datos = new DatosJournals();
                        datos.descripcion = _Context.GetText(Resource.String.There_are_no_journals);
                        ListaJornadas.Add(datos);
                        JornadaLista.Adapter = new AdapterJornadas(_Context, ListaJornadas, false, ModeloFoto1, ModeloFoto2, ModeloFoto3);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Toast.MakeText(_Context, Resource.String.The_information, ToastLength.Short).Show();
                }
            }
        }

        public async Task FiltroActividadesAsync(ListView ActividadesLista, string Date)
        {
            List<DatosActivities> ListaActividades = new List<DatosActivities>();
            if (CrossConnectivity.Current.IsConnected)
            {
                #region Consultar Actividades Servidor
                string server = "https://cabasus-mobile.azurewebsites.net/v1/horses/" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "/activities?from=" + Date + "&to=" + Date;
                var uri = new System.Uri(string.Format(server, string.Empty));
                HttpClient Cliente = new HttpClient();
                Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
                var consulta = await Cliente.GetAsync(uri);
                consulta.EnsureSuccessStatusCode();
                #endregion
                if (consulta.IsSuccessStatusCode)
                {
                    var contenido = await consulta.Content.ReadAsStringAsync();
                    var DatosActividades = JsonConvert.DeserializeObject<List<ActividadCluod>>(contenido.ToString());
                    if (DatosActividades.Count > 0)
                    {
                        foreach (var item in DatosActividades)
                        {
                            //Math.Round(item.distance.slow + item.distance.normal + item.distance.strong, 2).ToString(), Math.Round(item.duration, 2).ToString(), item.id, item.horse_status, "Nube"
                            var datos = new DatosActivities();
                            datos.Distancia = Math.Round(item.distance.slow + item.distance.normal + item.distance.strong, 2).ToString();
                            datos.Tiempo = Math.Round(item.duration, 2).ToString();
                            datos.Id = item.id;
                            datos.Status = item.horse_status;
                            datos.Ubicacion = "Nube";
                            ListaActividades.Add(datos);
                        }

                        #region Sin jornadas de la nube ni locales de la fecha seleccionada

                        ListaActividades = await DatosActividadLocal(ListaActividades, Date);

                        if (ListaActividades.Count == 0)
                        {
                            var datos = new DatosActivities();
                            datos.Ubicacion = _Context.GetText(Resource.String.There_are_no_activities);
                            ListaActividades.Add(datos);
                            ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades,false);
                        }
                        else
                        {
                            ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades,true);
                        }
                        #endregion
                    }
                    else
                    {
                        ListaActividades = await DatosActividadLocal(ListaActividades, Date);
                        if (ListaActividades.Count == 0)
                        {
                            var datos = new DatosActivities();
                            datos.Ubicacion = _Context.GetText(Resource.String.There_are_no_activities);
                            ListaActividades.Add(datos);
                            ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades, false);
                        }
                        else
                        {
                            ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades, true);
                        }
                    }
                }
                else
                {
                    ListaActividades = await DatosActividadLocal(ListaActividades, Date);
                    if (ListaActividades.Count == 0)
                    {
                        var datos = new DatosActivities();
                        datos.Ubicacion = _Context.GetText(Resource.String.There_are_no_activities);
                        ListaActividades.Add(datos);
                        ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades, false);
                    }
                    else
                    {
                        ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades, true);
                    }
                }
            }
            else
            {
                ListaActividades = await DatosActividadLocal(ListaActividades, Date);
                if (ListaActividades.Count == 0)
                {
                    var datos = new DatosActivities();
                    datos.Ubicacion = _Context.GetText(Resource.String.There_are_no_activities);
                    ListaActividades.Add(datos);
                    ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades, false);
                }
                else
                {
                    ActividadesLista.Adapter = new AdapterActivities(_Context, ListaActividades, true);
                }
            }
        }

        public async Task<List<DatosActivities>> DatosActividadLocal(List<DatosActivities> ListaActividades, string _Date)
        {
            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
            var consulta2 = con.Query<ActividadesCloud>("select * from ActividadPorCaballo where ID_Caballo = '" + new ShareInside().ConsultarPosicionPiker()[0].Id_HorseSelected + "'", new ActividadesCloud().ID_ActividadLocal);
            if (consulta2.Count > 0)
            {
                foreach (var item in consulta2)
                {
                    var fecha = Convert.ToDateTime(item.Dates).ToString("yyyy-MM-dd");
                    if (fecha == _Date)
                    {
                        var datos = new DatosActivities();
                        datos.Distancia = Math.Round(item.Slow + item.Normal + item.Strong, 2).ToString();
                        datos.Tiempo = Math.Round(Convert.ToDouble(item.Duration), 2).ToString();
                        datos.Id = item.ID_ActividadLocal;
                        datos.Status = item.Horse_Status;
                        datos.Ubicacion = "Local";
                        ListaActividades.Add(datos);
                    }
                }
            }
            return ListaActividades;
        }
    }

    public class DatosActivities
    {
        private string _Longitud;
        private string _Tiempo;
        private string _Id;
        private string _status;
        private string _ubicacion;
        public string Distancia { get { return _Longitud; } set { _Longitud = value; } }
        public string Tiempo { get { return _Tiempo; } set { _Tiempo = value; } }
        public string Id { get { return _Id; } set { _Id = value; } }
        public string Status { get { return _status; } set { _status = value; } }
        public string Ubicacion { get { return _ubicacion; } set { _ubicacion = value; } }

        public DatosActivities(string Longitud, string Tiempo, string Id, string Status, string Ubicacion)
        {
            _Longitud = Longitud;
            _Tiempo = Tiempo;
            _Id = Id;
            _status = Status;
            _ubicacion = Ubicacion;
        }

        public DatosActivities()
        { }
    }
    public class DatosReminder
    {
        private string _Description;
        private string _Begin;
        private string _End;
        private string _Notification;
        private string _Tipo;
        private string _id_reminder;
        private string _fk_usuario;
        private string _fk_caballo;
        private string _fecha;

        public string Description { get { return _Description; } set { _Description = value; } }
        public string Begin { get { return _Begin; } set { _Begin = value; } }
        public string End { get { return _End; } set { _End = value; } }
        public string Notification { get { return _Notification; } set { _Notification = value; } }
        public string Tipo { get { return _Tipo; } set { _Tipo = value; } }
        public string id_reminder { get { return _id_reminder; } set { _id_reminder = value; } }
        public string fk_usuario { get { return _fk_usuario; } set { _fk_usuario = value; } }
        public string fk_caballo { get { return _fk_caballo; } set { _fk_caballo = value; } }
        public string fecha { get { return _fecha; } set { _fecha = value; } }

        public DatosReminder(string Description, string Begin, string End, string Notification, string id_reminder, string Tipo, string fk_usuario, string fk_caballo, string fecha)
        {
            _Description = Description;
            _Begin = Begin;
            _End = End;
            _Notification = Notification;
            _id_reminder = id_reminder;
            _Tipo = Tipo;
            _fk_usuario = fk_usuario;
            _fk_caballo = fk_caballo;
            _fecha = fecha;
        }

        public DatosReminder()
        {
        }
    }
    public class DatosJournals
    {
        private string _id_diary;
        private string _fk_usuario;
        private string _fk_caballo;
        private string _dates;
        private string _estado_caballo;
        private string _descripcion;
        private string _foto1;
        private string _foto2;
        private string _foto3;

        public string id_diary { get { return _id_diary; } set { _id_diary = value; } }
        public string fk_usuario { get { return _fk_usuario; } set { _fk_usuario = value; } }
        public string fk_caballo { get { return _fk_caballo; } set { _fk_caballo = value; } }
        public string dates { get { return _dates; } set { _dates = value; } }
        public string estado_caballo { get { return _estado_caballo; } set { _estado_caballo = value; } }
        public string descripcion { get { return _descripcion; } set { _descripcion = value; } }
        public string foto1 { get { return _foto1; } set { _foto1 = value; } }
        public string foto2 { get { return _foto2; } set { _foto2 = value; } }
        public string foto3 { get { return _foto3; } set { _foto3 = value; } }
        
        public DatosJournals(string id_diary, string fk_usuario, string fk_caballo, string dates, string estado_caballo, string descripcion, string foto1, string foto2, string foto3)
        {
            id_diary = _id_diary;
            fk_usuario = _fk_usuario;
            fk_caballo = _fk_caballo;
            dates = _dates;
            estado_caballo = _estado_caballo;
            descripcion = _descripcion;
            foto1 = _foto1;
            foto2 = _foto2;
            foto3 = _foto3;
        }

        public DatosJournals()
        {
        }
    }

    public class AdapterActivities : BaseAdapter<DatosActivities>
    {
        List<DatosActivities> items;
        Android.App.Activity Context;
        bool bandera = true;

        public AdapterActivities(Android.App.Activity context, List<DatosActivities> items, bool _Bandera) : base()
        {
            this.Context = context;
            this.items = items;
            this.bandera = _Bandera;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override DatosActivities this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!bandera)
            {
                View viewNodata = convertView;
                viewNodata = Context.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                if (items.Count == 0)
                {
                    var datos = new DatosActivities();
                    datos.Ubicacion = Context.GetText(Resource.String.There_are_no_activities);
                    items.Add(datos);
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].Ubicacion;
                }
                else
                {
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].Ubicacion;
                }
                return viewNodata;
            }
            else
            {
                var item = items[position];
                View view = convertView;
                view = Context.LayoutInflater.Inflate(Resource.Layout.Layout_ContenidoActividades, null);

                var txtTiempo = view.FindViewById<TextView>(Resource.Id.lblTiempo);
                var txtDistancia = view.FindViewById<TextView>(Resource.Id.lblDistacia);
                var imgEstado = view.FindViewById<TextView>(Resource.Id.imgEstado);
                var btnEliminar = view.FindViewById<ImageView>(Resource.Id.btnEliminar);
                var txtLocal = view.FindViewById<TextView>(Resource.Id.txtLocal);

                if (items[position].Ubicacion == "Local")
                    txtLocal.Text = item.Ubicacion.ToString();
                else
                    txtLocal.Text = "";


                txtTiempo.Text = new ShareInside().StoH(Convert.ToInt16(item.Tiempo)) + " hrs";
                txtDistancia.Text = item.Distancia + " mts";
                if (item.Status == "1")
                    imgEstado.Text = "VW";
                else if (item.Status == "2")
                    imgEstado.Text = "WT";
                else if (item.Status == "3")
                    imgEstado.Text = "N";
                else if (item.Status == "4")
                    imgEstado.Text = "UW";
                else
                    imgEstado.Text = "OW";

                btnEliminar.Click += delegate
                {
                    Android.App.Dialog alertar = new Android.App.Dialog(Context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.CustomAlertLogOut);
                    alertar.FindViewById<TextView>(Resource.Id.lblDescripcionReminder).Text = Context.GetText(Resource.String.Do_you_want_to_delete_this_activity);
                    alertar.FindViewById<TextView>(Resource.Id.btnYesClose).Click += async delegate
                    {

                        #region Progress
                        ProgressBar progressBar;
                        progressBar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                        alertar.FindViewById<RelativeLayout>(Resource.Id.optionsactivities).AddView(progressBar, p);

                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        #endregion

                        if (item.Ubicacion == "Nube")
                        {
                            var respu = await EliminarActividadNube(items[position].Id);
                            if (respu == true)
                            {
                                items.Remove(items[position]);
                                if (items.Count == 0)
                                {
                                    bandera = false;
                                    GetView(position, convertView, parent);
                                }
                                NotifyDataSetChanged();

                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                alertar.Dismiss();
                            }
                            else
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                Toast.MakeText(Context, Resource.String.You_need_internet, ToastLength.Long).Show();
                            }

                        }
                        else
                        {
                            var respu = await EliminarActividadLocal(items[position].Id);
                            if (respu == true)
                            {
                                items.Remove(items[position]);
                                if (items.Count == 0)
                                {
                                    bandera = false;
                                    GetView(position, convertView, parent);
                                }
                                NotifyDataSetChanged();

                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                alertar.Dismiss();
                            }
                            else
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Visible;
                                alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                Toast.MakeText(Context, Context.GetText(Resource.String.Error_deleting_local_activity), ToastLength.Long).Show();
                            }

                        }
                    };
                    alertar.FindViewById<TextView>(Resource.Id.btnNoClose).Click += delegate
                    {
                        alertar.Dismiss();
                    };
                    alertar.Show();
                };

                return view;
            }

        }
        public async Task<bool> EliminarActividadNube(string id)
        {
            #region Servidor
            string server = "https://cabasus-mobile.azurewebsites.net/v1/activities/" + id;
            var uri = new System.Uri(string.Format(server, string.Empty));
            HttpClient Cliente = new HttpClient();
            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());
            var consulta = await Cliente.DeleteAsync(uri);
            consulta.EnsureSuccessStatusCode();
            if (consulta.IsSuccessStatusCode)
                return true;
            else
                return false;
            #endregion
        }

        public async Task<bool> EliminarActividadLocal(string id)
        {
            try
            {
                var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "VaciadoActividades.sqlite"));
                var consulta2 = con.Query<ActividadesCloud>("delete from ActividadPorCaballo where ID_ActividadLocal = '" + id + "'", new ActividadesCloud().ID_ActividadLocal);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public class AdapterRecordatorios : BaseAdapter<DatosReminder>
    {
        List<DatosReminder> items;
        Android.App.Activity Context;
        bool Bandera;

        public AdapterRecordatorios(Android.App.Activity context, List<DatosReminder> items, bool _Bandera) : base()
        {
            this.Context = context;
            this.items = items;
            this.Bandera = _Bandera;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override DatosReminder this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!Bandera)
            {
                View viewNodata = convertView;
                viewNodata = Context.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                if (items.Count == 0)
                {
                    var datos = new DatosReminder();
                    datos.Description = Context.GetText(Resource.String.There_are_no_reminders);
                    items.Add(datos);
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].Description;
                }
                else
                {
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].Description;
                }
                return viewNodata;
            }
            else
            {
                var item = items[position];
                View view = convertView;

                view = Context.LayoutInflater.Inflate(Resource.Layout.layout_ContenidoRecordatorios, null);

                #region Elementos
                var txtDescription = view.FindViewById<TextView>(Resource.Id.txtDescripcionReminder);
                var txtBegin = view.FindViewById<TextView>(Resource.Id.lblBegin);
                var txtEnd = view.FindViewById<TextView>(Resource.Id.lblEnd);
                var txtNotification = view.FindViewById<TextView>(Resource.Id.lblNotification);
                var btnOpciones = view.FindViewById<ImageView>(Resource.Id.btnOpcionesRecordatorio);
                #endregion
                #region Datos
                txtDescription.Text = items[position].Description;
                txtBegin.Text = items[position].Begin.Substring(0, 10) + " " + items[position].Begin.Substring(11, 8);
                txtEnd.Text = items[position].End.Substring(0, 10) + " " +items[position].End.Substring(11, 8);
                txtNotification.Text = new ShareInside().StoH(int.Parse(items[position].Notification)) + " " + Context.GetText(Resource.String.hours_before);
                #endregion
                #region Tipo
                if (items[position].Tipo == "1")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenAspirinaD);
                else if (items[position].Tipo == "2")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenFlechasD);
                else if (items[position].Tipo == "3")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenEstrellaD);
                else if (items[position].Tipo == "4")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenPelotaD);
                else if (items[position].Tipo == "5")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenMedallaD);
                else if (items[position].Tipo == "6")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenCaballoD);
                else if (items[position].Tipo == "7")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenManzanaD);
                else if (items[position].Tipo == "8")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenGraneroD);
                else if (items[position].Tipo == "9")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenEscalaD);
                else if (items[position].Tipo == "10")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenGeringaD);
                else if (items[position].Tipo == "11")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenStetoscopioD);
                else if (items[position].Tipo == "12")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenCuboD);
                else if (items[position].Tipo == "13")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenMapaD);
                else if (items[position].Tipo == "14")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenTijerasD);
                else if (items[position].Tipo == "15")
                    view.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenClavosD);
                #endregion

                try
                {
                    int.Parse(items[position].id_reminder);
                    view.FindViewById<TextView>(Resource.Id.txtUbicacionRecordatorios).Text = "Local";
                }
                catch (Exception)
                {
                }
                

                btnOpciones.Click += delegate
                {
                    Android.App.Dialog alertar = new Android.App.Dialog(Context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.Layout_CustomAlertOptionsRecordatorios);
                    #region Tipo
                    if (items[position].Tipo == "1")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenAspirinaD);
                    else if (items[position].Tipo == "2")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenFlechasD);
                    else if (items[position].Tipo == "3")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenEstrellaD);
                    else if (items[position].Tipo == "4")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenPelotaD);
                    else if (items[position].Tipo == "5")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenMedallaD);
                    else if (items[position].Tipo == "6")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenCaballoD);
                    else if (items[position].Tipo == "7")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenManzanaD);
                    else if (items[position].Tipo == "8")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenGraneroD);
                    else if (items[position].Tipo == "9")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenEscalaD);
                    else if (items[position].Tipo == "10")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenGeringaD);
                    else if (items[position].Tipo == "11")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenStetoscopioD);
                    else if (items[position].Tipo == "12")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenCuboD);
                    else if (items[position].Tipo == "13")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenMapaD);
                    else if (items[position].Tipo == "14")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenTijerasD);
                    else if (items[position].Tipo == "15")
                        alertar.FindViewById<ImageView>(Resource.Id.imgTypeReminder).SetImageResource(Resource.Drawable.ResumenClavosD);
                    #endregion
                    alertar.FindViewById<TextView>(Resource.Id.lblDescripcionReminder).Text = items[position].Description;

                    alertar.FindViewById<TextView>(Resource.Id.btnEditar).Click += delegate {
                        Intent intent = new Intent(Context, typeof(ActivityNuevoRecordatorio));
                        intent.PutExtra("ActualizarRecordatorio", "true" + "$" + items[position].id_reminder);
                        Context.StartActivity(intent);
                    };
                    alertar.FindViewById<TextView>(Resource.Id.btnEliminar).Click += async delegate {
                        #region Progress
                        ProgressBar progressBar;
                        progressBar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                        alertar.FindViewById<RelativeLayout>(Resource.Id.OptionsRecordatorios).AddView(progressBar, p);

                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        #endregion

                        try
                        {
                            int.Parse(items[position].id_reminder);
                            #region Eliminar recordatorio local
                            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                            var ConsultaRecordatorios = con.Query<RedordatoriosLocales>("delete from reminders where id_reminder = " + items[position].id_reminder + " and fk_usuario = '" + items[position].fk_usuario + "' and fk_caballo = '" + items[position].fk_caballo + "';", (new RedordatoriosLocales()).id_reminder);
                            items.Remove(items[position]);
                            if (items.Count == 0)
                            {
                                Bandera = false;
                                GetView(position, convertView, parent);
                            }
                            NotifyDataSetChanged();
                            
                            alertar.Dismiss();

                            new ShareInside().AlarmNotifiation(Context, 20000);

                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            #endregion
                        }
                        catch (Exception)
                        {
                            #region Servidor
                            string server = "https://cabasus-mobile.azurewebsites.net/v1/events/" + items[position].id_reminder;
                            var uri = new System.Uri(string.Format(server, string.Empty));
                            HttpClient Cliente = new HttpClient();
                            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                            var eliminar = await Cliente.DeleteAsync(uri);
                            eliminar.EnsureSuccessStatusCode();
                            #endregion
                            #region Eliminar recordatorio nube
                            if (eliminar.IsSuccessStatusCode)
                            {
                                items.Remove(items[position]);
                                if (items.Count == 0)
                                {
                                    Bandera = false;
                                    GetView(position, convertView, parent);
                                }
                                NotifyDataSetChanged();
                                alertar.Dismiss();

                                new ShareInside().AlarmNotifiation(Context, 20000);

                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);
                            }
                            else
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                Toast.MakeText(Context, Context.GetText(Resource.String.You_need_internet), ToastLength.Short).Show();
                            }
                                
                            #endregion
                        }
                    };

                    alertar.Show();
                };

                return view;
            }
        }
    }
    public class AdapterJornadas : BaseAdapter<DatosJournals>
    {
        List<DatosJournals> items;
        Android.App.Activity Context;
        bool bandera;
        ImagenesNube id_Foto1, id_Foto2, id_Foto3;

        public AdapterJornadas(Android.App.Activity context, List<DatosJournals> items, bool _bandera, ImagenesNube _id_Foto1, ImagenesNube _id_Foto2, ImagenesNube _id_Foto3) : base()
        {
            this.Context = context;
            this.items = items;
            this.bandera = _bandera;
            this.id_Foto1 = _id_Foto1;
            this.id_Foto2 = _id_Foto2;
            this.id_Foto3 = _id_Foto3;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override DatosJournals this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (!bandera)
            {
                View viewNodata = convertView;
                viewNodata = Context.LayoutInflater.Inflate(Resource.Layout.Layout_NoData, null);
                if (items.Count == 0)
                {
                    var datos = new DatosJournals();
                    datos.descripcion = Context.GetText(Resource.String.There_are_no_journals);
                    items.Add(datos);
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].descripcion;
                }
                else
                {
                    viewNodata.FindViewById<TextView>(Resource.Id.lblNoData).Text = items[position].descripcion;
                }
                return viewNodata;
            }
            else
            {
                var item = items[position];
                View view = convertView;

                view = Context.LayoutInflater.Inflate(Resource.Layout.Layout_ContenidoJornadas, null);

                #region Datos
                var txtEstado = view.FindViewById<TextView>(Resource.Id.lblStatus);
                var txtDescripcion = view.FindViewById<TextView>(Resource.Id.txtComentariosJornada);
                var btnOpciones = view.FindViewById<ImageView>(Resource.Id.btnOpcionesJornadas);

                if (items[position].estado_caballo == "happy")
                    txtEstado.Text = Context.GetText(Resource.String.Happy);
                else if (items[position].estado_caballo == "normal")
                    txtEstado.Text = Context.GetText(Resource.String.Normal);
                else
                    txtEstado.Text = Context.GetText(Resource.String.Sad);
                txtDescripcion.Text = item.descripcion;

                var imgFoto1 = view.FindViewById<ImageView>(Resource.Id.imgFoto1Jornada);
                var imgFoto2 = view.FindViewById<ImageView>(Resource.Id.imgFoto2Jornada);
                var imgFoto3 = view.FindViewById<ImageView>(Resource.Id.imgFoto3Jornada);

                byte[] imageAsBytes1 = Base64.Decode(items[position].foto1, Base64Flags.Default);
                var bit1 = BitmapFactory.DecodeByteArray(imageAsBytes1, 0, imageAsBytes1.Length);
                imgFoto1.SetImageBitmap(new ShareInside().getResizedBitmap(bit1, 30, 30));

                byte[] imageAsBytes2 = Base64.Decode(items[position].foto2, Base64Flags.Default);
                var bit2 = BitmapFactory.DecodeByteArray(imageAsBytes2, 0, imageAsBytes2.Length);
                imgFoto2.SetImageBitmap(new ShareInside().getResizedBitmap(bit2, 30, 30));

                byte[] imageAsBytes3 = Base64.Decode(items[position].foto3, Base64Flags.Default);
                var bit3 = BitmapFactory.DecodeByteArray(imageAsBytes3, 0, imageAsBytes3.Length);
                imgFoto3.SetImageBitmap(new ShareInside().getResizedBitmap(bit3, 30, 30));
                #endregion

                try
                {
                    int.Parse(items[position].id_diary);
                    view.FindViewById<TextView>(Resource.Id.txtUbicacionRecordatorios).Text = "Local";
                }
                catch (Exception)
                {
                }

                btnOpciones.Click += delegate {
                    #region Datos Alerta
                    Android.App.Dialog alertar = new Android.App.Dialog(Context, Resource.Style.Theme_Dialog_Translucent);
                    alertar.RequestWindowFeature(1);
                    alertar.SetCancelable(true);
                    alertar.SetContentView(Resource.Layout.Layout_CustomAlertOptionsJornadas);

                    alertar.FindViewById<TextView>(Resource.Id.lblDescripcionJornada).Text = items[position].descripcion;
                    
                    byte[] imageAsBytes = Base64.Decode(items[position].foto1, Base64Flags.Default);
                    var bit = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto1Reminder).SetImageBitmap(new ShareInside().RedondearbitmapImagen(new ShareInside().getResizedBitmap(bit, 30, 30), 200)); 
                    byte[] imageAsBytes2alert = Base64.Decode(items[position].foto2, Base64Flags.Default);
                    var bit2alert = BitmapFactory.DecodeByteArray(imageAsBytes2alert, 0, imageAsBytes2alert.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto2Reminder).SetImageBitmap(new ShareInside().RedondearbitmapImagen(new ShareInside().getResizedBitmap(bit2alert, 30, 30), 200)); 
                    byte[] imageAsBytes3alert = Base64.Decode(items[position].foto3, Base64Flags.Default);
                    var bit3alert = BitmapFactory.DecodeByteArray(imageAsBytes3alert, 0, imageAsBytes3alert.Length);
                    alertar.FindViewById<ImageView>(Resource.Id.imgFoto3Reminder).SetImageBitmap(new ShareInside().RedondearbitmapImagen(new ShareInside().getResizedBitmap(bit3alert, 30, 30), 200));
                    #endregion

                    alertar.FindViewById<TextView>(Resource.Id.btnEliminar).Click += async delegate {
                        #region Progress
                        ProgressBar progressBar;
                        progressBar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
                        RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(100, 100);
                        p.AddRule(LayoutRules.CenterInParent);
                        progressBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Rgb(207, 147, 0), Android.Graphics.PorterDuff.Mode.Multiply);

                        alertar.FindViewById<RelativeLayout>(Resource.Id.OptionsJornadas).AddView(progressBar, p);

                        progressBar.Visibility = Android.Views.ViewStates.Visible;
                        alertar.Window.AddFlags(Android.Views.WindowManagerFlags.NotTouchable);
                        #endregion

                        try
                        {
                            int.Parse(items[position].id_diary);
                            var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                            var ConsultaDiarios = con.Query<DiarioLocal>("delete from diarys where id_diary = " + items[position].id_diary + " and fk_usuario = '" + items[position].fk_usuario + "' and fk_caballo = '" + items[position].fk_caballo + "' and dates = '" + items[position].dates + "';", (new DiarioLocal()).id_diary);
                            items.Remove(items[position]);
                            if (items.Count == 0)
                            {
                                bandera = false;
                                GetView(position, convertView, parent);
                            }
                            NotifyDataSetChanged();

                            progressBar.Visibility = Android.Views.ViewStates.Invisible;
                            alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                            alertar.Dismiss();
                        }
                        catch (Exception)
                        {
                            await EliminarImagen(id_Foto1.id);
                            await EliminarImagen(id_Foto2.id);
                            await EliminarImagen(id_Foto3.id);

                            #region Servidor
                            string server = "https://cabasus-mobile.azurewebsites.net/v1/journal/" + items[position].id_diary;
                            var uri = new System.Uri(string.Format(server, string.Empty));
                            HttpClient Cliente = new HttpClient();
                            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

                            var eliminar = await Cliente.DeleteAsync(uri);
                            eliminar.EnsureSuccessStatusCode();
                            #endregion
                            #region Eliminar recordatorio nube
                            if (eliminar.IsSuccessStatusCode)
                            {
                                items.Remove(items[position]);
                                if (items.Count == 0)
                                {
                                    bandera = false;
                                    GetView(position, convertView, parent);
                                }
                                NotifyDataSetChanged();

                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                alertar.Dismiss();
                            }
                            else
                            {
                                progressBar.Visibility = Android.Views.ViewStates.Invisible;
                                alertar.Window.ClearFlags(Android.Views.WindowManagerFlags.NotTouchable);

                                Toast.MakeText(Context, Context.GetText(Resource.String.You_need_internet), ToastLength.Short).Show();
                            }
                            #endregion
                        }
                    };

                    alertar.FindViewById<TextView>(Resource.Id.btnEditar).Click += delegate {
                        Intent intent = new Intent(Context, typeof(ActivityNuecaJornada));
                        intent.PutExtra("ActualizarDiario", 
                            "true" + "$" +
                            items[position].id_diary+ "$" +
                            System.Text.Encoding.UTF8.GetString(id_Foto1.data.data) + "$" +
                            System.Text.Encoding.UTF8.GetString(id_Foto2.data.data) + "$" +
                            System.Text.Encoding.UTF8.GetString(id_Foto3.data.data) +"$"+
                            id_Foto1.id + "$" +
                            id_Foto2.id + "$" +
                            id_Foto3.id);
                        Context.StartActivity(intent);
                    };

                    alertar.Show();
                };

                return view;
            }
        }

        public async Task EliminarImagen(string id_imagen)
        {
            #region Servidor
            string server = "https://cabasus-mobile.azurewebsites.net/v1/images/" + id_imagen;
            var uri = new System.Uri(string.Format(server, string.Empty));
            HttpClient Cliente = new HttpClient();
            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(new ShareInside().ConsultToken());

            var eliminar = await Cliente.DeleteAsync(uri);
            eliminar.EnsureSuccessStatusCode();
            #endregion
            #region Eliminar recordatorio nube
            if (eliminar.IsSuccessStatusCode) { }
            else
                await EliminarImagen(id_imagen);
            #endregion
        }
    }

    public class RecordatoriosNube
    {
        public string updatedAt { get; set; }
        public string createdAt { get; set; }
        public string date { get; set; }
        public string end_date { get; set; }
        public int type { get; set; }
        public int alert_before { get; set; }
        public string owner { get; set; }
        public string id { get; set; }
        public string horse { get; set; }
        public string description { get; set; }
    }
    public class JornadasNube
    {
        public string updatedAt { get; set; }
        public string createdAt { get; set; }
        public string owner { get; set; }
        public string horse { get; set; }
        public string content { get; set; }
        public string horse_status { get; set; }
        public string visibility { get; set; }
        public List<string> images { get; set; }
        public string date { get; set; }
        public string id { get; set; }
    }
    public class Imagen
    {
        public string type { get; set; }
        public byte[] data { get; set; }
    }
    public class ImagenesNube
    {
        public string updatedAt { get; set; }
        public string createdAt { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public Imagen data { get; set; }
        public string contentType { get; set; }
        public string id { get; set; }
    }
}