// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SessionViewModel : ViewModelBase
    {
        #region Fields

        private Command deleteSessionCommand;
        private Command pickLoopCommand;
        #endregion

        #region Ctor

        public SessionViewModel(Session session)
        {
            Session = session;
        }
        #endregion

        #region Properties

        public Session Session { get; }

        public Command DeleteSessionCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command((o) => ExecuteDeleteSessionCommandAsync(o)));
        public Command PickLoopCommand => pickLoopCommand ?? (pickLoopCommand = new Command(async (o) => await ExecutePickLoopCommandAsync(o)));

        #endregion

        #region Methods

        private void ExecuteDeleteSessionCommandAsync(object o)
        {
            MainThread.BeginInvokeOnMainThread(() => MessagingCenter.Send(this, MessengerKeys.DeleteSession, this));
        }

        private async Task ExecutePickLoopCommandAsync(object o)
        {
            try
            {
                await NavigationService.NavigateToAsync<LoopsDetailsViewModel>(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
