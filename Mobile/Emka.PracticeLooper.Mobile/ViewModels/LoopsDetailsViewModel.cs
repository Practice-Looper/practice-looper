// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class LoopsDetailsViewModel : ViewModelBase
    {
        private SessionViewModel session;
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
        #endregion

        #region Methods

        public override Task InitializeAsync(object parameter)
        {
            if (parameter is SessionViewModel session)
            {
                Session = session;
                foreach (var item in session.Session.Loops)
                {
                    Loops.Add(new LoopViewModel(item));
                }
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
