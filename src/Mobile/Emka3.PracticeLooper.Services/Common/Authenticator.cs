// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka3.PracticeLooper.Config;
using Emka3.PracticeLooper.Services.Contracts.Rest;
using Xamarin.Auth;

namespace Emka3.PracticeLooper.Services.Common
{
    public class Authenticator : IAuthenticator<OAuth2Authenticator>
    {
        readonly IConfigurationService configService;

        public Authenticator()
        {
            this.configService = Factory.GetConfigService();
            Instance = new OAuth2Authenticator(
          configService.GetValue("auth:spotify:client:id"),
          configService.GetValue("auth:spotify:client:secret"),
          configService.GetValue("auth:spotify:client:scopes"),
          new Uri(configService.GetValue("auth:spotify:client:uri:auth")),
          new Uri(configService.GetValue("auth:spotify:client:uri:redirect")),
          new Uri(configService.GetValue("auth:spotify:client:uri:redirect")),
          null,
          true);
        }

        public OAuth2Authenticator Instance { get; }
    }
}
