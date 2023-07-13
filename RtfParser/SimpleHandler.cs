using System;
using System.Runtime.CompilerServices;

namespace RtfParser;
public class SimpleHandler : IRTFParserHandler
{
    internal FontTableParser FontTable = new();
    internal ColorTableParser ColorTable = new();
    public void AddText(ReadOnlyMemory<int> text)
    {
        
    }

    public void EnterGroup(RTFGroup group)
    {
        
    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        switch (commandContext.TextCommand)
        {
            case "fonttbl": // Font Table
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = FontTable;
                break;
            case "colortbl": // Font Table
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = ColorTable;
                break;
        }
    }

    public void ExitGroup(RTFGroup group)
    {
        
    }
}
