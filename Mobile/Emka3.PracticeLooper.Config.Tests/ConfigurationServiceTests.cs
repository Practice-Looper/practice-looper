// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Emka3.PracticeLooper.Config.Contracts;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Emka3.PracticeLooper.Config.Tests
{
    [TestFixture()]
    public class ConfigurationServiceTests
    {
        private IConfigurationService configurationService;
        private readonly Mock<IPersistentConfigService> persistentConfigServiceMock;
        private readonly Mock<IConfigurationLoader> configurationLoaderMock;
        private readonly Mock<ISecureConfigService> secureConfigServiceMock;
        private IDictionary<string, object> persistedValues;

        public ConfigurationServiceTests()
        {
            persistentConfigServiceMock = new Mock<IPersistentConfigService>();
            configurationLoaderMock = new Mock<IConfigurationLoader>();
            secureConfigServiceMock = new Mock<ISecureConfigService>();
            persistedValues = new Dictionary<string, object>();
        }

        [Test()]
        public void When_ReadJsonConfigs_Expect_Has2ConfigValues()
        {
            configurationLoaderMock
                .Setup(c => c.LoadConfiguration(It.Is<string>(s => s.Contains("testSecrets.json"))))
                .Returns(() =>
                {
                    return ReadConfig("Emka3.PracticeLooper.ConfigTests.testSecrets.json");
                });

            var configService = new ConfigurationService(new PersistentConfigService(), configurationLoaderMock.Object, new SecureConfigService());
            configService.ReadConfigs("Emka3.PracticeLooper.ConfigTests.testSecrets.json");
            var intConfig = configService.GetValue<int>("practice");
            var stringConfig = configService.GetValue<string>("looper");
            Assert.NotNull(intConfig);
            Assert.AreEqual(intConfig, 30);
            Assert.NotNull(stringConfig);
            Assert.AreEqual(stringConfig, "string");
        }

        [TestCase("string", "stringValue")]
        [TestCase("int", 666)]
        public void When_SetValue_Expect_CanReadConfigValue(string key, object value)
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            configurationService.SetValue(key, value);
            var getValueMethodInfo = typeof(ConfigurationService).GetMethods().First(m => m.Name == "GetValue" && m.IsGenericMethod);
            var method = getValueMethodInfo.MakeGenericMethod(new[] { value.GetType() });
            var result = method.Invoke(configurationService, new[] { key, default });

            Assert.NotNull(result);
            Assert.AreEqual(result, value);
        }

        [Test()]
        public void When_UpdatesExistingValue_Expect_HasNewValue()
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            configurationService.SetValue("test", "testValue");
            var result = configurationService.GetValue("test");

            Assert.NotNull(result);
            Assert.AreEqual("testValue", result);

            configurationService.SetValue("test", "newTestValue");

            result = configurationService.GetValue("test");

            Assert.NotNull(result);
            Assert.AreEqual("newTestValue", result);
        }

        [Test()]
        public void When_UpdatesValue_Expect_ChangedEventFires()
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            string changedValue = string.Empty;
            configurationService.ValueChanged += (s, e) =>
            {
                changedValue = e;
            };

            configurationService.SetValue("a", "b");
            Assert.AreEqual("a", changedValue);
            Assert.AreEqual("b", configurationService.GetValue("a"));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void When_SetValueWithInValidKey_Expect_ThrowsArgumentException(string key)
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            Assert.Throws<ArgumentException>(() => configurationService.SetValue(key, null));
        }

        [Test()]
        public async Task When_SetValueAndPersist_Expect_ValuePersisted()
        {
            persistentConfigServiceMock
                .Setup(p => p.PersistValue(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Type>()))
                .Callback((string key, object value, Type type) =>
                {
                    persistedValues.Add(key, value);
                });

            persistentConfigServiceMock.Setup(p => p.GetPersistedValue(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string key, object defaultValue) =>
                {
                    var result = (int)persistedValues[key];
                    return result;
                });
            var configLoader = new JsonConfigLoader();
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configLoader, secureConfigServiceMock.Object);
            await configurationService.SetValueAsync("magicNumber", 666, true);
            var magicNumber = await configurationService.GetValueAsync<int>("magicNumber");

            Assert.AreEqual(666, magicNumber);
        }


        [TestCase("", "abc")]
        [TestCase(" ", "abc")]
        [TestCase(null, "abc")]
        [TestCase("abc", "")]
        [TestCase("abc", " ")]
        [TestCase("abc", null)]
        public void When_SetSecureValueInvalidArguments_ThrowsArgumentNullException(string key, object value)
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            Assert.Throws<ArgumentNullException>(() => configurationService.SetSecureValue(key, value));
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await configurationService.SetSecureValueAsync(key, value));
            Assert.NotNull(ex);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void When_GetSecureValueInvalidKey_ThrowsArgumentNullException(string key)
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            Assert.Throws<ArgumentNullException>(() => configurationService.GetSecureValue(key));
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await configurationService.GetSecureValueAsync(key));
            Assert.NotNull(ex);
        }

        [Test]
        public void When_ClearValue_Expect_ValueDeletedFromAllStorageVariations()
        {
            configurationLoaderMock
                .Setup(c => c.LoadConfiguration(It.Is<string>(s => s.Contains("testSecrets.json"))))
                .Returns(() =>
                    {
                        return ReadConfig("Emka3.PracticeLooper.ConfigTests.testSecrets.json");
                    });

            secureConfigServiceMock.Setup(s => s.ClearValue(It.IsAny<string>())).Verifiable();
            persistentConfigServiceMock.Setup(p => p.ClearValue(It.IsAny<string>())).Verifiable();
            secureConfigServiceMock.Setup(s => s.SetSecureValue(It.IsAny<string>(), It.IsAny<object>()));

            var configService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            configService.ReadConfigs("Emka3.PracticeLooper.ConfigTests.testSecrets.json");
            var key = "key";
            var value = "value";
            configService.SetValue(key, value, true);
            configService.SetSecureValue(key, value);

            configService.ClearValue(key);
            secureConfigServiceMock.Verify(s => s.ClearValue(It.IsAny<string>()), Times.Once);
            persistentConfigServiceMock.Verify(p => p.ClearValue(It.IsAny<string>()), Times.Once);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void When_ClearValue_InvalidArgument_Expect_ThrowsArgumentNulltException(string key)
        {
            configurationLoaderMock
                .Setup(c => c.LoadConfiguration(It.Is<string>(s => s.Contains("testSecrets.json"))))
                .Returns(() =>
                {
                    return ReadConfig("Emka3.PracticeLooper.ConfigTests.testSecrets.json");
                });

            var configService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object, secureConfigServiceMock.Object);
            configService.ReadConfigs("Emka3.PracticeLooper.ConfigTests.testSecrets.json");
            var ex = Assert.Throws<ArgumentNullException>(() => configService.ClearValue(key));
            Assert.NotNull(ex);
        }

        private IDictionary<string, object> ReadConfig(string path)
        {
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
