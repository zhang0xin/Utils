using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Utils.WebControls
{
  public class DateTimeSelector : WebControlBase, INamingContainer
  {
    TextBox tbDateTime;
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
          tbDateTime.Text = value.Value.ToShortDateString();
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
      Page.ClientScript.RegisterStartupScript(GetType(), "formdate",
       @"
$('.form_date').datetimepicker({
    language: 'zh-CN',
    weekStart: 1,
    todayBtn: 1,
    autoclose: 1,
    todayHighlight: 1,
    startView: 2,
    minView: 2,
    forceParse: 0
});", true);
    }
    protected override void Render(HtmlTextWriter writer)
    {
      writer.Write("<div class='input-group date form_date' data-date='' data-date-format='yyyy-mm-dd' data-link-format='yyyy-mm-dd'>");
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
