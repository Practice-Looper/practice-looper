// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Text;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SessionViewModel : ViewModelBase
    {
        #region Fields

        private Command deleteSessionCommand;
        private Command pickLoopCommand;
        private readonly IDialogService dialogService;
        #endregion

        #region Ctor

        public SessionViewModel(Session session,
            IDialogService dialogService,
            ILogger logger,
            INavigationService navigationService,
            IAppTracker appTracker)
            : base(navigationService, logger, appTracker)
        {
            Session = session;
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
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
            MessagingCenter.Send(this, MessengerKeys.DeleteSession, this);
        }

        private async Task ExecutePickLoopCommandAsync(object o)
        {
            try
            {
                await NavigationService?.NavigateToAsync<SessionDetailsViewModel>(this);
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                await dialogService?.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
        }

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"session: {Session}");
            builder.AppendLine($"audio source: {Session?.AudioSource}");
            builder.AppendLine($"loops: {Session?.Loops}");
            return builder.ToString();
        }
        #endregion
    }
}
