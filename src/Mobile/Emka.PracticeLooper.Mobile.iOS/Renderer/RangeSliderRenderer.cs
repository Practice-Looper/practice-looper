// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using Emka.PracticeLooper.Mobile.iOS.Renderer;
using Emka.PracticeLooper.Mobile.Views;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RangeSlider), typeof(RangeSliderRenderer))]
namespace Emka.PracticeLooper.Mobile.iOS.Renderer
{
    public class RangeSliderRenderer : SliderRenderer
    {
        private UISlider _rangeSlider;

        public RangeSliderRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null || this.Element == null)
                return;

            this.InitSlider();
        }

        private void Element_SizeChanged(object sender, EventArgs e)
        {
            this.InitRangeSlider();
        }

        private void RangeSlider_PanGestureRecognized(UIPanGestureRecognizer recognizer)
        {
            this.SetThumbValue((float)recognizer.LocationInView(this.NativeView).X);
        }

        /**
        *** Custom functions
        **/

        private void InitSlider()
        {
            this.Control.MinimumTrackTintColor = UIColor.Blue;
            this.Control.MaximumTrackTintColor = UIColor.LightGray;

            this.Element.SizeChanged += Element_SizeChanged;
        }

        private void InitRangeSlider()
        {
            this._rangeSlider = new UISlider();
            this._rangeSlider.Value = (float)(this.Element as RangeSlider).RangeValue;
            this._rangeSlider.MinValue = (float)this.Element.Minimum;
            this._rangeSlider.MaxValue = (float)this.Element.Maximum;
            this._rangeSlider.MinimumTrackTintColor = UIColor.LightGray;
            this._rangeSlider.MaximumTrackTintColor = UIColor.Clear;
            this._rangeSlider.Frame = new System.Drawing.RectangleF(0, 0, (float)this.Element.Width, (float)this.Element.Height);

            UIPanGestureRecognizer panGestureRecognizer = new UIPanGestureRecognizer();
            panGestureRecognizer.AddTarget(() => RangeSlider_PanGestureRecognized(panGestureRecognizer));
            this._rangeSlider.AddGestureRecognizer(panGestureRecognizer);

            this.AddSubview(this._rangeSlider);
        }

        private void SetThumbValue(float x)
        {
            if (x <= this.Element.Width / this.Element.Maximum * (this.Element.Value + 10) && x >= this.Element.Width / this.Element.Maximum * (this.Element.Value - 10) && this._rangeSlider.Value < (float)(this.Element.Maximum / this.Element.Width * x))
                this.Element.Value = (float)(this.Element.Maximum / this.Element.Width * x);
            else if (x <= this.Element.Width / this.Element.Maximum * (this._rangeSlider.Value + 10) && x >= this.Element.Width / this.Element.Maximum * (this._rangeSlider.Value - 10) && this.Element.Value > (float)(this.Element.Maximum / this.Element.Width * x))
            {
                this._rangeSlider.Value = (float)(this.Element.Maximum / this.Element.Width * x);
                (this.Element as RangeSlider).RangeValue = this._rangeSlider.Value;
            }
        }
    }
}
