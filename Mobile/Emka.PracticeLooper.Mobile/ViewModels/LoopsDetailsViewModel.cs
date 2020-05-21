﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class LoopsDetailsViewModel : ViewModelBase
    {
        private SessionViewModel session;
        private LoopViewModel selectedLoop;
        private bool isBusy;
        #region Ctor

        public LoopsDetailsViewModel()
        {
            Loops = new ObservableCollection<LoopViewModel>();
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, OnDeleteLoop);
        }
        #endregion

        #region Properties


        public SessionViewModel Session
        {
            get => session; set
            {
                session = value;
                NotifyPropertyChanged();
            }
        }

        public double SongDuration { get; set; }

        public ObservableCollection<LoopViewModel> Loops
        {
            get;
            private set;
        }

        public LoopViewModel SelectedLoop
        {
            get => selectedLoop;
            set
            {
                if (value != null && value != selectedLoop)
                {
                    selectedLoop = value;
                    MessagingCenter.Send(this, MessengerKeys.LoopChanged, selectedLoop);
                }

                NotifyPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy; set
            {
                isBusy = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Methods

        public override Task InitializeAsync(object parameter)
        {
            IsBusy = true;
            try
            {
                if (parameter is SessionViewModel session)
                {
                    Session = session;
                    foreach (var item in session.Session.Loops)
                    {
                        Loops.Add(new LoopViewModel(item));
                    }

                    var currentLoopId = Preferences.Get(PreferenceKeys.LastLoop, -1);
                    if (currentLoopId > 0)
                    {
                        MainThread.BeginInvokeOnMainThread(() => SelectedLoop = Loops.FirstOrDefault(l => l.Loop.Id == currentLoopId));
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }

            return Task.CompletedTask;
        }

        private void OnDeleteLoop(LoopViewModel sender, Loop loop)
        {
            Loops.Remove(sender);
        }
        #endregion
    }
}
