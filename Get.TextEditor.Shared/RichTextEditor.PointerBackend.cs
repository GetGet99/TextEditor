namespace Get.TextEditor;

partial class RichTextEditor : UserControl
{
    public void ProgrammaticFocus()
    {
        HasFocus = true;
        Focus(FocusState.Programmatic);
        EditContext.NotifyFocusEnter();
    }
    void InitPointerBackendHook()
    {
        int clickCount = 0;
        bool pressed = false;
        bool moved = false;
        DateTime prevPointerPressed = DateTime.MinValue;
        EditorCanvas.PointerPressed += (_, e) =>
        {
            EditorCanvas.ResetManipulationScrollTracker();
            HasFocus = true;
            Focus(FocusState.Programmatic);
            EditContext.NotifyFocusEnter();

            if (e.GetCurrentPoint(EditorCanvas).Properties.IsLeftButtonPressed)
            {
                pressed = true;
                EditorCanvas.SelectionHandle = ShouldShowHandle(e.Pointer.PointerDeviceType);
                if (ShouldManipulationScroll(e.Pointer.PointerDeviceType))
                    return;
            }
            PointerDoPressed(e);
        };
        EditorCanvas.PointerReleased += (_, e) =>
        {
            if (!pressed) goto End;
            if (!EditorCanvas.ManipulationScrolled && ShouldManipulationScroll(e.Pointer.PointerDeviceType))
            {
                PointerDoPressed(e);
            }
            VirtualizedPointerReleased(e);
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
        //CoreWindow.GetForCurrentThread().PointerPressed += (o, e) =>
        //{
        //    var ele = VisualTreeHelper.FindElementsInHostCoordinates(e.CurrentPoint.Position, XamlRoot.Content).FirstOrDefault();
        //    if (ele != EditorCanvas)
        //    {
        //        //HasFocus = false;
        //        //EditContext.NotifyFocusLeave();
        //    }
        //};
    }
}