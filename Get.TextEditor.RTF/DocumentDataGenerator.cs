using Get.RichTextKit;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using Get.RichTextKit.Utils;
using Get.RichTextKit.Editor.Paragraphs;
using System.Text;
using Get.RichTextKit.Editor;
using SkiaSharp;

namespace Get.TextEditor.Data;
public interface IDataDocumentGeneratorHandler
{
    void EnterPanel(IParagraphPanel paragraphPanel, DataInfo info, ref GroupGeneratorInfo groupInfo);
    void ExitPanel(IParagraphPanel paragraphPanel, DataInfo info, ref GroupGeneratorInfo groupInfo);
    void ProcessParagraph(Paragraph paragraph, DataInfo info, ref ProcessGeneratorInfo procInfo);
}
public class DataInfo
{
    public StringBuilder Text { get; set; } = new();
    public RTFData Rtf { get; set; } = new();
    public StringBuilder HTML { get; set; } = new();
    public Dictionary<string, string> OtherFormat { get; } = new();
}
public class RTFData
{
    private Dictionary<SKColor, int> ColorTable = new();
    private Dictionary<string, int> FontTable = new();
    int nextFontId = 0;
    int nextColorId = 1;
    public int FontId(string fontName)
    {
        if (FontTable.TryGetValue(fontName, out var fontId)) return fontId;
        fontId = nextFontId++;
        FontTable.Add(fontName, fontId);
        return fontId;
    }
    public int ColorId(SKColor color)
    {
        if (ColorTable.TryGetValue(color, out var colorId)) return colorId;
        colorId = nextColorId++;
        ColorTable.Add(color, colorId);
        return colorId;
    }
    public StringBuilder Body { get; } = new();
    public override string ToString()
    {
        return $$"""
            {\rtf1\ansi\deff0 {\fonttbl
            {{string.Join("\n",
                from x in FontTable
                    select @$"{{\f{x.Value} {x.Key};}}"
            )}}}{\colortbl
            ;
            {{string.Join("\n",
                from x in ColorTable
                select $@"\red{x.Key.Red}\green{x.Key.Green}\blue{x.Key.Blue};"
            )}}}

            {{Body}}

            }
            """;
    }
}
public struct GroupGeneratorInfo
{
    public TextRange Range { get; set; }
    public bool ShouldRunInsidePanel { get; set; }
    public IDataDocumentGeneratorHandler Handler { get; set; }
}
public struct ProcessGeneratorInfo
{
    public TextRange Range { get; set; }
}
public static class DocumentDataGenerator
{
    public static DataInfo Generate(Document doc, TextRange selection, IDataDocumentGeneratorHandler handler)
    {
        DataInfo info = new();
        Generate(doc.Paragraphs, selection, handler, ref info);
        return info;
    }
    static void Generate(IParagraphCollection parent, TextRange selection, IDataDocumentGeneratorHandler handler, ref DataInfo info)
    {
        foreach (var run in parent.Paragraphs.AsReadOnly().GetInterectingRuns(selection.Minimum, selection.Length))
        {
            var para = parent.Paragraphs[run.Index];
            if (para is IParagraphPanel paragraphPanel)
            {
                var groupInfo = new GroupGeneratorInfo()
                {
                    ShouldRunInsidePanel = false,
                    Range = new(run.Offset, run.Length),
                    Handler = handler
                };
                handler.EnterPanel(paragraphPanel, info, ref groupInfo);
                if (groupInfo.ShouldRunInsidePanel)
                    Generate(paragraphPanel, groupInfo.Range, groupInfo.Handler, ref info);
                handler.ExitPanel(paragraphPanel, info, ref groupInfo);
                continue;
            } else
            {
                var procInfo = new ProcessGeneratorInfo()
                {
                    Range = new(run.Offset, run.Length)
                };
                handler.ProcessParagraph(para, info, ref procInfo);
            }
        }
    }
}