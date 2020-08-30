// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.TemplateSelectors
{
    [Preserve(AllMembers = true)]
    public class AudioSourceTypeTemplateSelector : DataTemplateSelector
    {
        #region Properties
        public DataTemplate SpotifyTemplate { get; set; }
        public DataTemplate AudioFileTemplate { get; set; }
        public DataTemplate DeletedTemplate { get; set; }
        #endregion

        #region Methods

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is SessionViewModel session)
            {
                if (session?.Session.AudioSource == null)
                {
                    return DeletedTemplate;
                }

                switch (session.Session.AudioSource?.Type)
                {
                    case AudioSourceType.LocalInternal:
                        return AudioFileTemplate;
                    case AudioSourceType.Spotify:
                        return SpotifyTemplate;
                    default:
                        break;
                }
            }

            if (item is AudioSourceVieModel vm)
            {
                switch (vm.AudioType)
                {
                    case AudioSourceType.LocalInternal:
                        return AudioFileTemplate;
                    case AudioSourceType.Spotify:
                        return SpotifyTemplate;
                    default:
                        break;
                }
            }

            return AudioFileTemplate;
        }
        #endregion
    }
}
