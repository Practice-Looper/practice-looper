// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class EditSessionView : Rg.Plugins.Popup.Pages.PopupPage
    {

        public EditSessionView(Session session)
        {
            InitializeComponent();
            BindingContext = session;
        }

        private async void OnClose(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void OnSave(object sender, System.EventArgs e)
        {
            if (BindingContext is Session session)
            {
                MessagingCenter.Send<object, Session>(this, MessengerKeys.UpdateSession, session);
                await PopupNavigation.Instance.PopAsync();
            }
        }
    }
}
