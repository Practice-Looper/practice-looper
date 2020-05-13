﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using System.Threading.Tasks;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Services.Contracts
{
    [Preserve(AllMembers = true)]
    public interface IDialogService
    {
        Task ShowAlertAsync(string message, string caption = "Error occured");
        Task<bool> ShowConfirmAsync(string caption, string message, string negative, string confirm);
        Task<string> ShowPromptAsync(string title, string message, string accept, string cancel, string placeholder, int maxLength = 250);
    }
}
