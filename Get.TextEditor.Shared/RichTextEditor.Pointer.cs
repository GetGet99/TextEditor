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
    void InitPointerHook()
    {
        InitPointerBackendHook();
        EditorCanvas.PointerWheelChanged += (_, e) =>
        {
            var properties = e.GetCurrentPoint(EditorCanvas).Properties;
            if (!properties.IsHorizontalMouseWheel)
                DocumentView.YScroll += -properties.MouseWheelDelta;
        };
    }

    HitTestResult HitTest(PointerRoutedEventArgs e)
        => HitTest(e.GetCurrentPoint(this).Position);
    HitTestResult HitTest(Point pt)
        => DocumentView.Controller.HitTest(new((float)pt.X, (float)pt.Y));
    CaretPosition SelectionStart;
    void VirtualizedPointerPressed(PointerRoutedEventArgs e, int clickCount)
    {
        var pt = e.GetCurrentPoint(this);
        var hitTest = HitTest(pt.Position);
        if (pt.Properties.IsRightButtonPressed)
        {
            if (DocumentView.Selection.Range.Contains(
                hitTest.IsHit() ? hitTest.OverCodePointIndex : hitTest.ClosestCodePointIndex,
                true))
                // Don't do any selection
                return;
        }
        bool Shifting = e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Shift);
        if (hitTest.IsHit())
        {
            SelectionStart = hitTest.CaretPosition;
        }
        else
        {
            SelectionStart = hitTest.GetCloestCarretPosition();
        }
        if (Shifting)
        {
            SelectionStart = new(DocumentView.Selection.Range.Start);
            DocumentView.Controller.Select(new(SelectionStart.CodePointIndex, hitTest.ClosestCodePointIndex, hitTest.AltCaretPosition));
        }
        else if (clickCount is 1)
        {
            DocumentView.Controller.MoveCaret(SelectionStart);
        }
        if (Shifting) return;
        if (clickCount % 2 is 0)
        {
            DocumentView.Controller.Select(hitTest.CaretPosition, SelectionKind.Word);
        }
        if (clickCount is >= 3 && clickCount % 2 is 1)
        {
            DocumentView.Controller.Select(hitTest.CaretPosition, SelectionKind.Paragraph);
        }
    }
    void VirtualizedPointerReleased(PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);
        if (pt.Properties.IsRightButtonPressed)
        {
            ContextFlyoutShowing?.Invoke(this, new());
            ContextFlyout.ShowAt(this, new() {
                Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.TopEdgeAlignedLeft,
                Position = pt.Position
            });
        }
    }
    public event RoutedEventHandler? ContextFlyoutShowing;
    CoreCursor? oldPointer;
    bool _isCursorInside = false;
    bool IsCursorInside
    {
        set
        {
            if (value == _isCursorInside) return;
            _isCursorInside = value;
            if (value)
            {
                oldPointer = CoreWindow.GetForCurrentThread().PointerCursor;
                CoreWindow.GetForCurrentThread().PointerCursor = new CoreCursor(CoreCursorType.IBeam, 0);
            } else
            {
                CoreWindow.GetForCurrentThread().PointerCursor = oldPointer;
            }
        }
    }
    private void VirtualizedPointerMoved(PointerRoutedEventArgs e, bool IsHolding)
    {
        var point = e.GetCurrentPoint(this);
        var hitTest = HitTest(point.Position);
        IsCursorInside = hitTest.IsHit();
        if (IsHolding && point.IsInContact && point.Properties.IsLeftButtonPressed)
        {
            DocumentView.Controller.Select(new(SelectionStart.CodePointIndex, hitTest.ClosestCodePointIndex, hitTest.AltCaretPosition));
        }
    }
    bool ShouldManipulationScroll(PointerDeviceType p) => p is not PointerDeviceType.Mouse;
    bool ShouldShowHandle(PointerDeviceType p) => true;
}
static partial class Extension
{
    public static bool IsHit(this HitTestResult hitTest) => hitTest.OverCodePointIndex is not -1;
    public static CaretPosition GetCloestCarretPosition(this HitTestResult hitTest) => new(hitTest.ClosestCodePointIndex, false);
}