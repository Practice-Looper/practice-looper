// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model;

namespace Emka3.PracticeLooper.Services.Contracts.Rest
{
    public interface IAuthenticationService
    {
        #region Methods
        Task<AuthenticationResponse> AuthenticateAsync();
        #endregion Methods
    }
}
