using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.Paragraphs;
using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Windows.UI.Xaml;
using static Get.RichTextKit.Editor.Paragraphs.Paragraph;

namespace Get.TextEditor.UWP.Decoration;

public class UIDecoration : IParagraphDecoration
{
    readonly IUIElementFactory Factory;
    public UIDecoration(IUIElementFactory factory)
    {
        Factory = factory;
    }

    public float FrontOffset => 90;

    public virtual string TypeIdentifier => "UIDecoration";

    public virtual CountMode CountMode => CountMode.Default;

    public virtual IParagraphDecoration Clone()
    {
        return new UIDecoration(Factory);
    }

    readonly List<IDocumentViewOwner> KnownOwners = new();
    public void Paint(SKCanvas canvas, DecorationPaintContext context)
    {
        RichTextEditorUICanvas.SetArrangeRect(
            GetElement(context.ViewOwner),
            new(context.AvaliableSpace.X, context.AvaliableSpace.Y, context.AvaliableSpace.Width, context.AvaliableSpace.Height)
        );
    }
    UIElement GetElement(IDocumentViewOwner? view)
    {
        var element = Factory.GetUIElement(view);
        if (view is not RichTextEditor editor) return element;
        if (!KnownOwners.Contains(view))
        {
            editor.UnsafeGetUICanvas().Children.Add(element);
            KnownOwners.Add(view);
        }
        return element;
    }

    public virtual void NotifyGoingOffscreen(DecorationOffscreenNotifyContext context)
    {
        var view = context.ViewOwner;
        if (KnownOwners.Contains(view))
        {
            if (view is not RichTextEditor editor) return;
            var element = Factory.GetUIElement(view);
            editor.UnsafeGetUICanvas().Children.Remove(element);
            KnownOwners.Remove(view);
        }
    }

    public virtual void RemovedFromLayout()
    {
        foreach (var view in KnownOwners)
        {
            if (view is not RichTextEditor editor) continue;
            var element = Factory.GetUIElement(view);
            editor.UnsafeGetUICanvas().Children.Remove(element);
        }
        KnownOwners.Clear();
    }
}
