// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020
using System;
using Xamarin.Forms;

namespace Emka.PracticeLooper.Mobile.Behaviors
{
    public class CustomCellBackgroundBehavior : Behavior<ViewCell>
    {
        ViewCell lastCell;

        public CustomCellBackgroundBehavior()
        {
        }

        protected override void OnAttachedTo(ViewCell cell)
        {
            base.OnAttachedTo(cell);
            cell.Tapped += Cell_Tapped;
        }

        private void Cell_Tapped(object sender, EventArgs e)
        {
            if (lastCell != null)
                lastCell.View.BackgroundColor = Color.Transparent;
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = (Color)Application.Current.Resources["SecondaryColor"];
                lastCell = viewCell;
            }
        }

        protected override void OnDetachingFrom(ViewCell cell)
        {
            base.OnDetachingFrom(cell);
            cell.Tapped -= Cell_Tapped;
        }
    }
}
