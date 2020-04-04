// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.Messenger;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class DialogService
    {
        private readonly Page page;

        public DialogService(Page page)
        {
            this.page = page;
            MessagingCenter.Subscribe<ShowDialogArgs>(this, MessengerKeys.ShowDialog, OnShowAlert);
        }

        private async void OnShowAlert(ShowDialogArgs obj)
        {
            await page?.DisplayAlert(obj.Caption, obj.Message, obj.ButtonText);
        }
    }
}
