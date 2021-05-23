﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Xamarin.Forms;
using MappingsFactory = Emka3.PracticeLooper.Mappings;

namespace Emka.PracticeLooper.Mobile.Views
{
    [Preserve(AllMembers = true)]
    public class LoopButton : ImageButton
    {
        #region Fields

        private readonly IConfigurationService configurationService;
        #endregion

        #region Ctor

        public LoopButton()
        {
            var resolver = MappingsFactory.Factory.GetResolver();
            configurationService = resolver.Resolve<IConfigurationService>() ?? throw new ArgumentNullException(nameof(configurationService));
        }
        #endregion
    }
}
