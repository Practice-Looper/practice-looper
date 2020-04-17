// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka.PracticeLooper.Model;
using Emka3.PracticeLooper.Model.Player;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class LoopViewModel : ViewModelBase
    {
        #region Fields

        private Command deleteSessionCommand;
        #endregion

        #region Ctor

        public LoopViewModel(Loop loop)
        {
            Loop = loop;
        }
        #endregion

        #region Properties

        public Command DeleteCommand => deleteSessionCommand ?? (deleteSessionCommand = new Command((o) => ExecuteDeleteCommandAsync(o)));

        public Loop Loop { get; }
        #endregion

        #region Methods

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }


        private void ExecuteDeleteCommandAsync(object o)
        {
            MessagingCenter.Send(this, MessengerKeys.DeleteLoop, Loop);
        }
        #endregion
    }
}
