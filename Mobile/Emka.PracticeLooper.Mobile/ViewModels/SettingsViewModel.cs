// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields
        private readonly IDialogService dialogService;
        private readonly IConfigurationService configurationService;
        private Command showDataPrivacyCommand;
        private Command showThirdPartyComponentsCommand;
        #endregion

        #region Ctor

        public SettingsViewModel(
            ILogger logger,
            IDialogService dialogService,
            IAppTracker appTracker,
            INavigationService navigationService,
            IConfigurationService configurationService)
            : base(navigationService, logger, appTracker)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService)); 
        }
        #endregion

        #region Properties
        public Command ShowDataPrivacyCommand => showDataPrivacyCommand ?? new Command(async () => await ExecuteShowDataPrivacyCommand());
        public Command ShowThirdPartyComponentsCommand => showThirdPartyComponentsCommand ?? new Command(async () => await ExecuteShowThirdPartyComponentsCommand());

        public string AppVersion => VersionTracking.CurrentVersion;
        #endregion

        #region Methods
        private async Task ExecuteShowThirdPartyComponentsCommand()
        {
            try
            {
                var thirdPartyRawUrl = await configurationService.GetValueAsync(PreferenceKeys.ThirdPartyComponentsUrl, "https://www.practice-looper.com");
                await Browser.OpenAsync(thirdPartyRawUrl, new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Hide
                });
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
            }
        }

        private async Task ExecuteShowDataPrivacyCommand()
        {
            try
            {
                var dataPrivacyUrl = await configurationService.GetValueAsync(PreferenceKeys.DataPrivacyUrl, "https://www.practice-looper.com");
                await Browser.OpenAsync(dataPrivacyUrl, new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Hide
                });
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
            }
        }

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
