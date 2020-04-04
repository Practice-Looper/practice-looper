// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Messenger;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase()
        {
            NavigationService = Factory.GetResolver().Resolve<INavigationService>();
            Logger = Factory.GetResolver().Resolve<ILogger>();
            Tracker = Factory.GetResolver().Resolve<IAppTracker>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract Task InitializeAsync(object parameter);

        public INavigationService NavigationService { get; set; }
        protected ILogger Logger { get; }
        protected IAppTracker Tracker { get; }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected async Task ShowErrorDialogAsync(Exception ex = default)
        {
            await Task.Run(() => ShowErrorDialog(ex));
        }

        protected void ShowErrorDialog(Exception ex = default)
        {
            MessagingCenter.Send(this, MessengerKeys.ShowDialog, new ShowDialogArgs("Error", "Oops, that should not have happened."));
        }
    }
}
