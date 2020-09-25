// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Emka3.PracticeLooper.Model.Common;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class InAppBillingProductViewModel : ViewModelBase
    {
        #region Fields
        private bool purchased;
        #endregion

        #region Ctor

        public InAppBillingProductViewModel(InAppPurchaseProduct model)
        {
            Model = model;
        }
        #endregion

        #region Properties

        public InAppPurchaseProduct Model { get; }
        public bool Purchased
        {
            get => purchased; set
            {
                purchased = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Methods

        public override Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }
        #endregion Methods
    }
}
