using System.Collections.Generic;

namespace Get.TextEditor.Tools;
public abstract class FocusHandler<T> where T : UIElement
{
#if WINDOWS_UWP
    public bool ShouldKeepFocus(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
#else
    public bool ShouldKeepFocus(T context, DependencyObject newFocusElement)
#endif
    {
        bool disallowTakingFocus = false;
        if (newFocusElement is Popup && !GetParent(context).Contains(newFocusElement))
            return false;
        foreach (var popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(context.XamlRoot))
        {
            if (GetParent(newFocusElement).Contains(popup.Child) && !GetParent(context).Contains(popup.Child))
            {
                // We should not take away the focus from opening flyout/popup/dialog,
                // providing that the new focus element is not in the same popup as the current element
                // The focus should be returned to the control once the flyout is closed
                // so we don't have to be worried.
                return false;
            }
        }
        foreach (var parent in GetParent(newFocusElement))
        {
            if (parent == context)
            {
                // We should do the normal logic if the elements inside the context is not consider the text field.
                // Obviously, we should not say that we can't take away
                // focus because we are the text field.
                goto OverriddenLogic;
            }
            if (TextEditorDetection.IsTextEditorAuto(parent))
            {
                // We should not take away the focus from a text field,
                // providing that the new focus element is not in the same popup as the current element
                disallowTakingFocus = true;
                goto OverriddenLogic;
            }
        }
    OverriddenLogic:
#if WINDOWS_UWP
        bool ret = ShouldKeepFocusOverride(context, positionRelativeToWindow, newFocusElement);
#else
        bool ret = ShouldKeepFocusOverride(context, newFocusElement);
#endif
        if (disallowTakingFocus && ret)
        {
            // So we do need the element to return the focus
            if (newFocusElement is UIElement ele)
            {
                void LostFocusEv(object _1, RoutedEventArgs _2)
                {
                    ele.LostFocus -= LostFocusEv;
                    _ = FocusManager.TryFocusAsync(context, FocusState.Programmatic);
                }
                ele.LostFocus += LostFocusEv;
            }
            return false;
        }
        return ret;
    }
#if WINDOWS_UWP
    protected abstract bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement);
#else
    protected abstract bool ShouldKeepFocusOverride(T context, DependencyObject newFocusElement);
#endif
#if WINDOWS_UWP
    protected static bool IsPointerOn(UIElement element, Point positionRelativeToWindow)
        => VisualTreeHelper.FindElementsInHostCoordinates(
            positionRelativeToWindow,
            Window.Current.Content
        ).Contains(element);
    protected static bool IsPointerDirectlyOn(UIElement element, Point positionRelativeToWindow)
        => VisualTreeHelper.FindElementsInHostCoordinates(
            positionRelativeToWindow,
            Window.Current.Content
        ).FirstOrDefault() == element;
#endif
    protected static IEnumerable<DependencyObject> GetParent(DependencyObject element)
    {
        while (element is not null)
        {
            yield return element;
            element = VisualTreeHelper.GetParent(element);
        }
    }
}
#if WINDOWS_UWP
public class PointerInteractingOnlyFocusHandler : InteractingContextOnlyFocusHandler<RichTextEditor> { }
public class PointerInteractingOnlyFocusHandler<T> : FocusHandler<T> where T : UIElement
{
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
        => IsPointerDirectlyOn(context, positionRelativeToWindow);
}
public class InteractingOrInsideFocusHandler : InteractingOrInsideFocusHandler<RichTextEditor> { }
public class InteractingOrInsideFocusHandler<T> : FocusHandler<T> where T : UIElement
{
    public virtual UIElement Element { get; set; }
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
        => IsPointerOn(Element, positionRelativeToWindow);
}
public class InteractingOrInsideContextFocusHandler : InteractingOrInsideContextFocusHandler<RichTextEditor> { }
public class InteractingOrInsideContextFocusHandler<T> : FocusHandler<T> where T : UIElement
{
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
        => IsPointerOn(context, positionRelativeToWindow);
}
#endif
public class InteractingContextOnlyFocusHandler : InteractingContextOnlyFocusHandler<RichTextEditor> { }
public class InteractingContextOnlyFocusHandler<T> : FocusHandler<T> where T : UIElement
{
#if WINDOWS_UWP
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
#else
    protected override bool ShouldKeepFocusOverride(T context, DependencyObject newFocusElement)
#endif
        => context == newFocusElement;
}
public class InteractingElementFocusHandler : InteractingElementFocusHandler<RichTextEditor> { }
public class InteractingElementFocusHandler<T> : FocusHandler<T> where T : UIElement
{
    public virtual UIElement Element { get; set; }
#if WINDOWS_UWP
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
#else
    protected override bool ShouldKeepFocusOverride(T context, DependencyObject newFocusElement)
#endif
        => GetParent(newFocusElement).Contains(Element);
}
public class InteractingContextFocusHandler : InteractingContextFocusHandler<RichTextEditor> { }
public class InteractingContextFocusHandler<T> : FocusHandler<T> where T : UIElement
{
#if WINDOWS_UWP
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
#else
    protected override bool ShouldKeepFocusOverride(T context, DependencyObject newFocusElement)
#endif
        => GetParent(newFocusElement).Contains(context);
}
public class AlwyasInFocusHandler : AlwyasInFocusHandler<RichTextEditor> { }
public class AlwyasInFocusHandler<T> : FocusHandler<T> where T : UIElement
{
#if WINDOWS_UWP
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
        => true;
#else
    protected override bool ShouldKeepFocusOverride(T context, DependencyObject newFocusElement)
        => true;
#endif
}
