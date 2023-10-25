using Get.RichTextKit.Editor.Paragraphs.Panel;
using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor;
using SkiaSharp;
using System;
using System.Net;
using System.Text;
using System.Linq;

namespace Get.RichTextKit.Data;

partial class Processor
{
    public static ProcessGeneratorInfo ProcessTableParagraph(TableParagraph tablePara, (DataInfo Info, ProcessGeneratorInfo ProcInfo) param)
    {
        var (info, procInfo) = param;
        var header = $$"""
            \trowd \trgaph180
                {{string.Join("",
                from x in tablePara.GetCurrentLayoutInfo().ColumnsWidth
                select $@"\cellx{x}" /* TODO: Use the actual value for our customized table properties */
            )}}
            """;
        info.HTML.Append("<table>");
        foreach (var currRow in tablePara.Rows)
        {
            info.HTML.Append("<tr>"); // TODO: Height property
            info.Rtf.Body.Append('{');
            info.Rtf.Body.Append(header);
            foreach (var cell in currRow)
            {
                info.HTML.Append("<th>"); // TODO: Width property
                info.Rtf.Body.Append('{');
                var childprocInfo = new ProcessGeneratorInfo() {
                    Range = new(cell.UserStartCaretPosition.CodePointIndex,
                    cell.UserEndCaretPosition.CodePointIndex,
                    cell.UserEndCaretPosition.AltPosition
                ), RTFEndLineImplicit = true, HTMLEndLineImplicit = true };
                childprocInfo = DocumentDataGenerator.ParagraphProcessor.Call(cell, (info, childprocInfo));
                info.Rtf.Body.Append('}');
                info.Rtf.Body.Append(@"\cell");
                info.HTML.Append("</th>");
            }
            info.Rtf.Body.Append('}');
            info.HTML.Append("</tr>");
        }
        info.HTML.Append("</table>");
        return procInfo;
    }
}
