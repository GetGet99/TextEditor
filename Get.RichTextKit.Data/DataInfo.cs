using System.Collections.Generic;
using System.Text;

namespace Get.RichTextKit.Data;

public class DataInfo
{
    public StringBuilder Text { get; set; } = new();
    public RTFData Rtf { get; set; } = new();
    public StringBuilder HTML { get; set; } = new();
    public Dictionary<string, string> OtherFormat { get; } = new();
}