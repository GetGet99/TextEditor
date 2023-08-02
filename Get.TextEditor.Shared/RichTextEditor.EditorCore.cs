using Get.RichTextKit;
using System.Diagnostics;

namespace Get.TextEditor;
partial class RichTextEditor : UserControl
{
    readonly CoreTextEditContext EditContext = CoreTextServicesManager.GetForCurrentView().CreateEditContext();
    bool _hasFocus;
    bool HasFocus
    {
        get { return _hasFocus; }
        set
        {
            //if (value is false)
            //    Debugger.Break();
            _hasFocus = value;
        }
    }
    void InitEditorCore()
    {
        EditContext.InputScope = CoreTextInputScope.Text;
        EditContext.SelectionUpdating += (sender, args) =>
        {
            DocumentView.Controller.Select(args.Selection.ToRTKRange());
        };
        EditContext.TextUpdating += (sender, args) =>
        {
            if (IsKeyDown(VirtualKey.Control))
            {
                args.Result = CoreTextTextUpdatingResult.Failed;
                return;
            }
            var selection = args.Range.ToRTKRange();
            DocumentView.Controller.Select(selection);
            DocumentView.Controller.Type(args.Text);
            DocumentView.Controller.Select(args.NewSelection.ToRTKRange());
            args.Result = CoreTextTextUpdatingResult.Succeeded;
        };
        EditContext.FormatUpdating += (sender, args) =>
        {

        };
        EditContext.FocusRemoved += (sender, args) =>
        {

        };
        EditContext.CompositionStarted += (sender, args) =>
        {

        };
        EditContext.CompositionCompleted += (sender, args) =>
        {

        };
        EditContext.TextRequested += (sender, args) =>
        {
            var requestPart = new TextRange(args.Request.Range.StartCaretPosition, args.Request.Range.EndCaretPosition);
            args.Request.Text = DocumentView.OwnerDocument.GetText(requestPart).ToString();
        };
        EditContext.SelectionRequested += (sender, args) =>
        {
            args.Request.Selection = DocumentView.Selection.Range.ToCore();
        };
        EditContext.LayoutRequested += (sender, args) =>
        {
            //var selection = DocumentView.Selection.Range;
            //args.Request.LayoutBoundsVisualPixels.ControlBounds = TransformToVisual(null).TransformBounds(new(0, 0, ActualWidth, ActualHeight));
            //args.Request.LayoutBoundsVisualPixels.TextBounds = TextDocument.boun;
        };
        GotFocus += delegate
        {
            HasFocus = true;
            //Focus(Platform.UI.Xaml.FocusState.Pointer);
            EditContext.NotifyFocusEnter();
        };
        DocumentView.Selection.RangeChanged += delegate
        {
            EditContext.NotifySelectionChanged(DocumentView.Selection.Range.ToCore());
        };
        DocumentView.OwnerDocument.TextChanged += (modifiedRange) =>
        {
            EditContext.NotifyTextChanged(
                modifiedRange.ToCore(),
                newLength: DocumentView.OwnerDocument.Layout.Length,
                DocumentView.Selection.Range.ToCore()
            );
        };
        EditContext.FocusRemoved += (sender, args) =>
        {
            HasFocus = false;
        };
        //PointerPressed += delegate
        //{
        //    HasFocus = true;
        //    Focus(Platform.UI.Xaml.FocusState.Pointer);
        //    EditContext.NotifyFocusEnter();
        //};
    }
}
static partial class Extension
{
    public static CoreTextRange ToCore(this TextRange r)
        => new() { StartCaretPosition = r.Minimum, EndCaretPosition = r.Maximum };
    public static TextRange ToRTKRange(this CoreTextRange r)
        => new(r.StartCaretPosition, r.EndCaretPosition);
}