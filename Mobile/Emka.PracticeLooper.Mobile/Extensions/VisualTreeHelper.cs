// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Extensions
{
    public static class VisualTreeHelper
    {
        public static T GetParent<T>(this Element element) where T : Element
        {
            if (element is T)
            {
                return element as T;
            }
            else
            {
                if (element.Parent != null)
                {
                    return element.Parent.GetParent<T>();
                }

                return default;
            }
        }
    }
}
