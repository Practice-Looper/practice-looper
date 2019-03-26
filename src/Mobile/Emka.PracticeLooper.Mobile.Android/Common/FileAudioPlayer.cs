// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        MediaPlayer player;
        Session session;
        CancellationTokenSource cancelTokenSource;
        #endregion

        #region Ctor
        public FileAudioPlayer()
        {
            player = new MediaPlayer();
        }
        #endregion

        #region Properties
        public bool IsPlaying => player.IsPlaying;

        public double SongDuration { get; set; }

        public Loop CurrentLoop { get; set; }

        public event EventHandler<bool> PlayStatusChanged;
        #endregion

        #region Methods
        public void Init(Session session)
        {
            this.session = session;
            CurrentLoop = session.Loops[0];
            CurrentLoop.StartPositionChanged += OnStartPositionChanged;
            CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            player.Reset();
            player.SetDataSource(session.AudioSource.Source);
            player.Prepare();
            cancelTokenSource = new CancellationTokenSource();
            //var asset = AVAsset.FromUrl(new NSUrl(new Uri(session.AudioSource.Source).AbsoluteUri));
            //var playerItem = new AVPlayerItem(asset);
            //audioPlayer = new AVPlayer(playerItem);
            //var playerLayer = AVPlayerLayer.FromPlayer(audioPlayer);

            //TimeObserver = audioPlayer.AddBoundaryTimeObserver(new[] { NSValue.FromCMTime(CMTime.FromSeconds(CurrentLoop.EndPosition * SongDuration, 1)) }, null, () =>
            //{
            //    Seek(CurrentLoop.StartPosition);
            //});
            //player.Play();

            //audioPlayer = AVAudioPlayer.FromUrl(new NSUrl(new Uri(audioSource.Source).AbsoluteUri));
            ////audioPlayer2.Set
            //audioPlayer.PrepareToPlay();
            SongDuration = player.Duration;
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            if (IsPlaying)
            {
                cancelTokenSource.Cancel();
                cancelTokenSource = new CancellationTokenSource();
                Task.Run(async () => await StartRangeTimer(e), cancelTokenSource.Token);
            }
        }

        private async Task StartRangeTimer(double time)
        {
            while (!cancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    var delta =(CurrentLoop.EndPosition - CurrentLoop.StartPosition) * SongDuration;
                    var t = TimeSpan.FromMilliseconds(delta).TotalMilliseconds;
                    Console.WriteLine("waiting for " + t);
                    await Task.Delay(Convert.ToInt32(t), cancelTokenSource.Token);
                    Seek(CurrentLoop.StartPosition);
                }
                catch (TaskCanceledException)
                {

                }
            }
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            cancelTokenSource.Cancel();
            Seek(e);
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                player.Pause();
                RaisePlayingStatusChanged();
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                player.Start();
                RaisePlayingStatusChanged();
            }
        }

        public void Seek(double time)
        {
            try
            {
                var offset = Convert.ToInt32(time * SongDuration);
                player.SeekTo(offset);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }
        #endregion
    }
}
