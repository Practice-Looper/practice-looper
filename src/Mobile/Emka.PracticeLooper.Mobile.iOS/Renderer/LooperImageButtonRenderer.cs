// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(LooperImageButton), typeof(LooperImageButtonRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class LooperImageButtonRenderer : ImageButtonRenderer
    {
        public LooperImageButtonRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
        {
            base.OnElementChanged(e);

        }
    }
}
