// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Linq;
using CoreTelephony;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;

namespace Emka.PracticeLooper.Mobile.iOS.Common
{
    [Preserve(AllMembers = true)]
    public class ConnectivityService : IConnectivityService
    {
        public ConnectivityService()
        {
        }

        public bool HasFastConnection()
        {
            var profiles = Connectivity.ConnectionProfiles;
            var telephonyInfo = new CTTelephonyNetworkInfo();
            Foundation.NSDictionary<Foundation.NSString, Foundation.NSString> networkType = telephonyInfo.ServiceCurrentRadioAccessTechnology;

            if (Connectivity.NetworkAccess == NetworkAccess.None || Connectivity.NetworkAccess == NetworkAccess.Unknown)
            {
                return false;
            }

            if (profiles.Contains(ConnectionProfile.WiFi) && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                return true;
            }
            else if (profiles.Contains(ConnectionProfile.Cellular) && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var values = networkType.Values.Select(s => s.ToString()).ToList();

                if (values.Contains("CTRadioAccessTechnologyCDMA1x"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyCDMAEVDORev0"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyCDMAEVDORevA"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyCDMAEVDORevB"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyEdge"))
                {
                    return false;
                }
                else if (values.Contains("CTRadioAccessTechnologyGPRS"))
                {
                    return false;
                }
                else if (values.Contains("CTRadioAccessTechnologyHSDPA"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyHSUPA"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyLTE"))
                {
                    return true;
                }
                else if (values.Contains("CTRadioAccessTechnologyWCDMA"))
                {
                    return false;
                }
                else if (values.Contains("CTRadioAccessTechnologyeHRPD"))
                {
                    return false;
                }
            }

            return false;
        }
    }
}
