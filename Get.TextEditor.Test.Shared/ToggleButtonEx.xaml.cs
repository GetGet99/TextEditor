using Get.RichTextKit.Editor;
using Get.XAMLTools;
using System;
namespace Get.TextEditor.Shared.UI.Controls;
[DependencyProperty<StyleStatus>("StyleStatus", CheckForChanges = true, GenerateLocalOnPropertyChangedMethod = true)]
public sealed partial class ToggleButtonEx : ToggleButton
{
    public ToggleButtonEx()
    {
        InitializeComponent();
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
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

static partial class Extension
{
    public static StyleStatus Toggle(this StyleStatus status)
        => status switch
        {
            StyleStatus.On => StyleStatus.Off,
            StyleStatus.Off or StyleStatus.Undefined => StyleStatus.On,
            _ => throw new ArgumentOutOfRangeException()
        };
}