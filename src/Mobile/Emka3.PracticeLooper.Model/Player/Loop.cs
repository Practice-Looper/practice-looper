// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
namespace Emka3.PracticeLooper.Model.Player
{
    /// <summary>
    /// Represents a single loop.
    /// </summary>
    public struct Loop
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Emka3.PracticeLooper.Model.Player.Loop"/> struct.
        /// </summary>
        /// <param name="startPosition">Start position.</param>
        /// <param name="endPosition">End position.</param>
        /// <param name="repititions">Repititions.</param>
        public Loop(int startPosition, int endPosition, int repititions)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Repititions = repititions;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the start position.
        /// </summary>
        /// <value>The start position.</value>
        public int StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the end position.
        /// </summary>
        /// <value>The end position.</value>
        public int EndPosition { get; set; }

        /// <summary>
        /// Gets or sets the number of repititions.
        /// </summary>
        /// <value>The repititions.</value>
        public int Repititions { get; set; }
        #endregion Properties
    }
}
