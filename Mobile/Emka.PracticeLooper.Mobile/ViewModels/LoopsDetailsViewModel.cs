// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Model.Player;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class LoopsDetailsViewModel : ViewModelBase
    {
        private SessionViewModel session;
        private Command deleteSessionCommand;
        private double songDuration;
        #region Ctor

        public LoopsDetailsViewModel()
        {
            Loops = new ObservableCollection<Loop>();
        }
        #endregion

        #region Properties
        public Command DeleteCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command(async (o) => await ExecuteDeleteCommandAsync(o)));

        public SessionViewModel Session
        {
            get => session; set
            {
                session = value;
                NotifyPropertyChanged();
            }
        }

        public double SongDuration { get => songDuration; set => songDuration = value; }

        public ObservableCollection<Loop> Loops
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
                    Loops.Add(item);
                }
            }

            return Task.CompletedTask;
        }

        private async Task ExecuteDeleteCommandAsync(object o)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
