// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

using Emka.PracticeLooper.Mobile.ViewModels;
using Emka3.PracticeLooper.Model.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.TemplateSelectors
{
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
            if (item is Session session)
            {
                if (session.AudioSource == null)
                {
                    return DeletedTemplate;
                }

                switch (session?.AudioSource?.Type)
                {
                    case AudioSourceType.Local:
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
                    case AudioSourceType.Local:
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
