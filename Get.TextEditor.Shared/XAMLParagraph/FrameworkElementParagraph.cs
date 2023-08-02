#nullable enable
using SkiaSharp;
using System.Collections.Generic;
using Get.RichTextKit.Editor;

using System.Drawing;
using Get.RichTextKit.Editor.Paragraphs;

using Get.RichTextKit.Styles;
using Get.RichTextKit.Editor.Structs;
using Get.RichTextKit;
using Get.RichTextKit.Utils;
using Get.RichTextKit.Editor.DocumentView;
using Get.EasyCSharp;
using System;

namespace Get.TextEditor;
class DataBindFactory : UIElementSimpleFactory<ContentControl>
{
    object? _Object;
    public object? Object
    {
        get => _Object; set {
            _Object = value;
            foreach (var c in Controls.Values) c.Content = value;
            if (nullControl is not null) nullControl.Content = value;
        }
    }
    DataTemplate? _DataTemplate;

    public DataBindFactory()
    {
        Factory = _ => new() { Content = Object, ContentTemplate = DataTemplate };
    }

    public DataTemplate? DataTemplate
    {
        get => _DataTemplate; set {
            _DataTemplate = value;
            if (nullControl is not null) nullControl.ContentTemplate = value;
            foreach (var c in Controls.Values) c.ContentTemplate = value;
        }
    }
}
class FactoryWrapper : UIElementSimpleFactory<UIElement>
{
    public FactoryWrapper(IUIElementFactory original, Action<IDocumentViewOwner?, UIElement> afterInitialize) : base(view =>
    {
        var element = original.GetUIElement(view);
        afterInitialize(view, element);
        return element;
    })
    {
        
    }
}
class UIElementSimpleFactory<T> : IUIElementFactory<T> where T : UIElement
{
    protected Func<IDocumentViewOwner?, T> Factory;
    public UIElementSimpleFactory(Func<IDocumentViewOwner?, T> factory)
    {
        Factory = factory;
    }
    public UIElementSimpleFactory(Func<T> factory)
    {
        Factory = _ => factory();
    }
    protected UIElementSimpleFactory() { Factory = null!; }
    protected readonly Dictionary<IDocumentViewOwner, T> Controls = new();
    protected T? nullControl;
    public T GetElement(IDocumentViewOwner? view)
    {
        if (view is null)
            return nullControl ??= Factory(view);
        if (Controls.TryGetValue(view, out var control)) return control;
        else return Controls[view] = Factory(view);
    }

    UIElement IUIElementFactory.GetUIElement(IDocumentViewOwner? view)
    => GetElement(view);
}
public interface IUIElementFactory
{
    UIElement GetUIElement(IDocumentViewOwner? view);
}
interface IUIElementFactory<T> : IUIElementFactory where T : UIElement
{
    T GetElement(IDocumentViewOwner? view);
}
internal partial class UIElementParagraph : Paragraph, IAlignableParagraph
{
    readonly IUIElementFactory UIElementFactory;
    UIElement FirstControl;
    public UIElementParagraph(IStyle style, IUIElementFactory factory, IDocumentViewOwner? MainView)
    {
        InternalStyle = style;
        UIElementFactory = factory;
        FirstControl = UIElementFactory.GetUIElement(MainView);
    }
    IStyle InternalStyle { get; set; }
    public override IStyle StartStyle => InternalStyle;
    public override IStyle EndStyle => InternalStyle;

    public override int CodePointLength => 1;

    public override int LineCount => 1;

    public override int DisplayLineCount => 1;

    public RichTextKit.TextAlignment Alignment { get; set; }

    [Property(
        Visibility = GeneratorVisibility.Protected, OverrideKeyword = true,
        PropertyName = "ContentWidthOverride",
        SetVisibility = GeneratorVisibility.DoNotGenerate
    )]
    float _ContentWidth;
    [Property(
        Visibility = GeneratorVisibility.Protected, OverrideKeyword = true,
        PropertyName = "ContentHeightOverride",
        SetVisibility = GeneratorVisibility.DoNotGenerate
    )]
    float _ContentHeight;

    public override void ApplyStyle(IStyle style, int position, int length)
    {
        if (length >= 1)
            InternalStyle = style;
    }

    public override bool CanDeletePartial(DeleteInfo deleteInfo, out TextRange requestedSelection)
    {
        requestedSelection = new(StartCaretPosition.CodePointIndex, EndCaretPosition.CodePointIndex, EndCaretPosition.AltPosition);
        return false;
    }

    public override bool DeletePartial(DeleteInfo deleteInfo, out TextRange requestedSelection, UndoManager<Document, DocumentViewUpdateInfo> UndoManager)
    {
        requestedSelection = new(StartCaretPosition.CodePointIndex, EndCaretPosition.CodePointIndex, EndCaretPosition.AltPosition);
        return false;
    }

    public override CaretInfo GetCaretInfo(CaretPosition position)
    {
        var xOffset = XAlignmentOffset;
        if (position.CodePointIndex == StartCaretPosition.CodePointIndex)
        {
            return new()
            {
                CaretRectangle = new((float)xOffset, 0, (float)xOffset, ContentHeight),
                CaretXCoord = (float)xOffset,
                CodePointIndex = 0,
                LineIndex = 0
            };
        } else
        {
            return new()
            {
                CaretRectangle = new((float)(xOffset + FirstControl.DesiredSize.Width), 0, (float)(xOffset + FirstControl.DesiredSize.Width), ContentHeight),
                CaretXCoord = (float)(xOffset + FirstControl.DesiredSize.Width),
                CodePointIndex = position.CodePointIndex,
                LineIndex = 0
            };
        }
    }

    public override LineInfo GetLineInfo(int line) => new(line, 0, ContentHeight, StartCaretPosition, EndCaretPosition, null, null);

    public override TextRange GetSelectionRange(CaretPosition position, ParagraphSelectionKind kind)
    {
        if (kind is not ParagraphSelectionKind.None)
            return new(StartCaretPosition.CodePointIndex, EndCaretPosition.CodePointIndex, EndCaretPosition.AltPosition);
        else
            return new(position);
    }

    public override IStyle GetStyleAtPosition(CaretPosition position)
    {
        return InternalStyle;
    }

    public override IReadOnlyList<StyleRunEx> GetStyles(int position, int length)
    {
        return new StyleRunEx[] { new(position, length, InternalStyle) };
    }

    public override void GetTextByAppendTextToBuffer(Utf32Buffer buffer, int position, int length)
    {
        buffer.Add(new int[] { Document.NewParagraphSeparator });
    }

    public override HitTestResult HitTest(PointF pt)
        => HitTestLine(0, pt.X);

    public override HitTestResult HitTestLine(int lineIndex, float x)
    {
        if (x > ContentWidth / 2)
        {
            return new()
            {
                ClosestCodePointIndex = 1,
                OverCodePointIndex = 1,
                AltCaretPosition = true,
                ClosestLine = 0,
                OverLine = 0
            };
        } else
        {
            return new()
            {
                ClosestCodePointIndex = 0,
                OverCodePointIndex = 0,
                AltCaretPosition = false,
                ClosestLine = 0,
                OverLine = 0
            };
        }
    }
    readonly List<IDocumentViewOwner> KnownOwners = new();
    public override void Paint(SKCanvas canvas, PaintOptions options)
    {
        RichTextEditorUICanvas.SetArrangeRect(GetElement(options.ViewOwner),
            new(DrawingContentPosition.X + XAlignmentOffset
            , DrawingContentPosition.Y, FirstControl.DesiredSize.Width, FirstControl.DesiredSize.Height)
        );
    }
    public override void NotifyGoingOffscreen(PaintOptions options)
    {
        var view = options.ViewOwner;
        if (view is null) goto End;
        if (KnownOwners.Contains(view))
        {
            if (view is not RichTextEditor editor) return;
            var element = UIElementFactory.GetUIElement(view);
            editor.UnsafeGetUICanvas().Children.Remove(element);
            KnownOwners.Remove(view);
        }
    End:
        base.NotifyGoingOffscreen(options);
    }
    protected override void OnParagraphRemoved(Document owner)
    {
        foreach (var view in KnownOwners)
        {
            if (view is not RichTextEditor editor) continue;
            var element = UIElementFactory.GetUIElement(view);
            editor.UnsafeGetUICanvas().Children.Remove(element);
        }
        KnownOwners.Clear();
        base.OnParagraphRemoved(owner);
    }
    double XAlignmentOffset =>
            Alignment switch
            {
                RichTextKit.TextAlignment.Right => ContentWidth - FirstControl.DesiredSize.Width,
                RichTextKit.TextAlignment.Center => (ContentWidth - FirstControl.DesiredSize.Width) / 2,
                RichTextKit.TextAlignment.Left or RichTextKit.TextAlignment.Auto or _ => 0
            };
    UIElement GetElement(IDocumentViewOwner? view)
    {
        var element = UIElementFactory.GetUIElement(view);
        if (view is not RichTextEditor editor) return element;
        if (!KnownOwners.Contains(view))
        {
            editor.UnsafeGetUICanvas().Children.Add(element);
            KnownOwners.Add(view);
        }
        return element;
    }
    public override bool ShouldDeletAll(DeleteInfo deleteInfo)
    {
        if (deleteInfo.Range.Start <= StartCaretPosition.CodePointIndex && deleteInfo.Range.End >= EndCaretPosition.CodePointIndex)
            return true;
        return false;
    }

    public override Paragraph Split(UndoManager<Document, DocumentViewUpdateInfo> UndoManager, int splitIndex)
    {
        throw new System.NotImplementedException();
    }

    protected override void LayoutOverride(LayoutParentInfo owner)
    {
        FirstControl.Measure(new(owner.AvaliableWidth, double.PositiveInfinity));
        var desiredSize = FirstControl.DesiredSize;
        _ContentWidth = owner.AvaliableWidth;
        _ContentHeight = (float)desiredSize.Height;
    }

    protected override NavigationStatus NavigateOverride(TextRange selection, NavigationSnap snap, NavigationDirection direction, bool keepSelection, ref float? ghostXCoord, out TextRange newSelection)
    {
        if (direction is NavigationDirection.Up or NavigationDirection.Down)
            return VerticalNavigateUsingLineInfo(selection, snap, direction, keepSelection, ref ghostXCoord, out newSelection);
        ghostXCoord = null;
        if (direction is NavigationDirection.Forward)
        {
            if (selection.End < EndCaretPosition.CodePointIndex)
            {
                selection.EndCaretPosition = EndCaretPosition;
                if (!keepSelection) selection.Start = selection.End;
                newSelection = selection;
                return NavigationStatus.Success;
            }
            newSelection = default;
            return NavigationStatus.MoveAfter;
        }
        else if (direction is NavigationDirection.Backward)
        {
            if (selection.Start > StartCaretPosition.CodePointIndex)
            {
                selection.EndCaretPosition = EndCaretPosition;
                if (!keepSelection) selection.Start = selection.End;
                newSelection = selection;
                return NavigationStatus.Success;
            }
            newSelection = default;
            return NavigationStatus.MoveBefore;
        }
        else throw new System.ArgumentOutOfRangeException();
    }
}