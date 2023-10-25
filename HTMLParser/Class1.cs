using System;
using System.Web;
using System.Xml;
namespace HTMLParser;

public class Class1
{
    public Class1()
    {
        string HTML = "";
        HtmlAgilityPack.HtmlDocument doc = new();
        doc.LoadHtml(HTML);
        //HttpUtility.HtmlDecode()
    }
}
