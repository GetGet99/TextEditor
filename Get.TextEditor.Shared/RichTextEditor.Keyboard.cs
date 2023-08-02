using Windows.UI.Core;


using System;
using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor;
using Get.RichTextKit;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    void InitKeyboardHook()
    {
        CoreWindow.GetForCurrentThread().KeyDown += RichTextEditor_KeyDown;
    }
    static bool IsKeyDown(VirtualKey key)
        => (CoreWindow.GetForCurrentThread().GetKeyState(key) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    private void RichTextEditor_KeyDown(CoreWindow sender, KeyEventArgs e)
    {
        if (!HasFocus) return;
        else if (FocusManager.GetFocusedElement(XamlRoot) != this)
        {
            Focus(FocusState.Keyboard);
        }
        bool handled = true;
        switch (e.VirtualKey)
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
                    e.VirtualKey switch
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
                    e.VirtualKey switch
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