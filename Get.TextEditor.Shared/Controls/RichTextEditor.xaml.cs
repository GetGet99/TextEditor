using Windows.UI.Xaml.Controls;
using Get.XAMLTools;
using Windows.UI.Xaml;
using Get.RichTextKit.Editor.Paragraphs;
using Get.RichTextKit.Editor.DocumentView;
using System.Collections.Generic;
using Get.RichTextKit.Editor.Paragraphs.Panel;
using System.Diagnostics;
using Get.RichTextKit.Editor;
using System.Numerics;

namespace Get.TextEditor;
[DependencyProperty<DataTemplateSelector>("UIConfigParagraphTemplateSelector")]
partial class RichTextEditor : UserControl
{
    internal RichTextEditorUICanvas UnsafeGetUICanvas() => UICanvas;
    void InitXAML()
    {
        InitializeComponent();
        ManipulationDelta += (o, e) =>
        {
            if (ShouldManipulationScroll(e.PointerDeviceType))
            {
                DocumentView.YScroll += (float)-e.Delta.Translation.Y;
            }
        };
        UIConfigParagraphTemplateSelector = new EmptySeleector(EmptyTemplate);
    }
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
        var factory = new FactoryWrapper(uIElementFactory, (view, ele) =>
        {
            if (view == this)
            {
                ele.PointerEntered += (_, e) =>
                {
                    IsCursorInside = false;
                };
            }
        });
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