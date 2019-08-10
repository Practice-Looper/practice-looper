// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using System.ComponentModel;
using System.Diagnostics;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CircleImage), typeof(CircleImageRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class CircleImageRenderer : ImageRenderer
    {
        private Color border;
        private byte width;

        public CircleImageRenderer()
        {
        }

        private void CreateCircle(Color borderColor, byte borderWidth)
        {
            try
            {
                double min = Math.Min(Element.Width, Element.Height);
                Control.Layer.CornerRadius = (float)(min / 2.0);
                Control.Layer.MasksToBounds = false;
                //Control.Layer.BorderColor = borderColor.ToCGColor();
                //Control.Layer.BorderWidth = borderWidth;
                Control.ClipsToBounds = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to create circle image: " + ex);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            var circleImage = e.NewElement as CircleImage;
            border = circleImage != null ? circleImage.BorderColor : Color.Transparent;
            width = (byte)(circleImage != null ? circleImage.BorderWidth : 0);
            CreateCircle(border, width);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == VisualElement.HeightProperty.PropertyName ||
                e.PropertyName == VisualElement.WidthProperty.PropertyName)
            {
                CreateCircle(border, width);
            }
        }
    }
}
