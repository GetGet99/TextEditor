#nullable enable
using Get.RichTextKit.Editor;
using System;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace Get.TextEditor;
public partial class RichTextEditor : UserControl
{
    public RichTextEditor() : this(null) { }
    public RichTextEditor(Document? document)
    {
        DocumentView = new(this, document ?? new(DefaultStyle)) { BottomAdditionalScrollHeight = 100 };
        InitTextDocument();
        InitXAML();
        InitEditorCore();
        InitPointerHook();
        InitKeyboardHook();
        // Intentionally update property
        DocumentView.Selection.Range = DocumentView.Selection.Range;
    }
}
class StringStreamProvider
{
    public static Stream CreateWithString(string str)
    {
        var stream = new MemoryStream();
        using var streamwriter = new StreamWriter(stream);
        streamwriter.Write(str);
        return stream;
    }
}
class LazyStream : Stream
{
    public LazyStream(Lazy<Stream> streamProvider)
    {
        _stream = streamProvider;
    }
    readonly Lazy<Stream> _stream;
    Stream Stream => _stream.Value;
    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => Stream.CanWrite;

    public override long Length => Stream.Length;

    public override long Position {
        get => Stream.Position;
        set => Stream.Position = value;
    }

    public override void Flush()
    {
        Stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return Stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        Stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Stream.Write(buffer, offset, count);
    }
}