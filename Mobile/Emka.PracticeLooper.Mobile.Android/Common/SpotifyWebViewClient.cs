// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2021
using Android.Webkit;

namespace Emka.PracticeLooper.Mobile.Droid.Common
{
    public class SpotifyWebViewClient : WebChromeClient
    {
        public SpotifyWebViewClient()
        {
        }

        public override void OnPermissionRequest(PermissionRequest request)
        {
            var resources = request.GetResources();
            foreach (var item in resources)
            {
                if (PermissionRequest.ResourceProtectedMediaId == item)
                {
                    request.Grant(resources);
                    break;
                }
            }
        }
    }
}
