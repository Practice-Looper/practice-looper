// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Android.Content;
using Android.Views;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    [BroadcastReceiver]
    [Android.App.IntentFilter(new[] { Intent.ActionMediaButton })]
    public class RemoteControlBroadCastReceiver : BroadcastReceiver
    {
        public RemoteControlBroadCastReceiver()
        {
        }
        
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != Intent.ActionMediaButton)
                return;

            var key = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
            if (key.Action != KeyEventActions.Down)
                return;

            switch (key.KeyCode)
            {
                //case Keycode.Headsethook:
                case Keycode.MediaPlayPause:  break;
                case Keycode.MediaPlay:  break;
                case Keycode.MediaPause:break;
                case Keycode.MediaStop: break;
                case Keycode.MediaNext: break;
                case Keycode.MediaPrevious: break;
                default: return;
            }
            // Why this:?
            var remoteIntent = new Intent();
            context.StartService(remoteIntent);
        }
    }
}
