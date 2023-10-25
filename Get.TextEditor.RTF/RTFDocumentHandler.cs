using Get.RichTextKit;
using Get.RichTextKit.Editor;
using Get.RichTextKit.Editor.DataStructure.Table;
using Get.RichTextKit.Editor.DocumentView;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using Get.RichTextKit.Editor.Structs;
using Get.RichTextKit.Editor.UndoUnits;
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
    public TextRange GetHandlerSelectionRange() => DocumentView.Selection.Range;
    public RTFDocumentHandler(DocumentView document, AllowedFormatting importFormatting)
    {
        DocumentView = new DocumentView(document.OwnerView, document.OwnerDocument);
        DocumentView.Selection.Range = document.Selection.Range;
        ImportFormatting = importFormatting;
    }
    public void AddText(ReadOnlyMemory<int> text)
    {
        CleanUp();
        DocumentView.Controller.Type(text.Span);
    }

    public void EnterGroup(RTFGroup group)
    {

    }

    public void ExecuteCommand(ref RTFCommandContext commandContext, int? param, RTFGroup? context)
    {
        var strCmd = commandContext.TextCommand.ToString();
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
                CleanUp();
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = FontTable;
                break;
            case "colortbl": // Color Table
                CleanUp();
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = ColorTable;
                break;
            case "stylesheet":
                CleanUp();
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = StylesheetParser;
                break;
            case "info":
                CleanUp();
                commandContext.ShouldBeGroupCommand = true;
                commandContext.CommandGroupHandler = EmptyRTFParserHandler.Instance;
                break;
            case "deff":
                CleanUp();
                if (param is null) goto default;
                DefaultFontIndex = param.Value;
                FontTable.Changed += delegate
                {
                    if (FontTable.TryGetValue(DefaultFontIndex.Value, out var font))
                        ApplyComplexStyle(
                            ImportFormatting.FontFamily,
                            () => DocumentView.Selection.FontFamily,
                            x => DocumentView.Selection.FontFamily = x,
                            font
                        );
                };
                break;
            case "i":
                CleanUp();
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Italic, x => DocumentView.Selection.Italic = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Italic, x => DocumentView.Selection.Italic = x);
                break;
            case "b":
                CleanUp();
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Bold, x => DocumentView.Selection.Bold = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Bold, x => DocumentView.Selection.Bold = x);
                break;
            case "ul":
                CleanUp();
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Underline, x => DocumentView.Selection.Underline = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Underline, x => DocumentView.Selection.Underline = x);
                break;
            case "ulnone":
                CleanUp();
                ApplySimpleStyleOff(ImportFormatting.Underline, x => DocumentView.Selection.Underline = x);
                break;
            case "super":
                CleanUp();
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.SuperScript, x => DocumentView.Selection.SuperScript = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.SuperScript, x => DocumentView.Selection.SuperScript = x);
                break;
            case "sub":
                CleanUp();
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.SubScript, x => DocumentView.Selection.SubScript = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.SubScript, x => DocumentView.Selection.SubScript = x);
                break;
            case "f":
                CleanUp();
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.FontFamily,
                    () => DocumentView.Selection.FontFamily,
                    x => DocumentView.Selection.FontFamily = x,
                    FontTable[param.Value]
                );
                break;
            case "fs":
                CleanUp();
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.FontSize,
                    () => DocumentView.Selection.FontSize,
                    x => DocumentView.Selection.FontSize = x,
                    (float)param.Value / 2
                );
                break;
            case "cf":
                CleanUp();
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.TextColor,
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
                CleanUp();
                if (param is null) goto default;
                ApplyComplexStyle(
                    ImportFormatting.TextColor,
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
                CleanUp();
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
                        ImportFormatting.FontFamily,
                        () => DocumentView.Selection.FontFamily,
                        x => DocumentView.Selection.FontFamily = x,
                        font
                    );
                ApplyComplexStyle(
                    ImportFormatting.FontSize,
                    () => DocumentView.Selection.FontSize,
                    x => DocumentView.Selection.FontSize = x,
                    12f
                );

                ApplyComplexStyle(
                    ImportFormatting.TextColor,
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
                CleanUp();
                DocumentView.Controller.Type(Document.NewParagraphSeparator.ToString());
                break;
            case "line":
                CleanUp();
                DocumentView.Controller.Type("\n");
                break;
            case "strike":
                CleanUp();
                if (param is 0)
                    ApplySimpleStyleOff(ImportFormatting.Strikethrough, x => DocumentView.Selection.Strikethrough = x);
                else
                    ApplySimpleStyleOn(ImportFormatting.Strikethrough, x => DocumentView.Selection.Strikethrough = x);
                break;
            // scaps
            case "trowd":
                // table
                tableInfo = new();
                break;
            case "cellx":
                // cell width
                if (tableInfo.HasValue && param.HasValue)
                {
                    // Add new cell
                    tableInfo.Value.Columns.Add(new(param.Value, TableLengthMode.Ratio));
                }
                break;
            case "cell": // Moves to the next cell
                //DocumentView.Controller.MoveCaret(Direction.Right);
                //{
                //    DocumentView.OwnerDocument.Layout.EnsureValid();
                //    var table = (TableParagraph)DocumentView.Selection.CurrentlyInteractingParagraph.First(x => x is TableParagraph);
                //    var currentIdx = TableParagraph.TableIndex.FromCaretPosition(table, table.GlobalInfo.OffsetToThis(DocumentView.Selection.Range.EndCaretPosition));
                //    var nextIdx = TableParagraph.TableIndex.FromActualIndex(table, currentIdx.ActualIndex + 1);
                //    if (nextIdx.Row >= table.Rows.Count)
                //    {
                //        // Move outside the table. nextIdx is not a valid index
                //        DocumentView.Controller.MoveCaret(table.EndCaretPosition);
                //        DocumentView.Controller.MoveCaret(Direction.Right);
                //    }
                //    else
                //    {
                //        // Move to the next cell
                //        var nextPara = table[nextIdx.Row, nextIdx.Column];
                //        DocumentView.Controller.MoveCaret(nextPara.GlobalInfo.OffsetFromThis(nextPara.StartCaretPosition));
                //    }
                //}
                {
                    DocumentView.OwnerDocument.Layout.EnsureValid();
                    // Make sure we are in the table
                    if (DocumentView.Selection.CurrentlyInteractingParagraph.Any(x => x is TableParagraph))
                    {
                        var beforePara = DocumentView.Selection.CurrentlyInteractingParagraph.First();
                        var style = DocumentView.Selection.CurrentCaretStyle;
                        var range = DocumentView.OwnerDocument[new(
                            beforePara.GlobalInfo.OffsetFromThis(beforePara.UserEndCaretPosition).CodePointIndex,
                            beforePara.GlobalInfo.OffsetFromThis(new CaretPosition(beforePara.CodePointLength,true)).CodePointIndex
                        )];
                        range.ApplyStyle(_ => style);
                        DocumentView.Controller.MoveCaret(Direction.Right);
                        DocumentView.Selection.ApplyStyle(_ => style);
                    }

                }
                break;
            case "trgaph":
                // Table Row Gap Half aka Cell padding
                goto default;
            case "clvertalt":
                // top vertical alignment
                goto default;
            case "clvertalc":
                // center vertical alignment
                goto default;
            case "clvertalb":
                // bottom vertical alignment
                goto default;
            case "row":
                // End table
                goto default;
            default:
                if (commandContext.ShouldBeGroupCommand) commandContext.CommandGroupHandler = EmptyRTFParserHandler.Instance;
                break;
        }
    }
    void CleanUp()
    {
        if (tableInfo.HasValue)
        {
            var tableInfo = this.tableInfo.Value;
            var currPara = DocumentView.Selection.CurrentlyInteractingParagraph.First();
            var currParaIdx = currPara.GlobalParagraphIndex;
            if (currParaIdx.RecursiveIndexArray[^1] > 0)
            {
                var prevParaIdx = new ParagraphIndex(currParaIdx.RecursiveIndexArray.Select(x => x).ToArray());
                prevParaIdx.RecursiveIndexArray[^1]--;
                if (DocumentView.OwnerDocument.Paragraphs[prevParaIdx] is TableParagraph table)
                {
                    if (table.Columns.Count != tableInfo.Columns.Count)
                        goto CreateNewTable;
                    foreach (var i in ..tableInfo.Columns.Count)
                    {
                        if (table.Columns[i].Width != tableInfo.Columns[i])
                            goto CreateNewTable;
                    }
                    // The table is equivalent to the previous row
                    // TODO: Replace with UndoUnits
                    table.Rows.Add(
                        (..tableInfo.Columns.Count)
                        .Select(_ => TableParagraph.CreateInner(table.EndStyle))
                        .ToArray(),
                        TableLength.Auto
                    );
                    DocumentView.OwnerDocument.Layout.InvalidateAndValid();
                    var cell = table.Rows[^1][0];
                    DocumentView.Controller.MoveCaret(cell.GlobalInfo.OffsetFromThis(cell.UserStartCaretPosition));
                    goto EndTableCreation;
                }
                else goto CreateNewTable;
            }
            else
            {
                goto CreateNewTable;
            }
        CreateNewTable:
            var newtable = new TableParagraph(currPara.StartStyle, initialRows: 1, initialCols: tableInfo.Columns.Count);

            foreach (var (i, width) in tableInfo.Columns.WithIndex())
            {
                var col = newtable.Columns[i];
                col.Width = width;
            }
            DocumentView.OwnerDocument.UndoManager.Do(new UndoInsertParagraph(
                (IParagraphCollection)DocumentView.OwnerDocument.Paragraphs[currParaIdx.Parent],
                currParaIdx.IndexRelativeToParent,
                newtable
            ));
            DocumentView.OwnerDocument.Layout.EnsureValid();
            DocumentView.Controller.MoveCaret(newtable.GlobalInfo.OffsetFromThis(newtable.UserStartCaretPosition));
        EndTableCreation:
            this.tableInfo = null;
        }
    }
    TableInfo? tableInfo;
    struct TableInfo
    {
        public List<TableLength> Columns = new();
        public TableInfo()
        {
        }
        int PreviousOffset = 0;
        void AddColumn(int Offset)
        {
            Columns.Add(new(Offset - PreviousOffset, TableLengthMode.Ratio));
            PreviousOffset = Offset;
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