// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Navigation;
using Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.ViewModels.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase()
        {
            NavigationService = Factory.GetResolver().Resolve<INavigationService>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract Task InitializeAsync(object parameter);

        public INavigationService NavigationService { get; set; }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
