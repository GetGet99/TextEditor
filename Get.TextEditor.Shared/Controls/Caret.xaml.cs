#nullable enable
using Get.XAMLTools;
using System.Numerics;
using Get.RichTextKit.Editor.DocumentView;
using System.ComponentModel;
using Get.EasyCSharp;
namespace Get.TextEditor;
[DependencyProperty<DocumentView>("DocumentView", GenerateLocalOnPropertyChangedMethod = true)]
[DependencyProperty<bool>("SelectionHandle", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(Update))]
[DependencyProperty<CaretDisplayMode>("CaretDisplayMode", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(Update))]
public sealed partial class Caret : INotifyPropertyChanged
{
    public Caret()
    {
        this.InitializeComponent();
        scrollTimer.Tick += delegate
        {
            scrollTimer.Stop();
            TranslationTransition = TranslateTransitionCache;
        };
        PlatformLibrary.SetCursor(HandleElement, PlatformCursorType.SizeAll);
    }
    //CoreCursor? oldCursor;
    //void GridPoitnerEnter(object sender, Platform.UI.Xaml.Input.PointerRoutedEventArgs e)
    //{
    //    oldCursor = CoreWindow.GetForCurrentThread().PointerCursor;
    //    CoreWindow.GetForCurrentThread().PointerCursor = new(CoreCursorType.SizeAll, default);
    //}
    //void GridPoitnerLeave(object sender, Platform.UI.Xaml.Input.PointerRoutedEventArgs e)
    //{
    //    CoreWindow.GetForCurrentThread().PointerCursor = oldCursor;
    //}
    private void CancelPointerEvent(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid && e.Pointer.PointerDeviceType is not PointerDeviceType.Touch)
            return;
        e.Handled = true;
    }

    private void CancelPointerEvent2(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid && e.Pointer.PointerDeviceType is not PointerDeviceType.Touch)
            return; 
        e.Handled = true;
    }
    Point? handleDragStart;
    private void HandleManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        if (sender is Grid && e.PointerDeviceType is not PointerDeviceType.Touch)
            return;
        e.Handled = true;
        handleDragStart = new(_CaretRect.X, _CaretRect.Y);
        TranslationTransition = null;
    }

    private void HandleManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        if (sender is Grid && e.PointerDeviceType is not PointerDeviceType.Touch)
            return;
        e.Handled = true;
        var translation = e.Cumulative.Translation;
        var pt = handleDragStart!.Value;
        pt.X += translation.X;
        pt.Y += translation.Y;
        var hittest = DocumentView.Controller.HitTest(new((float)pt.X, (float)(pt.Y + _CaretRect.Height / 2)));
        var range = DocumentView.Selection.Range;
        if (CaretDisplayMode is CaretDisplayMode.Primary)
        {
            if (range.IsRange)
                DocumentView.Selection.Range = new(range.Start, hittest.ClosestCodePointIndex, hittest.AltCaretPosition);
            else
                DocumentView.Selection.Range = new(hittest.CaretPosition);
        }
        else
        {
            DocumentView.Selection.Range = new(hittest.ClosestCodePointIndex, range.End, range.AltPosition);
        }
        CaretRect = new(pt, new Size(CaretRect.Width, CaretRect.Height));
    }
    private void HandleManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        if (sender is Grid touchHitTest && e.PointerDeviceType is not PointerDeviceType.Touch)
            return;
        e.Handled = true;
        handleDragStart = null;
        TranslationTransition = TranslateTransitionCache;
        Update();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    [AutoNotifyProperty(Visibility = GeneratorVisibility.Private)]
    Rect _CaretRect;
    Vector3 MainTranslation(Rect caretRect) => Vector(caretRect.X - 15, caretRect.Y);
    Vector3 Vector(double X, double Y)
        => Vector((float)X, (float)Y);
    Vector3 Vector(float X, float Y)
        => new(X, Y, 1);
    double GetOpacity(CaretDisplayMode mode)
        => mode is CaretDisplayMode.Primary ? 1 : 0.2;
    partial void OnDocumentViewChanged(DocumentView oldValue, DocumentView newValue)
    {
        newValue.Selection.RangeChanged += Update;
        newValue.RedrawRequested += Update;
        newValue.OwnerDocument.Layout.Updated += Update;
        newValue.YScrollChanged += ScrollChanged;
        Update();
        ScrollChanged();
    }
    DispatcherTimer scrollTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    [Event<EventHandler>]
    void ScrollChanged()
    {
        TranslationTransition = null;
        Update();

        scrollTimer.Start();
    }
    [Event<Action<DocumentView>>]
    [Event<EventHandler>]
    void Update()
    {
        if (handleDragStart is not null) return;
        var view = DocumentView;
        if (view is null) return;
        if (CaretDisplayMode is CaretDisplayMode.Secondary)
        {
            Visibility = SelectionHandle && view.Selection.Range.IsRange ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            Visibility = Visibility.Visible;
        }
        var info = CaretDisplayMode is CaretDisplayMode.Primary ? view.Selection.EndCaretInfo : view.Selection.StartCaretInfo;
        var newRect = new Rect(Math.Min(info.CaretRectangle.Left, info.CaretRectangle.Right), Math.Min(info.CaretRectangle.Top, info.CaretRectangle.Bottom), Math.Abs(info.CaretRectangle.Width), Math.Abs(info.CaretRectangle.Height));
        //if (handleDragStart is null)
        CaretRect = newRect;
        //else
        //    CaretRect = new(handleDragStart.Value, new Size(newRect.Width, newRect.Height));
    }
}
public enum CaretDisplayMode
{
    Primary,
    Secondary
}