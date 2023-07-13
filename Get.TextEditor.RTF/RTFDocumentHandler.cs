using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DocumentView;
using RtfParser;
using System.Drawing;

namespace Get.TextEditor.RTF;

public class RTFDocumentHandler : IRTFParserHandler
{
    int? DefaultFontIndex;
    public bool AllowFontColorChange { get; set; }
    public bool AllowFontSizeChange { get; set; }
    public bool AllowFontFamilyChange { get; set; }
    public bool AllowBold { get; set; }
    public bool AllowItalic { get; set; }
    public bool AllowUnderline { get; set; }
    public bool AllowSuperScript { get; set; }
    public bool AllowSubScript { get; set; }
    public bool AllowStrikethrough { get; set; }
    internal FontTableParser FontTable = new();
    internal ColorTableParser ColorTable = new();
    internal StylesheetParser StylesheetParser = new();
    DocumentView DocumentView;
    public RTFDocumentHandler(DocumentView document)
    {
        DocumentView = document;
    }
    public void AddText(ReadOnlyMemory<int> text)
    {
        DocumentView.Controller.Type(text.Span);
    }

    public void EnterGroup(RTFGroup group)
    {

    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        void ApplyStyle<T>(bool allow, Action<T> Setter, T value, T valueRevert)
        {

            if (!allow) return;
            Setter(value);
            if (context is not null)
                context.GroupEnded += () =>
                    Setter(valueRevert);
        }
        void ApplySimpleStyleOn(bool allow, Action<StyleStatus> Setter)
            => ApplyStyle(allow, Setter, StyleStatus.On, StyleStatus.Off);
        void ApplySimpleStyleOff(bool allow, Action<StyleStatus> Setter)
            => ApplyStyle(allow, Setter, StyleStatus.Off, StyleStatus.On);
        void ResetSimpleStyle(bool allow, Func<StyleStatus> Getter, Action<StyleStatus> Setter)
            => ApplyStyle(allow, Setter, StyleStatus.Off, Getter());
        void ApplyComplexStyle<T>(bool allow, Func<T> Getter, Action<T> Setter, T newValue)
            => ApplyStyle(allow, Setter, newValue, Getter());
        switch (commandContext.TextCommand)
        {
            case "fonttbl": // Font Table
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = FontTable;
                break;
            case "colortbl": // Color Table
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = ColorTable;
                break;
            case "stylesheet":
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = StylesheetParser;
                break;
            case "info":
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = EmptyRTFParserHandler.Instance;
                break;
            case "deff":
                if (param is null) goto default;
                DefaultFontIndex = param.Value;
                FontTable.Changed += delegate
                {
                    if (FontTable.TryGetValue(DefaultFontIndex.Value, out var font))
                        ApplyComplexStyle(
                            AllowFontFamilyChange,
                            () => DocumentView.Selection.FontFamily,
                            x => DocumentView.Selection.FontFamily = x,
                            font
                        );
                };
                break;
            case "i":
                if (param is 0)
                    ApplySimpleStyleOff(AllowItalic, x => DocumentView.Selection.Italic = x);
                else
                    ApplySimpleStyleOn(AllowItalic, x => DocumentView.Selection.Italic = x);
                break;
            case "b":
                if (param is 0)
                    ApplySimpleStyleOff(AllowBold, x => DocumentView.Selection.Bold = x);
                else
                    ApplySimpleStyleOn(AllowBold, x => DocumentView.Selection.Bold = x);
                break;
            case "ul":
                if (param is 0)
                    ApplySimpleStyleOff(AllowUnderline, x => DocumentView.Selection.Underline = x);
                else
                    ApplySimpleStyleOn(AllowUnderline, x => DocumentView.Selection.Underline = x);
                break;
            case "ulnone":
                ApplySimpleStyleOff(AllowUnderline, x => DocumentView.Selection.Underline = x);
                break;
            case "super":
                if (param is 0)
                    ApplySimpleStyleOff(AllowSuperScript, x => DocumentView.Selection.SuperScript = x);
                else
                    ApplySimpleStyleOn(AllowSuperScript, x => DocumentView.Selection.SuperScript = x);
                break;
            case "sub":
                if (param is 0)
                    ApplySimpleStyleOff(AllowSubScript, x => DocumentView.Selection.SubScript = x);
                else
                    ApplySimpleStyleOn(AllowSubScript, x => DocumentView.Selection.SubScript = x);
                break;
            case "f":
                if (param is null) goto default;
                ApplyComplexStyle(
                    AllowFontFamilyChange,
                    () => DocumentView.Selection.FontFamily,
                    x => DocumentView.Selection.FontFamily = x,
                    FontTable[param.Value]
                );
                break;
            case "fs":
                if (param is null) goto default;
                ApplyComplexStyle(
                    AllowFontSizeChange,
                    () => DocumentView.Selection.FontSize,
                    x => DocumentView.Selection.FontSize = x,
                    (float)param.Value / 2
                );
                break;
            case "cf":
                if (param is null) goto default;
                ApplyComplexStyle(
                    AllowFontColorChange,
                    delegate
                    {
                        var color = DocumentView.Selection.TextColor.Value;
                        return color is null ? default(Color?) : Color.FromArgb(color.Value.Alpha, color.Value.Red, color.Value.Green, color.Value.Blue);
                    },
                    x => DocumentView.Selection.TextColor = new(x is null ? null : new(x.Value.R, x.Value.G, x.Value.B, x.Value.A)),
                    ColorTable[param.Value]
                );
                break;
            case "cb":
                if (param is null) goto default;
                ApplyComplexStyle(
                    AllowFontColorChange,
                    delegate
                    {
                        var color = DocumentView.Selection.BackgroundColor!.Value;
                        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
                    },
                    x => DocumentView.Selection.BackgroundColor = new(x.R, x.G, x.B, x.A),
                    ColorTable[param.Value] ?? Color.Transparent
                );
                break;
            case "plain":
                ResetSimpleStyle(AllowItalic,
                    () => DocumentView.Selection.Italic,
                    x => DocumentView.Selection.Italic = x
                );
                ResetSimpleStyle(AllowBold,
                    () => DocumentView.Selection.Bold,
                    x => DocumentView.Selection.Bold = x
                );
                ResetSimpleStyle(AllowUnderline,
                    () => DocumentView.Selection.Underline,
                    x => DocumentView.Selection.Underline = x
                );
                ResetSimpleStyle(AllowSuperScript,
                    () => DocumentView.Selection.SuperScript,
                    x => DocumentView.Selection.SuperScript = x
                );
                ResetSimpleStyle(AllowSubScript,
                    () => DocumentView.Selection.SubScript,
                    x => DocumentView.Selection.SubScript = x
                );
                if (DefaultFontIndex is not null && FontTable.TryGetValue(DefaultFontIndex.Value, out var font))
                    ApplyComplexStyle(
                        AllowFontFamilyChange,
                        () => DocumentView.Selection.FontFamily,
                        x => DocumentView.Selection.FontFamily = x,
                        font
                    );
                ApplyComplexStyle(
                    AllowFontSizeChange,
                    () => DocumentView.Selection.FontSize,
                    x => DocumentView.Selection.FontSize = x,
                    12f
                );

                ApplyComplexStyle(
                    AllowFontColorChange,
                    delegate
                    {
                        var color = DocumentView.Selection.TextColor.Value;
                        return color is null ? default(Color?) : Color.FromArgb(color.Value.Alpha, color.Value.Red, color.Value.Green, color.Value.Blue);
                    },
                    x => DocumentView.Selection.TextColor = new(x is null ? null : new(x.Value.R, x.Value.G, x.Value.B, x.Value.A)),
                    null
                );
                break;
            case "par":
                DocumentView.Controller.Type(Document.NewParagraphSeparator.ToString());
                break;
            case "line":
                DocumentView.Controller.Type("\n");
                break;
            case "strike":

                if (param is 0)
                    ApplySimpleStyleOff(AllowStrikethrough, x => DocumentView.Selection.Strikethrough = x);
                else
                    ApplySimpleStyleOn(AllowStrikethrough, x => DocumentView.Selection.Strikethrough = x);
                break;
            // scaps
            default:
                if (commandContext.ShouldBeGroupCommand) commandContext.CommandGroupHandler = EmptyRTFParserHandler.Instance;
                break;
        }
    }

    public void ExitGroup(RTFGroup group)
    {

    }
}
public static class RTFProvider
{
    public static void PasteText(DocumentView documentView, string rtfString, RTFDocumentHandler handler)
    {
    }
}