// (c) Liveit. All rights reserved. 2018
// Maksim Kolesnik, David Symhoven

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
        public TokenChangedEventArgs(string token)
        {
            Token = token;
        }
        #endregion Ctor

        #region Properties
        public string Token { get; }
        #endregion Properties
    }
}
