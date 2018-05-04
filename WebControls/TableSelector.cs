using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utils.Database;

namespace Utils.WebControls
{
  [ValidationProperty("Value")]
  public class TableSelector : WebControlBase
  {
    DBHelper dbh = new DBHelper();
    TextBox _tbText;
    HiddenField _hfValue;
    LinkButton _btnSelect;
    LinkButton _btnClear;
    GridView _gvMain;
    LinkButton _btnCancel;
    Button _btnQuery;
    bool showDialog
    {
      get { return ViewState["showDialog"] == null ? false : (bool)ViewState["showDialog"]; }
      set { ViewState["showDialog"] = value; }
    }

    #region Properties
    public string ValueField
    {
      get { EnsureChildControls(); return _gvMain.DataKeyNames[0]; }
      set
      {
        EnsureChildControls();
        if (_gvMain.DataKeyNames == null || _gvMain.DataKeyNames.Length == 0)
          _gvMain.DataKeyNames = new string[] { value };
        else
          _gvMain.DataKeyNames[0] = value;
      }
    }
    public string TextField
    {
      get { EnsureChildControls(); return _gvMain.DataKeyNames[1]; }
      set
      {
        EnsureChildControls();
        if (_gvMain.DataKeyNames == null || _gvMain.DataKeyNames.Length == 0)
          _gvMain.DataKeyNames = new string[] { "", value };
        else if (_gvMain.DataKeyNames.Length == 1)
          _gvMain.DataKeyNames = new string[] { _gvMain.DataKeyNames[0], value };
        else
          _gvMain.DataKeyNames[1] = value;
      }
    }
    public string QueryFields
    {
      get { return ViewState["QueryFields"] + ""; }
      set { ViewState["QueryFields"] = value; }
    }
    public string SelectSql 
    {
      get { return ViewState["SelectSql"] + ""; }
      set { ViewState["SelectSql"] = value; }
    }
    public string Value
    {
      get { EnsureChildControls(); return _hfValue.Value; }
      set
      {
        EnsureChildControls();
        DataTable dt = dbh.GetTable(SelectSql);
        DataRow[] rows = dt.Select(ValueField + " = '" + value+"'");
        if (rows.Length > 0)
        {
          _hfValue.Value = rows[0][ValueField]+"";
          if (string.IsNullOrWhiteSpace(TextField))
            _tbText.Text = _hfValue.Value;
          else
            _tbText.Text = rows[0][TextField] + "";
        }
      }
    }
    public string Text
    {
      get { EnsureChildControls(); return _tbText.Text; }
    }

    public object DataSource
    {
      get { EnsureChildControls(); return _gvMain.DataSource; }
      set { EnsureChildControls(); _gvMain.DataSource = value; }
    }
    #endregion

    #region Events
    static object ValueChangedKey = new object();
    public event EventHandler ValueChanged
    {
      add { Events.AddHandler(ValueChangedKey, value); }
      remove { Events.RemoveHandler(ValueChangedKey, value); }
    }
    public void OnValueChanged()
    {
      if (Events[ValueChangedKey] != null)
        (Events[ValueChangedKey] as EventHandler)(this, null);
    }
    #endregion
    
    public void BindGrid()
    {
      _gvMain.DataSource = dbh.GetTable(
        dbh.WrapSqlWithWhere(SelectSql, GetCondition()));
      _gvMain.DataBind();
    }

    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);
      if (showDialog)
      {
        Page.ClientScript.RegisterStartupScript(GetType(), "popup"+ClientID,
         @"$(function () { $('#popup_" + ClientID + "').modal({ keyboard: true })}); ", true);
      }
      BindGrid();
    }
    protected override void Render(HtmlTextWriter writer)
    {
      writer.RenderWithClass("div", "input-group", delegate()
      {
        _tbText.RenderControl(writer);
        _hfValue.RenderControl(writer);
        writer.RenderWithClass("span", "input-group-btn", delegate()
        {
          _btnClear.RenderControl(writer);
          _btnSelect.RenderControl(writer);
        });
      });

      if (showDialog)
      {
        writer.RenderWithIdClass("div", "popup_" + ClientID, "modal", delegate()
        {
          writer.RenderWithClass("div", "modal-dialog", delegate()
          {
            writer.RenderWithClass("div", "modal-content", delegate()
            {
              writer.RenderWithClass("div", "modal-header", delegate()
              {
                _btnCancel.RenderControl(writer);
                writer.RenderWithClass("div", "modal-title", delegate()
                {
                  writer.Write("请选择项目");
                });
              });
              writer.RenderWithClass("div", "modal-body", delegate()
              {
                writer.RenderWithClass("div", "", delegate()
                {
                  RenderQuerInputs(writer);
                  writer.RenderWithClass("div", "col-sm-offset-10 col-sm-2", delegate()
                  {
                    _btnQuery.RenderControl(writer);
                  });
                  _gvMain.RenderControl(writer);
                });
              });
            });
          });
        });
      }
    }
    void RenderQuerInputs(HtmlTextWriter writer)
    {
      int index = 0;
      QueryFieldsEach(delegate(string field)
      {
        writer.RenderWithClass("div", "col-sm-6", delegate()
        {
          writer.Render("label", delegate()
          {
            writer.Write(field + "：");
          });
          FindControl("tb_" + field).RenderControl(writer);
        });
      });
    }

    protected override void CreateChildControls()
    {
      Controls.Clear();

      _tbText = new TextBox();
      _tbText.CssClass = "form-control";
      Controls.Add(_tbText);

      _hfValue = new HiddenField();
      Controls.Add(_hfValue);

      _btnClear = new LinkButton();
      _btnClear.CssClass = "btn btn-default";
      _btnClear.CausesValidation = false;
      _btnClear.Text = "<span class='glyphicon glyphicon-remove'></span>";
      _btnClear.Click += delegate(object sender, EventArgs args)
      {
        _tbText.Text = "";
        _hfValue.Value = "";
      };
      Controls.Add(_btnClear);

      _btnSelect = new LinkButton();
      _btnSelect.CssClass = "btn btn-default";
      _btnSelect.CausesValidation = false;
      _btnSelect.Text = "<span class='glyphicon glyphicon-search'></span>";
      _btnSelect.Click += delegate(object sender, EventArgs e)
      {
        showDialog = true;
      };
      Controls.Add(_btnSelect);

      _btnCancel = new LinkButton();
      _btnCancel.CssClass = "close";
      _btnCancel.Text = "×";      
      _btnCancel.CausesValidation = false;
      _btnCancel.Click += delegate(object sender, EventArgs e)
      {
        showDialog = false;
      }; 
      Controls.Add(_btnCancel);

      _gvMain = new GridView();
      _gvMain.CssClass = "table";
      _gvMain.GridLines = GridLines.None;
      _gvMain.AutoGenerateSelectButton = true;
      _gvMain.DataKeyNames = new string[] { };
      _gvMain.AllowPaging = true;
      _gvMain.PageSize = 10;
      _gvMain.ShowHeaderWhenEmpty = true;
      _gvMain.PageIndexChanging += delegate(object sender, GridViewPageEventArgs e)
      {
        _gvMain.PageIndex = e.NewPageIndex;
      };
      _gvMain.SelectedIndexChanged += delegate(object sender, EventArgs e)
      {
        string selectedValue = _gvMain.DataKeys[_gvMain.SelectedIndex][0].ToString();
        _hfValue.Value = selectedValue;
        if (_gvMain.DataKeys[_gvMain.SelectedIndex].Values.Count > 1)
          _tbText.Text = _gvMain.DataKeys[_gvMain.SelectedIndex][1].ToString();
        else
          _tbText.Text = selectedValue;
        OnValueChanged();
        showDialog = false;
      };
      Controls.Add(_gvMain);

      QueryFieldsEach(delegate(string field)
      {
        TextBox tb = new TextBox();
        tb.ID = "tb_" + field;
        tb.CssClass = "form-control";
        Controls.Add(tb);
      });

      _btnQuery = new Button();
      _btnQuery.Text = "查询";
      _btnQuery.CausesValidation = false;
      _btnQuery.CssClass = "btn btn-default";
      _btnQuery.Click += delegate(object sender, EventArgs args)
      {
        //BindGrid();
      };
      Controls.Add(_btnQuery);
    }

    void QueryFieldsEach(Action<string> doEach)
    {
      string[] fields = QueryFields.Split(',');
      foreach (string field in fields)
      {
        if (doEach != null) doEach(field.Trim());
      }
    }
    string GetCondition()
    {
      string condition = "";
      QueryFieldsEach(delegate(string field) 
      {
        string value = (FindControl("tb_" + field) as TextBox).Text;
        if (string.IsNullOrWhiteSpace(value)) return;
        condition += field + " like '%" + value + "%' and ";
      });
      if (!string.IsNullOrWhiteSpace(condition))
        condition = condition.Substring(0, condition.Length - 5);
      return condition;
    }

  }

  public delegate void LoadDataEventHandler(object sender, LoadDataEventArgs e);
  public class LoadDataEventArgs : EventArgs
  {
    public DataTable Data { get; set; }
    public string Condition { get; set; }
    public string ConditionWithWhere
    {
      get { return string.IsNullOrWhiteSpace(Condition) ? "" : " where " + Condition + " "; }
    }
  }
}
