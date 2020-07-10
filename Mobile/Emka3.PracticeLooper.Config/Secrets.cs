// Copyright (C) Emka3 - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// simonsymhoven post@simon-symhoven.de, 2020
using System;
using System.IO;
using Newtonsoft.Json;

namespace Emka3.PracticeLooper.Config
{
    public static class Secrets
    {
        public static dynamic GetSecrets()
        {
            string json = File.ReadAllText("secrets.json");
            return JsonConvert.DeserializeObject(json);

        }
    }
}
