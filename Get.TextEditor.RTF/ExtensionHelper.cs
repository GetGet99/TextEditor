using System;
using System.Collections.Generic;
using System.Text;

namespace Get.RichTextKit.Editor;

static class ExtensionHelper
{
    /// <summary>
    /// Create Enumerator from Range, for `foreach` syntax sugar
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. Indexing from end infers ending from length</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    public static IEnumerator<int> GetEnumerator(this Range range)
    {
        return GetEnumerable(range).GetEnumerator();
    }
    /// <summary>
    /// Create Enumerator from Range, for `foreach` syntax sugar
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. Indexing from end infers ending from length</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    public static IEnumerable<TOut> Select<TOut>(this Range range, Func<int, TOut> func)
    {
        if (range.End.IsFromEnd) throw new ArgumentException("Range.End cannot start from the end value");
        if (range.Start.IsFromEnd) range = (range.End.Value - range.Start.Value)..range.End.Value;
        for (int i = range.Start.Value; i < range.End.Value; i++)
        {
            yield return func(i);
        }
    }
    /// <summary>
    /// Create Enumerable from Range, with given length
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. Indexing from end infers ending from length</param>
    /// <param name="step">The step, defaults to 1</param>
    /// <param name="length">The length to refer to. If null, it refers to the end range</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IEnumerable<int> Iterate(this Range range, int? length = null, int step = 1, bool startInclusive = true, bool endInclusive = false)
    {
        if (length is null)
        {
            length = range.End.Value;
            if (range.End.IsFromEnd)
                throw new ArgumentException("Range.End cannot start from the end value");
        }
        var start = range.Start.GetOffset(length.Value);
        var end = range.End.GetOffset(length.Value);
        switch (step)
        {
            case > 0:
                if (!startInclusive)
                    start += step;
                for (int i = start; endInclusive ? i <= end : i < end; i += step)
                {
                    yield return i;
                }
                break;
            case < 0: // step is negative so addition makes the value less
                if (!endInclusive)
                    end += step;
                for (int i = end; startInclusive ? i >= start : i > start; i += step)
                {
                    yield return i;
                }
                break;
            default:
                throw new ArgumentException("step cannot be 0");
        }
    }
    /// <summary>
    /// Create Enumerable from Range
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. `range.End` must not be from end. If `range.Start` is from end, the end value is infered from `range.End`</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<int> GetEnumerable(Range range)
    {
        if (range.End.IsFromEnd) throw new ArgumentException("Range.End cannot start from the end value");
        if (range.Start.IsFromEnd) range = (range.End.Value - range.Start.Value)..range.End.Value;
        //for (int i = range.Start.Value; i < range.End.Value; i++)
        //{
        //    yield return i;
        //}
        if (range.Start.Value < range.End.Value)
            return Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value);
        return Enumerable.Empty<int>();
    }

    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> item)
    {
        int idx = 0;
        foreach (var a in item)
            yield return (idx++, a);
    }
}
