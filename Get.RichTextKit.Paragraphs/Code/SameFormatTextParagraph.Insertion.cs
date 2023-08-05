
using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.UndoUnits;
using Get.RichTextKit.Utils;

namespace Get.RichTextKit.Editor.Paragraphs;

partial class CodeParagraph
{
    protected override (InsertTextStatus Status, StyledText RemainingText) AddText(int codePointIndex, StyledText text, UndoManager<Document, DocumentViewUpdateInfo> UndoManager)
    {
        var noforamttext = new StyledText();
        noforamttext.AddText(text.CodePoints.AsSlice(), StartStyle);
        noforamttext.CodePoints.Replace(Document.NewParagraphSeparator, '\n');
        UndoManager.Do(new UndoInsertText(GlobalParagraphIndex, codePointIndex, noforamttext));
        UpdateColor();
        return (InsertTextStatus.AlreadyAdd, new());
    }
}
