// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka3.PracticeLooper.Services.Contracts.Common;
using Emka3.PracticeLooper.Utils;
using Xamarin.Essentials;

namespace Emka3.PracticeLooper.Services.Common
{
    [Preserve(AllMembers = true)]
    public class PermissionsManager : IPermissionsManager
    {
        #region Methods

        public async Task<bool> CheckStorageWritePermissionAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            return permissionStatus == PermissionStatus.Granted;
        }

        public async Task RequestStorageWritePermissionAsync()
        {
            await Permissions.RequestAsync<Permissions.StorageWrite>();
        }
        #endregion
    }
}
