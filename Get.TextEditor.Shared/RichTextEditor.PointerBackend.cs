#nullable enable
using Get.RichTextKit;
using Get.RichTextKit.Editor;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    void InitPointerBackendHook()
    {
        int clickCount = 0;
        bool pressed = false;
        bool moved = false;
        DateTime prevPointerPressed = DateTime.MinValue;
        EditorCanvas.PointerPressed += (_, e) =>
        {
            HasFocus = true;
            Focus(FocusState.Programmatic);
            EditContext.NotifyFocusEnter();

            pressed = true;

            EditorCanvas.SelectionHandle = ShouldShowHandle(e.Pointer.PointerDeviceType);
            if (ShouldManipulationScroll(e.Pointer.PointerDeviceType))
                return;

            PointerDoPressed(e);
        };
        EditorCanvas.PointerReleased += (_, e) =>
        {
            if (!pressed || !moved) goto End;

            if (ShouldManipulationScroll(e.Pointer.PointerDeviceType))
                PointerDoPressed(e);
            
            End:
            moved = false;
            pressed = false;
        };
        void PointerDoPressed(PointerRoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now - prevPointerPressed;
            if (now - prevPointerPressed <= TimeSpan.FromMilliseconds(Constants.UISettings.DoubleClickTime))
            {
                clickCount++;
            }
            else
            {
                clickCount = 1;
            }
            prevPointerPressed = now;
            VirtualizedPointerPressed(e, clickCount);
        }
        EditorCanvas.PointerMoved += (_, e) =>
        {
            moved = true;
            if (ShouldManipulationScroll(e.Pointer.PointerDeviceType)) return;
            VirtualizedPointerMoved(e, pressed);
        };
        CoreWindow.GetForCurrentThread().PointerPressed += (o, e) =>
        {
            static Rect GetElementRect(FrameworkElement element)
            {
                GeneralTransform transform = element.TransformToVisual(null);
                Point point = transform.TransformPoint(new Point());
                return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
            }
            // See whether the pointer is inside or outside the control.
            Rect contentRect = GetElementRect(this);
            if (!contentRect.Contains(e.CurrentPoint.Position))
            {
                HasFocus = false;
                EditContext.NotifyFocusLeave();
            }
        };
    }
}