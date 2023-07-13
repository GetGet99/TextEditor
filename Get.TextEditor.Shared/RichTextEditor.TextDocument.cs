using Windows.UI.ViewManagement;
using SkiaSharp.Views.UWP;
using Windows.UI.Xaml.Controls;
using Get.RichTextKit.Editor.DocumentView;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    public readonly DocumentView DocumentView;
    void InitTextDocument()
    {
        void SetSelectionColor()
        {
            DocumentView.SelectionColor = Constants.UISettings
                .GetColorValue(UIColorType.AccentDark1)
                .ToSKColor()
                .WithAlpha(255 / 2)
                ;
            EditorCanvas?.Invalidate();
        }
        Constants.UISettings.ColorValuesChanged += (_, _) => SetSelectionColor();
        SetSelectionColor();
        DocumentView.OwnerDocument.Layout.LineWrap = true;

        var all = DocumentView.OwnerDocument[DocumentView.OwnerDocument.GetSelectionRange(default, Get.RichTextKit.Editor.SelectionKind.Document)];
        all.FontFamily = "Segoe UI";
        all.FontSize = 32;
    }
}