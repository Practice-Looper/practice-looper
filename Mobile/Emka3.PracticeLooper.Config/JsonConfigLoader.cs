// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Emka3.PracticeLooper.Config
{
    [Preserve(AllMembers = true)]
    public class JsonConfigLoader : IConfigurationLoader
    {
        public IDictionary<string, object> LoadConfiguration(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            string json;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            try
            {
                JObject conf = JObject.Parse(json);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(conf.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
