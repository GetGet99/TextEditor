using Get.RichTextKit.Editor.Paragraphs.Properties.Decoration;


namespace Get.TextEditor.UWP.Decoration;
using UIVerticalAlignment = Platform.UI.Xaml.VerticalAlignment;

public class CheckboxDecoration : UIDecoration
{
    public override string TypeIdentifier => "CheckBox";
    public CheckboxDecoration() : base(new UIElementSimpleFactory<CheckBox>(() => new CheckBox() { VerticalAlignment = UIVerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, MinWidth = 0, MinHeight = 0 }))
    {
        
    }
    public override IParagraphDecoration Clone()
    {
        return new CheckboxDecoration();
    }
}
