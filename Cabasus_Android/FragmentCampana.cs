using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Cabasus_Android.Screens;

namespace Cabasus_Android
{
    public class FragmentCampana : Fragment
    {

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var NumeroNotificacion = View.FindViewById<TextView>(Resource.Id.numnotificacion);
            NumeroNotificacion.Text = new ShareInside().ConsultarEstaodNotificacion().ToString();

            try
            {
                if (NumeroNotificacion.Text == "0")
                {
                    var valornoti = View.FindViewById<RelativeLayout>(Resource.Id.relativenoti);
                    valornoti.Alpha = 0;
                }
                else
                {
                    var valornoti = View.FindViewById<RelativeLayout>(Resource.Id.relativenoti);
                }
            }
            catch (Exception)
            {
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.FragmentCampana, container,false);
        }

       
    }
}