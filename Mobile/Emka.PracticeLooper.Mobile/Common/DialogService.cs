// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Common
{
    [Preserve(AllMembers = true)]
    public class DialogService : IDialogService
    {
        public DialogService()
        {
        }

        public async Task ShowAlertAsync(string message, string caption = "Error occured")
        {
            await MainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage?.DisplayAlert(caption, message, "OK"));
        }

        public async Task<bool> ShowConfirmAsync(string caption, string message, string negative, string confirm)
        {
            return await MainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage?.DisplayAlert(caption, message, confirm, negative));
        }

        public async Task<string> ShowPromptAsync(string title, string message, string accept, string cancel, string placeholder, int maxLength = 250)
        {
            return await MainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage?.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength));
        }
    }
}
