using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RtfParser;

public class FontTableParser : IRTFParserHandler
{
    public int Count => FontDict.Count;
    public string this[int index] => FontDict[index];
    public bool TryGetValue(int index, [NotNullWhen(true)] out string? str)
        => FontDict.TryGetValue(index, out str);
    readonly Dictionary<int, string> FontDict = new();
    public event Action? Changed;
    int CurrentGroup = 0;
    public void AddText(ReadOnlyMemory<int> text)
    {
        var str = new StringBuilder(text.Length);
        var span = text.Span;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] is ';') continue;
            str.Append((char)span[i]);
        }
        FontDict[CurrentGroup] = str.ToString();
        Changed?.Invoke();
    }

    public void EnterGroup(RTFGroup group)
    {
        
    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        switch (commandContext.TextCommand)
        {
            case "f":
                if (param.HasValue)
                    CurrentGroup = param.Value;
                else
                    break;
                break;
            case "fmodern"
            or "froman"
            or "fswiss"
            or "fnil":
                break;
            default:
                if (commandContext.ShouldBeGroupCommand) commandContext.CommandGroupHandler = EmptyRTFParserHandler.Instance;
                break;
        }
    }

    public void ExitGroup(RTFGroup group)
    {
        
    }
}
