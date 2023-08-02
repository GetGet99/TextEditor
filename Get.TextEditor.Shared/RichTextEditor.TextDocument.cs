using SkiaSharp.Views.UWP;

using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Styles;

namespace Get.TextEditor;
using Style = RichTextKit.Style;
partial class RichTextEditor : UserControl
{
    static readonly IStyle DefaultStyle = new Style()
    {
        FontFamily = "Segoe UI",
        FontSize = 32
    };
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
    }
}