global using Platform = Microsoft;
global using Microsoft.UI;
global using Microsoft.UI.Input;
global using Microsoft.UI.Xaml;
global using Microsoft.UI.Xaml.Controls;
global using Microsoft.UI.Xaml.Controls.Primitives;
global using Windows.UI.ViewManagement;
global using Windows.Foundation;
global using Windows.System;
global using Microsoft.UI.Xaml.Input;
global using Windows.UI.Text.Core;
global using Microsoft.UI.Xaml.Hosting;
global using Microsoft.UI.Composition.Interactions;
global using Microsoft.UI.Xaml.Media;
global using Microsoft.UI.Xaml.Media.Imaging;
global using SkiaSharp.Views.Windows;
global using CommunityToolkit.WinUI.UI;
global using PlatformCursor = Microsoft.UI.Input.InputCursor;
global using PlatformCursorType = Microsoft.UI.Input.InputSystemCursorShape;
global using PointerDeviceType = Microsoft.UI.Input.PointerDeviceType;
global using SkiaSharp.Views.Platform;
using WinWrapper.Input;
using Get.XAMLTools;

namespace SkiaSharp.Views.Platform
{
    class PlatformSKSwapChainPanel : SKXamlCanvas { }
}
namespace Get.TextEditor
{
    class PlatformLibrary
    {
        public static bool IsKeyDown(Windows.System.VirtualKey key)
            => Keyboard.IsKeyDown((WinWrapper.Input.VirtualKey)key);
        public static void SetCursor(FrameworkElement element, PlatformCursorType value)
            => typeof(UIElement).GetProperty("ProtectedCursor",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        !.SetValue(element, InputSystemCursor.Create(value));
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