// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public class CircleImage : Image
    {
        public CircleImage()
        {
        }

        public Color BorderColor { get; set; }
        public byte BorderWidth { get; set; }
    }
}
