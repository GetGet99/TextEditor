using Get.RichTextKit.Editor;
using Windows.UI.Xaml.Controls;
using System;
using Get.RichTextKit.Editor.DocumentView;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
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
                var handler = new RTF.RTFDocumentHandler(DocumentView)
                {
                    AllowBold = false,
                    AllowItalic = true,
                    AllowUnderline = false,
                    AllowSubScript = true,
                    AllowSuperScript = true
                };
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