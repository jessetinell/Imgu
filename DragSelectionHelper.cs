using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Imgu
{
    public class DragSelectionHelper : DependencyObject
    {
        #region Random Static Properties

        // need a static reference to the listbox otherwise it can't be accessed
            // (this only happened in the project I'm working on, if you're using a regular ListBox, with regular ListBoxItems you can get the ListBox from the ListBoxItems)
        public static ListBox ListBox { get; private set; }

        #endregion Random Static Properties

        #region IsDragSelectionEnabledProperty

        public static bool GetIsDragSelectionEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSelectionEnabledProperty);
        }

        public static void SetIsDragSelectionEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSelectionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsDragSelectionEnabledProperty =
            DependencyProperty.RegisterAttached("IsDragSelectingEnabled", typeof(bool), typeof(DragSelectionHelper), new UIPropertyMetadata(false, IsDragSelectingEnabledPropertyChanged));

        public static void IsDragSelectingEnabledPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var listBox = o as ListBox;

            bool isDragSelectionEnabled = GetIsDragSelectionEnabled(listBox);

            // if DragSelection is enabled
            if (isDragSelectionEnabled)
            {
                // set the listbox's selection mode to multiple ( didn't work with extended )
                listBox.SelectionMode = SelectionMode.Multiple;

                // set the static listbox property
                ListBox = listBox;

                // and subscribe to the required events to handle the drag selection and the attached properties
                listBox.PreviewMouseLeftButtonDown += listBox_PreviewMouseLeftButtonDown;
                listBox.PreviewMouseRightButtonDown += listBox_PreviewMouseRightButtonDown;
                listBox.MouseLeftButtonUp += listBox_MouseLeftButtonUp;
            }
            else // is selection is disabled
            {
                // set selection mode to the default
                listBox.SelectionMode = SelectionMode.Single;

                // dereference the listbox
                ListBox = null;

                // unsuscribe from the events
                listBox.PreviewMouseLeftButtonDown -= listBox_PreviewMouseLeftButtonDown;
                listBox.MouseLeftButtonUp -= listBox_MouseLeftButtonUp;
                listBox.MouseLeftButtonUp -= listBox_MouseLeftButtonUp;
            }
        }

        static void listBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // to prevent the listbox from selecting / deselecting wells on right click
            e.Handled = true;
        }

        private static void listBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // notify the helper class that the listbox has initiated the drag click
            SetIsDragClickStarted(ListBox, true);
        }

        private static void listBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // notify the helper class that the list box has terminated the drag click
            SetIsDragClickStarted(ListBox, false);
        }

        #endregion IsDragSelectionEnabledProperty

        #region IsDragSelectinProperty

        public static bool GetIsDragSelecting(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSelectingProperty);
        }

        public static void SetIsDragSelecting(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSelectingProperty, value);
        }

        public static readonly DependencyProperty IsDragSelectingProperty =
            DependencyProperty.RegisterAttached("IsDragSelecting", typeof(bool), typeof(DragSelectionHelper), new UIPropertyMetadata(false, IsDragSelectingPropertyChanged));

        public static void IsDragSelectingPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var item = o as ListBoxItem;

            bool clickInitiated = GetIsDragClickStarted(ListBox);

            // this is where the item.Parent was null, it was supposed to be the ListBox, I guess it's null because items are not
            // really ListBoxItems but are wells
            if (clickInitiated)
            {
                bool isDragSelecting = GetIsDragSelecting(item);

                if (isDragSelecting)
                {
                    // using the ListBox static reference because could not get to it through the item.Parent property
                    ListBox.SelectedItems.Add(item);
                }
            }
        }

        #endregion IsDragSelectinProperty

        #region IsDragClickStartedProperty

        public static bool GetIsDragClickStarted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragClickStartedProperty);
        }

        public static void SetIsDragClickStarted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragClickStartedProperty, value);
        }

        public static readonly DependencyProperty IsDragClickStartedProperty =
            DependencyProperty.RegisterAttached("IsDragClickStarted", typeof(bool), typeof(DragSelectionHelper), new UIPropertyMetadata(false, IsDragClickStartedPropertyChanged));

        public static void IsDragClickStartedPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            bool isDragClickStarted = DragSelectionHelper.GetIsDragClickStarted(DragSelectionHelper.ListBox);

            // if click has been drag click has started, clear the current selected items and start drag selection operation again
            if (isDragClickStarted)
                DragSelectionHelper.ListBox.SelectedItems.Clear();
        }

        #endregion IsDragClickInitiatedProperty
    }
}

