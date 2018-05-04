using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Utils.WebControls
{
  public static class HtmlTextWriterExtender
  {
    public static void RenderWithIdClass(this HtmlTextWriter writer, string tagName, string id, string cssClass, Action innerRender = null)
    {
      Renderer(writer, tagName, 
        new AttributeNameValue() { Attr = HtmlTextWriterAttribute.Id, Value = id },
        new AttributeNameValue() { Attr = HtmlTextWriterAttribute.Class, Value = cssClass }
      ).Render(innerRender);
    }
    public static void RenderWithClass(this HtmlTextWriter writer, string tagName, string cssClass, Action innerRender = null)
    {
      Renderer(writer, tagName, new AttributeNameValue() { Attr = HtmlTextWriterAttribute.Class, Value = cssClass }).Render(innerRender);
    }
    public static void Render(this HtmlTextWriter writer, string tag, Action innerRender = null)
    {
      Render(writer, ConvertToHtmlTextWriterTag(tag), innerRender);
    }
    public static void Render(this HtmlTextWriter writer, HtmlTextWriterTag tag, Action innerRender = null)
    {
      (new HtmlTagRenderer(writer, tag)).Render(innerRender);
    }
    public static HtmlTagRenderer RendererWithIdClass(this HtmlTextWriter writer, string tagName, string id, string cssClass)
    {
      return Renderer(writer, tagName, 
        new AttributeNameValue() { Attr = HtmlTextWriterAttribute.Id, Value = id },
        new AttributeNameValue() { Attr = HtmlTextWriterAttribute.Class, Value = cssClass }
      );
    }
    public static HtmlTagRenderer RendererWithClass(this HtmlTextWriter writer, string tagName, string cssClass)
    {
      return Renderer(writer, tagName, new AttributeNameValue() { Attr = HtmlTextWriterAttribute.Class, Value = cssClass });
    }
    public static HtmlTagRenderer Renderer(this HtmlTextWriter writer, string tagName, params string[] attrValArr)
    {
      AttributeNameValue[] attrs = new AttributeNameValue[attrValArr.Length/2];
      for (int i = 0; i < attrs.Length; i++)
      {
        attrs[i] = new AttributeNameValue();
        attrs[i].Attr = ConvertToHtmlTextWriterAttribute(attrValArr[i*2].Trim());
        attrs[i].Value = attrValArr[i*2+1].Trim();
      }
      return new HtmlTagRenderer(writer, ConvertToHtmlTextWriterTag(tagName), attrs);
    }
    public static HtmlTagRenderer Renderer(this HtmlTextWriter writer, string tagName, params AttributeNameValue[] attrs)
    {
      return new HtmlTagRenderer(writer, ConvertToHtmlTextWriterTag(tagName), attrs);
    }
    public static HtmlTagRenderer Renderer(this HtmlTextWriter writer, HtmlTextWriterTag tag, params AttributeNameValue[] attrs)
    {
      return new HtmlTagRenderer(writer, tag, attrs);
    }
    static HtmlTextWriterTag ConvertToHtmlTextWriterTag(string tagName)
    {
      string newTagName = tagName.Substring(0, 1).ToUpper() + tagName.Substring(1);
      return (HtmlTextWriterTag)Enum.Parse(typeof(HtmlTextWriterTag), newTagName);
    }
    static HtmlTextWriterAttribute ConvertToHtmlTextWriterAttribute(string attrName)
    {
      string newAttrName = attrName.Substring(0, 1).ToUpper() + attrName.Substring(1);
      return (HtmlTextWriterAttribute)Enum.Parse(typeof(HtmlTextWriterAttribute), newAttrName);
    }
  }
  public class HtmlTagRenderer
  {
    public HtmlTextWriter Writer { get; set; }
    public HtmlTextWriterTag Tag { get; set; }
    public List<AttributeNameValue> Attrs { get; set; }

    public HtmlTagRenderer(HtmlTextWriter writer, HtmlTextWriterTag tag, params AttributeNameValue[] attrs)
    {
      this.Writer = writer;
      this.Tag = tag;
      this.Attrs = new List<AttributeNameValue>();
      this.Attrs.AddRange(attrs);
    }
    public void Render(Action innerRender)
    {
      foreach(AttributeNameValue item in Attrs)
      {
        Writer.AddAttribute(item.Attr, item.Value);
      }
      Writer.RenderBeginTag(Tag);
      if(innerRender != null) innerRender();
      Writer.RenderEndTag();
    }

    public void Render()
    {
      Render(delegate() { });
    }
  }
  public class AttributeNameValue
  {
    public HtmlTextWriterAttribute Attr { get; set;}
    public string Value { get; set; }
  }
}
