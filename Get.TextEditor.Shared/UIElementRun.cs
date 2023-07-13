using SkiaSharp;
using System.Collections.Generic;
using Get.RichTextKit.Editor;
using Get.RichTextKit;
using Windows.UI.Xaml;
using SkiaSharp.Views.UWP;
using Style = Get.RichTextKit.Style;
using System.Drawing;
using Get.RichTextKit.Utils;
using Get.RichTextKit.Editor.Paragraphs;
using System.Numerics;
using System;
using Get.RichTextKit.Styles;

namespace Get.TextEditor;

internal class FrameworkElementRun : StyledText
{
    class UIElementStyleC : Style, INotifyRenderStyle, ICustomRenderSizeStyle, IKeepCustomStylingStyle, IDoNotCombineStyle, INotifyInvalidateStyle
    {
        FrameworkElementRun Owner;
        public UIElementStyleC(FrameworkElementRun owner) {
            Owner = owner;
        }

        public SKSize CustomRenderSize { get; set; }
        public TextVerticalAlignment TextVerticalAlignment { get; set; }

        public event Action Invalidate;

        public void OnDelete()
        {
            Owner.Canvas.Children.Remove(Owner.ele);
        }
        public void InvalidateText() => Invalidate?.Invoke();

        public void OnRender(SKRect region)
        {
            RichTextEditorUICanvas.SetArrangeRect(Owner.ele, new(region.Left, region.Top, region.Width, region.Height));
            //Owner.ele.Translation = new(region.Left, region.Top, 0);
            Owner.Canvas.InvalidateArrange();
        }
    }
    readonly UIElementStyleC UIElementStyle;
    RichTextEditorUICanvas Canvas;
    RichTextEditor editor;
    readonly FrameworkElement ele;
    IStyle _PlaceholderStyle;
    public IStyle PlaceholderStyle { get => _PlaceholderStyle; set => PlaceholderStyle = value; }
    public FrameworkElementRun(RichTextEditor editor, FrameworkElement element, IStyle textStyle) : base(new Utf32Buffer("a"/*"\uFFFD"*/).AsSlice())
    {
        var owner = editor.UnsafeGetUICanvas();


        UIElementStyle = new(this);
        CopyStyle.Copy(textStyle, UIElementStyle);
        Canvas = owner;
        ele = element;
        owner.Children.Add(ele);
        UIElementStyle.CustomRenderSize = ele.ActualSize.ToSize().ToSKSize();
        ele.SizeChanged += (_, _) => {
            UIElementStyle.CustomRenderSize = ele.ActualSize.ToSize().ToSKSize();
            ApplyStyle(0, Length, UIElementStyle);
            UIElementStyle.InvalidateText();
            editor.DocumentView.OwnerDocument.Layout.Invalidate();
            editor.DocumentView.OwnerDocument.RequestRedraw();
        };
        ele.RegisterPropertyChangedCallback(FrameworkElement.VerticalAlignmentProperty, (o, e) =>
        {
            SetVerticalAlignment();
            ApplyStyle(0, Length, UIElementStyle);
        });
        SetVerticalAlignment();
        void SetVerticalAlignment()
        {
            UIElementStyle.TextVerticalAlignment = ele.VerticalAlignment switch
            {
                VerticalAlignment.Top => TextVerticalAlignment.Top,
                VerticalAlignment.Center => TextVerticalAlignment.Center,
                VerticalAlignment.Bottom => TextVerticalAlignment.Bottom,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
        ApplyStyle(0, Length, UIElementStyle);
        //Owner?.Layout.Invalidate();
        //_textBlock = new TextBlock();
        //_textBlock.AddText("?", new Style());
    }
    //float _width;
    /// <inheritdoc />
    //public override void Layout(IParentInfo owner)
    //{
    //    _width = 
    //            owner.AvaliableWidth
    //            - Margin.Left - Margin.Right;

    //    // For layout just need to set the appropriate layout width on the text block
    //    if (owner.LineWrap)
    //    {
    //        ele.MaxWidth = ele.Width;
    //    }
    //    else
    //        ele.MaxWidth = double.PositiveInfinity;
    //}

    ///// <inheritdoc />
    //public override void Paint(SKCanvas canvas, TextPaintOptions options) {
    //    RichTextEditorCanvas.SetArrangeRect(ele, new(GlobalContentPosition.X, GlobalContentPosition.Y, _width, ele.DesiredSize.Height));

    //    ele.Translation = new(GlobalContentPosition.X, GlobalContentPosition.Y, 0);

    //    if (options.Selection.HasValue)
    //    {
    //        var range = options.Selection.Value;
    //        if (range.Minimum is <= 0 && range.Maximum is >= 1)
    //            canvas.DrawRect(GlobalContentPosition.X, GlobalContentPosition.Y, (float)ele.ActualWidth, (float)ele.ActualHeight, new() { Color = options.SelectionColor });
    //    }
    //}

    ///// <inheritdoc />
    //public override CaretInfo GetCaretInfo(CaretPosition position)
    //{
    //    if (position.CodePointIndex == 0)
    //    {
    //        return
    //            new()
    //            {
    //                CaretXCoord = 0,
    //                LineIndex = 0,
    //                CodePointIndex = position.CodePointIndex,
    //                CaretRectangle = new(0, 0, 0, (float)ele.ActualHeight)
    //            };
    //    } else
    //    {

    //        return
    //            new()
    //            {
    //                CaretXCoord = 0,
    //                LineIndex = 0,
    //                CodePointIndex = position.CodePointIndex,
    //                CaretRectangle = new(0, 0, 0, (float)ele.ActualHeight)
    //            };
    //    }
    //}

    ///// <inheritdoc />
    //public override HitTestResult HitTest(PointF pt) => HitTestResult.None;

    ///// <inheritdoc />
    //public override HitTestResult HitTestLine(int lineIndex, float x) => HitTestResult.None;

    //public override void DeletePartial(UndoManager<Document> UndoManager, SubRun range)
    //{
    //    // do nothing
    //}

    //public override bool TryJoin(UndoManager<Document> UndoManager, int thisIndex)
    //{
    //    // can't join with others
    //    return false;
    //}

    //public override Paragraph Split(UndoManager<Document> UndoManager, int splitIndex)
    //{
    //    // no can't split
    //    return null;
    //}
    //public string PlaceholderText = "UIElement";
    //public override void GetText(Utf32Buffer buffer, SubRun range)
    //{
    //    buffer.Add(PlaceholderText);
    //}

    //public override IStyle GetStyleAtPosition(CaretPosition position)
    //    => PlaceholderStyle;

    //public override IReadOnlyList<StyleRunEx> GetStyles(SubRun range)
    //    => new StyleRunEx[] { new(range.Offset, range.Length, PlaceholderStyle) };

    //public override void ApplyStyle(IStyle style, int position, int length)
    //{
    //    _PlaceholderStyle = style;
    //}

    ///// <inheritdoc />
    //public override IReadOnlyList<int> CaretIndicies => new int[] { 0, 1 };

    ///// <inheritdoc />
    //public override IReadOnlyList<int> WordBoundaryIndicies => new int[] { 0, 1 };

    ///// <inheritdoc />
    //public override IReadOnlyList<int> LineIndicies => new int[] { 0, 1 };

    ///// <inheritdoc />
    //public override int Length => 1;

    ///// <inheritdoc />
    //public override float ContentWidth => (float)ele.DesiredSize.Width;

    ///// <inheritdoc />
    //public override float ContentHeight
    //{
    //    get
    //    {
    //        return (float)ele.DesiredSize.Height;
    //    }
    //}

    //public override IStyle StartStyle => PlaceholderStyle;

    //public override IStyle EndStyle => PlaceholderStyle;
    //protected override void OnParagraphAdded(Document owner)
    //{
    //    base.OnParagraphAdded(owner);
    //    Canvas.Children.Add(ele);
    //}
    //protected override void OnParagraphRemoved(Document owner)
    //{
    //    base.OnParagraphRemoved(owner);
    //    Canvas.Children.Remove(ele);
    //}


    // Private attributes
    TextBlock _textBlock;
}