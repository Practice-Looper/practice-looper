// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka3.PracticeLooper.Model.Common;

namespace Emka3.PracticeLooper.Model.Player
{
    /// <summary>
    /// Represents a single loop.
    /// </summary>
    public class Loop : EntityBase
    {
        private double _startPosition;
        private double _endPosition;

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Emka3.PracticeLooper.Model.Player.Loop"/> struct.
        /// </summary>
        public Loop()
        {
        }
        #endregion

        #region Events
        public event EventHandler<double> StartPositionChanged;
        public event EventHandler<double> EndPositionChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the loop.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start position.
        /// </summary>
        /// <value>The start position.</value>
        public double StartPosition
        {
            get => _startPosition;
            set
            {
                _startPosition = value;
                RaiseStartPositionChanged();
            }
        }

        /// <summary>
        /// Gets or sets the end position.
        /// </summary>
        /// <value>The end position.</value>
        public double EndPosition
        {
            get => _endPosition;
            set
            {
                _endPosition = value;
                RaiseEndPositionChanged();
            }
        }

        /// <summary>
        /// Gets or sets the number of repititions.
        /// </summary>
        /// <value>The repititions.</value>
        public int Repititions { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }
        #endregion Properties

        #region Methods
        private void RaiseStartPositionChanged()
        {
            StartPositionChanged?.Invoke(this, StartPosition);
        }

        private void RaiseEndPositionChanged()
        {
            EndPositionChanged?.Invoke(this, EndPosition);
        }
        #endregion
    }
}
