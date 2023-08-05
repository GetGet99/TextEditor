using ColorCode;
using ColorCode.Common;
using ColorCode.Parsing;
using ColorCode.Styling;
using SkiaSharp;
using static Get.RichTextKit.FontFallback;
using System;
using Get.RichTextKit.Styles;
using System.Drawing;
using System.Threading.Tasks;

namespace Get.RichTextKit.Editor.Paragraphs;

partial class CodeParagraph
{
    enum Theme
    {
        Dark,
        Light
    }
    //class Colorizer : CodeColorizerBase
    //{
    //    /// <summary>
    //    /// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
    //    /// </summary>
    //    /// <param name="Theme">The Theme to use, determines whether to use Default Light or Default Dark.</param>
    //    public Colorizer(Theme Theme, ILanguageParser languageParser = null) : this(Theme == CodeParagraph.Theme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight, languageParser)
    //    {
    //    }

    //    /// <summary>
    //    /// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
    //    /// </summary>
    //    /// <param name="Style">The Custom styles to Apply to the formatted Code.</param>
    //    /// <param name="languageParser">The language parser that the <see cref="RichTextBlockFormatter"/> instance will use for its lifetime.</param>
    //    public Colorizer(StyleDictionary Style = null, ILanguageParser languageParser = null) : base(Style, languageParser)
    //    {
    //    }
    //    public RichString FormatText(string sourceCode, ILanguage Language, IStyle DefaultStyle)
    //    {
    //        RichString = new() { DefaultStyle = DefaultStyle };
    //        offset = 0;
    //        languageParser.Parse(sourceCode, Language, Write);
    //        return RichString;
    //    }
    //    RichString RichString;
    //    int offset = 0;
    //    protected override void Write(string parsedSourceCode, IList<Scope> scopes)
    //    {
    //        //foreach (var a in scopes)
    //        //{
    //        //    if (Styles.Contains(a.Name))
    //        //        CodeParagraph.TextBlock.ApplyStyle(
    //        //            offset, a.Length,
    //        //            new CopyStyle(defaultStyle)
    //        //            {
    //        //                TextColor = GetColor(Styles[a.Name].Foreground)
    //        //            }
    //        //        );
    //        //}
    //        //offset += parsedSourceCode.Length;

    //        var styleInsertions = new List<TextInsertion>();

    //        foreach (Scope scope in scopes)
    //            GetStyleInsertionsForCapturedStyle(scope, styleInsertions);

    //        styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

    //        int offset = 0;

    //        Scope PreviousScope = null;

    //        foreach (var styleinsertion in styleInsertions)
    //        {
    //            var text = parsedSourceCode.Substring(offset, styleinsertion.Index - offset);
    //            CreateSpan(text, PreviousScope);
    //            if (!string.IsNullOrWhiteSpace(styleinsertion.Text))
    //            {
    //                CreateSpan(text, PreviousScope);
    //            }
    //            offset = styleinsertion.Index;

    //            PreviousScope = styleinsertion.Scope;
    //        }

    //        var remaining = parsedSourceCode.Substring(offset);
    //        // Ensures that those loose carriages don't run away!
    //        if (remaining != "\r")
    //        {
    //            CreateSpan(remaining, null);
    //        }
    //    }
    //    private void CreateSpan(string Text, Scope scope)
    //    {
    //        if (scope is not null && Styles.Contains(scope.Name))
    //            RichString.TextColor(GetColor(Styles[scope.Name].Foreground));
    //        else
    //            RichString.TextColor(SKColors.White);
    //        RichString.Add(Text);
    //    }

    //    private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
    //    {
    //        styleInsertions.Add(new TextInsertion
    //        {
    //            Index = scope.Index,
    //            Scope = scope
    //        });

    //        foreach (Scope childScope in scope.Children)
    //            GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

    //        styleInsertions.Add(new TextInsertion
    //        {
    //            Index = scope.Index + scope.Length
    //        });
    //    }
    //}

    class Colorizer : CodeColorizerBase
    {
        /// <summary>
        /// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
        /// </summary>
        /// <param name="Theme">The Theme to use, determines whether to use Default Light or Default Dark.</param>
        public Colorizer(Theme Theme, ILanguageParser languageParser = null) : this(Theme == CodeParagraph.Theme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight, languageParser)
        {
        }

        /// <summary>
        /// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
        /// </summary>
        /// <param name="Style">The Custom styles to Apply to the formatted Code.</param>
        /// <param name="languageParser">The language parser that the <see cref="RichTextBlockFormatter"/> instance will use for its lifetime.</param>
        public Colorizer(StyleDictionary Style = null, ILanguageParser languageParser = null) : base(Style, languageParser)
        {
        }
        IStyle defaultStyle;
        public async Task<bool> FormatTextAsync(string sourceCode, ILanguage Language, IStyle DefaultStyle, TextBlock toModify)
        {
            if (IsRunning)
            {
                await CancelAsync();
            }
            defaultStyle = DefaultStyle;
            globalOffset = 0;
            ToModify = toModify;
            return await Task.Run(async () =>
            {
                try
                {
                    IsRunning = true;
                    bool thisTaskShouldCancel = false;
                    int offset = 0;
                    
                    var task = Task.Run(() => languageParser.Parse(sourceCode, Language, (x, y) =>
                    {
                        if (thisTaskShouldCancel) throw new OperationCanceledException();
                        Write(x, y, ref offset, ref thisTaskShouldCancel);
                    }));
                    while (task.Status is TaskStatus.Running)
                    {
                        if (shouldCancel)
                        {
                            thisTaskShouldCancel = true;
                            await Task.Delay(200);
                            // Just let the task do its thing
                            shouldCancel = false;
                            cancelNotify.TrySetResult(true);
                            return false;
                        }
                        await Task.Delay(100);
                    }
                    if (task.IsCanceled) throw new OperationCanceledException();
                    IsRunning = false;
                    return true;
                }
                catch (OperationCanceledException)
                {
                    IsRunning = false;
                    shouldCancel = false;
                    cancelNotify.TrySetResult(true);
                    return false;
                }
                finally
                {
                    IsRunning = false;
                }
            });
        }
        TextBlock ToModify;
        int globalOffset = 0;
        bool shouldCancel = false;
        bool IsRunning = false;
        TaskCompletionSource<bool> cancelNotify = new();
        public Task CancelAsync()
        {
            if (IsRunning)
            {
                cancelNotify = new();
                shouldCancel = true;
                if (IsRunning)
                    return cancelNotify.Task;
            }
            return Task.CompletedTask;
        }
        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
            => Write(parsedSourceCode, scopes, ref globalOffset, ref shouldCancel);
        protected void Write(string parsedSourceCode, IList<Scope> scopes, ref int offset1, ref bool shouldCancel)
        {
            if (shouldCancel) throw new OperationCanceledException();
            //foreach (var a in scopes)
            //{
            //    if (Styles.Contains(a.Name))
            //        CodeParagraph.TextBlock.ApplyStyle(
            //            offset, a.Length,
            //            new CopyStyle(defaultStyle)
            //            {
            //                TextColor = GetColor(Styles[a.Name].Foreground)
            //            }
            //        );
            //}
            //offset += parsedSourceCode.Length;

            var styleInsertions = new List<TextInsertion>();

            foreach (Scope scope in scopes)
            {
                if (shouldCancel) throw new OperationCanceledException();
                GetStyleInsertionsForCapturedStyle(scope, styleInsertions);
            }

            if (shouldCancel) throw new OperationCanceledException();
            styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

            int offset = 0;

            Scope PreviousScope = null;

            foreach (var styleinsertion in styleInsertions)
            {
                if (shouldCancel) throw new OperationCanceledException();
                var text = parsedSourceCode.Substring(offset, styleinsertion.Index - offset);
                CreateSpan(text, PreviousScope, ref offset1, ref shouldCancel);
                if (!string.IsNullOrWhiteSpace(styleinsertion.Text))
                {
                    CreateSpan(text, PreviousScope, ref offset1, ref shouldCancel);
                }
                offset = styleinsertion.Index;

                PreviousScope = styleinsertion.Scope;
            }

            if (shouldCancel) throw new OperationCanceledException();
            var remaining = parsedSourceCode.Substring(offset);
            // Ensures that those loose carriages don't run away!
            if (remaining != "\r")
            {
                CreateSpan(remaining, null, ref offset1, ref shouldCancel);
            }
        }
        private void CreateSpan(string Text, Scope scope, ref int offset, ref bool shouldCancel)
        {
            if (shouldCancel) throw new OperationCanceledException();
            lock (ToModify)
            {
                if (scope is not null && Styles.Contains(scope.Name))
                {
                    var color = GetColor(Styles[scope.Name].Foreground);
                    ToModify.ApplyStyle(offset, Text.Length, new CopyStyle(defaultStyle) { TextColor = color });
                }
                else
                    ToModify.ApplyStyle(offset, Text.Length, defaultStyle);
                offset += Text.Length;
            }
        }

        private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
        {
            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index,
                Scope = scope
            });

            foreach (Scope childScope in scope.Children)
                GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index + scope.Length
            });
        }
    }
    static SKColor GetColor(string hex)
    {
        hex = hex.Replace("#", string.Empty);

        byte a = 255;
        int index = 0;

        if (hex.Length == 8)
        {
            a = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
            index += 2;
        }

        byte r = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
        index += 2;
        byte g = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
        index += 2;
        byte b = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
        return new SKColor(r, g, b, a);
    }
}