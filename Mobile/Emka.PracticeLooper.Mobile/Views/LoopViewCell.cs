// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Utils;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public class LoopViewCell : CustomViewCell
    {
        public LoopViewCell()
        {
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            if (BindingContext is LoopViewModel vm && vm.Loop.IsDefault && ContextActions?.Count != 0)
            {
                ContextActions?.Clear();
            }
        }
    }
}
