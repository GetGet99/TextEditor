#nullable enable
using System.Numerics;
using Get.XAMLTools;
using Get.RichTextKit.Editor.DocumentView;
using Get.EasyCSharp;

using SkiaSharp;

namespace Get.TextEditor;
[DependencyProperty<DocumentView>("DocumentView", GenerateLocalOnPropertyChangedMethod = true)]
[DependencyProperty<bool>("SelectionHandle", GenerateLocalOnPropertyChangedMethod = true)]
sealed partial class RichTextEditorCanvas : PlatformSKSwapChainPanel
{
    internal bool ManipulationScrolled { get; set; } = false;
    internal void ResetManipulationScrollTracker() => ManipulationScrolled = false;
    readonly ElementInteractionTracker tracker;
    public RichTextEditorCanvas()
    {
        InitializeComponent();
        tracker = new(this);
        tracker.ValuesChangedEvent += (obj) =>
        {
            var delta = obj.Position - _prevPosition;
            _prevPosition = obj.Position;
            DocumentView.YScroll += delta.Y;
        };
        SizeChanged += delegate
        {
            DocumentView.ViewWidth = (float)ActualWidth;
            DocumentView.ViewHeight = (float)ActualHeight;
        };
        PaintSurface += (o, e) =>
        {
            DocumentView.OwnerDocument.Layout.PageWidth = e.Info.Width;
            e.Surface.Canvas.Clear();
            DocumentView.Paint(e.Surface.Canvas, ActualTheme is ElementTheme.Dark ? SKColors.White : SKColors.Black
            );
        };
        ActualThemeChanged += (_, _) => Invalidate();
        ManipulationDelta += (o, e) =>
        {
            ManipulationScrolled = true;
            DocumentView.YScroll += (float)e.Delta.Translation.Y;
        };
    }
    Vector3 _prevPosition;
    partial void OnDocumentViewChanged(DocumentView? oldValue, DocumentView newValue)
    {
        if (oldValue is not null)
        {
            oldValue.OwnerDocument.Layout.Updating -= InvalidateMeasure;
            oldValue.OwnerDocument.Layout.Updated -= InvalidateArrange;
            oldValue.RedrawRequested -= RedrawRequested;
        }
        newValue.OwnerDocument.Layout.Updating += InvalidateMeasure;
        newValue.OwnerDocument.Layout.Updated += InvalidateArrange;
        newValue.RedrawRequested += RedrawRequested;
    }
    [Event<Action<DocumentView>>]
    void RedrawRequested()
    {
        Invalidate();
    }
}

class ElementInteractionTracker : IInteractionTrackerOwner
{
    public InteractionTracker InteractionTracker { get; }
    public VisualInteractionSource ScrollPresenterVisualInteractionSource { get; }
    public ElementInteractionTracker(UIElement element)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        InteractionTracker = InteractionTracker.CreateWithOwner(visual.Compositor, this);
        InteractionTracker.MinPosition = new Vector3(-1000, -1000, -1000);
        InteractionTracker.MaxPosition = new Vector3(1000, 1000, 1000);
        InteractionTracker.MinScale = 0.5f;
        InteractionTracker.MaxScale = 5f;

        InteractionTracker.InteractionSources.Add(
            ScrollPresenterVisualInteractionSource = VisualInteractionSource.Create(visual)
        );
        ScrollPresenterVisualInteractionSource.IsPositionXRailsEnabled =
            ScrollPresenterVisualInteractionSource.IsPositionYRailsEnabled = true;


        ScrollPresenterVisualInteractionSource.PointerWheelConfig.PositionXSourceMode =
            ScrollPresenterVisualInteractionSource.PointerWheelConfig.PositionYSourceMode
            = InteractionSourceRedirectionMode.Enabled;

        ScrollPresenterVisualInteractionSource.PositionXChainingMode =
            ScrollPresenterVisualInteractionSource.ScaleChainingMode =
            InteractionChainingMode.Auto;

        ScrollPresenterVisualInteractionSource.PositionXSourceMode =
            ScrollPresenterVisualInteractionSource.PositionYSourceMode =
            ScrollPresenterVisualInteractionSource.ScaleSourceMode =
            InteractionSourceMode.EnabledWithInertia;

    }
    public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
    {

    }

    public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
    {

    }

    public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
    {

    }

    public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
    {

    }

    public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
    {

    }
    Vector3 Vec = new(1000, 1000, 1000);
    public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
    {
        InteractionTracker.MinPosition = args.Position - Vec;
        InteractionTracker.MaxPosition = args.Position + Vec;
        ValuesChangedEvent?.Invoke(args);
    }
    public event Action<InteractionTrackerValuesChangedArgs>? ValuesChangedEvent;
}