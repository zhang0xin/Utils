using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utils.Database;

namespace Utils.WebControls
{
  public enum LayoutType { Grid, Inline };
  public class QueryForm : WebControlBase, INamingContainer
  {
    DBHelper dbh = new DBHelper(); 
    #region Properties
    public LayoutType Layout
    {
      get { return GetViewStateValue<LayoutType>("Layout", LayoutType.Grid); }
      set { SetViewStateValue("Layout", value); }
    }

    public int Columns
    {
      get { return ViewState["Columns"]==null? 3 : (int) ViewState["Columns"]; }
      set { ViewState["Columns"] = value; }
    }

    public bool Debug 
    {
      get { return ViewState["Debug"] == null ? false : (bool)ViewState["Debug"]; }
      set { ViewState["Debug"] = value; } 
    }

    [PersistenceMode(PersistenceMode.InnerProperty)]
    public List<QueryField> QueryFields
    {
      get 
      {
        if (ViewState["QueryFields"] == null)
          ViewState["QueryFields"] = new List<QueryField>();

        return (List<QueryField>)ViewState["QueryFields"]; 
      }
      set { ViewState["QueryFields"] = value; }
    }

    public string SqlConditionWithWhere
    {
      get
      {
        if (string.IsNullOrWhiteSpace(SqlCondition)) return "";
        return " where " + SqlCondition;
      }
    }
    public string SqlCondition
    {
      get 
      {
        EnsureChildControls();
        string condition = "";
        foreach (QueryField field in QueryFields)
        {
          string subCond = field.GetCondition(this);
          if (string.IsNullOrWhiteSpace(subCond)) continue;
          condition += subCond + " and ";
        }
        if (!string.IsNullOrWhiteSpace(condition))
          condition = condition.Substring(0, condition.Length - 4);
        return condition;
      }
    }
    public string UrlParams
    {
      get
      {
        EnsureChildControls();
        string urlparams = "";
        foreach (QueryField field in QueryFields)
        {
          urlparams += field.Name + "=" + field.GetQueryParam(this);
        }
        return urlparams;
      }
    }
    string GetControlValue(Control control)
    {
      if (control is TextBox)
        return (control as TextBox).Text;
      else if (control is DropDownList)
        return (control as DropDownList).SelectedValue;
      return null;
    }

    public string TableCssClass 
    { 
      get{ return ViewState["TableCssClass"]==null? null:ViewState["TableCssClass"].ToString();} 
      set{ ViewState["TableCssClass"] = value;} 
    }
    public string ButtonRowCssClass 
    { 
      get{ return ViewState["ButtonRowCssClass"]==null? null:ViewState["ButtonRowCssClass"].ToString();} 
      set{ ViewState["ButtonRowCssClass"] = value;} 
    }
    #endregion

    #region Events
    static object QueryKey = new object();
    public event EventHandler Query
    {
      add
      {
        Events.AddHandler(QueryKey, value);
      }
      remove
      {
        Events.RemoveHandler(QueryKey, value);
      }
    }
    #endregion

    LinkButton _btnQuery;
    LinkButton _btnClear;

    protected override void CreateChildControls()
    {
      Controls.Clear();
      foreach(QueryField queryField in QueryFields)
      {
        queryField.InstantiateControl(this);
      }
      _btnQuery = new LinkButton();
      _btnQuery.Text = "<span class='glyphicon glyphicon-search'></span>查询";
      _btnQuery.CssClass = "btn btn-default";
      _btnQuery.CommandName = "Query";
      Controls.Add(_btnQuery);
      _btnClear = new LinkButton();
      _btnClear.Text = "<span class='glyphicon glyphicon-remove'></span>全部";
      _btnClear.CssClass = "btn btn-default";
      _btnClear.CommandName = "Clear";
      Controls.Add(_btnClear);
    }
    protected override bool OnBubbleEvent(object source, EventArgs args)
    {
      CommandEventArgs cea = args as CommandEventArgs;
      if (cea != null)
      {
        if (cea.CommandName == "Query")
        {
          //HttpContext.Current.Request.Params.Add(;
          if (Events[QueryKey] != null)
          {
            (Events[QueryKey] as EventHandler)(this, null);
            return true;
          }
        }
        else if (cea.CommandName == "Clear")
        {
          foreach(QueryField field in QueryFields)
          {
            field.ClearInputControl(this);
          }
          if (Events[QueryKey] != null) 
            (Events[QueryKey] as EventHandler)(this, null);
          return true;
        }
      }
      return false;
    }
    protected override void Render(HtmlTextWriter writer)
    {
      EnsureChildControls();

      if (Layout == LayoutType.Grid) RenderOnGrid(writer);
      else if (Layout == LayoutType.Inline) RenderOnInline(writer);

      if (Debug) writer.Write(SqlConditionWithWhere);
    }
    protected void RenderOnGrid(HtmlTextWriter writer)
    {
      writer.Render("div", delegate()
      {
        int count = QueryFields.Count / Columns * Columns + (QueryFields.Count % Columns == 0 ? 0 : Columns);
        int colWid = 12 / Columns;
        for (int i = 0; i < count; i++)
        {
          writer.RenderWithClass("div", "col-sm-" + colWid, delegate()
          {
            writer.Render("label", delegate()
            {
              if (i < QueryFields.Count) writer.Write(QueryFields[i].Label);
            });
            if (i < QueryFields.Count) QueryFields[i].RenderControls(this, writer);
          });
        }

        writer.RenderWithClass("div", "col-sm-12", delegate()
        {
          writer.RenderWithClass("div", "pull-right", delegate()
          {
            _btnQuery.RenderControl(writer);
            _btnClear.RenderControl(writer);
          });
        });
      });
    }
    protected void RenderOnInline(HtmlTextWriter writer)
    {
      writer.RenderWithClass("div", "form-inline", delegate()
      {
        foreach (QueryField qf in QueryFields)
        {
          writer.RenderWithClass("div", "form-group", delegate()
          {
            writer.Render("label", delegate()
            {
              writer.Write(qf.Label);
            });
            qf.RenderControls(this, writer);
            writer.Write("&nbsp;");
          });
        }

        writer.RenderWithClass("div", "pull-right", delegate()
        {
          _btnQuery.RenderControl(writer);
          _btnClear.RenderControl(writer);
        });
      });
    }
  }

  #region QueryFields
  public abstract class QueryField 
  {
    protected DBHelper dbh = new DBHelper(); 
    public QueryField() : this("", "")
    { }
    public QueryField(string name, string label=null)
    {
      Name = name;
      Label = label;
      ConditionFormat = " {0} = '{1}' ";
    }

    public string ConditionFormat { get; set; }
    public string Name { get; set; }
    public object DefaultValue { get; set; }
    #region Property ControlId
    string _controlId;
    public string ControlId
    {
      get { if (_controlId == null) return Name; return _controlId; }
      set { _controlId = value; }
    }
    #endregion
    #region Property Label
    string _label;
    public string Label 
    {
      get { if (_label == null) return Name; return _label; } 
      set { _label = value; } 
    }
    #endregion

    public abstract void InstantiateControl(Control container);
    public abstract string GetCondition(Control container);
    public abstract string GetQueryParam(Control container);
    public virtual void RenderControls(Control container, HtmlTextWriter writer)
    {
      Control ctrl = container.FindControl(ControlId);
      if (ctrl != null) ctrl.RenderControl(writer);
    }
    public virtual void ClearInputControl(Control container)
    {
      Control ctrl = container.FindControl(ControlId);
      InputControlAdapter adapter = InputControlAdapter.Create(ctrl);
      adapter.Clear();
    }
  }
  public class TextQueryField : QueryField 
  { 
    public TextQueryField() : this("") 
    { }
    public TextQueryField(string name, string label = null) : base(name, label) 
    { 
      ConditionFormat = " {0} like '%{1}%' ";
    }
    public override void InstantiateControl(Control container)
    {
      TextBox tb = new TextBox();
      tb.CssClass = "form-control";
      tb.ID = ControlId;
      tb.Text = DefaultValue+"";
      container.Controls.Add(tb);
    }
    public override string GetCondition(Control container)
    {
      TextBox tb = container.FindControl(ControlId) as  TextBox;
      if (string.IsNullOrWhiteSpace(tb.Text)) return "";
      return string.Format(ConditionFormat, Name, tb.Text);
    }
    public override string GetQueryParam(Control container)
    {
      throw new NotImplementedException();
    }
  }
  [ParseChildren(true, "Options")]
  public class ListQueryField : QueryField
  {
    public ListQueryField() : base() 
    { 
      Options = new List<ListItem>();
      FillEmptyItem = true;
    }
    public ListQueryField(string name, string label, ICollection<ListItem> options = null)
      : base(name, label)
    {
      Options = new List<ListItem>();
      if(options != null) Options.AddRange(options);
      FillEmptyItem = true;
    }
    public override void InstantiateControl(Control container)
    {
      DropDownList ddl = new DropDownList();
      ddl.CssClass = "form-control";
      ddl.ID = ControlId;
      if (!string.IsNullOrWhiteSpace(SelectSql))
      {
        DataTable dt = dbh.GetTable(SelectSql);
        ddl.DataSource = dt;
        if (dt.Columns.Count > 0)
          ddl.DataValueField = dt.Columns[0].ColumnName;
        if (dt.Columns.Count > 1)
          ddl.DataTextField = dt.Columns[1].ColumnName;
        ddl.DataBind();
      }
      ddl.Items.AddRange(Options.ToArray());
      if (FillEmptyItem && (ddl.Items.Count==0 || !string.IsNullOrWhiteSpace(ddl.Items[0].Value)))
        ddl.Items.Insert(0, "");

      if (ddl.Items.Count > 1)
        ddl.SelectedValue = DefaultValue + "";
      container.Controls.Add(ddl);
    }
    public override string GetCondition(Control container)
    {
      DropDownList ddl = container.FindControl(ControlId) as  DropDownList;
      if (string.IsNullOrWhiteSpace(ddl.SelectedValue)) return "";
      return string.Format(ConditionFormat, Name, ddl.SelectedValue);
    }
    public override string GetQueryParam(Control container)
    {
      throw new NotImplementedException();
    }
    //[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
    public List<ListItem> Options { get; set; }
    public string SelectSql { get; set; }
    public bool FillEmptyItem { get; set; }
  }
  public class TemplateQueryField : QueryField
  {
    private ITemplate _inputTemplate;
    public TemplateQueryField()
      : this("", "")
    { }
    public TemplateQueryField(string name, string label)
      : base(name, label)
    { }

    [PersistenceMode(PersistenceMode.InnerProperty)]
    public ITemplate InputTemplate
    {
      get { return _inputTemplate; }
      set { _inputTemplate = value; }
    }
    public override void InstantiateControl(Control container)
    {
      Control ctrlContainer = new Control();
      ctrlContainer.ID = ControlId + "Container";
      InputTemplate.InstantiateIn(ctrlContainer);

      container.Controls.Add(ctrlContainer);
    }
    public override void RenderControls(Control container, HtmlTextWriter writer)
    {
      container.FindControl(ControlId + "Container").RenderControl(writer);
    }
    public override string GetCondition(Control container)
    {
      Control ctrl = container.FindControl(ControlId + "Container") as Control;
      List<object> values = new List<object>();
      foreach (Control childCtrl in ctrl.Controls)
      {
        if (!InputControlAdapter.IsInputControl(childCtrl)) continue;
        InputControlAdapter adapter = InputControlAdapter.Create(childCtrl);
        values.Add(adapter.Value);
      }
      bool hasValue = false;
      foreach (object val in values)
      {
        if (string.IsNullOrWhiteSpace(val + "")) continue;
        hasValue = true;
        break;
      }
      values.Insert(0, Name);
      if (hasValue && values.Count >1) return string.Format(ConditionFormat, values.ToArray());
      return "";
    }
    public override string GetQueryParam(Control container)
    {
      throw new NotImplementedException();
    }
    public override void ClearInputControl(Control container)
    {
      Control ctrl = container.FindControl(ControlId + "Container");
      foreach (Control childCtrl in ctrl.Controls)
      {
        if (!InputControlAdapter.IsInputControl(childCtrl)) continue;
        InputControlAdapter adapter = InputControlAdapter.Create(childCtrl);
        adapter.Clear();
      }
    }
  }
  public class DateQueryField : QueryField
  {
    public DateQueryField() : this("", "") { }
    public DateQueryField(string name, string label, DateQueryType type = DateQueryType.Range) : base(name, label)
    {
      Type = type;

      if (type == DateQueryType.Single)
        ConditionFormat = " {0} between {1} and ({1}+1) ";
      else if (type == DateQueryType.Range)
        ConditionFormat = " {0} between {1} and {2} ";
      else if (type == DateQueryType.GreaterThen)
      {
        ConditionFormat = " {0} >= {1} ";
        ControlId = name + "_start";
      }
      else if (type == DateQueryType.LessThen)
      {
        ConditionFormat = " {0} <= {1} ";
        ControlId = name + "_end";
      }
    }
    public DateQueryType Type { get; set; }
    public override void InstantiateControl(Control container)
    {
      if (Type == DateQueryType.Single ||
          Type == DateQueryType.GreaterThen ||
          Type == DateQueryType.LessThen)
        InstantiateControlSingle(container);
      else if (Type == DateQueryType.Range)
        InstantiateControlRange(container);
    }
    public override void ClearInputControl(Control container)
    {
      if (Type == DateQueryType.Single ||
          Type == DateQueryType.GreaterThen ||
          Type == DateQueryType.LessThen)
      {
        (container.FindControl(ControlId) as DateTimeSelector).Value = null; 
      }
      else if (Type == DateQueryType.Range)
      {
        (container.FindControl(ControlId+"Start") as DateTimeSelector).Value = null; 
        (container.FindControl(ControlId+"End") as DateTimeSelector).Value = null; 
      }
    }
    public override string GetCondition(Control container)
    {
      if (Type == DateQueryType.Single ||
          Type == DateQueryType.GreaterThen ||
          Type == DateQueryType.LessThen)
        return GetConditionSingle(container);
      else if (Type == DateQueryType.Range)
        return GetConditionRange(container);
      return "";
    }
    public override string GetQueryParam(Control container)
    {
      throw new NotImplementedException();
    }
    void InstantiateControlSingle(Control container)
    {
      DateTimeSelector dtSelector = new DateTimeSelector();
      dtSelector.ID = ControlId;
      dtSelector.Value = (DateTime?)DefaultValue;
      container.Controls.Add(dtSelector);
    }
    string GetConditionSingle(Control container)
    {
      DateTimeSelector dts = container.FindControl(ControlId) as  DateTimeSelector;
      if (dts.Value.HasValue)
      {
        return string.Format(ConditionFormat, Name, 
          string.Format("to_date('{0} 00:00:00','yyyy-mm-dd hh24:mi:ss')", dts.Value.Value.ToShortDateString()));
      }
      return "";
    }

    void InstantiateControlRange(Control container)
    {
      Panel ltContainer = new Panel();
      ltContainer.CssClass = "input-group";
      ltContainer.ID = ControlId;

      DateTimeSelector dtSelectorStart = new DateTimeSelector();
      dtSelectorStart.ID = ControlId+"Start";
      ltContainer.Controls.Add(dtSelectorStart);

      Literal lt = new Literal();
      lt.Text = "<span class='input-group-addon'>至</span>";
      ltContainer.Controls.Add(lt);

      DateTimeSelector dtSelectorEnd = new DateTimeSelector();
      dtSelectorEnd.ID = ControlId+"End";
      ltContainer.Controls.Add(dtSelectorEnd);

      container.Controls.Add(ltContainer);
    }
    string GetConditionRange(Control container)
    {
      DateTimeSelector dtsStart = container.FindControl(ControlId + "Start") as  DateTimeSelector;
      DateTimeSelector dtsEnd = container.FindControl(ControlId + "End") as  DateTimeSelector;

      DateTime dtStart = dtsStart.Value.HasValue ? dtsStart.Value.Value : DateTime.MinValue;
      DateTime dtEnd = dtsEnd.Value.HasValue ? dtsEnd.Value.Value : DateTime.MaxValue;
      if (dtsStart.Value.HasValue || dtsEnd.Value.HasValue)
      {
        return string.Format(ConditionFormat, Name,
          string.Format("to_date('{0} 00:00:00','yyyy-mm-dd hh24:mi:ss')", dtStart.ToShortDateString()), 
          string.Format("to_date('{0} 00:00:00','yyyy-mm-dd hh24:mi:ss')", dtEnd.ToShortDateString()));
      }

      return "";
    }
  }
  public enum DateQueryType
  { Single, Range, GreaterThen, LessThen }
#endregion
}
