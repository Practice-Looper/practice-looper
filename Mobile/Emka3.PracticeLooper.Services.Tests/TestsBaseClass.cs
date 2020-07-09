// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Mappings;

namespace Emka3.PracticeLooper.Services.Tests
{
    public class TestsBaseClass
    {
        public TestsBaseClass()
        {
            Factory.GetResolver().BuildContainer();
        }
    }
}
