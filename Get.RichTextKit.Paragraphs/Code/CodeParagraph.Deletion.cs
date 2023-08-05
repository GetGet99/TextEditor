using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.Structs;
using Get.RichTextKit.Editor.UndoUnits;
using Get.RichTextKit.Utils;

namespace Get.RichTextKit.Editor.Paragraphs;

partial class CodeParagraph
{
    public override bool DeletePartial(DeleteInfo delInfo, out TextRange requestedSelection, UndoManager<Document, DocumentViewUpdateInfo> UndoManager)
    {
        var lastCodePoint = CodePointLength - 1;
        var joinWithNext = delInfo.Range.Contains(lastCodePoint);
        if (joinWithNext)
        {
            if (delInfo.Range.IsReversed)
                delInfo.Range = delInfo.Range with { Start = lastCodePoint };
            else
                delInfo.Range = delInfo.Range with { End = lastCodePoint };
        }
        _savedParaEndingStyle = _textBlock.GetStyleAtOffset(_textBlock.CaretIndicies[^1]);
        var range = delInfo.Range;
        var offset = range.Minimum;
        if (offset < 0)
            offset = 0;
        var length = range.Maximum - offset;
        if (offset + length > _textBlock.Length)
            length = _textBlock.Length - offset;
        UndoManager.Do(new UndoDeleteText(GlobalParagraphIndex, offset, length));
        requestedSelection = new TextRange(offset, altPosition: false);
        if (joinWithNext)
        {
            TryJoinWithNextParagraph(UndoManager);
        }
        UpdateColor();
        return true;
    }
    public override bool CanJoinWith(Paragraph next)
    {
        return next is ITextParagraph;
    }
    public override bool CanDeletePartial(DeleteInfo delInfo, out TextRange requestedSelection)
    {
        requestedSelection = new(delInfo.Range.MinimumCaretPosition);
        var lastCodePoint = CodePointLength - 1;
        return true;
    }
    public override bool ShouldDeleteAll(DeleteInfo deleteInfo)
    {
        return false;
    }
}
