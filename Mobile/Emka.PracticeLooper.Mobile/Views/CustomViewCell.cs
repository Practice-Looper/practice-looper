// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka.PracticeLooper.Mobile.Extensions;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Mappings;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public class CustomViewCell : ViewCell
    {
        #region Fields
        /// <summary>
        /// The SelectedBackgroundColor property.
        /// </summary>
        public static readonly BindableProperty SelectedBackgroundColorProperty =
            BindableProperty.Create("SelectedBackgroundColor", typeof(Color), typeof(CustomViewCell), Color.Default);
        public static readonly BindableProperty FontColorProperty =
            BindableProperty.Create("FontColor", typeof(Color), typeof(CustomViewCell), Color.Default);
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly ILogger logger;
        ListView parent = null;
        #endregion

        #region Ctor

        public CustomViewCell()
        {
            configurationService = Factory.GetResolver().Resolve<IConfigurationService>();
            dialogService = Factory.GetResolver().Resolve<IDialogService>();
            logger = Factory.GetResolver().Resolve<ILogger>();
            configurationService.ValueChanged += ConfigValueChanged;
        }
        #endregion

        #region Properties

        public Color SelectedBackgroundColor
        {
            get { return (Color)GetValue(SelectedBackgroundColorProperty); }
            set { SetValue(SelectedBackgroundColorProperty, value); }
        }

        public Color FontColor
        {
            get { return (Color)GetValue(FontColorProperty); }
            set { SetValue(FontColorProperty, value); }
        }
        #endregion

        #region Methods

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (View != null)
            {
                parent = VisualTreeHelper.GetParent<ListView>(View);
            }
            else
            {
                logger.LogError(new Exception("View NullReference in CustomViewCell"));
                dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                return;
            }

            if (parent != null)
            {
                parent.ItemSelected += ListViewItemSelected;
                if (parent.SelectedItem == BindingContext)
                {
                    View.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
                    FontColor = (Color)Application.Current.Resources["BackgroundColor"];
                }
                else
                {
                    View.BackgroundColor = (Color)Application.Current.Resources["BackgroundColor"];
                    FontColor = (Color)Application.Current.Resources["PrimaryColor"];
                }
            }
            else
            {
                logger.LogError(new Exception("parent NullReference in CustomViewCell"));
                dialogService.ShowAlertAsync(AppResources.Error_Content_General, AppResources.Error_Caption);
                return;
            }
        }

        private void ListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null && e.SelectedItem == BindingContext)
            {
                View.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
                FontColor = (Color)Application.Current.Resources["BackgroundColor"];
            }
            else
            {
                View.BackgroundColor = Color.Transparent;
                FontColor = (Color)Application.Current.Resources["PrimaryColor"];
            }
        }

        private void ConfigValueChanged(object sender, string e)
        {
            if (e == PreferenceKeys.ColorScheme)
            {
                OnAppearing();
            }
        }
        #endregion
    }
}
