using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Moq;
using Xunit;

namespace Emka3.PracticeLooper.Config.Tests
{
    public class ConfigurationServiceTests
    {
        private IConfigurationService configurationService;
        private readonly Mock<IPersistentConfigService> persistentConfigServiceMock;
        private readonly Mock<IConfigurationLoader> configurationLoaderMock;
        private IDictionary<string, object> persistedValues;

        public ConfigurationServiceTests()
        {
            persistentConfigServiceMock = new Mock<IPersistentConfigService>();
            configurationLoaderMock = new Mock<IConfigurationLoader>();
            persistedValues = new Dictionary<string, object>();
        }

        [Theory]
        [InlineData("string", "stringValue")]
        [InlineData("int", 666)]
        public void When_SetValue_Expect_CanReadConfigValue(string key, object value)
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object);
            configurationService.SetValue(key, value);
            var getValueMethodInfo = typeof(ConfigurationService).GetMethods().First(m => m.Name == "GetValue" && m.IsGenericMethod);
            var method = getValueMethodInfo.MakeGenericMethod(new[] { value.GetType() });
            var result = method.Invoke(configurationService, new[] { key, default });

            Assert.NotNull(result);
            Assert.Equal(result, value);
        }

        [Fact]
        public void When_UpdatesExistingValue_Expect_HasNewValue()
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object);
            configurationService.SetValue("test", "testValue");
            var result = configurationService.GetValue("test");

            Assert.NotNull(result);
            Assert.Equal("testValue", result);

            configurationService.SetValue("test", "newTestValue");

            result = configurationService.GetValue("test");

            Assert.NotNull(result);
            Assert.Equal("newTestValue", result);
        }

        [Fact]
        public void When_UpdatesValue_Expect_ChangedEventFires()
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object);
            string changedValue = string.Empty;
            configurationService.ValueChanged += (s, e) =>
            {
                changedValue = e;
            };

            configurationService.SetValue("a", "b");
            Assert.Equal("a", changedValue);
            Assert.Equal("b", configurationService.GetValue("a"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void When_SetValueWithInValidKey_Expect_ThrowsArgumentException(string key)
        {
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configurationLoaderMock.Object);
            Assert.Throws<ArgumentException>(() => configurationService.SetValue(key, null));
        }

        [Fact]
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
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object, configLoader);
            await configurationService.SetValueAsync("magicNumber", 666, true);
            var magicNumber = await configurationService.GetValueAsync<int>("magicNumber");

            Assert.Equal(666, magicNumber);
        }
    }
}
