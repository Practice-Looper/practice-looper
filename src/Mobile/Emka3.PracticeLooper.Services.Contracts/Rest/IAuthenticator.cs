// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
namespace Emka3.PracticeLooper.Services.Contracts.Rest
{
    /// <summary>
    /// Authenticator.
    /// </summary>
    public interface IAuthenticator<T>
    {
        /// <summary>
        /// Gets the generic authenticator.
        /// </summary>
        /// <value>The generic authenticator.</value>
        T Instance { get; }
    }
}
