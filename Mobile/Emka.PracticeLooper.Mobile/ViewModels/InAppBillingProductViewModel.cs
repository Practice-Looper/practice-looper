// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using System.Threading.Tasks;
using Emka.PracticeLooper.Mobile.ViewModels.Common;
using Plugin.InAppBilling;

namespace Emka.PracticeLooper.Mobile.ViewModels
{
    public class InAppBillingProductViewModel : ViewModelBase
    {
        #region Fields
        private bool purchased;
        #endregion

        #region Ctor

        public InAppBillingProductViewModel(InAppBillingProduct model)
        {
            Model = model;
        }
        #endregion

        #region Properties

        public InAppBillingProduct Model { get; }
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
