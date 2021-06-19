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
        private Command updateSessionCommand;
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
            MessagingCenter.Subscribe<object, Session>(this, MessengerKeys.UpdateSession, (sender, arg) => OnUpdateSession(arg));
        }

        #endregion

        #region Properties
        public string Name
        {
            get => Session?.Name;
            set
            {
                Session.Name = value;
                NotifyPropertyChanged();
            }
        }

        public string Artist
        {
            get => Session?.Artist;
            set
            {
                Session.Artist = value;
                NotifyPropertyChanged();
            }
        }

        public string CoverSource
        {
            get => Session?.CoverSource;
            set
            {
                Session.CoverSource = value;
                NotifyPropertyChanged();
            }
        }

        public Session Session { get; private set; }
        public Command DeleteSessionCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command((o) => ExecuteDeleteSessionCommandAsync(o)));
        public Command UpdateSessionCommand => updateSessionCommand ?? (updateSessionCommand = new Command((o) => ExecuteUpdateSessionCommand(o)));
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

        private async void ExecuteUpdateSessionCommand(object o)
        {
            await dialogService.ShowEditSessionDialog(Session);
        }

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }

        private void OnUpdateSession(Session session)
        {
            if (session != null &&  session.Id == Session.Id)
            {
                Session = session;
                NotifyPropertyChanged(nameof(Artist));
                NotifyPropertyChanged(nameof(Name));
            }
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
