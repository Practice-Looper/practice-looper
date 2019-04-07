﻿// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Collections.Generic;
using Emka3.PracticeLooper.Model.Common;

namespace Emka3.PracticeLooper.Model.Player
{
    /// <summary>
    /// Represents a session containing all loops and the file to play.
    /// </summary>
    public class Session : EntityBase
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name of the session.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public AudioSource AudioSource { get; set; }

        /// <summary>
        /// Gets or sets the loops.
        /// </summary>
        /// <value>The loops.</value>
        public List<Loop> Loops { get; set; }
        #endregion
    }
}
