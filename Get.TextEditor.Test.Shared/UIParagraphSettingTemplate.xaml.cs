using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using Get.TextEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace TryRichText.Shared;

public sealed partial class UIParagraphSettingTemplate : Page
{
    public UIParagraphSettingTemplate()
    {
        this.InitializeComponent();
        
    }
    public DataTemplateSelector GetDataTemplateSelector() => new D(this);
    class D : DataTemplateSelector
    {
        UIParagraphSettingTemplate Parent;
        public D(UIParagraphSettingTemplate uIParagraphSettingTemplate)
        {
            Parent = uIParagraphSettingTemplate;
        }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case TextParagraph para:
                    if (para.TextBlock.Length is 1 && para.GetText(0, 1)[0] is Document.NewParagraphSeparator)
                        return Parent.EmptyTextParagraph;
                    goto default;
                case TableParagraph _:
                    return Parent.TableParagraph;
                default:
                    return null;
            }
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }

    private void ConfirmTableCreation(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not Paragraph para) return;
        if (para.Owner is not { } document) return;
        var style = para.StartStyle;
        document.Paragraphs[para.GlobalParagraphIndex] = new TableParagraph(style, 3, 3);
    }

    private async void AddImage(object sender, RoutedEventArgs e)
    {
        FileOpenPicker picker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            FileTypeFilter = { ".jpg", ".jpeg", ".png" },
            CommitButtonText = "Insert"
        };
        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not Paragraph para) return;
            if (para.Owner is not { } document) return;
            if (GetParent(btn).FirstOrDefault(x => x is IDocumentViewOwner) is not IDocumentViewOwner parentEditor) return;
            var style = para.StartStyle;
            var stream = await file.OpenReadAsync();
            var img = new BitmapImage();
            img.SetSource(stream);
            document.Paragraphs[para.GlobalParagraphIndex] = new ImageParagraph(
                style,
                parentEditor,
                img
            );
        }
    }
    IEnumerable<DependencyObject> GetParent(DependencyObject element)
    {
        while (element is not null)
        {
            yield return element;
            element = VisualTreeHelper.GetParent(element);
        }
    }
}
static class XAMLHelper
{
    public static Vector3 Vector3(float x, float y, float z = 0)
        => new(x, y, z);
    public static Vector3 Vector3(Paragraph.LayoutInfo info, float height)
        => Vector3(info.OffsetFromThis(new PointF(0, height + 10)));
    public static Vector3 Vector3(PointF pt)
        => Vector3(pt.X, pt.Y);
}