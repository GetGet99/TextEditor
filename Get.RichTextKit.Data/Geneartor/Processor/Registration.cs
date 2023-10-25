using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using MethodInjector;

namespace Get.RichTextKit.Data;

partial class DocumentDataGenerator
{
    public static bool NormalizeTextParagraphSeparator { get; set; } = true;
    public static ExternalMethodManager<Paragraph, (DataInfo info, ProcessGeneratorInfo procInfo), ProcessGeneratorInfo> ParagraphProcessor { get; } = new();
    static DocumentDataGenerator()
    {
        ParagraphProcessor.Register<TextParagraph>(Processor.ProcessTextParagraph);
        ParagraphProcessor.Register<VerticalParagraph>(Processor.ProcessVerticalParagraph);
        ParagraphProcessor.Register<TableParagraph>(Processor.ProcessTableParagraph);
    }
}