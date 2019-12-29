// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019

using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;

namespace Emka.PracticeLooper.Mobile.Navigation
{
    public interface INavigationService
    {
        // Returns the view model type associated with the previous page in the navigation stack.
        ViewModelBase PreviousePageViewModel { get; }

        // Performs navigation to one of two pages when the app is launched.
        Task InitializeAsync();

        // Performs hierarchical navigation to a specified page.
        Task NavigateToAsync<TViewModel>() where TViewModel : ViewModelBase;

        // Performs hierarchical navigation to a specified page, passing a parameter.
        Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : ViewModelBase;

        // Return to the previous page in the navigation stack
        Task GoBackAsync();

        // Removes the previous page from the navigation stack.
        Task RemoveLastFromStackAsync();

        // Removes all the previous pages from the navigation stack.
        Task RemoveBackStackAsync();
    }
}
