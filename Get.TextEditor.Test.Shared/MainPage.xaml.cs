using Get.EasyCSharp;
using Get.TextEditor.Tools;

namespace TryRichText.Shared;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
#if WINDOWS_UWP
        BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
#endif
        Editor.UIConfigParagraphTemplateSelector = new UIParagraphSettingTemplate().GetDataTemplateSelector();
        Editor.FocusHandler = new InteractingElementFocusHandler() { Element = Grid1 };
    }
    [Event<RoutedEventHandler>]
    void Undo()
        => Editor.DocumentView.OwnerDocument.UndoManager.Undo(Editor.DocumentView.InvokeUpdateInfo);
    [Event<RoutedEventHandler>]
    void Redo()
        => Editor.DocumentView.OwnerDocument.UndoManager.Redo(Editor.DocumentView.InvokeUpdateInfo);
    [Event<PointerEventHandler>]
    void HandleMouseEvent(PointerRoutedEventArgs ev)
        => ev.Handled = true;
}
