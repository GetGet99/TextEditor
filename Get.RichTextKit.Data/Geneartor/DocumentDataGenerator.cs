using Get.RichTextKit.Editor.Paragraphs.Panel;
using Get.RichTextKit.Editor;
using Get.RichTextKit.Utils;
using Get.RichTextKit.Editor.Paragraphs;
using MethodInjector;

namespace Get.RichTextKit.Data;


public static partial class DocumentDataGenerator
{
    public static DataInfo Generate(Document doc, TextRange selection)
    {
        DataInfo info = new();
        ParagraphProcessor.Call(doc.Paragraphs.UnsafeGetRootParagraph(), (info, (new() {
            Range = selection,
            RTFEndLineImplicit = false,
            HTMLEndLineImplicit = false
        })));
        return info;
    }
    
}
