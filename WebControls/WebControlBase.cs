using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Utils.WebControls
{
  public class WebControlBase : WebControl, INamingContainer
  {
    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);

      AddCss("Utils.Resources.utils.css");
      AddJs("Utils.Resources.utils.js");

      //AddJs("Utils.Resources.jquery.js");

      //AddCss("Utils.Resources.bootstrap.css");
      //AddJs("Utils.Resources.bootstrap.js");

      //AddCss("Utils.Resources.bootstrap-datetimepicker.css");
      //AddJs("Utils.Resources.bootstrap-datetimepicker.js");
    }
    protected void AddCss(string resourceName)
    {
      if (Page.Header.FindControl(resourceName) == null)
      {
        HtmlLink regCss = new HtmlLink();
        regCss.ID = resourceName;
        regCss.Href = Page.ClientScript.GetWebResourceUrl(GetType(), resourceName);
        regCss.Attributes.Add("type", "text/css");
        regCss.Attributes.Add("rel", "stylesheet");
        this.Page.Header.Controls.Add(regCss);
      }
    }
    protected void AddJs(string resourceName)
    {
      if (!Page.ClientScript.IsClientScriptIncludeRegistered(resourceName))
      {
        Page.ClientScript.RegisterClientScriptInclude(
          resourceName, Page.ClientScript.GetWebResourceUrl(GetType(), resourceName));
      }
    }
    protected T GetViewStateValue<T>(string name, T defaultValue)
    {
      return ViewState[name] == null ? defaultValue : (T)ViewState[name];
    }
    protected void SetViewStateValue(string name, object value)
    {
      ViewState[name] = value;
    }
  }
}
