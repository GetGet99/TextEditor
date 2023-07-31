using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TryRichText.UWP;

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