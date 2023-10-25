namespace Get.RichTextKit.Data;

using Get.RichTextKit.Editor;
using SkiaSharp;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public class RTFData
{
    private readonly Dictionary<SKColor, int> ColorTable = new();
    private readonly Dictionary<string, int> FontTable = new();
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