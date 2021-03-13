// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    [Preserve(AllMembers = true)]
    public class WebPlayerLoginViewModel : ViewModelBase
    {
        public string Service { get; private set; }

        public override Task InitializeAsync(object parameter)
        {
            if (parameter is string service)
            {
                Service = service;
            }

            return Task.CompletedTask;
        }
    }
}
