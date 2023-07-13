using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TryRichText.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            RootGrid.ColumnDefinitions.RemoveAt(1);
            var editor1 = new Get.TextEditor.RichTextEditor() { AllowFocusOnInteraction = true };
            RootGrid.Children.Add(editor1);
            //var editor2 = new Get.TextEditor.RichTextEditor(editor1.DocumentView.OwnerDocument) { AllowFocusOnInteraction = true };
            //RootGrid.Children.Add(editor2);
            //Grid.SetColumn(editor2, 1);
        }
    }
}
