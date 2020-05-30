// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Emka.PracticeLooper.Mobile.Common
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public void InsertRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            foreach (var item in items)
                Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
