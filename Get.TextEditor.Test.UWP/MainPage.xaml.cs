using Windows.UI.Xaml.Controls;

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
            var editor1 = new Get.TextEditor.RichTextEditor() { AllowFocusOnInteraction = true };
            RootGrid.Children.Add(editor1);
            //var editor2 = new Get.TextEditor.RichTextEditor(editor1.DocumentView.OwnerDocument) { AllowFocusOnInteraction = true };
            //RootGrid.Children.Add(editor2);
            //Grid.SetColumn(editor2, 1);
        }
    }
}
