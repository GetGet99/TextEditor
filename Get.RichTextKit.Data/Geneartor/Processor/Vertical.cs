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
    public static ProcessGeneratorInfo ProcessVerticalParagraph(VerticalParagraph verticalPara, (DataInfo Info, ProcessGeneratorInfo ProcInfo) param)
    {
        var (info, procInfo) = param;
        var startparaIdx = verticalPara.LocalChildrenFromCodePointIndexAsIndex(procInfo.Range.StartCaretPosition, out var idxStart);
        var endparaIdx = verticalPara.LocalChildrenFromCodePointIndexAsIndex(procInfo.Range.EndCaretPosition, out var idxEnd);
        if (startparaIdx == endparaIdx)
        {
            DocumentDataGenerator.ParagraphProcessor.Call(verticalPara.Children[startparaIdx],
                (info, procInfo with {
                    Range = new(idxStart, idxEnd, procInfo.Range.AltPosition)
                })
            );
        } else
        {
            var startPara = verticalPara.Children[startparaIdx];
            DocumentDataGenerator.ParagraphProcessor.Call(startPara,
                (info, new() {
                    Range = new(idxStart, startPara.TrueEndCaretPosition.CodePointIndex, startPara.TrueEndCaretPosition.AltPosition),
                    RTFEndLineImplicit = false,
                    HTMLEndLineImplicit = false
                })
            );
            for (int curparaIdx = startparaIdx + 1; curparaIdx < endparaIdx; curparaIdx++)
            {
                var curPara = verticalPara.Children[curparaIdx];
                DocumentDataGenerator.ParagraphProcessor.Call(curPara,
                    (info, new()
                    {
                        Range = new(startPara.UserStartCaretPosition.CodePointIndex, startPara.TrueEndCaretPosition.CodePointIndex, startPara.TrueEndCaretPosition.AltPosition),
                        RTFEndLineImplicit = false,
                        HTMLEndLineImplicit = false
                    })
                );
            }
            var endPara = verticalPara.Children[endparaIdx];
            DocumentDataGenerator.ParagraphProcessor.Call(endPara,
                (info, new()
                {
                    Range = new(endPara.UserStartCaretPosition.CodePointIndex, idxEnd, procInfo.Range.AltPosition),
                    RTFEndLineImplicit = procInfo.RTFEndLineImplicit,
                    HTMLEndLineImplicit = false
                })
            );
        }
        return procInfo;
    }
}
