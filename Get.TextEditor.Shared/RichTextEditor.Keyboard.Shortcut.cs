using Get.RichTextKit.Editor;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Xaml.Controls;
using System;
using StyleStatus = Get.RichTextKit.Editor.StyleStatus;
using Get.RichTextKit.Editor.Paragraphs;
using System.Linq;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using Get.RichTextKit;
using UITextBlock = Windows.UI.Xaml.Controls.TextBlock;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{

    private void HandleShortcut(CoreWindow sender, KeyEventArgs e)
    {
        if (!HasFocus) return;
        bool handled = true;
        switch (e.VirtualKey)
        {
            case VirtualKey.C:
                if (IsKeyDown(VirtualKey.Control))
                {
                    if (IsKeyDown(VirtualKey.Shift)) goto case VirtualKey.E;
                    Copy();
                    break;
                }
                else goto default;
            case VirtualKey.V:
                if (IsKeyDown(VirtualKey.Control))
                {
                    // PasteAsync will run asyncronously, we set handled first just in case it isn't running the code below on time
                    e.Handled = true;

                    // Paste
                    // Ctrl + Shift + V - paste without formatting
                    // No need to wait for the method to finish
                    _ = PasteAsync(forceNoFormatting: IsKeyDown(VirtualKey.Shift));
                    // Skip the setting of e.Handled
                    return;
                }
                else goto default;
            case VirtualKey.T:
                if (IsKeyDown(VirtualKey.Control))
                {
                    void TestUIElement()
                    {
                        var ele = new Button { Content = "Test", Flyout = new Flyout { Content = new UITextBlock { Text = "Hi" } } };
                        //var ele = new CheckBox { MinWidth = 0, MinHeight = 0, Padding = default };
                        ele.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                        InsertUIFrameworkElement(ele);
                    }
                    void TestHorizontalParagraph()
                    {
                        var style = DocumentView.Selection.CurrentCaretStyle;
                        var para = new HorizontalParagraph(style);
                        ((para.Children[0] as VerticalParagraph).Children[0] as TextParagraph).TextBlock.InsertText(0,
                            """
                            First Column
                            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Eget egestas purus viverra accumsan. Leo vel fringilla est ullamcorper eget. Convallis a cras semper auctor neque vitae. Eu facilisis sed odio morbi. Ut placerat orci nulla pellentesque. Sed pulvinar proin gravida hendrerit lectus. A scelerisque purus semper eget duis at tellus at urna. Odio facilisis mauris sit amet massa vitae tortor condimentum. Vitae purus faucibus ornare suspendisse sed nisi lacus. Ac placerat vestibulum lectus mauris ultrices eros in cursus turpis. Duis at tellus at urna condimentum mattis pellentesque id.
                            """, style);
                        ((para.Children[1] as VerticalParagraph).Children[0] as TextParagraph).TextBlock.InsertText(0,
                            """
                            Second Column
                            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Cursus sit amet dictum sit amet justo donec enim. Ut porttitor leo a diam sollicitudin tempor id. In hac habitasse platea dictumst quisque sagittis. Orci porta non pulvinar neque laoreet suspendisse interdum. Ut tellus elementum sagittis vitae et leo duis ut diam. Velit egestas dui id ornare arcu odio ut sem. Odio ut sem nulla pharetra diam sit amet nisl suscipit. Lorem sed risus ultricies tristique nulla. Quam adipiscing vitae proin sagittis nisl rhoncus mattis. Diam sollicitudin tempor id eu nisl nunc mi. Placerat orci nulla pellentesque dignissim enim sit. Nisl vel pretium lectus quam id leo. Diam quam nulla porttitor massa id neque aliquam. Ac tortor vitae purus faucibus ornare suspendisse. In arcu cursus euismod quis viverra nibh.
                            """, style);
                        DocumentView.Controller.InsertNewParagraph(para);
                    }
                    void TestTyping()
                    {
                        DocumentView.Controller.Type("A");
                        DocumentView.Controller.Type(Document.NewParagraphSeparator.ToString());
                        DocumentView.Controller.Type("A");
                    }
                    void TestTable()
                    {
                        var style = DocumentView.Selection.CurrentCaretStyle;
                        var table = new TableParagraph(style, 3, 3);
                        DocumentView.Controller.InsertNewParagraph(table);
                    }
                    TestTable();
                }
                else goto default;
                break;
            case VirtualKey.A:
                if (IsKeyDown(VirtualKey.Control)) DocumentView.Controller.Select(default, SelectionKind.Document);
                else goto default;
                break;
            case VirtualKey.B:
                if (IsKeyDown(VirtualKey.Control)) DocumentView.Selection.Bold = DocumentView.Selection.Bold.Toggle();
                else goto default;
                break;
            case VirtualKey.U:
                if (IsKeyDown(VirtualKey.Control)) DocumentView.Selection.Underline = DocumentView.Selection.Underline.Toggle();
                else goto default;
                break;
            case VirtualKey.I:
                if (IsKeyDown(VirtualKey.Control)) DocumentView.Selection.Italic = DocumentView.Selection.Italic.Toggle();
                else goto default;
                break;
            case VirtualKey.L:
                if (IsKeyDown(VirtualKey.Control))
                    DocumentView.Selection.ApplyParagraphSetting(TextAlignment.Left, AlignmentGetter, AlignmentSetter);
                else goto default;
                break;
            case VirtualKey.E:
                if (IsKeyDown(VirtualKey.Control))
                    DocumentView.Selection.ApplyParagraphSetting(TextAlignment.Center, AlignmentGetter, AlignmentSetter);
                else goto default;
                break;
            case VirtualKey.R:
                if (IsKeyDown(VirtualKey.Control))
                    DocumentView.Selection.ApplyParagraphSetting(TextAlignment.Right, AlignmentGetter, AlignmentSetter);
                else goto default;
                break;
            case (VirtualKey)0xBB: // OEM + or = key
                if (IsKeyDown(VirtualKey.Control))
                {
                    if (IsKeyDown(VirtualKey.Shift))
                        DocumentView.Selection.SuperScript = DocumentView.Selection.SuperScript.Toggle();
                    else
                        DocumentView.Selection.SubScript = DocumentView.Selection.SubScript.Toggle();
                }
                else goto default;
                break;
            case VirtualKey.Z:
                if (IsKeyDown(VirtualKey.Control))
                {
                    if (IsKeyDown(VirtualKey.Shift)) goto case VirtualKey.Y;
                    DocumentView.OwnerDocument.UndoManager.Undo(DocumentView.InvokeUpdateInfo);
                }
                else goto default;
                break;
            case VirtualKey.Y:
                if (IsKeyDown(VirtualKey.Control))
                    DocumentView.OwnerDocument.UndoManager.Redo(DocumentView.InvokeUpdateInfo);
                else goto default;
                break;
            default:
                handled = false;
                break;
        }
        e.Handled = handled;
    }
    static TextAlignment AlignmentGetter(Paragraph p)
        => p is IAlignableParagraph ap ? ap.Alignment : default;
    static bool AlignmentSetter(Paragraph p, TextAlignment x)
    {
        if (p is IAlignableParagraph ap)
        {
            ap.Alignment = x;
            return true;
        }
        return false;
    }
}
static partial class Extension
{
    public static StyleStatus Toggle(this StyleStatus status)
        => status switch
        {
            StyleStatus.On => StyleStatus.Off,
            StyleStatus.Off or StyleStatus.Undefined => StyleStatus.On,
            _ => throw new ArgumentOutOfRangeException()
        };
}