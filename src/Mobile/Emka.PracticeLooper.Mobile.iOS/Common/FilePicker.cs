// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;
using MobileCoreServices;
using UIKit;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    public class FilePicker : IFilePicker
    {
        public event System.EventHandler<FileAudioSource> SourceSelected;

        void IFilePicker.ShowPicker()
        {
            string result = string.Empty;
            var docPicker = new UIDocumentPickerViewController(new string[] { UTType.Audio }, UIDocumentPickerMode.Import);
            docPicker.AllowsMultipleSelection = false;
            docPicker.WasCancelled += (s, cancelArgs) =>
            {
                OnSourceSelected(null);
            };

            docPicker.DidPickDocumentAtUrls += (s, e) =>
            {
                result = e.Urls[0].Path;
                OnSourceSelected(new FileAudioSource(result));
            };

            var window = UIApplication.SharedApplication.KeyWindow;
            var currentViewController = window.RootViewController;
            //while (currentViewController.PresentedViewController != null)
            //{
            currentViewController.PresentViewController(docPicker, true, null);
            //}
        }

        private void OnSourceSelected(FileAudioSource audioSource)
        {
            SourceSelected?.Invoke(this, audioSource);
        }
    }
}
