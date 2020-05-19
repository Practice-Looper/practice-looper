// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Linq;
using Android.Runtime;
using Android.Telephony;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Xamarin.Essentials;

namespace Emka.PracticeLooper.Mobile.Droid.Common
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
            TelephonyManager tm = TelephonyManager.FromContext(Android.App.Application.Context);

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
                switch (tm.NetworkType)
                {
                    //case TelephonyManager.NETWORK_TYPE_1xRTT:
                    case NetworkType.OneXrtt:
                        return false; // ~ 50-100 kbps
                                      //case TelephonyManager.NETWORK_TYPE_CDMA:
                    case NetworkType.Cdma:
                        return false; // ~ 14-64 kbps
                                      //case TelephonyManager.NETWORK_TYPE_EDGE:
                    case NetworkType.Edge:
                        return false; // ~ 50-100 kbps
                                      //case TelephonyManager.NETWORK_TYPE_EVDO_0:
                    case NetworkType.Evdo0:
                        return true; // ~ 400-1000 kbps
                                     //case TelephonyManager.NETWORK_TYPE_EVDO_A:
                    case NetworkType.EvdoA:
                        return true; // ~ 600-1400 kbps
                                     //case TelephonyManager.NETWORK_TYPE_GPRS:
                    case NetworkType.Gprs:
                        return false; // ~ 100 kbps
                                      //case TelephonyManager.NETWORK_TYPE_HSDPA:
                    case NetworkType.Hsdpa:
                        return true; // ~ 2-14 Mbps
                                     //case TelephonyManager.NETWORK_TYPE_HSPA:
                    case NetworkType.Hspa:
                        return true; // ~ 700-1700 kbps
                                     //case TelephonyManager.NETWORK_TYPE_HSUPA:
                    case NetworkType.Hsupa:
                        return true; // ~ 1-23 Mbps
                                     //case TelephonyManager.NETWORK_TYPE_UMTS:
                    case NetworkType.Umts:
                        return true; // ~ 400-7000 kbps
                    /*
	                 * Above API level 7, make sure to set android:targetSdkVersion
	                 * to appropriate level to use these
	                 */
                    //case TelephonyManager.NETWORK_TYPE_EHRPD: // API level 11
                    case NetworkType.Ehrpd:
                        return true; // ~ 1-2 Mbps
                                     //case TelephonyManager.NETWORK_TYPE_EVDO_B: // API level 9
                    case NetworkType.EvdoB:
                        return true; // ~ 5 Mbps
                                     //case TelephonyManager.NETWORK_TYPE_HSPAP: // API level 13
                    case NetworkType.Hspap:
                        return true; // ~ 10-20 Mbps
                                     //case TelephonyManager.NETWORK_TYPE_IDEN: // API level 8
                    case NetworkType.Iden:
                        return false; // ~25 kbps
                                      //case TelephonyManager.NETWORK_TYPE_LTE: // API level 11
                    case NetworkType.Lte:
                        return true; // ~ 10+ Mbps
                                     // Unknown
                                     //case TelephonyManager.NETWORK_TYPE_UNKNOWN:
                    case NetworkType.Unknown:
                        return false;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}
