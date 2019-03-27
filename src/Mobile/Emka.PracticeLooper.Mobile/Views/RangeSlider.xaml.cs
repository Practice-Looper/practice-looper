// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Views
{
    public partial class RangeSlider : ContentView
    {
        #region Fields
        float leftThumbX;
        float rightThumbX;
        float thumbRadius = 50f;
        double canvasWidth;
        static byte alpha = 0x90;
        private static SKCanvasView cView;

        SKPaint thumb = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.LightGray.WithAlpha(alpha)
        };

        SKPaint track = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0x48, 0xCC, 0xA8, alpha),// #48CCA8
            StrokeWidth = 8,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPaint rangeTrack = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xF9, 0xF8, 0x71), // #F9F871
            StrokeWidth = 8,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPaint whiteFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        SKPaint verticalThumbTrack = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xF9, 0xF8, 0x71),
            StrokeWidth = 8,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        public static readonly BindableProperty LeftThumbValueProperty = BindableProperty.Create(
                           "LeftThumbValue",
                           typeof(double),
                            typeof(RangeSlider),
                            0.0,
                            BindingMode.TwoWay,
                           propertyChanged: LeftThumbValueChanged,
                           validateValue: IsValidValue);

        public static readonly BindableProperty RightThumbValueProperty = BindableProperty.Create(
                                                         "RightThumbValue",
                                                          typeof(double),
                                                          typeof(RangeSlider),
                                                          1.0,
                                                         BindingMode.TwoWay,
                                                         propertyChanged: RightThumbValueChanged,
                                                         validateValue: IsValidValue);
        #endregion

        #region Properties
        public double LeftThumbValue
        {
            get { return (double)GetValue(LeftThumbValueProperty); }
            set { SetValue(LeftThumbValueProperty, value); }
        }

        public double RightThumbValue
        {
            get { return (double)GetValue(RightThumbValueProperty); }
            set { SetValue(RightThumbValueProperty, value); }
        }
        #endregion

        #region Ctor
        public RangeSlider()
        {
            InitializeComponent();
            cView = canvasView;
        }
        #endregion

        #region Methods
        void OnHandleTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    break;
                case SKTouchAction.Moved:
                    if (e.InContact)
                    {
                        // x position within canvas?
                        if (e.Location.X >= 0.0 && e.Location.X <= canvasWidth)
                        {
                            // x position within left thumb?
                            if (e.Location.X >= leftThumbX - thumbRadius && e.Location.X <= leftThumbX + thumbRadius)
                            {
                                // thumbs should not collide!
                                var willThumbsCollide = (e.Location.X + 100) >= rightThumbX;

                                if (!willThumbsCollide)
                                {
                                    LeftThumbValue = (double)Math.Round((decimal)(e.Location.X / canvasWidth), 2);
                                }
                            }

                            // x position within right thumb?
                            if (e.Location.X >= rightThumbX - thumbRadius && e.Location.X <= rightThumbX + thumbRadius)
                            {
                                // thumbs should not collide!
                                var willThumbsCollide = (e.Location.X - 100 < leftThumbX);

                                if (!willThumbsCollide)
                                {
                                    RightThumbValue = (double)Math.Round((decimal)(e.Location.X / canvasWidth), 2);
                                }
                            }
                        }
                    }
                    break;
                case SKTouchAction.Released:

                    break;
                case SKTouchAction.Cancelled:
                    break;
            }

            // we have handled these events
            e.Handled = true;

            // update the UI
            ((SKCanvasView)sender).InvalidateSurface();
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            int width = e.Info.Width;
            int height = e.Info.Height;

            canvasWidth = width - Convert.ToInt32(thumbRadius);
            leftThumbX = (float)(LeftThumbValue * canvasWidth);
            rightThumbX = (float)(RightThumbValue * canvasWidth);

            // clear canvas
            canvas.Clear();

            // save canvas state
            canvas.Save();

            // draw tracks
            canvas.DrawLine(thumbRadius / 2, height / 2, width - thumbRadius, height / 2, track);
            canvas.DrawLine(leftThumbX + thumbRadius / 2, height / 2, rightThumbX + thumbRadius / 2, height / 2, rangeTrack);

            // draw thumbs
            canvas.DrawRoundRect(leftThumbX, 0, thumbRadius, height, 10, 10, thumb);
            canvas.DrawRoundRect(rightThumbX, 0, thumbRadius, height, 10, 10, thumb);

            // draw vertical thumb markers
            canvas.DrawLine(leftThumbX + thumbRadius / 2, 20, leftThumbX + thumbRadius / 2, height - 20, verticalThumbTrack);
            canvas.DrawLine(rightThumbX + thumbRadius / 2, 20, rightThumbX + thumbRadius / 2, height - 20, verticalThumbTrack);

            // restore canvas state
            canvas.Restore();
        }

        private static void RightThumbValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine("right " + newValue);
            cView.InvalidateSurface();
        }

        private static void LeftThumbValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine("left " + newValue);
            cView.InvalidateSurface();
        }

        private static bool IsValidValue(BindableObject bindable, object value)
        {
            double result;
            bool isDouble = double.TryParse(value.ToString(), out result);
            return (result >= 0.0 && result <= 1.0);
        }
        #endregion
    }
}
