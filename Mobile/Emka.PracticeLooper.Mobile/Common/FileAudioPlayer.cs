// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Player;
using MediaManager;
using MediaManager.Library;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        double internalSongDuration;
        private IMediaItem mediaItem;
        private IMediaManager mediaManager;
        bool pausedByUser;
        #endregion

        #region Ctor
        public FileAudioPlayer()
        {
        }
        #endregion


        #region Events
        public event EventHandler<bool> PlayStatusChanged;
        public event EventHandler CurrentTimePositionChanged;
        public event EventHandler TimerElapsed;
        #endregion

        #region Propeties
        public bool IsPlaying => mediaManager != null && mediaManager.IsPlaying();
        public double SongDuration { get { return internalSongDuration * 1000; } }
        private Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }
        #endregion

        #region Methods

        public void GetCurrentPosition(Action<double> callback)
        {
            callback?.Invoke(mediaManager.Position.TotalMilliseconds);
        }

        public void Init(Loop loop)
        {
            var task = Task.Run(() => InitAsync(loop));
            task.Wait();
        }

        public void Pause()
        {
            Task.Run(PauseAsync);
        }

        public void Play()
        {
            Task.Run(PlayAsync);
        }

        public void Seek(double time)
        {
            Task.Run(() => SeekAsync(time));
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            CurrentStartPosition = ConvertToInt(e);
            if (IsPlaying)
            {
                Pause();
                Seek(e);
                Play();
            }
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            CurrentEndPosition = ConvertToInt(e);
            if (IsPlaying)
            {
                Pause();
                Seek(e);
                Play();
            }
        }

        private int ConvertToInt(double inValue)
        {
            int result;
            try
            {
                result = Convert.ToInt32(inValue * internalSongDuration);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private void RaisePlayingStatusChanged()
        {
            PlayStatusChanged?.Invoke(this, IsPlaying);
        }

        private void OnPositionChanged(object sender, MediaManager.Playback.PositionChangedEventArgs e)
        {
            CurrentTimePositionChanged?.Invoke(this, new EventArgs());
        }

        private async void PlayerStateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            if (e.State == MediaManager.Player.MediaPlayerState.Paused && !pausedByUser)
            {
                try
                {
                    await PlayAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task PlayAsync()
        {
            if (!IsPlaying)
            {
                try
                {
                    CurrentStartPosition = (int)(CurrentLoop.StartPosition * internalSongDuration);//ConvertToInt(CurrentLoop.StartPosition);
                    CurrentEndPosition = (int)(CurrentLoop.EndPosition * internalSongDuration);//ConvertToInt(CurrentLoop.StartPosition);
                    await mediaManager.Play(mediaItem, TimeSpan.FromSeconds(CurrentStartPosition), TimeSpan.FromSeconds(CurrentEndPosition)).ConfigureAwait(false);
                    RaisePlayingStatusChanged();
                    pausedByUser = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public async Task PauseAsync()
        {
            if (IsPlaying)
            {
                try
                {
                    pausedByUser = true;
                    await mediaManager.Pause();
                    RaisePlayingStatusChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public async Task SeekAsync(double time)
        {
            try
            {
                await mediaManager.SeekTo(TimeSpan.FromMilliseconds(time)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task InitAsync(Loop loop)
        {
            if (loop == null)
            {
                throw new ArgumentNullException(nameof(loop));
            }

            if (loop.Session == null)
            {
                throw new ArgumentNullException(nameof(loop.Session));
            }

            if(mediaManager != null)
            {
                await PauseAsync();
                mediaManager.StateChanged -= PlayerStateChanged;
                mediaManager.PositionChanged -= OnPositionChanged;
                mediaManager = null;
            }

            try
            {
                CurrentLoop = loop;

                internalSongDuration = loop.Session.AudioSource.Duration;

                mediaManager = CrossMediaManager.Current;

                mediaManager.StateChanged += PlayerStateChanged;
                mediaManager.PositionChanged += OnPositionChanged;

                mediaItem = await mediaManager.Extractor.CreateMediaItem(loop.Session.AudioSource.Source);

                CurrentLoop.StartPositionChanged += OnStartPositionChanged;
                CurrentLoop.EndPositionChanged += OnEndPositionChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}
