using Get.XAMLTools;
using System.Collections.Generic;

namespace Get.TextEditor.Tools;

[AttachedProperty(typeof(bool?), "IsTextEditor")]
public static partial class TextEditorDetection
{
    readonly static List<Type> TextEditorTypes = new() { typeof(TextBox), typeof(RichEditBox), typeof(RichTextEditor) };
    public static void RegisterTextEditorType<T>() => RegisterTextEditorType(typeof(T));
    public static void RegisterTextEditorType(Type type) => TextEditorTypes.Add(type);
    public static bool IsTextEditorAuto(DependencyObject obj)
    {
        switch (GetIsTextEditor(obj))
        {
            case true:
                return true;
            case false:
                return false;
            case null:
            default:
                var objType = obj.GetType();
                return TextEditorTypes.FirstOrDefault(type => objType == type || objType.IsSubclassOf(type)) is not null;
        }
    }
}