
using Get.XAMLTools;

using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor.DocumentView;
using System.Collections.Generic;
using System.Numerics;
using Get.TextEditor.Tools;
using Windows.UI.Core;

namespace Get.TextEditor;
[DependencyProperty<DataTemplateSelector>("UIConfigParagraphTemplateSelector")]
[DependencyProperty<FocusHandler<RichTextEditor>>("FocusHandler")]
partial class RichTextEditor : UserControl
{
    internal RichTextEditorUICanvas UnsafeGetUICanvas() => UICanvas;
    void InitXAML()
    {
        FocusHandler = new InteractingContextOnlyFocusHandler();
        InitializeComponent();
        ManipulationDelta += (o, e) =>
        {
            if (ShouldManipulationScroll(e.PointerDeviceType))
            {
                EditorCanvas.ManipulationScrolled = true;
                DocumentView.YScroll += (float)-e.Delta.Translation.Y;
            }
        };
        UIConfigParagraphTemplateSelector = new EmptySeleector(EmptyTemplate);

        void FocusManager_GettingFocus(object sender, GettingFocusEventArgs e)
        {
            if (e.OldFocusedElement == this)
            {
#if WINDOWS_UWP
                if (FocusHandler.ShouldKeepFocus(this, CoreWindow.GetForCurrentThread().PointerPosition, e.NewFocusedElement))
#else
                if (FocusHandler.ShouldKeepFocus(this, e.NewFocusedElement))
#endif
                {
                    if (e.TryCancel())
                    {
                        HasFocus = true;
                        EditContext.NotifyFocusEnter();
                        e.Handled = true;
                    } else
                    {
                        HasFocus = false;
                        EditContext.NotifyFocusLeave();
                    }
                } else
                {
                    HasFocus = false;
                    EditContext.NotifyFocusLeave();
                }
            }
        }
        RegisterLoadUnloadEvent(() => FocusManager.GettingFocus += FocusManager_GettingFocus, () => FocusManager.GettingFocus -= FocusManager_GettingFocus);
    }
    void RegisterLoadUnloadEvent(Action addEv, Action removeEv)
    {
        Loaded += (_, _) => addEv();
        Unloaded += (_, _) => removeEv();
        if (IsLoaded)
            addEv();
    }

    private DispatcherTimer _focusTimer;
    class EmptySeleector : DataTemplateSelector
    {
        DataTemplate EmptyTemplate;
        public EmptySeleector(DataTemplate emptyTemplate)
        {
            EmptyTemplate = emptyTemplate;
        }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return EmptyTemplate;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return EmptyTemplate;
        }
    }
    //void InsertUIFrameworkElement(FrameworkElement ele)
    //{
    //    ele.PointerEntered += (_, e) =>
    //    {
    //        IsCursorInside = false;
    //    };
    //    DocumentView.Controller.Type(new FrameworkElementRun(
    //        this,
    //        ele,
    //        DocumentView.Selection.Range.IsRange ?
    //        DocumentView.OwnerDocument.GetStyleAtPosition(DocumentView.Selection.Range.Normalized.EndCaretPosition) :
    //        DocumentView.Selection.CurrentCaretStyle
    //    ));
    //}
    void InsertUIElement(IUIElementFactory uIElementFactory)
    {
        //var factory = new FactoryWrapper(uIElementFactory, (view, ele) =>
        //{
        //    if (view == this)
        //    {
        //        ele.PointerEntered += (_, e) =>
        //        {
        //            IsCursorInside = false;
        //        };
        //    }
        //});
        var factory = uIElementFactory;
        DocumentView.Controller.InsertNewParagraph(new UIElementParagraph(DocumentView.Selection.CurrentCaretStyle, factory, this));
    }
    // XAML Helper
    DataTemplate Select(DataTemplateSelector selector, IEnumerable<Paragraph> CurrentPosParagraph)
    {
        foreach (var para in CurrentPosParagraph)
        {
            var template = selector.SelectTemplate(para);
            if (template is not null)
            {
                ParagraphSettingUIContentPresenter.Content = para;
                return template;
            }
        }
        ParagraphSettingUIContentPresenter.Content = null;
        return EmptyTemplate;
    }
    Vector3 NegativeVector3(float x, float y)
        => new(-x, -y, 0);
}