// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using AVFoundation;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class AudioPlayer : IAudioPlayer
    {
        public AudioPlayer()
        {
        }

        public void Play(AudioSource source)
        {
            try
            {
                AVAudioPlayer av = AVAudioPlayer.FromUrl(new NSUrl(new Uri(source.Source).AbsoluteUri));
                av.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
