using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Common;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
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
            picker.BindingContext = new RangePickerViewModel(resolver.Resolve<IDialogService>(), resolver.Resolve<ILogger>(), resolver.Resolve<INavigationService>(), resolver.Resolve<IAppTracker>());
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
                vm.IsBusy = true;
                picker.ItemsSource = null;
                picker.SelectedItem = null;
                var pickerViewModel = (picker.BindingContext as RangePickerViewModel);
                pickerViewModel.AudioSource = vm.CurrentSession.Session.AudioSource;
                pickerViewModel.Loop = vm.CurrentLoop;
                await pickerViewModel.InitializeAsync(true);
                picker.HeaderText = AppResources.LoopStartPosition;
                picker.ItemsSource = pickerViewModel.Time;
                picker.SelectedItem = pickerViewModel.SelectedTime;
                picker.SelectionChanged += SelectionChanged;
                picker.IsOpen = true;
                vm.IsBusy = false;
            }
        }

        async void OnEndPositionClicked(object sender, EventArgs e)
        {
            if (BindingContext is MainViewModel vm)
            {
                vm.IsBusy = true;
                picker.ItemsSource = null;
                picker.SelectedItem = null;
                var pickerViewModel = (picker.BindingContext as RangePickerViewModel);
                pickerViewModel.AudioSource = vm.CurrentSession.Session.AudioSource;
                pickerViewModel.Loop = vm.CurrentLoop;
                await pickerViewModel.InitializeAsync(false);
                picker.HeaderText = AppResources.LoopEndPosition;
                picker.ItemsSource = pickerViewModel.Time;
                picker.SelectedItem = pickerViewModel.SelectedTime;
                picker.SelectionChanged += SelectionChanged;
                picker.IsOpen = true;
                vm.IsBusy = false;
            }
        }

        void SelectionChanged(object sender, Syncfusion.SfPicker.XForms.SelectionChangedEventArgs e)
        {
            if (picker.ItemsSource != null && e.NewValue is IList && (picker.ItemsSource as IList).Count > 1 && (picker.SelectedItem as IList).Count > 1)
            {
                //Updated the second column collection based on first column selected value.
                if (picker.BindingContext is RangePickerViewModel vm)
                {
                    if (e.NewValue is IList newValue
                        && newValue.Count > 0
                        && e.OldValue is IList oldValue
                        && oldValue.Count > 0
                        && picker.SelectedItem is IList selectedTime
                        && selectedTime.Count > 1)
                    {
                        // minutes value changed
                        if (vm.SelectedTime[0] != newValue[0])
                        {
                            //fetch seconds
                            var itemsSourceCopy = new List<object>(picker.ItemsSource as List<object>);
                            itemsSourceCopy.RemoveAt(1);
                            itemsSourceCopy.Add(vm.GetValidSeconds(newValue[0].ToString()));
                            picker.ItemsSource = itemsSourceCopy;
                             
                            if (itemsSourceCopy != null && (itemsSourceCopy.Last() as RangeObservableCollection<object>).Count < int.Parse(selectedTime[1].ToString()))
                            {
                                var selectedSeconds = (itemsSourceCopy.Last() as RangeObservableCollection<object>).Last().ToString();
                                picker.SelectedItem = new List<object> { newValue[0], selectedSeconds };

                            }

                            if (itemsSourceCopy != null && int.Parse(selectedTime[1].ToString()) < int.Parse((itemsSourceCopy.Last() as RangeObservableCollection<object>).First().ToString()))
                            {
                                var selectedSeconds = (itemsSourceCopy.Last() as RangeObservableCollection<object>).First().ToString();
                                picker.SelectedItem = new List<object> { newValue[0], selectedSeconds };
                            }
                        }
                    }

                    if (BindingContext is MainViewModel mainVm)
                    {
                        var time = picker.SelectedItem as IList;
                        var timeString = time.Count == 1 ? $"{0}:{0}:{time[0]}" : $"{0}:{time[0]}:{time[1]}";
                        if (vm.IsStartPosition)
                        {
                            var secs = TimeSpan.Parse(timeString).TotalSeconds;
                            var currentStartTime = 100 / mainVm.CurrentSession.Session.AudioSource.Duration * secs;
                            mainVm.MinimumValue = (currentStartTime < 0 ? 0 : currentStartTime / 100);
                        }
                        else
                        {
                            var secs = TimeSpan.Parse(timeString).TotalSeconds;
                            var currentEndTime = 100 / mainVm.CurrentSession.Session.AudioSource.Duration * secs;
                            mainVm.MaximumValue = (currentEndTime > 100 ? 1 : currentEndTime / 100);
                        }
                    }

                    // update time in viewmodel
                    vm.SelectedTime.Clear();
                    vm.SelectedTime.Add((picker.SelectedItem as IList)[0]);
                    vm.SelectedTime.Add((picker.SelectedItem as IList)[1]);
                }
            }
        }

        void OnClosePicker(object sender, EventArgs e)
        {
            picker.IsOpen = !picker.IsOpen;
            picker.SelectionChanged -= SelectionChanged;
        }
        #endregion
    }
}
