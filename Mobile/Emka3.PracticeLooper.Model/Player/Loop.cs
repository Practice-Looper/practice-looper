﻿// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka3.PracticeLooper.Model.Common;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Emka3.PracticeLooper.Model.Player
{
    /// <summary>
    /// Represents a single loop.
    /// </summary>
    [Table("Loops")]
    [Utils.Preserve(AllMembers = true)]
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

        [ForeignKey(typeof(Session))]
        public int SessionId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Session Session { get; set; }

        // <summary>
        /// Used to auto select last used loop.
        /// </summary>
        /// <value><c>true</c> if is favorite; otherwise, <c>false</c>.</value>
        public bool IsDefault { get; set; }
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

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (GetType() != obj.GetType()) return false;

            Loop s = (Loop)obj;
            return (Id == s.Id) && Name.Equals(s.Name);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion
    }
}
