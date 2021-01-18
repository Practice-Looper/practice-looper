// (c) Liveit. All rights reserved. 2018
// Maksim Kolesnik, David Symhoven

using Emka3.PracticeLooper.Model.Player;

namespace Emka3.PracticeLooper.Services.Contracts.EventArgs
{
    /// <summary>
    /// Token changed event arguments.
    /// </summary>
    public class TokenChangedEventArgs : System.EventArgs
    {
        #region Fields
        #endregion Fields

        #region Ctor
        public TokenChangedEventArgs(AudioSourceType sourceType, string token)
        {
            SourceType = sourceType;
            Token = token;
        }

        #endregion Ctor

        #region Properties
        public AudioSourceType SourceType { get; }
        public string Token { get; }
        #endregion Properties
    }
}
