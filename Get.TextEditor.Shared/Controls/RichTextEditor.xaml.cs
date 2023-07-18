using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    internal RichTextEditorUICanvas UnsafeGetUICanvas() => UICanvas;
    void InitXAML()
    {
        InitializeComponent();
        ManipulationDelta += (o, e) =>
        {
            if (ShouldManipulationScroll(e.PointerDeviceType))
            {
                DocumentView.YScroll += (float)-e.Delta.Translation.Y;
            }
        };
    }
    void InsertUIFrameworkElement(FrameworkElement ele)
    {
        ele.PointerEntered += (_, e) =>
        {
            IsCursorInside = false;
        };
        DocumentView.Controller.Type(new FrameworkElementRun(
            this,
            ele,
            DocumentView.Selection.Range.IsRange ?
            DocumentView.OwnerDocument.GetStyleAtPosition(DocumentView.Selection.Range.Normalized.EndCaretPosition) :
            DocumentView.Selection.CurrentCaretStyle
        ));
    }
}