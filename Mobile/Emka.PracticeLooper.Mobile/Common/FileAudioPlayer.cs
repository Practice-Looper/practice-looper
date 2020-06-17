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
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class FileAudioPlayer : IAudioPlayer
    {
        #region Fields
        double internalSongDuration;
        private IMediaItem mediaItem;
        private bool isActive = false;
        private object locker = new object();
        private bool pausedByUser;
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
        public bool Initialized { get; private set; }
        public bool IsPlaying => CrossMediaManager.Current != null && CrossMediaManager.Current.IsPlaying();
        public double SongDuration { get { return internalSongDuration * 1000; } }
        private Loop CurrentLoop { get; set; }
        private int CurrentStartPosition { get; set; }
        private int CurrentEndPosition { get; set; }

        public AudioSourceType Type => AudioSourceType.Local;

        public string DisplayName => "File";

        public bool PausedByUser
        {
            get
            {
                lock (locker)
                {
                    return pausedByUser;
                }
            }
            private set
            {
                lock (locker)
                {
                    pausedByUser = value;
                }
            }
        }

        public bool IsActive
        {
            get
            {
                lock (locker)
                {
                    return isActive;
                }
            }
            set
            {
                lock (locker)
                {
                    isActive = value;
                }
            }
        }
        #endregion

        #region Methods

        public void GetCurrentPosition(Action<double> callback)
        {
            callback?.Invoke(CrossMediaManager.Current.Position.TotalMilliseconds);
        }

        public void Init(Loop loop)
        {
            var task = Task.Run(async () => await InitAsync(loop));
            task.Wait();
        }

        public void Pause(bool triggeredByUser = true)
        {
            var task = Task.Run(async () => await PauseAsync(triggeredByUser));
            task.Wait();
        }

        public void Play()
        {
            var task = Task.Run(async () => await PlayAsync());
            task.Wait();
        }

        public void Seek(double time)
        {
            var task = Task.Run(async () => await SeekAsync(time));
            task.Wait();
        }

        private void OnStartPositionChanged(object sender, double e)
        {
            if (IsPlaying)
            {
                Play();
            }
        }

        private void OnEndPositionChanged(object sender, double e)
        {
            if (IsPlaying)
            {
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
            /**/
            if (e.State == MediaManager.Player.MediaPlayerState.Paused && IsActive && !PausedByUser)
            {
                try
                {
                    await PlayAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            PlayStatusChanged?.Invoke(this, e.State == MediaManager.Player.MediaPlayerState.Playing);
        }

        public async Task PlayAsync()
        {
            try
            {
                CrossMediaManager.Current.StateChanged -= PlayerStateChanged;
                CrossMediaManager.Current.KeepScreenOn = true;
                CurrentStartPosition = (int)(CurrentLoop.StartPosition * internalSongDuration);
                CurrentEndPosition = (int)(CurrentLoop.EndPosition * internalSongDuration);
                CrossMediaManager.Current.StateChanged += PlayerStateChanged;
                await CrossMediaManager.Current.Play(mediaItem, TimeSpan.FromSeconds(CurrentStartPosition), TimeSpan.FromSeconds(CurrentEndPosition));

                IsActive = true;
                PausedByUser = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task PauseAsync(bool triggeredByUser = true)
        {
            if (IsPlaying)
            {
                try
                {
                    PausedByUser = triggeredByUser;
                    IsActive = false;
                    await CrossMediaManager.Current.Pause();
                    CrossMediaManager.Current.StateChanged -= PlayerStateChanged;
                    RaisePlayingStatusChanged();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task SeekAsync(double time)
        {
            try
            {
                await CrossMediaManager.Current.SeekTo(TimeSpan.FromMilliseconds(time));
            }
            catch (Exception)
            {
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

            if (CrossMediaManager.Current.IsPlaying())
            {
                await PauseAsync();
                CrossMediaManager.Current.PositionChanged -= OnPositionChanged;
                CrossMediaManager.Current.StateChanged -= PlayerStateChanged;
            }

            try
            {
                CurrentLoop = loop;
                internalSongDuration = loop.Session.AudioSource.Duration;
                mediaItem = await CrossMediaManager.Current.Extractor.CreateMediaItem(loop.Session.AudioSource.Source);

                CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                CrossMediaManager.Current.Notification.ShowPlayPauseControls = false;
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.PositionChanged += OnPositionChanged;
                CurrentLoop.StartPositionChanged += OnStartPositionChanged;
                CurrentLoop.EndPositionChanged += OnEndPositionChanged;
                Initialized = true;
                PausedByUser = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ~FileAudioPlayer()
        {
            if (CurrentLoop != null)
            {
                CurrentLoop.StartPositionChanged -= OnStartPositionChanged;
                CurrentLoop.EndPositionChanged -= OnEndPositionChanged;
            }

            if (CrossMediaManager.Current != null)
            {
                CrossMediaManager.Current.PositionChanged -= OnPositionChanged;
                CrossMediaManager.Current.StateChanged -= PlayerStateChanged;
            }
        }
#endregion
    }
}
