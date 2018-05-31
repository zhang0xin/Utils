using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Utils.WebControls
{
  public enum DisplayType
  { DateTime=0, Date=2 }
  [ValidationProperty("Value")]
  public class DateTimeSelector : WebControlBase, INamingContainer
  {
    TextBox tbDateTime;
    public DisplayType Type
    {
      get { return ViewState["Type"] == null ? DisplayType.Date : (DisplayType)ViewState["Type"]; }
      set { ViewState["Type"] = value; }
    }
    public int Width
    {
      get { return ViewState["Width"] == null ? -1 : (int)ViewState["Width"]; }
      set { ViewState["Width"] = value; }
    }
    public DateTime? Value
    {
      get
      {
        EnsureChildControls();
        try
        {
          return DateTime.Parse(tbDateTime.Text);
        }
        catch
        {
          return null;
        }
      }
      set
      {
        EnsureChildControls();
        if (value.HasValue)
        {
          if (Type == DisplayType.Date) tbDateTime.Text = value.Value.ToString("yyyy-MM-dd");
          else if (Type == DisplayType.DateTime) tbDateTime.Text = value.Value.ToString("yyyy-MM-dd HH:mm");
        }
        else
          tbDateTime.Text = "";
      }
    }
    public void SetValue(object strDateTime)
    {
      try
      {
        Value = DateTime.Parse(strDateTime+"");
      }
      catch
      {
        Value = null;
      }
    }
    public bool AutoPostBack
    {
      get { EnsureChildControls(); return tbDateTime.AutoPostBack; }
      set { EnsureChildControls(); tbDateTime.AutoPostBack = value; }
    }

    protected override void CreateChildControls()
    {
      Controls.Clear();
      tbDateTime = new TextBox();
      tbDateTime.CssClass="form-control";
      //tbDateTime.ReadOnly = true;
      Controls.Add(tbDateTime);
    }
    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);
      Page.ClientScript.RegisterStartupScript(GetType(), ClientID,
      string.Format(@"
$('#{0}').datetimepicker({{
    language: 'zh-CN',
    weekStart: 1,
    todayBtn: 1,
    autoclose: 1,
    todayHighlight: 1,
    startView: 2,
    minView: {1},
    forceParse: 0,
    showMeridian: 0
}});", ClientID, (int)Type), true);
    }
    protected override void Render(HtmlTextWriter writer)
    {
      string format = "";
      if (Type == DisplayType.Date) format = "yyyy-mm-dd";
      else if (Type == DisplayType.DateTime) format = "yyyy-mm-dd hh:ii";

      string style = "";
      if (Width != -1)
        style = string.Format("style='width: {0}px;'", Width);
      writer.Write(string.Format(
        "<div id='{0}' class='input-group date' data-date='' data-date-format='{1}' data-link-format='{1}' {2}>", 
        ClientID, format, style
        ));
      tbDateTime.RenderControl(writer);
      writer.Renderer("span", "class", "input-group-addon").Render(delegate()
      {
        writer.RendererWithClass("span", "glyphicon glyphicon-remove").Render();
      });
      writer.Renderer("span", "class", "input-group-addon").Render(delegate()
      {
        writer.RendererWithClass("span", "glyphicon glyphicon-calendar").Render();
      });
      writer.Write("</div>");
    }
  }
}
