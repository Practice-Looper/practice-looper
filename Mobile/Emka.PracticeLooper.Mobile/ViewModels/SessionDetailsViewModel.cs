// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SessionDetailsViewModel : ViewModelBase
    {
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private SessionViewModel session;
        private LoopViewModel selectedLoop;
        private bool isBusy;
        #region Ctor

        public SessionDetailsViewModel(IConfigurationService configurationService, IDialogService dialogService)
        {
            Loops = new ObservableCollection<LoopViewModel>();
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, OnDeleteLoop);
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            UiContext = SynchronizationContext.Current;
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

        public override async Task InitializeAsync(object parameter)
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

                    var currentLoopId = configurationService.GetValue(PreferenceKeys.LastLoop, 0);
                    UiContext.Send(x => SelectedLoop = Loops.FirstOrDefault(l => l.Loop.Id == currentLoopId), null);
                }
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                await dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnDeleteLoop(LoopViewModel sender, Loop loop)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (loop == null)
            {
                throw new ArgumentNullException(nameof(loop));
            }

            var loopToRemove = Loops?.FirstOrDefault(l => l.Loop.Id == loop.Id && l.Loop.SessionId == loop.SessionId);

            if (loopToRemove != null)
            {
                Loops.Remove(loopToRemove);
            }
        }
        #endregion
    }
}
