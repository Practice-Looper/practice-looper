// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Utils;
using Rg.Plugins.Popup.Services;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class AlertPopUp : Rg.Plugins.Popup.Pages.PopupPage
    {
        public AlertPopUp(string message)
        {
            InitializeComponent();
            MessageLabel.Text = message;
        }

        private async void OnClose(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
