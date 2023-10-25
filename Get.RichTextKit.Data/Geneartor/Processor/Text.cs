using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor;
using SkiaSharp;
using System;
using System.Net;
using System.Text;

namespace Get.RichTextKit.Data;

partial class Processor
{
    public static ProcessGeneratorInfo ProcessTextParagraph(TextParagraph textPara, (DataInfo Info, ProcessGeneratorInfo ProcInfo) param)
    {
        var (info, procInfo) = param;
        info.Rtf.Body.Append(@"{\pard");
        if (procInfo.RTFEndLineImplicit)
            info.HTML.Append("<span>");
        else
            info.HTML.Append("<p>");
        bool hasEndPara = false;
        var styleRuns = textPara.GetStyles(procInfo.Range.Start, procInfo.Range.End);
        foreach (var styleRun in styleRuns)
        {
            var text = textPara.GetText(styleRun.Start, styleRun.Length);
            info.Rtf.Body.Append('{');
            var style = styleRun.Style;
            // Set Style
            static string CSSRGBA(SKColor color)
                => $"rgba({color.Red}, {color.Green}, {color.Blue}, {color.Alpha / 255d})";
            StringBuilder HTMLCloseTag = new();
            {
                if (style.BackgroundColor is not null)
                {
                    info.Rtf.Body.Append(@$"\cb{info.Rtf.ColorId(style.BackgroundColor.Value)} ");
                    info.HTML.Append($"""<span style="background-color: {CSSRGBA(style.BackgroundColor.Value)};">""");
                    HTMLCloseTag.Append(@$"</span>");
                }
                info.Rtf.Body.Append(@$"\f{info.Rtf.FontId(style.FontFamily)} ");
                if (style.FontItalic)
                {
                    info.Rtf.Body.Append(@"\i ");
                    info.HTML.Append("<em>"); // alternatives: <i> // font-style: italic;
                    HTMLCloseTag.Append("</em>");
                }
                info.Rtf.Body.Append(@$"\fs{(uint)Math.Round(style.FontSize * 2)} ");
                if (style.FontVariant is FontVariant.SuperScript)
                {
                    info.Rtf.Body.Append(@"\super ");
                    info.HTML.Append("<sup>"); // vertical-align: super; font-size: smaller;
                    HTMLCloseTag.Append("</sup>");
                }
                if (style.FontVariant is FontVariant.SubScript)
                {
                    info.Rtf.Body.Append(@"\sub ");
                    info.HTML.Append("<sub>"); // vertical-align: sub; font-size: smaller;
                    HTMLCloseTag.Append("</sub>");
                }
                if (style.FontWeight >= TextRangeBase.FontWeightBold)
                {
                    info.Rtf.Body.Append(@"\b ");
                    info.HTML.Append("<strong>"); // alternatives: <b> // font-weight: bold;
                    HTMLCloseTag.Append("</strong>");
                }
                // style.FontWidth
                // style.HaloBlur
                // style.HaloColor
                // style.HaloWidth
                // style.LetterSpacing
                // style.LineHeight
                // style.ReplacementCharacter
                if (style.StrikeThrough is StrikeThroughStyle.Solid)
                {
                    info.Rtf.Body.Append(@"\strike ");
                    info.HTML.Append("<del>");
                    HTMLCloseTag.Append("</del>");
                }
                if (style.TextColor is not null)
                {
                    info.Rtf.Body.Append(@$"\cf{info.Rtf.ColorId(style.TextColor.Value)} ");
                    info.HTML.Append($"""<span style="color: {CSSRGBA(style.TextColor.Value)};">""");
                    HTMLCloseTag.Append(@$"</span>");
                }
                // style.TextDirection
                if (style.Underline is UnderlineStyle.Solid)
                {
                    info.Rtf.Body.Append(@"\ul ");
                    info.HTML.Append("<u>");
                    HTMLCloseTag.Append("</u>");
                }
            }
            // Add Text
            {
                ExternalHelper.AppendText(info, text, ref hasEndPara);
            }
            // End Group
            info.Rtf.Body.Append('}');
        }
        if (!procInfo.RTFEndLineImplicit && hasEndPara)
            info.Rtf.Body.Append("\\par}\n");
        else
            info.Rtf.Body.Append("}\n");

        if (procInfo.RTFEndLineImplicit)
            info.HTML.Append("</span>\n");
        else
            info.HTML.Append("</p>\n");
        return procInfo;
    }
}
public static class ExternalHelper {

    static readonly Encoding CodePage1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
    public static void AppendText(DataInfo info, Utils.Utf32Buffer text, ref bool hasEndPara) {
        foreach (var charCode in text)
        {
            if (charCode is Document.NewParagraphSeparator)
            {
                hasEndPara = true;
                if (DocumentDataGenerator.NormalizeTextParagraphSeparator)
                {
                    info.Text.Append('\n');
                } else
                {
                    info.Text.Append(Document.NewParagraphSeparator);
                }
                continue;
            } else
            {
                unsafe
                {
                    info.Text.Append(Encoding.UTF32.GetString((byte*)&charCode, sizeof(int)));
                }
            }
            if (charCode is '{' or '}' or '\\')
            {
                info.Rtf.Body.Append('\\');
                info.Rtf.Body.Append((char)charCode);
            }
            else if (charCode < 128)
                info.Rtf.Body.Append((char)charCode);
            else if (charCode is (char)160)
            {
                // nonbreaking space
                info.Rtf.Body.Append(@"\~");
                info.HTML.Append("&nbsp;");
            }
            else if (charCode is '\n')
            {
                info.Rtf.Body.Append(@"\line");
                info.HTML.Append("<br>");
            }
            else if (charCode is 0x0c)
            {
                info.Rtf.Body.Append(@"\page");
                //info.HTML.Append("<strong>");
                //HTMLCloseTag.Append("</strong>");
            }
            else if (charCode is (char)2011)
            {
                // nonbreaking hyphen
                info.Rtf.Body.Append(@"\_");
                info.HTML.Append("&#8209;");
            }
            else if (charCode < 256)
            {
                info.Rtf.Body.Append(@"\'");
                info.Rtf.Body.Append(Convert.ToString(CodePage1252.GetBytes(new char[] { (char)charCode })[0], 16));
            }
            else if (charCode < 65536)
            {
                info.Rtf.Body.Append(@"\uc1\u");
                info.Rtf.Body.Append((charCode > 32768 ? charCode - 65536 : charCode).ToString());
                info.Rtf.Body.Append('*');
            }
            else
            {
                info.Rtf.Body.Append(@"\gettexteditorunicode");
                info.Rtf.Body.Append(charCode.ToString());
            }
        }
        info.HTML.Append(WebUtility.HtmlEncode(text.ToString()));
    }
}