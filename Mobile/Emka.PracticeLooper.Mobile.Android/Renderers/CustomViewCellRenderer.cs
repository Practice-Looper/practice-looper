// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2020

//using System.ComponentModel;
//using Android.Content;
//using Android.Graphics.Drawables;
//using Android.Runtime;
//using Android.Views;
//using Emka.PracticeLooper.Mobile.Droid.Renderers;
//using Emka.PracticeLooper.Mobile.Views;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.Android;


using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Emka.PracticeLooper.Mobile.Droid.Renderers;
using Emka.PracticeLooper.Mobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomViewCell), typeof(CustomViewCellRenderer))]
namespace Emka.PracticeLooper.Mobile.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CustomViewCellRenderer : ViewCellRenderer
    {
        private Android.Views.View cellCore;
        private Drawable unselectedBackground;
        private bool selected;
        CustomViewCell cell;

        public CustomViewCellRenderer()
        {
        }

        protected override Android.Views.View GetCellCore(Cell item, Android.Views.View convertView, ViewGroup parent, Context context)
        {
            cellCore = base.GetCellCore(item, convertView, parent, context);
            var x = item as CustomViewCell;
            var listView = item.Parent;
            //// Save original background to rollback to it when not selected,
            //// We assume that no cells will be selected on creation.
            selected = false;
            unselectedBackground = cellCore.Background;

            return cellCore;
        }

        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnCellPropertyChanged(sender, args);

            //if (args.PropertyName == "IsSelected")
            //{
            //    // I had to create a property to track the selection because cellCore.Selected is always false.
            //    // Toggle selection
            //    selected = !selected;

            //    if (selected)
            //    {
            //        var customTextCell = sender as CustomViewCell;
            //        cellCore.SetBackgroundColor(customTextCell.SelectedBackgroundColor.ToAndroid());
            //    }
            //    else
            //    {
            //        cellCore.SetBackgroundColor(Color.Transparent.ToAndroid());
            //    }
            //}
        }


    }
}
