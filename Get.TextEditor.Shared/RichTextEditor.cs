#nullable enable
using Get.RichTextKit.Editor;
using System;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace Get.TextEditor;
public partial class RichTextEditor : UserControl
{
    public RichTextEditor() : this(null) { }
    public RichTextEditor(Document? document)
    {
        DocumentView = new(this, document ?? new(DefaultStyle)) { BottomAdditionalScrollHeight = 100 };
        InitTextDocument();
        InitXAML();
        InitEditorCore();
        InitPointerHook();
        InitKeyboardHook();
        // Intentionally update property
        DocumentView.Selection.Range = DocumentView.Selection.Range;
    }
}