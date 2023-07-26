using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Get.XAMLTools;
namespace Get.TextEditor;

[AttachedProperty(typeof(Rect), "ArrangeRect", GenerateLocalOnPropertyChangedMethod = true)]
partial class RichTextEditorUICanvas : Panel
{
    static partial void OnArrangeRectChanged(DependencyObject obj, Rect oldValue, Rect newValue)
    {
        (VisualTreeHelper.GetParent(obj) as RichTextEditorUICanvas)?.InvalidateArrange();
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            child.Arrange(GetArrangeRect(child));
        }
        return finalSize;
    }
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }
        return base.MeasureOverride(availableSize);
    }
}
