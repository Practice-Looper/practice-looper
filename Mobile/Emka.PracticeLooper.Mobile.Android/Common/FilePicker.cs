// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka3.PracticeLooper.Model.Player;
using Emka3.PracticeLooper.Services.Contracts.Common;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class FilePicker : IFilePicker
    {
        #region Ctor
        public FilePicker()
        {
        }
        #endregion

        #region Properties
        public event EventHandler<AudioSource> SourceSelected;
        #endregion

        #region Methods
        public void ShowPicker()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
