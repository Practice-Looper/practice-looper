// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Extensions;
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
        #endregion

        #region Ctor

        public CustomViewCell()
        {
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
            var parent = VisualTreeHelper.GetParent<ListView>(View);
            parent.ItemSelected += ListViewItemSelected;
            if (parent != null && BindingContext != null && parent.SelectedItem == BindingContext)
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
        #endregion
    }
}
