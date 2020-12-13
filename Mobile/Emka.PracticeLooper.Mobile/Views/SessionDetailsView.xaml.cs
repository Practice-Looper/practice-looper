// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Services.Contracts;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public partial class SessionDetailsView : ContentPage
    {
        public SessionDetailsView()
        {
            InitializeComponent();
            var resolver = Emka3.PracticeLooper.Mappings.Factory.GetResolver();
            BindingContext = new SessionDetailsViewModel(resolver.Resolve<IConfigurationService>(), resolver.Resolve<IDialogService>());
        }
    }
}
