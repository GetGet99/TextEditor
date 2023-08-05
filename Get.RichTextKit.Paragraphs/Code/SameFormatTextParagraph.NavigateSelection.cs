using Get.RichTextKit.Utils;

namespace Get.RichTextKit.Editor.Paragraphs;

partial class CodeParagraph
{
    protected override NavigationStatus NavigateOverride(TextRange selection, NavigationSnap snap, NavigationDirection direction, bool keepSelection, ref float? ghostXCoord, out TextRange newSelection)
    {
        if (direction is NavigationDirection.Up or NavigationDirection.Down)
        {
            return VerticalNavigateUsingLineInfo(selection, snap, direction, keepSelection, ref ghostXCoord, out newSelection);
        }
        ghostXCoord = null;
        if (!keepSelection && selection.IsRange)
        {
            newSelection = direction is NavigationDirection.Backward ? new(selection.MinimumCaretPosition) : new(selection.MaximumCaretPosition);
            return NavigationStatus.Success;
        }
        newSelection = selection;
        var indicies = snap switch
        {
            NavigationSnap.Character => TextBlock.CaretIndicies,
            NavigationSnap.Word => TextBlock.WordBoundaryIndicies,
            _ => throw new ArgumentOutOfRangeException(nameof(snap))
        };

        var ii = indicies.BinarySearch(selection.End);

        // Work out the new position
        if (ii < 0)
        {
            ii = (~ii);
            if (direction is NavigationDirection.Forward)
                ii--;
        }
        ii += direction is NavigationDirection.Forward ? 1 : -1;


        if (ii < 0)
        {
            // Move to end of previous paragraph
            return NavigationStatus.MoveBefore;
        }

        if (ii >= indicies.Count)
        {
            // Move to start of next paragraph
            return NavigationStatus.MoveAfter;
        }

        // Move to new position in this paragraph
        selection.End = indicies[ii];
        if (!keepSelection) selection.Start = selection.End;
        newSelection = selection;

        // Safety: Don't allow steping over paragraph separator
        if (!selection.IsRange && newSelection.Maximum >= CodePointLength)
        {
            return NavigationStatus.MoveAfter;
        }
        return NavigationStatus.Success;
    }
    public override TextRange GetSelectionRange(CaretPosition position, ParagraphSelectionKind kind)
    {
        if (kind is ParagraphSelectionKind.Word)
        {
            var indicies = TextBlock.WordBoundaryIndicies;
            var ii = indicies.BinarySearch(position.CodePointIndex);
            if (ii < 0)
                ii = (~ii - 1);
            if (ii >= indicies.Count)
                ii = indicies.Count - 1;

            if (ii + 1 >= indicies.Count)
            {
                // Point is past end of paragraph
                return new TextRange(
                    indicies[ii],
                    indicies[ii],
                    true
                );
            }

            // Create text range covering the entire word
            return new TextRange(
                indicies[ii],
                indicies[ii + 1],
                true
            );
        }
        else
            return new(position);
    }
}
