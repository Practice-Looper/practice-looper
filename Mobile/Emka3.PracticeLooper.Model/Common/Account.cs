// (c) Liveit. All rights reserved. 2017
// Maksim Kolesnik, David Symhoven

using System.Collections.Generic;

namespace Emka3.PracticeLooper.Model.Common
{
    /// <summary>
    /// Local account for credentials storage.
    /// </summary>
    public class Account
    {
        #region Ctor
        public Account()
        {
            Properties = new Dictionary<string, string>();
        }
        #endregion Ctor

        #region Properties

        public IDictionary<string, string> Properties { get; private set; }

        public string Token => Properties["spotify_token"] ?? string.Empty;
        #endregion Properties
    }
}
