using System;
using System.Collections;
using System.Diagnostics;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Mappings.Contracts;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Services.Contracts.Player;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class MainView : ContentPage
    {
        #region Fields

        private readonly IConfigurationService configService;
        private IResolver resolver;
        #endregion

        #region Ctor
        public MainView()
        {
            InitializeComponent();

            resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            configService = resolver.Resolve<IConfigurationService>();
            configService.ValueChanged += ConfigService_ValueChanged;
            BindingContext = new MainViewModel(resolver.Resolve<IInterstitialAd>(),
                   resolver.Resolve<IRepository<Session>>(),
                   resolver.Resolve<IRepository<Loop>>(),
                   resolver.Resolve<IDialogService>(),
                   resolver.Resolve<IFileRepository>(),
                   resolver.Resolve<ISourcePicker>(),
                   resolver.Resolve<ISpotifyLoader>(),
                   resolver.Resolve<Common.IFilePicker>(),
                   resolver.Resolve<IConnectivityService>(),
                   resolver.Resolve<INavigationService>(),
                   resolver.Resolve<ILogger>(),
                   resolver.Resolve<IAppTracker>(),
                   configService);
        }

        private void ConfigService_ValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.PremiumGeneral)
            {
                ToggleAd();
            }
        }
        #endregion

        #region Properties
        public string AdUnitId { get; private set; }
        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            SettingsImage.Color = (Color)Application.Current.Resources["PrimaryColor"];
            ToggleAd();
        }

        private void ToggleAd()
        {
            if (configService.GetValue<bool>(PreferenceKeys.PremiumGeneral))
            {
                MainGrid.Children.Remove(AdmobBanner);
            }
            else
            {
                AdUnitId = App.BannerAddUnitId;
            }
        }

        void OnDraggingCompleted(object sender, EventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if (viewModel != null && viewModel.CurrentLoop != null)
            {
                viewModel.UpdateMinMaxValues();
            }
        }

        void OnDeleteSession(object sender, EventArgs e)
        {
            var menuItem = ((MenuItem)sender);
            var mainViewModel = BindingContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.DeleteSessionCommand.Execute(menuItem.CommandParameter);
            }
        }

        async void OnStartPositionClicked(object sender, EventArgs e)
        {
            if (BindingContext is MainViewModel vm)
            {
                var rangeVm = new RangePickerViewModel(vm.CurrentSession.Session.AudioSource, vm.CurrentLoop, true, resolver.Resolve<IDialogService>(), resolver.Resolve<ILogger>(), resolver.Resolve<INavigationService>(), resolver.Resolve<IAppTracker>());
                await rangeVm.InitializeAsync(default);
                picker.BindingContext = rangeVm;
                picker.IsOpen = true;
                picker.HeaderText = AppResources.LoopStartPosition;
            }
        }

        async void OnEndPositionClicked(object sender, EventArgs e)
        {
            if (BindingContext is MainViewModel vm)
            {
                var rangeVm = new RangePickerViewModel(vm.CurrentSession.Session.AudioSource, vm.CurrentLoop, false, resolver.Resolve<IDialogService>(), resolver.Resolve<ILogger>(), resolver.Resolve<INavigationService>(), resolver.Resolve<IAppTracker>());
                await rangeVm.InitializeAsync(default);
                picker.BindingContext = rangeVm;
                picker.IsOpen = true;
                picker.HeaderText = AppResources.LoopEndPosition;
            }
        }

        void SelectionChanged(object sender, Syncfusion.SfPicker.XForms.SelectionChangedEventArgs e)
        {
            if (picker.ItemsSource != null && e.NewValue is IList && (picker.ItemsSource as IList).Count > 1)
            {
                //Updated the second column collection based on first column selected value.
                if (picker.BindingContext is RangePickerViewModel vm)
                {
                    var time = (IList)e.NewValue;
                    if (e.OldValue != null)
                    {
                        vm.UpdateSeconds(time[0].ToString(), time[1].ToString());

                        if (BindingContext is MainViewModel mainVm && picker.SelectedItem is IList selectedTime)
                        {
                            if (vm.IsStartPosition)
                            {
                                var secs = TimeSpan.Parse($"{0}:{selectedTime[0]}:{selectedTime[1]}").TotalSeconds;
                                var currentStartTime = 100 / mainVm.CurrentSession.Session.AudioSource.Duration * secs;
                                mainVm.MinimumValue = (currentStartTime < 0 ? 0 : currentStartTime / 100);
                            }
                            else
                            {
                                var secs = TimeSpan.Parse($"{0}:{selectedTime[0]}:{selectedTime[1]}").TotalSeconds;
                                var currentEndTime = 100 / mainVm.CurrentSession.Session.AudioSource.Duration * secs;
                                Debug.WriteLine($"currentEndTime {currentEndTime}");
                                mainVm.MaximumValue = (currentEndTime > 100 ? 1 : currentEndTime / 100);
                            }
                        }
                    }
                }
            }
        }

        void OnClosePicker(object sender, EventArgs e)
        {
            picker.IsOpen = !picker.IsOpen;
        }
        #endregion
    }
}
