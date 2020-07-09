// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Emka3.PracticeLooper.Services.Common;

namespace Emka3.PracticeLooper.Services.Tests.TestData
{
    public class RepositoryTests
    {
        private readonly SessionsDbRepository sessionsDbRepository;
        public RepositoryTests()
        {

            sessionsDbRepository = new SessionsDbRepository();
        }
    }
}
