// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Config.Contracts;
using Emka3.PracticeLooper.Config.Feature;
using NUnit.Framework;

namespace Emka3.PracticeLooper.Config.Tests
{
    [TestFixture()]
    public class FeatureRegistryTests
    {
        FeatureRegistry registry;

        public FeatureRegistryTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            registry = new FeatureRegistry();
        }

        [Test()]
        public void When_AddsFeature_Expect_Has1Feature()
        {
            var feature = new MyAwesomeFeature();
            registry.Add(feature, false);

            var fetchedFeature = registry.GetFeature<MyAwesomeFeature>();
            var isEnabled = registry.IsEnabled<MyAwesomeFeature>();
            Assert.NotNull(fetchedFeature);
            Assert.AreEqual(feature.StoreId, fetchedFeature.StoreId);
            Assert.IsFalse(isEnabled);
        }

        [Test()]
        public void When_GetNotAddedFeature_Expect_ReturnsNull()
        {
            var fetchedFeature = registry.GetFeature<MyAwesomeFeature>();
            Assert.Null(fetchedFeature);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task When_RegisterForUpdates_ByType_Expect_CallbackInvoked(bool isEnabled)
        {
            var tcs = new TaskCompletionSource<bool>();
            var feature = new MyAwesomeFeature();
            registry.Add(feature, isEnabled);
            registry.RegisterForUpdates<MyAwesomeFeature>(e =>
            {
                tcs.SetResult(e);
            });

            registry.Update<MyAwesomeFeature>(!isEnabled);
            var taaskResult = await tcs.Task;

            var featureEnabledState = registry.IsEnabled<MyAwesomeFeature>();
            Assert.AreEqual(featureEnabledState, taaskResult);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task When_RegisterForUpdates_ByStoreId_Expect_CallbackInvoked(bool isEnabled)
        {
            var tcs = new TaskCompletionSource<bool>();
            var feature = new MyAwesomeFeature();
            registry.Add(feature, isEnabled);
            registry.RegisterForUpdates<MyAwesomeFeature>(e =>
            {
                tcs.SetResult(e);
            });

            registry.Update(feature.StoreId, !isEnabled);
            var taskResult = await tcs.Task;

            var featureEnabledState = registry.IsEnabled<MyAwesomeFeature>();
            Assert.AreEqual(featureEnabledState, taskResult);
        }

        [Test()]
        public void When_UpdateNotRegisteredFeature_Expect_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => registry.Update<MyAwesomeFeature>(false));
            Assert.Throws<InvalidOperationException>(() => registry.Update("this id doesn't even exist!", false));
        }

        [Test()]
        public void When_ClearRegistry_Expect_NoItemsLeft()
        {
            var initialState = true;
            var feature = new MyAwesomeFeature();
            registry.Add(feature, initialState);
            registry.Clear();

            // when no items, this method always returns FALSE;
            var newState = registry.IsEnabled<MyAwesomeFeature>();

            Assert.IsFalse(newState);
            Assert.AreNotSame(initialState, newState);
        }
    }

    internal class MyAwesomeFeature : IFeature
    {
        public string StoreId => "SexyFeature";
    }

    internal class AnotherAwesomeFeature : IFeature
    {
        public string StoreId => "AnotherSexyFeature";
    }
}
