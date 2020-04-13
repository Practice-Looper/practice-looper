// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.Views;
using Emka.PracticeLooper.Model;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Utils;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class DialogService : IDialogService
    {
        public DialogService()
        {
        }

        public async Task ShowAlertAsync(string message)
        {
            await PopupNavigation.Instance.PushAsync(new AlertPopUp(message));
        }

        public Task ShowConfirmAsync(string message, string negative, string confirm)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> ShowPromptAsync(string title, string message, string accept, string cancel, string placeholder, int maxLength = 250)
        {
            return await Application.Current.MainPage?.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength);
        }
    }
}
