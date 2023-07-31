using Get.EasyCSharp;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace TryRichText.UWP;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        Editor.UIConfigParagraphTemplateSelector = new UIParagraphSettingTemplate().GetDataTemplateSelector();
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
