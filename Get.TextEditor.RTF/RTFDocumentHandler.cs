﻿using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DocumentView;
using RtfParser;
using System.Drawing;

namespace Get.TextEditor.RTF;
public class RTFDocumentHandler : IRTFParserHandler
{
    int? DefaultFontIndex;
    AllowedFormatting ImportFormatting;
    internal FontTableParser FontTable = new();
    internal ColorTableParser ColorTable = new();
    internal StylesheetParser StylesheetParser = new();
    DocumentView DocumentView;
    public RTFDocumentHandler(DocumentView document, AllowedFormatting importFormatting)
    {
        DocumentView = document;
        ImportFormatting = importFormatting;
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
                            ImportFormatting.AllowFontFamilyChange,
                            () => DocumentView.Selection.FontFamily,
                            x => DocumentView.Selection.FontFamily = x,
                            font
                        );
                };
                break;
            case "i":
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Italic, x => DocumentView.Selection.Italic = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Italic, x => DocumentView.Selection.Italic = x);
                break;
            case "b":
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Bold, x => DocumentView.Selection.Bold = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Bold, x => DocumentView.Selection.Bold = x);
                break;
            case "ul":
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Underline, x => DocumentView.Selection.Underline = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Underline, x => DocumentView.Selection.Underline = x);
                break;
            case "ulnone":
                ApplySimpleStyleOff(ImportFormatting.Underline, x => DocumentView.Selection.Underline = x);
                break;
            case "super":
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.SuperScript, x => DocumentView.Selection.SuperScript = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.SuperScript, x => DocumentView.Selection.SuperScript = x);
                break;
            case "sub":
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.SubScript, x => DocumentView.Selection.SubScript = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.SubScript, x => DocumentView.Selection.SubScript = x);
                break;
            case "f":
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.AllowFontFamilyChange,
                    () => DocumentView.Selection.FontFamily,
                    x => DocumentView.Selection.FontFamily = x,
                    FontTable[param.Value]
                );
                break;
            case "fs":
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.AllowFontSizeChange,
                    () => DocumentView.Selection.FontSize,
                    x => DocumentView.Selection.FontSize = x,
                    (float)param.Value / 2
                );
                break;
            case "cf":
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.AllowFontColorChange,
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
                    ImportFormatting.AllowFontColorChange,
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
                ResetSimpleStyle(ImportFormatting.Italic,
                    () => DocumentView.Selection.Italic,
                    x => DocumentView.Selection.Italic = x
                );
                ResetSimpleStyle(ImportFormatting.Bold,
                    () => DocumentView.Selection.Bold,
                    x => DocumentView.Selection.Bold = x
                );
                ResetSimpleStyle(ImportFormatting.Underline,
                    () => DocumentView.Selection.Underline,
                    x => DocumentView.Selection.Underline = x
                );
                ResetSimpleStyle(ImportFormatting.SuperScript,
                    () => DocumentView.Selection.SuperScript,
                    x => DocumentView.Selection.SuperScript = x
                );
                ResetSimpleStyle(ImportFormatting.SubScript,
                    () => DocumentView.Selection.SubScript,
                    x => DocumentView.Selection.SubScript = x
                );
                if (DefaultFontIndex is not null && FontTable.TryGetValue(DefaultFontIndex.Value, out var font))
                    ApplyComplexStyle(
                        ImportFormatting.AllowFontFamilyChange,
                        () => DocumentView.Selection.FontFamily,
                        x => DocumentView.Selection.FontFamily = x,
                        font
                    );
                ApplyComplexStyle(
                    ImportFormatting.AllowFontSizeChange,
                    () => DocumentView.Selection.FontSize,
                    x => DocumentView.Selection.FontSize = x,
                    12f
                );

                ApplyComplexStyle(
                    ImportFormatting.AllowFontColorChange,
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
                    ApplySimpleStyleOff(ImportFormatting.Strikethrough, x => DocumentView.Selection.Strikethrough = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Strikethrough, x => DocumentView.Selection.Strikethrough = x);
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