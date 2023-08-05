using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DocumentView;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using Get.TextEditor.Data;
using System.Text;
using System.Collections.Generic;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    void Copy()
    {
        var data = DocumentDataGenerator.Generate(
            DocumentView.OwnerDocument,
            DocumentView.Selection.Range.Normalized,
            new DocumentDataGeneratorHandler()
        );
        var package = new DataPackage();
        package.SetRtf(data.Rtf.ToString());
        package.SetHtmlFormat(data.HTML.ToString());
        package.SetText(data.Text.ToString());
        Clipboard.SetContent(package);
    }
    async Task PasteAsync(bool forceNoFormatting)
    {
        using (DocumentView.OwnerDocument.UndoManager.OpenGroup("Paste"))
        {
            if (DocumentView.Selection.Range.IsRange)
                DocumentView.Controller.Delete();

            var clipboard = Clipboard.GetContent();

            if (forceNoFormatting) goto Text;

            if (clipboard.Contains(StandardDataFormats.Rtf))
            {
                var handler = new RTF.RTFDocumentHandler(DocumentView, new()
                {
                    Bold = true,
                    Italic = true,
                    Underline = true,
                    SubScript = true,
                    SuperScript = true,
                    Strikethrough = true
                });
                RtfParser.CoreRTFParser.Parse((await clipboard.GetRtfAsync()).AsSpan(), handler);
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