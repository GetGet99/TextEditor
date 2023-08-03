#nullable enable

using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Styles;

namespace Get.TextEditor;

public class ImageParagraph : UIElementParagraph
{
    ImageDisplay? ImageDisplay;
    public ImageParagraph(IStyle style, IDocumentViewOwner owner, BitmapImage img) : base(style,
        new UIElementSimpleFactory<ImageDisplay>(x => new ImageDisplay() { ImageSource = img }), owner)
    {

    }
}
static partial class Extension
{
    public static T Assign<T>(this T valIn, out T value)
    {
        value = valIn;
        return valIn;
    }
}