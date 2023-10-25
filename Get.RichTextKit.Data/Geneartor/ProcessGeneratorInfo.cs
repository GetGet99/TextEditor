namespace Get.RichTextKit.Data;

public struct ProcessGeneratorInfo
{
    public TextRange Range { get; set; }
    public bool RTFEndLineImplicit { get; set; }
    public bool HTMLEndLineImplicit { get; set; }
}