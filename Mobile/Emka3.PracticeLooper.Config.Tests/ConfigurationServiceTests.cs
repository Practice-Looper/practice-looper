using System;
using System.Linq;
using Emka3.PracticeLooper.Config.Contracts;
using Moq;
using Xunit;

namespace Emka3.PracticeLooper.Config.Tests
{
    public class ConfigurationServiceTests
    {
        private readonly IConfigurationService configurationService;
        private readonly Mock<IPersistentConfigService> persistentConfigServiceMock;

        public ConfigurationServiceTests()
        {
            persistentConfigServiceMock = new Mock<IPersistentConfigService>();
            configurationService = new ConfigurationService(persistentConfigServiceMock.Object);
        }

        [Theory]
        [InlineData("string", "stringValue")]
        [InlineData("int", 666)]
        public void When_SetValue_Expect_CanReadConfigValue(string key, object value)
        {
            configurationService.SetValue(key, value);
            var getValueMethodInfo = typeof(ConfigurationService).GetMethods().First(m => m.Name == "GetValue" && m.IsGenericMethod);
            var method = getValueMethodInfo.MakeGenericMethod(new[] { value.GetType() });
            var result = method.Invoke(configurationService, new[] { key });

            Assert.NotNull(result);
            Assert.Equal(result, value);
        }

        [Fact]
        public void When_UpdatesExistingValue_Expect_HasNewValue()
        {
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
            string changedValue = string.Empty;
            configurationService.ValueChanged += (s, e) =>
            {
                changedValue = e;
            };

            configurationService.SetValue("a", "b");
            Assert.Equal(string.Empty, changedValue);
            Assert.Equal("b", configurationService.GetValue("a"));

            configurationService.SetValue("a", "c");
            Assert.Equal("c", configurationService.GetValue("a"));
            Assert.Equal("a", changedValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void When_SetValueWithInValidKey_Expect_ThrowsArgumentException(string key)
        {
            Assert.Throws<ArgumentException>(() => configurationService.SetValue(key, null));
        }
    }
}
