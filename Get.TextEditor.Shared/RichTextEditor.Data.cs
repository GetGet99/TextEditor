using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit;
using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Data;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    public DataInfo GetData(TextRange Range)
    {
        return DocumentDataGenerator.Generate(
            DocumentView.OwnerDocument,
            Range
        );
    }
    public DataInfo GetDocumentData()
    {
        return GetData(new(0, DocumentView.OwnerDocument.Layout.Length));
    }
    public DataInfo GetSelectionData()
    {
        return GetData(DocumentView.Selection.Range.Normalized);
    }
    public void AddRTFData(string rtfString, AllowedFormatting AllowedFormatting, string operationName = "Add RTF Data", bool setNewSelection = false)
    {
        using var group = DocumentView.OwnerDocument.UndoManager.OpenGroup(operationName);
        var handler = new RTF.RTFDocumentHandler(DocumentView, AllowedFormatting);
        RtfParser.CoreRTFParser.Parse(rtfString.AsSpan(), handler);
        if (setNewSelection)
            DocumentView.Selection.Range = handler.GetHandlerSelectionRange();
    }
    public void SetDocumentRTFData(string rtfString, AllowedFormatting AllowedFormatting, bool setNewSelection = false)
    {
        var style = DocumentView.OwnerDocument.GetStyleAtPosition(new(0));
        DocumentView.OwnerDocument.Paragraphs.Paragraphs.Clear();
        DocumentView.OwnerDocument.Paragraphs.Add(new TextParagraph(style));
        DocumentView.OwnerDocument.Layout.InvalidateAndValid();
        var handler = new RTF.RTFDocumentHandler(DocumentView, AllowedFormatting);
        RtfParser.CoreRTFParser.Parse(rtfString.AsSpan(), handler);
        DocumentView.OwnerDocument.UndoManager.Clear();
        DocumentView.OwnerDocument.Layout.InvalidateAndValid();

        if (setNewSelection)
            DocumentView.Selection.Range = handler.GetHandlerSelectionRange();
        DocumentView.OwnerDocument.RequestRedraw();
    }
}