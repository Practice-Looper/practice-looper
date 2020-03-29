// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Threading.Tasks;
using Emka3.PracticeLooper.Utils;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    [Preserve(AllMembers = true)]
    public interface IFileRepository
    {
        Task<string> SaveFileAsync(string fileName, byte[] data);
        Task DeleteFileAsync(string fileName);
    }
}
