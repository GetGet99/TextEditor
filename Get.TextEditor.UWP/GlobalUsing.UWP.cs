global using Platform = Windows;
global using Windows.Devices;
global using Windows.Devices.Input;
global using Windows.UI;
global using Windows.UI.Core;
global using Windows.UI.Xaml;
global using Windows.UI.Xaml.Controls;
global using Windows.UI.Xaml.Controls.Primitives;
global using Windows.UI.ViewManagement;
global using Windows.Foundation;
global using Windows.System;
global using Windows.UI.Xaml.Input;
global using Windows.UI.Text.Core;
global using Windows.UI.Xaml.Hosting;
global using Windows.UI.Composition.Interactions;
global using Windows.UI.Xaml.Media;
global using Windows.UI.Xaml.Media.Imaging;
global using SkiaSharp.Views.UWP;
global using Microsoft.Toolkit.Uwp.UI;
global using PlatformCursor = Windows.UI.Core.CoreCursor;
global using PlatformCursorType = Windows.UI.Core.CoreCursorType;
global using SkiaSharp.Views.Platform;
using Get.XAMLTools;
using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.Platform
{
    class PlatformSKSwapChainPanel : SKSwapChainPanel { }
}
namespace Get.TextEditor
{
    static class PlatformLibrary
    {
        public static bool IsKeyDown(VirtualKey key)
            => (CoreWindow.GetForCurrentThread().GetKeyState(key) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        public static void SetCursor(FrameworkElement element, PlatformCursorType value)
            => FrameworkElementExtensions.SetCursor(element, value);
    }
    static class PlatformExtension
    {
        // Implementation based on newer .NET
        public static string ReplaceLineEndings(this string @this, string replacementText)
        {
            if (replacementText is null)
            {
                throw new ArgumentNullException(nameof(replacementText));
            }

            // Early-exit: do we need to do anything at all?
            // If not, return this string as-is.

            int idxOfFirstNewlineChar = IndexOfNewlineChar(@this.AsSpan(), out int stride);
            if (idxOfFirstNewlineChar < 0)
            {
                return @this;
            }

            // While writing to the builder, we don't bother memcpying the first
            // or the last segment into the builder. We'll use the builder only
            // for the intermediate segments, then we'll sandwich everything together
            // with one final string.Concat call.

            ReadOnlySpan<char> firstSegment = @this.AsSpan(0, idxOfFirstNewlineChar);
            ReadOnlySpan<char> remaining = @this.AsSpan(idxOfFirstNewlineChar + stride);

            ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[256]);
            while (true)
            {
                int idx = IndexOfNewlineChar(remaining, out stride);
                if (idx < 0) { break; } // no more newline chars
                builder.Append(replacementText);
                builder.Append(remaining.Slice(0, idx));
                remaining = remaining.Slice(idx + stride);
            }

            string retVal = string.Concat(firstSegment.ToString(), builder.ToString(), replacementText, remaining.ToString());
            builder.Dispose();
            return retVal;
        }
        // Implementation based on newer .NET
        private static int IndexOfNewlineChar(ReadOnlySpan<char> text, out int stride)
        {
            // !! IMPORTANT !!
            //
            // We expect this method may be called with untrusted input, which means we need to
            // bound the worst-case runtime of this method. We rely on MemoryExtensions.IndexOfAny
            // having worst-case runtime O(i), where i is the index of the first needle match within
            // the haystack; or O(n) if no needle is found. This ensures that in the common case
            // of this method being called within a loop, the worst-case runtime is O(n) rather than
            // O(n^2), where n is the length of the input text.
            //
            // The Unicode Standard, Sec. 5.8, Recommendation R4 and Table 5-2 state that the CR, LF,
            // CRLF, NEL, LS, FF, and PS sequences are considered newline functions. That section
            // also specifically excludes VT from the list of newline functions, so we do not include
            // it in the needle list.

            const string needles = "\r\n\f\u0085\u2028\u2029";

            stride = default;
            int idx = text.IndexOfAny(needles.AsSpan());
            if ((uint)idx < (uint)text.Length)
            {
                stride = 1; // needle found

                // Did we match CR? If so, and if it's followed by LF, then we need
                // to consume both chars as a single newline function match.

                if (text[idx] == '\r')
                {
                    int nextCharIdx = idx + 1;
                    if ((uint)nextCharIdx < (uint)text.Length && text[nextCharIdx] == '\n')
                    {
                        stride = 2;
                    }
                }
            }

            return idx;
        }
        // Implementation based on newer .NET
        ref partial struct ValueStringBuilder
        {
            private char[]? _arrayToReturnToPool;
            private Span<char> _chars;
            private int _pos;

            public ValueStringBuilder(Span<char> initialBuffer)
            {
                _arrayToReturnToPool = null;
                _chars = initialBuffer;
                _pos = 0;
            }

            public ValueStringBuilder(int initialCapacity)
            {
                _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
                _chars = _arrayToReturnToPool;
                _pos = 0;
            }

            public int Length
            {
                get => _pos;
                set
                {
                    Debug.Assert(value >= 0);
                    Debug.Assert(value <= _chars.Length);
                    _pos = value;
                }
            }

            public int Capacity => _chars.Length;

            public void EnsureCapacity(int capacity)
            {
                // This is not expected to be called this with negative capacity
                Debug.Assert(capacity >= 0);

                // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
                if ((uint)capacity > (uint)_chars.Length)
                    Grow(capacity - _pos);
            }

            /// <summary>
            /// Get a pinnable reference to the builder.
            /// Does not ensure there is a null char after <see cref="Length"/>
            /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
            /// the explicit method call, and write eg "fixed (char* c = builder)"
            /// </summary>
            public ref char GetPinnableReference()
            {
                return ref MemoryMarshal.GetReference(_chars);
            }

            /// <summary>
            /// Get a pinnable reference to the builder.
            /// </summary>
            /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
            public ref char GetPinnableReference(bool terminate)
            {
                if (terminate)
                {
                    EnsureCapacity(Length + 1);
                    _chars[Length] = '\0';
                }
                return ref MemoryMarshal.GetReference(_chars);
            }

            public ref char this[int index]
            {
                get
                {
                    Debug.Assert(index < _pos);
                    return ref _chars[index];
                }
            }

            public override string ToString()
            {
                string s = _chars.Slice(0, _pos).ToString();
                Dispose();
                return s;
            }

            /// <summary>Returns the underlying storage of the builder.</summary>
            public Span<char> RawChars => _chars;

            /// <summary>
            /// Returns a span around the contents of the builder.
            /// </summary>
            /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
            public ReadOnlySpan<char> AsSpan(bool terminate)
            {
                if (terminate)
                {
                    EnsureCapacity(Length + 1);
                    _chars[Length] = '\0';
                }
                return _chars.Slice(0, _pos);
            }

            public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
            public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);
            public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

            public bool TryCopyTo(Span<char> destination, out int charsWritten)
            {
                if (_chars.Slice(0, _pos).TryCopyTo(destination))
                {
                    charsWritten = _pos;
                    Dispose();
                    return true;
                }
                else
                {
                    charsWritten = 0;
                    Dispose();
                    return false;
                }
            }

            public void Insert(int index, char value, int count)
            {
                if (_pos > _chars.Length - count)
                {
                    Grow(count);
                }

                int remaining = _pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
                _chars.Slice(index, count).Fill(value);
                _pos += count;
            }

            public void Insert(int index, string? s)
            {
                if (s == null)
                {
                    return;
                }

                int count = s.Length;

                if (_pos > (_chars.Length - count))
                {
                    Grow(count);
                }

                int remaining = _pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
                s
                    .AsSpan()
                    .CopyTo(_chars.Slice(index));
                _pos += count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Append(char c)
            {
                int pos = _pos;
                Span<char> chars = _chars;
                if ((uint)pos < (uint)chars.Length)
                {
                    chars[pos] = c;
                    _pos = pos + 1;
                }
                else
                {
                    GrowAndAppend(c);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Append(string? s)
            {
                if (s == null)
                {
                    return;
                }

                int pos = _pos;
                if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
                {
                    _chars[pos] = s[0];
                    _pos = pos + 1;
                }
                else
                {
                    AppendSlow(s);
                }
            }

            private void AppendSlow(string s)
            {
                int pos = _pos;
                if (pos > _chars.Length - s.Length)
                {
                    Grow(s.Length);
                }

                s
                    .AsSpan()
                    .CopyTo(_chars.Slice(pos));
                _pos += s.Length;
            }

            public void Append(char c, int count)
            {
                if (_pos > _chars.Length - count)
                {
                    Grow(count);
                }

                Span<char> dst = _chars.Slice(_pos, count);
                for (int i = 0; i < dst.Length; i++)
                {
                    dst[i] = c;
                }
                _pos += count;
            }

            public unsafe void Append(char* value, int length)
            {
                int pos = _pos;
                if (pos > _chars.Length - length)
                {
                    Grow(length);
                }

                Span<char> dst = _chars.Slice(_pos, length);
                for (int i = 0; i < dst.Length; i++)
                {
                    dst[i] = *value++;
                }
                _pos += length;
            }

            public void Append(ReadOnlySpan<char> value)
            {
                int pos = _pos;
                if (pos > _chars.Length - value.Length)
                {
                    Grow(value.Length);
                }

                value.CopyTo(_chars.Slice(_pos));
                _pos += value.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<char> AppendSpan(int length)
            {
                int origPos = _pos;
                if (origPos > _chars.Length - length)
                {
                    Grow(length);
                }

                _pos = origPos + length;
                return _chars.Slice(origPos, length);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void GrowAndAppend(char c)
            {
                Grow(1);
                Append(c);
            }

            /// <summary>
            /// Resize the internal buffer either by doubling current buffer size or
            /// by adding <paramref name="additionalCapacityBeyondPos"/> to
            /// <see cref="_pos"/> whichever is greater.
            /// </summary>
            /// <param name="additionalCapacityBeyondPos">
            /// Number of chars requested beyond current position.
            /// </param>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Grow(int additionalCapacityBeyondPos)
            {
                Debug.Assert(additionalCapacityBeyondPos > 0);
                Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

                const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

                // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
                // to double the size if possible, bounding the doubling to not go beyond the max array length.
                int newCapacity = (int)Math.Max(
                    (uint)(_pos + additionalCapacityBeyondPos),
                    Math.Min((uint)_chars.Length * 2, ArrayMaxLength));

                // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
                // This could also go negative if the actual required length wraps around.
                char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

                _chars.Slice(0, _pos).CopyTo(poolArray);

                char[]? toReturn = _arrayToReturnToPool;
                _chars = _arrayToReturnToPool = poolArray;
                if (toReturn != null)
                {
                    ArrayPool<char>.Shared.Return(toReturn);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                char[]? toReturn = _arrayToReturnToPool;
                this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
                if (toReturn != null)
                {
                    ArrayPool<char>.Shared.Return(toReturn);
                }
            }
        }
    }
}
namespace CommunityToolkit.Platform.UI
{
    [AttachedProperty(typeof(PlatformCursorType), "Cursor", GenerateLocalOnPropertyChangedMethod = true)]
    static partial class SharedFrameworkElementExtensions
    {
        static partial void OnCursorChanged(DependencyObject obj, PlatformCursorType oldValue, PlatformCursorType newValue)
        {
            if (obj is FrameworkElement ele)
                FrameworkElementExtensions.SetCursor(ele, newValue);
        }
    }
}