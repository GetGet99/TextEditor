using Get.RichTextKit.Editor;
using Windows.UI.Xaml.Controls;
using System;
using Get.RichTextKit.Editor.DocumentView;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using Get.TextEditor.Data;

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
                    AllowBold = true,
                    AllowItalic = true,
                    AllowUnderline = true,
                    AllowSubScript = true,
                    AllowSuperScript = true,
                    AllowStrikethrough = true
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
                DocumentView.Controller.Type((await clipboard.GetTextAsync()).Replace('\n', Document.NewParagraphSeparator));
                return;
            }
        }
    }
}