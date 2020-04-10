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
        bool pausedByUser;
        private bool isActive = false;
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

        public void Pause()
        {
            var task = Task.Run(async () => await PauseAsync());
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
            CurrentStartPosition = ConvertToInt(e);
            if (IsPlaying)
            {
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
            if (e.State == MediaManager.Player.MediaPlayerState.Paused && isActive && !pausedByUser)
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

            if (e.State == MediaManager.Player.MediaPlayerState.Playing)
            {
                isActive = true;
            }

            PlayStatusChanged?.Invoke(this, e.State == MediaManager.Player.MediaPlayerState.Playing);
        }

        public async Task PlayAsync()
        {
            try
            {
                CurrentStartPosition = (int)(CurrentLoop.StartPosition * internalSongDuration);
                CurrentEndPosition = (int)(CurrentLoop.EndPosition * internalSongDuration);
                await CrossMediaManager.Current.Play(mediaItem, TimeSpan.FromSeconds(CurrentStartPosition), TimeSpan.FromSeconds(CurrentEndPosition));
                pausedByUser = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task PauseAsync()
        {
            if (IsPlaying)
            {
                try
                {
                    pausedByUser = true;
                    isActive = false;
                    await CrossMediaManager.Current.Pause();
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

                CrossMediaManager.Current.PositionChanged += OnPositionChanged;
                CrossMediaManager.Current.StateChanged += PlayerStateChanged;

                mediaItem = await CrossMediaManager.Current.Extractor.CreateMediaItem(loop.Session.AudioSource.Source);

                CurrentLoop.StartPositionChanged += OnStartPositionChanged;
                CurrentLoop.EndPositionChanged += OnEndPositionChanged;
                Initialized = true;
                pausedByUser = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ~FileAudioPlayer()
        {
            CurrentLoop.StartPositionChanged -= OnStartPositionChanged;
            CurrentLoop.EndPositionChanged -= OnEndPositionChanged;
            CrossMediaManager.Current.PositionChanged -= OnPositionChanged;
            CrossMediaManager.Current.StateChanged -= PlayerStateChanged;
        }
        #endregion
    }
}
