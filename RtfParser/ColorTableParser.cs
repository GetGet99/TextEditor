using System;
using System.Collections.Generic;
using System.Drawing;

namespace RtfParser;

public class ColorTableParser : IRTFParserHandler
{
    public int Count => Colors.Count - 1;
    public Color? this[int index] => Colors[index];
    readonly List<Color?> Colors = new() { default };
    int CurrentGroup = 0;
    public void AddText(ReadOnlyMemory<int> text)
    {
        var span = text.Span;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] is ';')
            {
                Colors.Add(null);
            }
        }
    }

    public void EnterGroup(RTFGroup group)
    {
        
    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        var command = commandContext.TextCommand;
        switch (command)
        {
            case "red":
            case "green":
            case "blue":
            case "alpha": // Not supported in other place, only here
                if (param is null) break;
                var color = Colors[^1] ?? Color.Black;
                Colors[^1] = Color.FromArgb(
                    command is "alpha" ? param.Value : color.A,
                    command is "red" ? param.Value : color.R,
                    command is "green" ? param.Value : color.G,
                    command is "blue" ? param.Value : color.B
                );
                break;
        }
    }

    public void ExitGroup(RTFGroup group)
    {
        
    }
}
