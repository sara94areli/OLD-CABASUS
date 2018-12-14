using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Cabasus_Android.Screens;
using Com.Gigamole.Infinitecycleviewpager;
using Newtonsoft.Json;
using Plugin.Connectivity;
using SQLite;

namespace Cabasus_Android
{
    public class FragmentCycle : Fragment
    {
        List<string> lstNombre = new List<string>();
        List<string> lstId = new List<string>();
        LayoutInflater _LayoutInflater;
        string Date;
        public FragmentCycle(LayoutInflater _layoutInflater, string _Date)
        {
            this._LayoutInflater = _layoutInflater;
            Date = _Date;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            llenarListaCaballo();

            HorizontalInfiniteCycleViewPager cycleViewPager = View.FindViewById<HorizontalInfiniteCycleViewPager>(Resource.Id.horizontal_viewPager_home);

            CaballosUpFormat CaballosUpFormat = new CaballosUpFormat(lstNombre, lstId,View.Context, cycleViewPager);
            cycleViewPager.Adapter = CaballosUpFormat;
            int posicionreal = 0;
            string x = new ShareInside().consultarPickerNotificacionIdCaballo();
            try
            {
                for (int i = 0; i < lstId.Count; i++)
                {
                    if (x == lstId[i])
                    {
                        posicionreal = i;
                    }
                }
            }
            catch (Exception)
            {
            }
            cycleViewPager.CurrentItem = posicionreal;

            new ShareInside().pickerNotificacionIdCaballo(lstId[posicionreal]);
            new ShareInside().GuardarPosicionPiker(lstId[posicionreal], lstNombre[posicionreal], posicionreal);

            cycleViewPager.CurrentItem = new ShareInside().ConsultarPosicionPiker()[0].Position_HorseSelected;

            Timer t = new Timer();
            t.Interval = 1000;
            t.Enabled = true;
            var con = 0;

            cycleViewPager.PageSelected += async delegate
            {
                t.Stop();

                new ShareInside().GuardarPosicionPiker(lstId[cycleViewPager.RealItem], lstNombre[cycleViewPager.RealItem], cycleViewPager.RealItem);

                new ShareInside().pickerNotificacionIdCaballo(lstId[cycleViewPager.RealItem]);

                View.FindViewById<TextView>(Resource.Id.lblHorseCycle).Text = new ShareInside().ConsultarPosicionPiker()[0].Name_HoserSelected;

                switch (new ShareInside().ConsultarPantallaActual())
                {
                    case "ActivityActivity":
                        t.Elapsed += (s, e) =>
                        {
                            (Activity).RunOnUiThread(() =>
                            {
                                con++;
                                if (con >= 2)
                                {
                                    t.Stop();
                                    StartActivity(new Intent(Activity, typeof(ActivityActivity)));
                                }
                            });
                        };
                        t.Start();
                        break;
                    case "ActivityDiary":
                        t.Elapsed += (s, e) =>
                        {
                            (Activity).RunOnUiThread(() =>
                            {
                                con++;
                                if (con >= 2)
                                {
                                    t.Stop();
                                    StartActivity(new Intent(Activity, typeof(ActivityDiary)));
                                }
                            });
                        };
                        t.Start();
                        break;
                    case "ActivityHome":
                        t.Elapsed += (s, e) =>
                        {
                        (Activity).RunOnUiThread(() =>
                        {
                        con++;
                        if (con >= 2)
                        {
                            t.Stop();
                                StartActivity(new Intent(Activity, typeof(ActivityHome)));

                            }
                        });
                        };
                        t.Start();
                        break;
                    case "ActivityFiltros":
                        t.Elapsed += (s, e) =>
                        {
                            (Activity).RunOnUiThread(() =>
                            {
                                con++;
                                if (con >= 2)
                                {
                                    t.Stop();
                                    Intent intent = new Intent(Activity, (typeof(Activity_FiltrosPrincipalesCalendar)));
                                    intent.PutExtra("Date", Date);
                                    this.StartActivity(intent);
                                }
                            });
                        };
                        t.Start();
                        break;
                }
            };

            View.FindViewById<TextView>(Resource.Id.lblHorseCycle).Text = new ShareInside().ConsultarPosicionPiker()[0].Name_HoserSelected;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
                return inflater.Inflate(Resource.Layout.FragmentCiclye, container, false);
        }

        public void llenarListaCaballo()
        {
            lstNombre.Clear();
            lstId.Clear();
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    
                    var Lectura = new StreamReader(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "IdNameHorse.xml"));
                    var Datos = JsonConvert.DeserializeObject<List<HorsesComplete>>(((IdNameHorse)(new XmlSerializer(typeof(IdNameHorse))).Deserialize(Lectura)).DatosCaballo);
                    Lectura.Close();
                    foreach (var item in Datos)
                    {
                        lstNombre.Add(item.name);
                        lstId.Add(item.id);
                    }
                }
                catch (Exception)
                {
                    Intent i = new Intent(Activity, typeof(ActivitySettings));
                  StartActivity(i);
                    
                }
            }
            else
            {
                try
                {
                    var con = new SQLiteConnection(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DBLocal.sqlite"));
                    var consulta = con.Query<HorsesCloud>("Select * from Horses", (new HorsesCloud()).id);
                    foreach (var item in consulta)
                    {
                        lstNombre.Add(item.name);
                        lstId.Add(item.id);
                    }
                }
                catch (Exception)
                {
                    Intent i = new Intent(Activity, typeof(ActivitySettings));
                    StartActivity(i);
                }
            }

        }
    }
}