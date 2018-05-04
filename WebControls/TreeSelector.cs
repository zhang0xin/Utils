using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Utils.WebControls
{
  public class TreeSelector : WebControlBase
  {
    TextBox _tbValue;
    LinkButton _btnSelect;
    LinkButton _btnClear;

    LinkButton _btnCancel;
    TreeView _tvData;

    bool showDialog = false;

    #region Properties
    TreeDataProvider provider;
    public TreeDataProvider TreeDataProvider
    {
      get
      {
        if (provider == null && !string.IsNullOrWhiteSpace(TreeDataProviderType))
        {
          Assembly asm = Assembly.Load(TreeDataProviderType.Split(',')[1]);
          ConstructorInfo constructor = asm.GetType(TreeDataProviderType.Split(',')[0]).GetConstructor(new Type[] { });
          provider = (TreeDataProvider)constructor.Invoke(new object[] { });
        }
        return provider;
      }
      set { provider = value; }
    }
    public string TreeDataProviderType
    {
      get
      {
        return ViewState["TreeDataProviderType"] + "";
      }
      set
      {
        ViewState["TreeDataProviderType"] = value;
      }
    }

    public string Value
    {
      get 
      {
        EnsureChildControls();
        if (_tvData.SelectedNode == null) return null;
        return TreeDataProvider.Id(_tvData.SelectedNode) + ""; 
      }
      set 
      {
        EnsureChildControls();
        if (!string.IsNullOrWhiteSpace(value)) 
          SelectNode(_tvData.Nodes[0], value); 
      }
    }
    bool SelectNode(TreeNode node, string id)
    {
      if (node.Value.Contains(id))
      {
        node.Select();
        _tbValue.Text = _tvData.SelectedNode.Text;
        return true;
      }
      else
      {
        node.Expand();
        foreach (TreeNode chidNode in node.ChildNodes)
        {
          if (SelectNode(chidNode, id)) return true;
        }
      }
      return false;
    }
    public string Text
    {
      get 
      { 
        EnsureChildControls();
        return _tbValue.Text; 
      }
    }
    //public bool WrappedWithInputGroup
    #endregion

    #region Events
    static object ValueChangedKey = new object();
    public event EventHandler ValueChanged
    {
      add { Events.AddHandler(ValueChangedKey, value); }
      remove { Events.RemoveHandler(ValueChangedKey, value); }
    }
    static object ValueClearedKey = new object();
    public event EventHandler ValueCleared
    {
      add { Events.AddHandler(ValueClearedKey, value); }
      remove { Events.RemoveHandler(ValueClearedKey, value); }
    }
    #endregion

    protected override void CreateChildControls()
    {
      Controls.Clear();

      _tbValue = new TextBox();
      _tbValue.CssClass = "form-control";
      _tbValue.ReadOnly = true;
      Controls.Add(_tbValue);

      _btnSelect = new LinkButton();
      _btnSelect.CssClass = "btn btn-default";
      _btnSelect.Text = "<span class='glyphicon glyphicon-search'></span>";
      _btnSelect.CommandName = "Select";
      _btnSelect.ToolTip = "查找";
      Controls.Add(_btnSelect);

      _btnClear = new LinkButton();
      _btnClear.CssClass = "btn btn-default";
      _btnClear.Text = "<span class='glyphicon glyphicon-remove'></span>";
      _btnClear.CommandName = "Clear";
      _btnClear.ToolTip = "清空";
      Controls.Add(_btnClear);


      _btnCancel = new LinkButton();
      _btnCancel.CssClass = "close";
      _btnCancel.Text = "×";
      _btnCancel.CommandName = "Close";
      Controls.Add(_btnCancel);

      _tvData = new TreeView();
      _tvData.ExpandDepth = 0;
      _tvData.SelectedNodeStyle.Font.Bold = true;
      InitTree();
      Controls.Add(_tvData);
    }
    public Literal CreateLiteral(string html)
    {
      Literal lt = new Literal();
      lt.Text = html;
      return lt;
    }
    public void InitTree()
    {
      _tvData.Nodes.Clear();
      if (TreeDataProvider != null)
      {
        _tvData.Nodes.Add(TreeDataProvider.Root());
        _tvData.TreeNodePopulate += delegate(object sender, TreeNodeEventArgs e)
        {
          TreeDataProvider.Populate(e.Node);
        };
        _tvData.SelectedNodeChanged += delegate(object sender, EventArgs e)
        {
          _tbValue.Text = _tvData.SelectedNode.Text;
          if (Events[ValueChangedKey] != null)
            (Events[ValueChangedKey] as EventHandler)(this, null);
          showDialog = false;
        };
      }
    }
    protected override bool OnBubbleEvent(object source, EventArgs args)
    {
      CommandEventArgs cea = args as CommandEventArgs;
      if (cea != null)
      {
        if (cea.CommandName == "Close")
        {
          showDialog= false;
        }
        else if (cea.CommandName == "Select")
        {
          showDialog = true;
        }
        else if (cea.CommandName == "Clear")
        {
          _tvData.SelectedNode.Selected = false;
          _tbValue.Text = "";
          if (Events[ValueClearedKey] != null)
            (Events[ValueClearedKey] as EventHandler)(this, null);
        }
      }
      return false;
    }
    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);
      if (showDialog)
      {
        Page.ClientScript.RegisterStartupScript(GetType(), "popup"+ClientID,
         @"$(function () { $('#popup_" + ClientID + "').modal({ keyboard: true })}); ", true);
      }
    }
    protected override void Render(HtmlTextWriter writer)
    {
      EnsureChildControls();
      writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group");
      writer.RenderBeginTag(HtmlTextWriterTag.Div);
      _tbValue.RenderControl(writer);

      writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group-btn");
      writer.RenderBeginTag(HtmlTextWriterTag.Span);
      _btnClear.RenderControl(writer);
      _btnSelect.RenderControl(writer);
      writer.RenderEndTag();
      writer.RenderEndTag();
      if (showDialog)
      {
        writer.AddAttribute(HtmlTextWriterAttribute.Id, "popup_" + ClientID);
        writer.AddAttribute(HtmlTextWriterAttribute.Class, "modal fade");
        writer.RenderBeginTag(HtmlTextWriterTag.Div);
        writer.AddAttribute(HtmlTextWriterAttribute.Class, "modal-dialog");
        writer.RenderBeginTag(HtmlTextWriterTag.Div);
        writer.AddAttribute(HtmlTextWriterAttribute.Class, "modal-content");
        writer.RenderBeginTag(HtmlTextWriterTag.Div);

        writer.AddAttribute(HtmlTextWriterAttribute.Class, "modal-header");
        writer.RenderBeginTag(HtmlTextWriterTag.Div);
        _btnCancel.RenderControl(writer);
        writer.AddAttribute(HtmlTextWriterAttribute.Class, "modal-title");
        writer.RenderBeginTag(HtmlTextWriterTag.H4);
        writer.WriteLine("请选择节点");
        writer.RenderEndTag();
        writer.RenderEndTag();

        writer.AddAttribute(HtmlTextWriterAttribute.Class, "modal-body");
        writer.RenderBeginTag(HtmlTextWriterTag.Div);
        _tvData.RenderControl(writer);
        writer.RenderEndTag();

        writer.RenderEndTag();
        writer.RenderEndTag();
        writer.RenderEndTag();
      }
    }
  }
}
