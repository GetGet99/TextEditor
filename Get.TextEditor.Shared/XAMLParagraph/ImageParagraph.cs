//#nullable enable
//using Windows.UI.Xaml.Media.Imaging;

//namespace Get.TextEditor;

//class ImageParagraph : FrameworkElementParagraph
//{
//    ImageDisplay? ImageDisplay;
//    public ImageParagraph(RichTextEditorCanvas owner, BitmapImage img) : base(owner, new ImageDisplay().Assign(out var imD))
//    {
//        ImageDisplay = imD;
//        ImageDisplay.ImageSource = img;
//    }
//}
//static partial class Extension
//{
//    public static T Assign<T>(this T valIn, out T value)
//    {
//        value = valIn;
//        return valIn;
//    }
//}