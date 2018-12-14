using System;
using Android.App;
using Android.OS;

namespace Cabasus_Android
{
    public class CountDown : CountDownTimer
    {
        private Activity _act;
        private Activity _actLaunch;

        public CountDown(long millisInFuture, long counDown, Activity act, Activity actLaunch) :
        base(millisInFuture, counDown)
        {
            _act = act;
            _actLaunch = actLaunch;
        }
        public override void OnFinish()
        {
            _act.StartActivity(_actLaunch.GetType());
            _act.Finish();
        }

        public override void OnTick(long millisUntilFinished)
        {

        }
    }
}