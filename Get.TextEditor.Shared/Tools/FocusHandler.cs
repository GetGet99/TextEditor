using System.Collections.Generic;
using Windows.UI.Xaml.Controls.Primitives;

namespace Get.TextEditor.Tools;
public abstract class FocusHandler<T> where T : UIElement
{
    public bool ShouldKeepFocus(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
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
        bool ret = ShouldKeepFocusOverride(context, positionRelativeToWindow, newFocusElement);
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
    protected abstract bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement);
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
    IEnumerable<DependencyObject> GetParent(DependencyObject element)
    {
        while (element is not null)
        {
            yield return element;
            element = VisualTreeHelper.GetParent(element);
        }
    }
}
public class InteractingOnlyFocusHandler : InteractingOnlyFocusHandler<RichTextEditor> { }
public class InteractingOnlyFocusHandler<T> : FocusHandler<T> where T : UIElement
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
public class AlwyasInFocusHandler : InteractingOrInsideContextFocusHandler<RichTextEditor> { }
public class AlwyasInFocusHandler<T> : FocusHandler<T> where T : UIElement
{
    protected override bool ShouldKeepFocusOverride(T context, Point positionRelativeToWindow, DependencyObject newFocusElement)
        => true;
}
