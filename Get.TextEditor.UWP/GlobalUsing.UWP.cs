global using Platform = Windows;
global using Windows.Devices;
global using Windows.Devices.Input;
global using Windows.UI;
global using Windows.UI.Core;
global using Windows.UI.Xaml;
global using Windows.UI.Xaml.Controls;
global using Windows.UI.Xaml.Controls.Primitives;
global using Windows.UI.ViewManagement;
global using Windows.Foundation;
global using Windows.System;
global using Windows.UI.Xaml.Input;
global using Windows.UI.Text.Core;
global using Windows.UI.Xaml.Hosting;
global using Windows.UI.Composition.Interactions;
global using Windows.UI.Xaml.Media;
global using Windows.UI.Xaml.Media.Imaging;
global using SkiaSharp.Views.UWP;
global using Microsoft.Toolkit.Uwp.UI;
global using PlatformCursor = Windows.UI.Core.CoreCursor;
global using PlatformCursorType = Windows.UI.Core.CoreCursorType;
global using SkiaSharp.Views.Platform;
using Get.XAMLTools;
namespace SkiaSharp.Views.Platform
{
    class PlatformSKSwapChainPanel : SKSwapChainPanel { }
}
namespace Get.TextEditor
{
    class PlatformLibrary
    {
        public static bool IsKeyDown(VirtualKey key)
            => (CoreWindow.GetForCurrentThread().GetKeyState(key) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }
}
namespace CommunityToolkit.Platform.UI
{
    [AttachedProperty(typeof(PlatformCursorType), "Cursor", GenerateLocalOnPropertyChangedMethod = true)]
    static partial class SharedFrameworkElementExtensions
    {
        static partial void OnCursorChanged(DependencyObject obj, PlatformCursorType oldValue, PlatformCursorType newValue)
        {
            if (obj is FrameworkElement ele)
                FrameworkElementExtensions.SetCursor(ele, newValue);
        }
    }
}