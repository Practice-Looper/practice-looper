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
        float thumbWidth = 50f;
        double canvasWidth;
        double internalLeftThumbValue;
        double internalRightThumbValue;
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
            Color = ColorConstants.ConvertToUint(ColorConstants.SecondaryHexColor),
            StrokeWidth = 8,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPaint rangeTrack = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ColorConstants.ConvertToUint(ColorConstants.PrimaryHexColor),
            StrokeWidth = 12,
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
            Color = ColorConstants.ConvertToUint(ColorConstants.PrimaryHexColor),
            StrokeWidth = 12,
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

        public event EventHandler DraggingCompleted;

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
            internalLeftThumbValue = LeftThumbValue;
            internalRightThumbValue = RightThumbValue;
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
                            var offset = (double)Math.Round((decimal)((thumbWidth / 2) / canvasWidth), 3);
                            // x position within left thumb?
                            if (e.Location.X >= leftThumbX - thumbWidth && e.Location.X <= leftThumbX + thumbWidth)
                            {
                                // thumbs should not collide!
                                var willThumbsCollide = (e.Location.X + 100) >= rightThumbX || e.Location.X - (thumbWidth / 2) <= 0; 
                                if (!willThumbsCollide)
                                {
                                    internalLeftThumbValue = (double)Math.Round((decimal)(e.Location.X / canvasWidth), 3);
                                    LeftThumbValue = internalLeftThumbValue - offset;
                                }
                            }

                            // x position within right thumb?
                            if (e.Location.X >= rightThumbX - thumbWidth && e.Location.X <= rightThumbX + thumbWidth)
                            {
                                // thumbs should not collide!
                                var willThumbsCollide = (e.Location.X - 100 < leftThumbX);
                                if (!willThumbsCollide)
                                {
                                    internalRightThumbValue = (double)Math.Round((decimal)(e.Location.X / canvasWidth), 3);
                                    RightThumbValue = internalRightThumbValue;
                                }
                            }
                        }
                    }
                    break;
                case SKTouchAction.Released:
                    RaiseOnDraggingCompleted();
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

            canvasWidth = width - Convert.ToInt32(verticalThumbTrack.StrokeWidth);

            leftThumbX = (float)(LeftThumbValue * canvasWidth);
            rightThumbX = (float)(RightThumbValue * canvasWidth);

            // clear canvas
            canvas.Clear();

            // save canvas state
            canvas.Save();

            // draw tracks
            canvas.DrawLine(verticalThumbTrack.StrokeWidth / 2, height / 3, (float)canvasWidth, height / 3, track);
            canvas.DrawLine(leftThumbX + verticalThumbTrack.StrokeWidth / 2, height / 3, rightThumbX - verticalThumbTrack.StrokeWidth / 2, height / 3, rangeTrack);


            // draw vertical thumb markers
            canvas.DrawLine(leftThumbX + verticalThumbTrack.StrokeWidth / 2, 20, leftThumbX+ verticalThumbTrack.StrokeWidth / 2, height - 20, verticalThumbTrack);
            canvas.DrawLine(rightThumbX, 20, rightThumbX, height - 20, verticalThumbTrack);

            // restore canvas state
            canvas.Restore();
        }

        private static void RightThumbValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            cView.InvalidateSurface();
        }

        private static void LeftThumbValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            cView.InvalidateSurface();
        }

        private static bool IsValidValue(BindableObject bindable, object value)
        {
            double result;
            bool isDouble = double.TryParse(value.ToString(), out result);
            return result >= 0.0 && result <= 1.0;
        }

        private void RaiseOnDraggingCompleted()
        {
            DraggingCompleted?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
