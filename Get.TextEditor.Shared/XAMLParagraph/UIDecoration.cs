using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor.Paragraphs.Properties.Decoration;
using Get.RichTextKit.Editor.Structs;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

using static Get.RichTextKit.Editor.Paragraphs.Paragraph;
using VerticalAlignment = Get.RichTextKit.Editor.Paragraphs.Properties.Decoration.VerticalAlignment;

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

    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

    public virtual IParagraphDecoration Clone()
    {
        return new UIDecoration(Factory);
    }

    readonly List<IDocumentViewOwner> KnownOwners = new();
    public void Paint(SKCanvas canvas, DecorationPaintContext context)
    {
        LineInfo l = default;
        var topLeft = new PointF(context.AvaliableSpace.Left, VerticalAlignment switch
        {
            VerticalAlignment.Top => (context.AvaliableSpace.Top + context.OwnerParagraph.GetLineInfo(0).Assign(out l).Y),
            VerticalAlignment.Center => context.AvaliableSpace.Top,
            VerticalAlignment.Bottom => (context.AvaliableSpace.Top + context.OwnerParagraph.GetLineInfo(context.OwnerParagraph.LineCount - 1).Assign(out l).Y),
            _ => throw new ArgumentOutOfRangeException()
        });
        var element = GetElement(context.ViewOwner);
        element.Measure(new(context.AvaliableSpace.Width, context.AvaliableSpace.Height));
        var height = VerticalAlignment switch
        {
            VerticalAlignment.Top or VerticalAlignment.Bottom => Math.Max(element.DesiredSize.Height, l.Height),
            VerticalAlignment.Center => context.AvaliableSpace.Height,
            _ => throw new ArgumentOutOfRangeException()
        };
        RichTextEditorUICanvas.SetArrangeRect(
            element,
            new(topLeft.X, topLeft.Y,
            context.AvaliableSpace.Width,
            height
            )
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
static partial class Extension
{
    public static T Assign<T>(this T item, out T variable)
    {
        variable = item;
        return item;
    }
}