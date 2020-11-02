// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using NUnit.Framework;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Tests.ViewModelTests
{
    [TestFixture()]
    public class LoopViewModelTests
    {
        public LoopViewModelTests()
        {
        }

        [Test]
        public void When_CreateDefaultLoop_Expect_LoopCanNotBeDeleted()
        {
            var loop = new Loop
            {
                Id = 0,
                Name = "Loop",
                Session = default,
                SessionId = default,
                StartPosition = 0,
                EndPosition = 1,
                IsDefault = true
            };

            var loopVm = new LoopViewModel(loop);
            Assert.IsFalse(loopVm.CanBeDeleted);
            Assert.IsFalse(loopVm.DeleteCommand.CanExecute(null));
        }

        [Test]
        public void When_CreateDefaultLoop_Expect_LoopCanBeDeleted()
        {
            var loop = new Loop
            {
                Id = 0,
                Name = "Loop",
                Session = default,
                SessionId = default,
                StartPosition = 0,
                EndPosition = 1,
                IsDefault = false
            };

            var loopVm = new LoopViewModel(loop);
            Assert.IsTrue(loopVm.CanBeDeleted);
            Assert.IsTrue(loopVm.DeleteCommand.CanExecute(null));
        }

        [Test]
        public async Task When_ExecuteDeleteCommand_Expect_ReceiveDeleteMessage()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            MessagingCenter.Subscribe<LoopViewModel, Loop>(this, MessengerKeys.DeleteLoop, (s, l) =>
            {
                if (s is LoopViewModel && l is Loop)
                {
                    Task.Run(() => tcs.SetResult(true));
                }
            });

            var loop = new Loop
            {
                Id = 0,
                Name = "Loop",
                Session = default,
                SessionId = default,
                StartPosition = 0,
                EndPosition = 1,
                IsDefault = false
            };

            var loopVm = new LoopViewModel(loop);
            loopVm.DeleteCommand.Execute(null);
            await tcs.Task;
            Assert.IsTrue(tcs.Task.Result);
        }
    }
}
