// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
namespace Emka.PracticeLooper.Mobile.Messenger
{
    public class ShowDialogArgs
    {
        public ShowDialogArgs(string caption, string message, string buttonText = "OK")
        {
            Caption = caption;
            Message = message;
            ButtonText = buttonText;
        }

        public string Caption { get; }
        public string Message { get; }
        public string ButtonText { get; }
    }
}
