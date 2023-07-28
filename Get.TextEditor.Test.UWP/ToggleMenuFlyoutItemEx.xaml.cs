using Get.RichTextKit.Editor;
using Get.XAMLTools;
using Windows.UI.Xaml.Controls;
namespace Get.TextEditor.UWP.UI.Controls;
[DependencyProperty<StyleStatus>("StyleStatus", CheckForChanges = true, GenerateLocalOnPropertyChangedMethod = true)]
public sealed partial class ToggleMenuFlyoutItemEx : ToggleMenuFlyoutItem
{
    public ToggleMenuFlyoutItemEx()
    {
        InitializeComponent();
    }

    private void ToggleButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        StyleStatus = StyleStatus.Toggle();
    }
    partial void OnStyleStatusChanged(StyleStatus oldValue, StyleStatus newValue)
    {
        if (oldValue == newValue) return;
        switch (newValue)
        {
            case StyleStatus.On:
                IsChecked = true;
                break;
            case StyleStatus.Off:
                IsChecked = false;
                break;
            case StyleStatus.Undefined:
                IsChecked = false;
                break;
        }
    }
}