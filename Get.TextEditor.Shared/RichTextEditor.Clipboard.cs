using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DocumentView;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    void Copy()
    {
        var data = GetSelectionData();
        var package = new DataPackage();
        package.SetRtf(data.Rtf.ToString());
        package.SetHtmlFormat(data.HTML.ToString());
        package.SetText(data.Text.ToString());
        Clipboard.SetContent(package);
    }
    public AllowedFormatting AllowedClipboardfFormatting { get; set; } = new()
    {
        Bold = true,
        Italic = true,
        Underline = true,
        SubScript = true,
        SuperScript = true,
        Strikethrough = true,
        Alignment = true,
        TextColor = true,
        FontFamily = true,
        FontSize = true
    };
    public async Task PasteAsync(bool forceNoFormatting, AllowedFormatting? AllowedFormatting = null)
    {
        using (DocumentView.OwnerDocument.UndoManager.OpenGroup("Paste"))
        {
            if (DocumentView.Selection.Range.IsRange)
                DocumentView.Controller.Delete();

            var clipboard = Clipboard.GetContent();

            if (forceNoFormatting) goto Text;

            if (clipboard.Contains(StandardDataFormats.Rtf))
            {
                var handler = new RTF.RTFDocumentHandler(DocumentView, AllowedFormatting ?? AllowedClipboardfFormatting);
                RtfParser.CoreRTFParser.Parse((await clipboard.GetRtfAsync()).AsSpan(), handler);
                DocumentView.Selection.Range = handler.GetHandlerSelectionRange();
                return;
            }
            if (clipboard.Contains(StandardDataFormats.Bitmap))
            {

            }
            if (clipboard.Contains(StandardDataFormats.Html))
            {
                // not implemented yet
                goto Text;
            }
        Text:
            if (clipboard.Contains(StandardDataFormats.Text))
            {
                static string ReplaceNewLine(string text)
                {
                    return text.ReplaceLineEndings(Document.NewParagraphSeparator.ToString());
                }
                DocumentView.Controller.Type(ReplaceNewLine(await clipboard.GetTextAsync()));
                return;
            }
        }
    }
}