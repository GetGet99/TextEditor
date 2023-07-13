using System;
using System.Collections.Generic;
using System.Linq;

namespace RtfParser;
interface IStyle
{
    List<(ReadOnlyMemory<char> Command, int? Param)> Commands { get; }
}
public record class Style(List<(ReadOnlyMemory<char> Command, int? Param)> Commands, Style? BaseStyle) : IStyle
{
    public void Apply(IRTFParserHandler handler, RTFGroup context)
    {
        BaseStyle?.Apply(handler, context);
        foreach (var (cmd, param) in Commands)
        {
            var cmdContext = new RTFCommandContext(cmd.Span, false, null);
            handler.ExecuteCommand(ref cmdContext, param, context);
        }
    }
}
public record class CharacterStyle(List<(ReadOnlyMemory<char> Command, int? Param)> Commands, bool Additive, CharacterStyle? BaseStyle) : IStyle
{
    public void Apply(IRTFParserHandler handler, RTFGroup context)
    {
        
    }
    private void Apply(IRTFParserHandler handler, RTFGroup context, bool additive)
    {
        if (BaseStyle is null && !(Additive || additive))
        {
            var cmdContext = new RTFCommandContext("plain".AsSpan(), false, null);
            handler.ExecuteCommand(ref cmdContext, null, context);
        }
        BaseStyle?.Apply(handler, context, additive || Additive);
        foreach (var (cmd, param) in Commands)
        {
            var cmdContext = new RTFCommandContext(cmd.Span, false, null);
            handler.ExecuteCommand(ref cmdContext, param, context);
        }
    }
}
public class StylesheetParser : IRTFParserHandler
{
    public int Count => CharacterStylesDict.Count;
    public CharacterStyle GetCharacterStyle(int index) => CharacterStylesDict[index];
    readonly Dictionary<int, CharacterStyle> CharacterStylesDict = new();
    public Style GetStyle(int index) => StylesDict[index];
    readonly Dictionary<int, Style> StylesDict = new();
    IStyle? Current;
    public void AddText(ReadOnlyMemory<int> text)
    {
        //var str = new StringBuilder(text.Length);
        //var span = text.Span;
        //for (int i = 0; i < span.Length; i++)
        //{
        //    if (span[i] is ';') break;
        //    str.Append((char)span[i]);
        //}
        //CharacterStylesDict[CurrentGroup] = str.ToString();
    }

    public void EnterGroup(RTFGroup group)
    {

    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        switch (commandContext.TextCommand)
        {
            case "s":
                {
                    if (param is null) goto default;
                    var style = new Style(new(), null);
                    Current = style;
                    StylesDict.Add(param.Value, style);
                    break;
                }
            case "cs":
                {
                    if (param is null) goto default;
                    // It is a group command, but we do not want to handle like it is one
                    commandContext.ShouldBeGroupCommand = false;
                    var style = new CharacterStyle(new(), false, null);
                    Current = style;
                    CharacterStylesDict.Add(param.Value, style);
                    break;
                }
            case "additive":
                {
                    if (Current is not CharacterStyle cs) goto default;
                    var key = CharacterStylesDict.FirstOrDefault(x => x.Value == cs).Key;
                    if (CharacterStylesDict.TryGetValue(key, out var style))
                    {
                        if (style == cs)
                        {
                            CharacterStylesDict[key] = cs with { Additive = true };
                        } else
                        {
                            goto default;
                        }
                    }
                    else goto default;
                    break;
                }
            case "sbasedon":
                {
                    if (param is null) goto default;

                    if (Current is CharacterStyle cs && CharacterStylesDict.TryGetValue(param.Value, out var cs2))
                    {

                        var key = CharacterStylesDict.FirstOrDefault(x => x.Value == cs).Key;
                        if (CharacterStylesDict.TryGetValue(key, out var style))
                        {
                            if (style == cs)
                            {
                                CharacterStylesDict[key] = cs with { BaseStyle = cs2 };
                            }
                            else
                            {
                                goto default;
                            }
                        }
                    }
                    else if (Current is Style s && StylesDict.TryGetValue(param.Value, out var s2))
                    {

                        var key = StylesDict.FirstOrDefault(x => x.Value == s).Key;
                        if (StylesDict.TryGetValue(key, out var style))
                        {
                            if (style == s)
                            {
                                StylesDict[key] = s with { BaseStyle = s2 };
                            }
                            else
                            {
                                goto default;
                            }
                        }
                    }
                }
                break;
            default:
                commandContext.ShouldBeGroupCommand = false;
                Current?.Commands.Add(new(commandContext.TextCommand.ToArray(), param));
                break;
        }
    }

    public void ExitGroup(RTFGroup group)
    {

    }
}
