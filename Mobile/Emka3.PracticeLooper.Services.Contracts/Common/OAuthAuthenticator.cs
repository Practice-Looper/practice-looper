// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Xamarin.Auth;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public class OAuthAuthenticator : OAuth2Authenticator
    {
        public OAuthAuthenticator(string clientId, string clientSecret, string scope, Uri authorizeUrl, Uri redirectUrl, Uri accessTokenUrl, GetUsernameAsyncFunc getUsernameAsync = null, bool isUsingNativeUI = false) : base(clientId, clientSecret, scope, authorizeUrl, redirectUrl, accessTokenUrl, getUsernameAsync, isUsingNativeUI)
        {
        }

        protected override void OnPageEncountered(Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
        {
            // Remove state from dictionaries. 
            // We are ignoring request state forgery status 
            // as we're hitting an ASP.NET service which forwards 
            // to a third-party OAuth service itself
            if (query.ContainsKey("state"))
            {
                query.Remove("state");
            }

            if (fragment.ContainsKey("state"))
            {
                fragment.Remove("state");
            }

            base.OnPageEncountered(url, query, fragment);
        }
    }
}
