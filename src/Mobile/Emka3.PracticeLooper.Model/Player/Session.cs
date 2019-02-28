// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Collections.Generic;

namespace Emka3.PracticeLooper.Model.Player
{
    /// <summary>
    /// Represents a session containing all loops and the file to play.
    /// </summary>
    public class Session
    {

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Emka3.PracticeLooper.Model.Player.Session"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="loops">Loops.</param>
        public Session(string name, string filePath, IList<Loop> loops)
        {
            Name = name;
            FilePath = filePath;
            Loops = loops;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the session.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; }

        /// <summary>
        /// Gets the loops.
        /// </summary>
        /// <value>The loops.</value>
        public IList<Loop> Loops { get; }
        #endregion
    }
}
