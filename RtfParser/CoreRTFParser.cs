using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using static RtfParser.CoreRTFParser;

namespace RtfParser;

public class RTFGroup
{
    internal bool InternalShouldExecuteEntireGroup { get; set; } = false;
    internal bool InternalIsEmpty { get; private set; } = true;
    internal IRTFParserHandler? Handler { get; set; }
    Action? _GroupEnded;
    public event Action? GroupEnded
    {
        add
        {
            InternalIsEmpty = false;
            _GroupEnded += value;
        }
        remove
        {
            _GroupEnded -= value;
        }
    }
    internal void EndGroup()
    {
        _GroupEnded?.Invoke();
    }
}
public class EmptyRTFParserHandler : IRTFParserHandler
{
    public static EmptyRTFParserHandler Instance { get; } = new();
    private EmptyRTFParserHandler() { }

    public void AddText(ReadOnlyMemory<int> text)
    {

    }

    public void EnterGroup(RTFGroup group)
    {

    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        if (commandContext.ShouldBeGroupCommand)
            commandContext.CommandGroupHandler = this;
    }

    public void ExitGroup(RTFGroup group)
    {

    }
}
public interface IRTFParserHandler
{
    void AddText(ReadOnlyMemory<int> text);
    void EnterGroup(RTFGroup group);
    void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context);
    void ExitGroup(RTFGroup group);
}
public ref struct RTFCommandContext
{
    public RTFCommandContext(ReadOnlySpan<char> textCommand, bool shouldBeGroupCommand, IRTFParserHandler? commandGroupHandler)
    {
        TextCommand = textCommand;
        ShouldBeGroupCommand = shouldBeGroupCommand;
        CommandGroupHandler = commandGroupHandler;
    }
    public ReadOnlySpan<char> TextCommand { get; set; }
    public bool ShouldBeGroupCommand { get; set; }
    public IRTFParserHandler? CommandGroupHandler { get; set; }
}
public class CoreRTFParser
{
    public ref struct ReadOnlySpanIndexEnumerator<T>
    {
        /// <summary>The span being enumerated.</summary>
        public readonly ReadOnlySpan<T> Span;
        /// <summary>The next index to yield.</summary>
        private int _index;

        /// <summary>Initialize the enumerator.</summary>
        /// <param name="span">The span to enumerate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpanIndexEnumerator(ReadOnlySpan<T> span)
        {
            Span = span;
            _index = -1;
        }

        /// <summary>Advances the enumerator to the next element of the span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext(int amount = 1)
        {
            return MoveTo(_index + amount);
        }

        /// <summary>Advances the enumerator to the next element of the span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveTo(int index)
        {
            if (index < Span.Length)
            {
                _index = index;
                return true;
            }

            return false;
        }

        /// <summary>Gets the current position of the enumerator.</summary>
        public int CurrentPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _index;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MoveTo(value);
        }

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        public ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Span[_index];
        }

        public readonly bool IsValid => _index < Span.Length;
    }
    static readonly Encoding CodePage1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
    public static void Parse(ReadOnlySpan<char> charss, IRTFParserHandler inHandler)
    {
        var chars = new ReadOnlySpanIndexEnumerator<char>(charss);
        Stack<RTFGroup> Groups = new();
        bool uc1 = false;
        Groups.Push(new());
        int? i = null;
        IRTFParserHandler Handler()
        {
            foreach (var group in Groups)
            {
                if (group.Handler is not null)
                    return group.Handler;
            }
            return inHandler;
        }

        if (!chars.MoveNext()) return;

        void Cleanup(ref ReadOnlySpanIndexEnumerator<char> chars)
        {
            if (i is null) return; // Nothing to do
            if (i.Value > 0)
            {
                var span = chars.Span;
                var curPos = chars.CurrentPosition;
                int[] UseChars = new int[chars.CurrentPosition - i.Value];
                for (int j = i.Value; j < curPos; j++)
                {
                    UseChars[j - i.Value] = span[j];
                }
                Handler().AddText(UseChars);
                i = null;
            }
        }
        while (chars.IsValid)
        {
            switch (chars.Current)
            {
                case '\\':
                    Cleanup(ref chars);
                    if (!chars.MoveNext()) goto EndLoop;
                    switch (chars.Current)
                    {
                        case '\'':
                            // Escape Character
                            // \XX where XX is hex code
                            if (!chars.MoveNext()) goto EndLoop;
                            if (ReadNChars(ref chars, 2, out var code))
                            {
                                byte b = byte.Parse(code.ToString(), NumberStyles.HexNumber);
                                Handler().AddText(new int[] { CodePage1252.GetChars(new byte[] { b })[0] });
                            }
                            break;
                        case '{' or '}' or '\\':
                            // Escaped {, }, \
                            Handler().AddText(new int[] { chars.Current });
                            if (!chars.MoveNext()) goto EndLoop;
                            break;
                        case '~':
                            // Nonbreaking Space
                            Handler().AddText(new int[] { (char)160 });
                            if (!chars.MoveNext()) goto EndLoop;
                            break;
                        case '_':
                            // Nonbreaking Hyphen
                            Handler().AddText(new int[] { (char)2011 });
                            if (!chars.MoveNext()) goto EndLoop;
                            break;
                        case '-':
                            // Hyphenation Point
                            if (!chars.MoveNext()) goto EndLoop;
                            break;
                        case '*':
                            {
                                var group = Groups.Peek();
                                if (group.InternalIsEmpty)
                                    group.InternalShouldExecuteEntireGroup = true;
                                if (!chars.MoveNext()) goto EndLoop;
                                break;
                            }
                        default:
                            if (GetContinuousAToZWord(ref chars, out var command))
                            {
                                int? param = null;
                                if (chars.IsValid && GetContinuousNumber(ref chars, out var _p)) { param = _p; }

                                if (command is "uc" && param is 1)
                                {
                                    uc1 = true;
                                    goto EndCommand;
                                }
                                else if (uc1 && command is not "u")
                                {
                                    uc1 = false;
                                    goto EndCommand;
                                }
                                var group = Groups.Peek();

                                var cmd = new RTFCommandContext()
                                {
                                    TextCommand = command,
                                    ShouldBeGroupCommand = group.InternalShouldExecuteEntireGroup,
                                    CommandGroupHandler = null
                                };
                                group.InternalShouldExecuteEntireGroup = false;
                                var isEmpty = group.InternalIsEmpty;
                                if (uc1 && param.HasValue && command is "u")
                                {
                                    Handler().AddText(new int[] { (char)param.Value });
                                    uc1 = false;
                                    if (chars.IsValid && chars.Current is '*')
                                        if (!chars.MoveNext()) goto EndLoop;
                                } else
                                    Handler().ExecuteCommand(ref cmd, param, group);
                                if (cmd.ShouldBeGroupCommand)
                                {
                                    if (!isEmpty) throw new InvalidOperationException();
                                    if (cmd.CommandGroupHandler is null) throw new NullReferenceException();
                                    group.Handler = cmd.CommandGroupHandler;
                                    group.Handler.ExecuteCommand(ref cmd, param, group);
                                }
                            }
                        EndCommand:
                            // ignore the space after command
                            if (chars.IsValid && chars.Current is ' ')
                                if (!chars.MoveNext()) goto EndLoop;
                            break;
                    }
                    break;
                case '{':
                    Cleanup(ref chars);
                    if (!chars.MoveNext()) goto EndLoop;
                    Groups.Push(new());
                    break;
                case '}':
                    Cleanup(ref chars);
                    if (!chars.MoveNext()) goto EndLoop;
                    Groups.Pop().EndGroup();
                    break;
                case '\r' or '\n':
                    // Ignore new line
                    Cleanup(ref chars);
                    if (!chars.MoveNext()) goto EndLoop;
                    break;
                default:
                    i ??= chars.CurrentPosition;
                    if (!chars.MoveNext()) goto EndLoop;
                    break;
            }
        }
    EndLoop:
        ;
    }
    static bool GetContinuousAToZWord(ref ReadOnlySpanIndexEnumerator<char> charsMem, out ReadOnlySpan<char> continuousWord)
    {
        var chars = charsMem.Span;
        int i;
        for (i = charsMem.CurrentPosition; i < chars.Length; i++)
        {
            if (!IsAToZ(chars[i])) break;
        }
        if (i == charsMem.CurrentPosition)
        {
            continuousWord = null;
            return false;
        }
        continuousWord = chars[charsMem.CurrentPosition..i];
        charsMem.MoveTo(i);
        return true;
    }
    static bool ReadNChars(ref ReadOnlySpanIndexEnumerator<char> charsEnumerator, int n, out ReadOnlySpan<char> chars)
    {
        if (PeakNChars(ref charsEnumerator, n, out chars))
        {
            charsEnumerator.MoveNext(n);
            return true;
        }
        else return false;
    }
    static bool PeakNChars(ref ReadOnlySpanIndexEnumerator<char> charsMem, int n, out ReadOnlySpan<char> chars)
    {
        var curPos = charsMem.CurrentPosition;
        if (charsMem.Span.Length - curPos >= n)
        {
            chars = charsMem.Span.Slice(curPos, n);
            return true;
        }
        chars = null;
        return false;
    }
    static bool GetContinuousNumber(ref ReadOnlySpanIndexEnumerator<char> enumerator, out int number)
    {
        var chars = enumerator.Span;
        int i = enumerator.CurrentPosition;
        int sign = 1;
        number = 0;
        bool hasNumber = false;
        if (enumerator.Current is '-')
        {
            sign = -1;
            i++;
        }
        for (; i < enumerator.Span.Length; i++)
        {
            if (!char.IsDigit(chars[i])) break;
            else
            {
                hasNumber = true;
                number = number * 10 + (chars[i] - '0');
            }
        }
        if (hasNumber)
            enumerator.MoveTo(i);
        number *= sign;
        return hasNumber;
    }
    static bool IsAToZ(char c) => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');
    static bool IsAToZLowercase(char c) => c is (>= 'a' and <= 'z');
    static bool IsValidHex(char c, out byte value)
    {
        switch (c)
        {
            case >= '0' and <= '9':
                value = (byte)(c - '0');
                return true;
            case >= 'a' and <= 'f':
                value = (byte)(c - 'a' + 10);
                return true;
            case >= 'A' and <= 'F':
                value = (byte)(c - 'A' + 10);
                return true;
            default:
                value = default;
                return false;
        }
    }
}