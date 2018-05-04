using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

/// <summary>
/// TreeNodeProvider 的摘要说明
/// </summary>
namespace Utils.WebControls
{
  public abstract class TreeDataProvider
  {
    public abstract TreeNode Root();
    public abstract void Populate(TreeNode node);
    public abstract object Id(TreeNode node);
  }
}