//using SkiaSharp;
//using System.Collections.Generic;
//using Get.RichTextKit.Editor;
//using Topten.RichTextKit;
//using Windows.UI.Xaml;
//using System.Drawing;
//using Topten.RichTextKit.Utils;
//using Get.RichTextKit.Editor.Paragraphs;
//using Windows.UI.Xaml.Controls;

//namespace Get.TextEditor;

//internal class FrameworkElementParagraph : Paragraph
//{
//    readonly Dictionary<IDocumentViewOwner, ContentControl> ViewerControls = new();
//    FrameworkElement ele;
//    IStyle _PlaceholderStyle;
//    public IStyle PlaceholderStyle { get => _PlaceholderStyle; set => ApplyStyle(value, 0, 0); }
//    public FrameworkElementParagraph()
//    {
//        ele.SizeChanged += (_, _) => Owner?.Layout.Invalidate();
//    }
//    float _width;
//    /// <inheritdoc />
//    public override void Layout(IParentInfo owner)
//    {
//        DataTemplate template = new();
//        _width = 
//                owner.AvaliableWidth
//                - Margin.Left - Margin.Right;

//        // For layout just need to set the appropriate layout width on the text block
//        if (owner.LineWrap)
//        {
//            ele.MaxWidth = ele.Width;
//        }
//        else
//            ele.MaxWidth = double.PositiveInfinity;
//    }
//    void LateLayout()
//    {
//        _width =
//                owner.AvaliableWidth
//                - Margin.Left - Margin.Right;

//        // For layout just need to set the appropriate layout width on the text block
//        if (owner.LineWrap)
//        {
//            ele.MaxWidth = ele.Width;
//        }
//        else
//            ele.MaxWidth = double.PositiveInfinity;
//    }

//    /// <inheritdoc />
//    public override void Paint(SKCanvas canvas, TextPaintOptions options, IDocumentViewOwner owner) {
//        RichTextEditorUICanvas.SetArrangeRect(ele, new(DrawingContentPosition.X, DrawingContentPosition.Y, _width, ele.DesiredSize.Height));

//        ele.Translation = new(DrawingContentPosition.X, DrawingContentPosition.Y, 0);

//        if (options.Selection.HasValue)
//        {
//            var range = options.Selection.Value;
//            if (range.Minimum is <= 0 && range.Maximum is >= 1)
//            {
//                using var paint = new SKPaint() { Color = options.SelectionColor };
//                canvas.DrawRect(DrawingContentPosition.X, DrawingContentPosition.Y, (float)ele.ActualWidth, (float)ele.ActualHeight, paint);
//            }
//        }
//    }

//    /// <inheritdoc />
//    public override CaretInfo GetCaretInfo(CaretPosition position)
//    {
//        if (position.CodePointIndex == 0)
//        {
//            return
//                new()
//                {
//                    CaretXCoord = 0,
//                    LineIndex = 0,
//                    CodePointIndex = position.CodePointIndex,
//                    CaretRectangle = new(0, 0, 0, (float)ele.ActualHeight)
//                };
//        } else
//        {

//            return
//                new()
//                {
//                    CaretXCoord = 0,
//                    LineIndex = 0,
//                    CodePointIndex = position.CodePointIndex,
//                    CaretRectangle = new(0, 0, 0, (float)ele.ActualHeight)
//                };
//        }
//    }

//    /// <inheritdoc />
//    public override HitTestResult HitTest(PointF pt) => HitTestResult.None;

//    /// <inheritdoc />
//    public override HitTestResult HitTestLine(int lineIndex, float x) => HitTestResult.None;

//    public override void DeletePartial(UndoManager<Document> UndoManager, SubRun range)
//    {
//        // do nothing
//    }

//    public override bool TryJoin(UndoManager<Document> UndoManager, int thisIndex)
//    {
//        // can't join with others
//        return false;
//    }

//    public override Paragraph Split(UndoManager<Document> UndoManager, int splitIndex)
//    {
//        // no can't split
//        return null;
//    }
//    public string PlaceholderText = "UIElement";
//    public override void GetText(Utf32Buffer buffer, SubRun range)
//    {
//        buffer.Add(PlaceholderText);
//    }

//    public override IStyle GetStyleAtPosition(CaretPosition position)
//        => PlaceholderStyle;

//    public override IReadOnlyList<StyleRunEx> GetStyles(SubRun range)
//        => new StyleRunEx[] { new(range.Offset, range.Length, PlaceholderStyle) };

//    public override void ApplyStyle(IStyle style, int position, int length)
//    {
//        _PlaceholderStyle = style;
//    }

//    /// <inheritdoc />
//    public override IReadOnlyList<int> CaretIndicies => new int[] { 0, 1 };

//    /// <inheritdoc />
//    public override IReadOnlyList<int> WordBoundaryIndicies => new int[] { 0, 1 };

//    /// <inheritdoc />
//    public override IReadOnlyList<int> LineIndicies => new int[] { 0, 1 };

//    /// <inheritdoc />
//    public override int Length => 1;

//    /// <inheritdoc />
//    public override float ContentWidth => (float)ele.DesiredSize.Width;

//    /// <inheritdoc />
//    public override float ContentHeight
//    {
//        get
//        {
//            return (float)ele.DesiredSize.Height;
//        }
//    }

//    public override IStyle StartStyle => PlaceholderStyle;

//    public override IStyle EndStyle => PlaceholderStyle;

//    public override int LineCount => 1;

//    protected override void OnParagraphAdded(Document owner)
//    {
//        base.OnParagraphAdded(owner);
//        Canvas.Children.Add(ele);
//    }
//    protected override void OnParagraphRemoved(Document owner)
//    {
//        base.OnParagraphRemoved(owner);
//        Canvas.Children.Remove(ele);
//    }
//}