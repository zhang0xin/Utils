using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Utils.WebControls.Intell
{
  public class DetailsView : DataBoundControl, INamingContainer
  {
    public int Columns
    {
      get { return ViewState["Columns"] == null ? 1 : (int)ViewState["Columns"]; }
      set { ViewState["Columns"] = value; }
    }
    [PersistenceMode(PersistenceMode.InnerProperty)]
    public List<FieldConfig> Configs
    {
      get
      {
        if (ViewState["Configs"] == null) ViewState["Configs"] = new List<FieldConfig>();
        return (List<FieldConfig>)ViewState["Configs"]; 
      }
      set
      {
        ViewState["Configs"] = value;
      }
    }
    List<FieldView> Fields
    {
      get
      {
        if (ViewState["Fields"] == null) ViewState["Fields"] = new List<FieldView>();
        return (List<FieldView>)ViewState["Fields"]; 
      }
      set
      {
        ViewState["Fields"] = value;
      }
    }
    protected override void PerformDataBinding(System.Collections.IEnumerable data)
    {
      if (data == null) return;
      if (data is DataView)
      {
        DataView dv = data as DataView;
        DataRowView row = dv.Count > 0 ? row = dv[0] : null;
        if (Configs.Count == 0)
        {
          foreach (DataColumn col in dv.Table.Columns)
          {
            Fields.Add(new FieldView(col.ColumnName, row == null ? "" : row[col.ColumnName].ToString()));
          }
        }
        else
        {
          foreach (FieldConfig config in Configs)
          {
            FieldView fv = new FieldView();
            fv.Label = config.Caption;
            string format =  config.ValueFormat == null? "{0}" : format = config.ValueFormat;
            fv.Value = string.Format(format, row[config.Field]);
            fv.ValueType = config.ValueType;
            Fields.Add(fv);
          }
        }
      }
    }

    protected override void Render(HtmlTextWriter writer)
    {
      writer.RenderWithClass("ul", "list-group", delegate()
      {
        foreach (FieldView field in Fields)
        {
          writer.RendererWithClass("li", "list-group-item").Render(delegate()
          {
            writer.RendererWithClass("div", "row").Render(delegate()
            {
              writer.RendererWithClass("label", "col-md-1").Render(delegate()
              {
                writer.Write(field.Label);
              });
              writer.RendererWithClass("div", "col-md-11").Render(delegate()
              {
                if (field.ValueType == DisplayType.Text)
                  writer.Write(field.Value);
                else if (field.ValueType == DisplayType.Image)
                {
                  if (File.Exists(HttpContext.Current.Server.MapPath(field.Value)))
                  {
                    Image img = new Image();
                    img.CssClass = "img-responsive";
                    img.ImageUrl = field.Value;
                    img.RenderControl(writer);
                  }
                }
              });
            });
          });
        }
      });
    }
  }
  [Serializable]
  public class FieldView 
  {
    public FieldView() { }
    public FieldView(string label, string value)
    {
      this.Label = label;
      this.Value = value;
    }
    public string Label { get; set; }
    public string Value { get; set; }
    public DisplayType ValueType { get; set; }
  }
  [Serializable]
  public class FieldConfig
  {
    public string Caption { get; set; }
    public string Field { get; set; }
    public string ValueFormat { get; set; }
    public DisplayType ValueType { get; set; }

    public FieldConfig()
    {
      ValueType = DisplayType.Text;
    }
  }
  public enum DisplayType
  {
    Text, Image
  }
}
