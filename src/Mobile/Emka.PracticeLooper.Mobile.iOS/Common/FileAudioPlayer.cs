// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using AVFoundation;
using CoreMedia;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Foundation;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        AVPlayer audioPlayer;
        Session session;
        #endregion


        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        #endregion

        #region Properties
        public bool IsPlaying => audioPlayer != null && audioPlayer.Rate != 0;
        public double SongDuration { get; private set; }
        public Loop CurrentLoop { get; set; }
        private NSObject TimeObserver { get; set; }
        #endregion Properties

        #region Methods
        public void Pause()
        {
            if (IsPlaying)
            {
                audioPlayer.Pause();
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                try
                {
                    audioPlayer.Play();
                    RaisePlayingStatusChanged();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Seek(double time)
        {
            var seconds = CMTime.FromSeconds(time * SongDuration, 1);
            Console.WriteLine("time " + seconds);
            if (audioPlayer != null)
            {
                audioPlayer.Seek(CMTime.FromSeconds(time * SongDuration, 1));
            }
        }

        // https://stackoverflow.com/questions/12902410/trying-to-understand-cmtime
        public void Init(Session session)
        {
            this.session = session;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            var asset = AVAsset.FromUrl(new NSUrl(new Uri(session.AudioSource.Source).AbsoluteUri));
            var playerItem = new AVPlayerItem(asset);
            audioPlayer = new AVPlayer(playerItem);
            var playerLayer = AVPlayerLayer.FromPlayer(audioPlayer);

            TimeObserver = audioPlayer.AddBoundaryTimeObserver(new[] { NSValue.FromCMTime(CMTime.FromSeconds(CurrentLoop.EndPosition * SongDuration, 1)) }, null, () =>
            {
                Seek(CurrentLoop.StartPosition);
            });
            //player.Play();

            //audioPlayer = AVAudioPlayer.FromUrl(new NSUrl(new Uri(audioSource.Source).AbsoluteUri));
            ////audioPlayer2.Set
            //audioPlayer.PrepareToPlay();
            SongDuration = audioPlayer.CurrentItem.Asset.Duration.Seconds;
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            audioPlayer.RemoveTimeObserver(TimeObserver);
            TimeObserver = audioPlayer.AddBoundaryTimeObserver(new[] { NSValue.FromCMTime(CMTime.FromSeconds(CurrentLoop.EndPosition * SongDuration, 1)) }, null, () =>
            {
                Seek(CurrentLoop.StartPosition);
            });
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            Seek(e);
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }
        #endregion
    }
}
