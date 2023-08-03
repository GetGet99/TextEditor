using System;
using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor;
using Get.RichTextKit;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    void InitKeyboardHook()
    {
#if WINDOWS_UWP
        CoreWindow.GetForCurrentThread().KeyDown += RichTextEditor_KeyDown;
#else
        KeyDown += RichTextEditor_KeyDown;
#endif
    }

    static bool IsKeyDown(VirtualKey key)
        => PlatformLibrary.IsKeyDown(key);
#if WINDOWS_UWP
    private void RichTextEditor_KeyDown(CoreWindow sender, KeyEventArgs e)
    {
        VirtualKey VirtualKeyOf(KeyEventArgs e)
            => e.VirtualKey;
#else
    private void RichTextEditor_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        VirtualKey VirtualKeyOf(KeyRoutedEventArgs e)
            => e.Key;
#endif
        if (!HasFocus) return;
        else if ((DependencyObject)FocusManager.GetFocusedElement(XamlRoot) != this)
        {
            Focus(FocusState.Keyboard);
        }
        bool handled = true;
        switch (VirtualKeyOf(e))
        {
            case VirtualKey.Back:
                DocumentView.Controller.Delete();
                break;
            case VirtualKey.Delete:
                DocumentView.Controller.Delete(deleteFront: true);
                break;
            case VirtualKey.Left:
            case VirtualKey.Right:
            case VirtualKey.Up:
            case VirtualKey.Down:
                DocumentView.Controller.MoveCaret(
                    VirtualKeyOf(e) switch
                    {
                        VirtualKey.Left => Direction.Left,
                        VirtualKey.Right => Direction.Right,
                        VirtualKey.Up => Direction.Up,
                        VirtualKey.Down => Direction.Down,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    wholeWord: IsKeyDown(VirtualKey.Control), selectionMode: IsKeyDown(VirtualKey.Shift)
                );
                break;
            case VirtualKey.Enter:
                if (IsKeyDown(VirtualKey.Shift))
                    DocumentView.Controller.Type("\n");
                else
                    DocumentView.Controller.Type(Document.NewParagraphSeparator.ToString()); // Paragraph separator
                //var selection = DocumentView.Selection.Range;
                //var caretPos = selection.CaretPosition;
                //EditContext.NotifyTextChanged(
                //    modifiedRange: new TextRange(caretPos.CodePointIndex - 1, caretPos.CodePointIndex).ToCore(),
                //    newLength: DocumentView.OwnerDocument.Layout.Length,
                //    selection.ToCore()
                //);
                break;
            case VirtualKey.Home:
            case VirtualKey.End:
            case VirtualKey.PageUp:
            case VirtualKey.PageDown:
                DocumentView.Controller.MoveCaret(
                    VirtualKeyOf(e) switch
                    {
                        VirtualKey.Home => IsKeyDown(VirtualKey.Control) ? SpacialCaretMovement.CtrlHome : SpacialCaretMovement.Home,
                        VirtualKey.End => IsKeyDown(VirtualKey.Control) ? SpacialCaretMovement.CtrlEnd : SpacialCaretMovement.End,
                        VirtualKey.PageDown => SpacialCaretMovement.PageDown,
                        VirtualKey.PageUp => SpacialCaretMovement.PageUp,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    IsKeyDown(VirtualKey.Shift)
                );
                break;
            default:
                HandleShortcut(sender, e);
                // Skip the setting e.Handled
                return;
        }
        e.Handled = handled;
    }
}