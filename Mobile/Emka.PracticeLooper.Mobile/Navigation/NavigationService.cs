// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Mobile.Views;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Model.Common;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.Navigation
{
    [Preserve(AllMembers = true)]
    public class NavigationService : INavigationService
    {
        #region Ctor

        public NavigationService()
        {
            Logger = Factory.GetResolver().Resolve<ILogger>();
            Tracker = Factory.GetResolver().Resolve<IAppTracker>();
        }
        #endregion

        #region Properties

        public ViewModelBase PreviousePageViewModel => throw new NotImplementedException();

        public ILogger Logger { get; }
        public IAppTracker Tracker { get; }
        #endregion Properties

        #region Methods
        public async Task GoBackAsync()
        {
            var navigationPage = Application.Current.MainPage as CustomNavigationView;
            if (navigationPage != null)
            {
                await navigationPage.PopAsync();
            }
        }

        public Task InitializeAsync()
        {
            return NavigateToAsync<MainViewModel>();
        }

        public Task NavigateToAsync<TViewModel>() where TViewModel : ViewModelBase
        {
            return InternalNavigateToAsync(typeof(TViewModel), null);
        }

        public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : ViewModelBase
        {
            return InternalNavigateToAsync(typeof(TViewModel), parameter);
        }

        private async Task InternalNavigateToAsync(Type viewModelType, object parameter)
        {
            try
            {
                Page page = CreatePage(viewModelType, parameter);

                if (page is MainView)
                {
                    Application.Current.MainPage = new CustomNavigationView(page);
                }
                else
                {
                    var navigationPage = Application.Current.MainPage as CustomNavigationView;
                    if (navigationPage != null)
                    {
                        await navigationPage.PushAsync(page);
                    }
                    else
                    {
                        Application.Current.MainPage = new CustomNavigationView(page);
                    }
                }

                await Tracker?.TrackAsync(TrackerEvents.Navigation, new Dictionary<string, string>()
                {
                    { "TargetPage",  page?.ToString()}
                });
                await (page.BindingContext as ViewModelBase).InitializeAsync(parameter);
            }
            catch (Exception ex)
            {
                await Logger?.LogErrorAsync(ex);
                throw;
            }
        }

        public Task RemoveBackStackAsync()
        {
            throw new NotImplementedException();
        }

        public Task RemoveLastFromStackAsync()
        {
            throw new NotImplementedException();
        }

        private Type GetPageTypeForViewModel(Type viewModelType)
        {
            try
            {
                var viewName = viewModelType.FullName.Replace("Model", string.Empty);
                var viewModelAssemblyName = Assembly.GetAssembly(viewModelType).FullName;
                var viewAssemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewModelAssemblyName);
                var viewType = Type.GetType(viewAssemblyName);
                return viewType;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex);
                throw;
            }
        }

        private Page CreatePage(Type viewModelType, object parameter)
        {
            Type pageType = GetPageTypeForViewModel(viewModelType);
            if (pageType == null)
            {
                throw new Exception($"Cannot locate page for type for {viewModelType}");
            }
            Page page = null;
            try
            {
                page = Activator.CreateInstance(pageType) as Page;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex);
                throw;
            }
            return page;
        }
        #endregion Properties
    }
}
