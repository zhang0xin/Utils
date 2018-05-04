using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Utils.WebControls
{
  public class Alerts : WebControlBase
  {
    List<AlertInfo> infos = new List<AlertInfo>();
    public void Add(string message, InfoType type = InfoType.Info)
    {
      infos.Add(new AlertInfo(message, type));
    }
    protected override void Render(HtmlTextWriter writer)
    {
      foreach (AlertInfo info in infos)
      {
        writer.RenderWithClass("div", "alert alert-"+info.Type.ToString().ToLower()+" alert-dismissable", delegate()
        {
          writer.AddAttribute("class", "close");
          writer.AddAttribute("data-dismiss", "alert");
          writer.AddAttribute("aria-hidden", "true");
          writer.RenderBeginTag("button");
          writer.Write("&times;");
          writer.RenderEndTag();
          writer.Write(info.Message);
        });
      }
    }
  }
  internal class AlertInfo
  {
    public AlertInfo(string message, InfoType type)
    {
      Message = message;
      Type = type;
    }
    public InfoType Type { get; set; }
    public string Message { get; set; }
  }
  public enum InfoType
  {
    Success, Info, Warning, Danger
  }
}
