// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
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
        private IRepository<Loop> looperRepository;
        private IDialogService dialogService;
        private ILogger logger;
        #endregion

        #region Ctor

        public SessionViewModel(Session session)
        {
            Session = session;
            Task.Run(() => InitializeAsync(null));
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, OnDeleteLoop);
        }

        private async void OnDeleteLoop(LoopViewModel sender, Loop loop)
        {
            try
            {
                await looperRepository.DeleteAsync(loop);
                Session.Loops.Remove(loop);
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync("Opps, failed to delete loop");
            }
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
                await logger.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync("Oops, could not delete loop.");
            }
        }

        public override Task InitializeAsync(object parameter)
        {
            looperRepository = Factory.GetResolver().Resolve<IRepository<Loop>>();
            dialogService = Factory.GetResolver().Resolve<IDialogService>();
            logger = Factory.GetResolver().Resolve<ILogger>();
            return Task.CompletedTask;
        }
        #endregion
    }
}
