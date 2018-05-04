using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Utils.WebControls
{
  public class ImageUpload : WebControlBase, INamingContainer
  {
    #region Properties
    public string ImageUrl
    {
      get 
      {
        EnsureChildControls();
        return _imgImage.ImageUrl;
      }
      set
      {
        EnsureChildControls();
        _imgImage.ImageUrl = value;
      }
    }
    public Unit ImageWidth
    {
      get
      {
        EnsureChildControls();
        return _imgImage.Width;
      }
      set
      {
        _imgImage.Width = value;
      }
    }
    public Unit ImageHeight
    {
      get
      {
        EnsureChildControls();
        return _imgImage.Height;
      }
      set
      {
        _imgImage.Height = value;
      }
    }
    public string StoreDirectory
    {
      get 
      {
        return ViewState["StoreDirectory"] == null? "~/UploadImages/" : (string)ViewState["StoreDirectory"];
      }
      set
      {
        ViewState["StoreDirectory"] = value;
      }
    }
    public bool HasUploadImage
    {
      get
      {
        EnsureChildControls();
        return _fuImage.HasFile;
      }
    }
    public string Accept
    {
      get
      {
        EnsureChildControls();
        return _fuImage.Attributes["accept"];
      }
      set
      {
        EnsureChildControls();
        _fuImage.Attributes["accept"] = value;
      }
    }
    #endregion

    FileUpload _fuImage;
    TextBox _tbFileName;
    HyperLink _hlSelect;
    HyperLink _hlCancel;
    Image _imgImage;

    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);

      string imgUrl = string.IsNullOrWhiteSpace(ImageUrl)? "":VirtualPathUtility.ToAbsolute(ImageUrl);
      string script = string.Format(@"
        function {0}_changeUpload(fileInput) {{
            var reader = new FileReader();
            reader.onload = function () {{
                $('#{1}').attr('src', reader.result);
            }};
            reader.readAsDataURL(fileInput.files[0]);
            $('#{2}').val(fileInput.value);
        }}
        function {0}_clearUpload() {{
            $('#{2}').val('');
            $('#{3}').val('');
            $('#{1}').attr('src', '{4}');
        }};
      ", 
        ClientID,  _imgImage.ClientID, _tbFileName.ClientID, _fuImage.ClientID, imgUrl);
      Page.ClientScript.RegisterClientScriptBlock(GetType(), "imageuploadscript_"+this.ID, script, true);
    }
    protected override void CreateChildControls()
    {
      Controls.Clear();
      _fuImage = new FileUpload();
      _fuImage.Style.Add(HtmlTextWriterStyle.Display, "none");
      _fuImage.Attributes.Add("onchange", string.Format("{0}_changeUpload(this)", this.ClientID));
      Controls.Add(_fuImage);

      _tbFileName = new TextBox();
      _tbFileName.CssClass = "form-control";
      Controls.Add(_tbFileName);

      _hlSelect = new HyperLink();
      _hlSelect.ID = "select";
      _hlSelect.CssClass = "btn btn-default";
      _hlSelect.Text = "<span class='glyphicon glyphicon-search'></span>";
      _hlSelect.NavigateUrl = string.Format("javascript:$('input[id={0}]').click();", _fuImage.ClientID);
      Controls.Add(_hlSelect);

      _hlCancel = new HyperLink();
      _hlCancel.ID = "cancel";
      _hlCancel.CssClass = "btn btn-default";
      _hlCancel.Text = "<span class='glyphicon glyphicon-share-alt'></span>";
      _hlCancel.NavigateUrl = string.Format("javascript:{0}_clearUpload()", this.ClientID);
      Controls.Add(_hlCancel);

      _imgImage = new Image();
      _imgImage.ID = "image";
      Controls.Add(_imgImage);
    }
    protected override void Render(HtmlTextWriter writer)
    {
      EnsureChildControls();
      writer.RendererWithClass("div", "input-group").Render( delegate() 
      {
        _fuImage.RenderControl(writer);
        _tbFileName.RenderControl(writer);
        writer.RendererWithClass("span", "input-group-btn").Render(delegate()
        {
          _hlCancel.RenderControl(writer);
          _hlSelect.RenderControl(writer);
        });
      });
      _imgImage.RenderControl(writer);
    }

    public string UploadImage(string imageId = null, bool returnImageUrl = false)
    {
      if (!HasUploadImage) return null;

      string id = string.IsNullOrWhiteSpace(imageId)? Guid.NewGuid().ToString() : imageId;
      string newPicName = id + Path.GetExtension(_fuImage.FileName);
      string newPicUrl = Path.Combine(StoreDirectory, newPicName);
      string newPicFilePath = HttpContext.Current.Server.MapPath(newPicUrl);
      string oldPicFilePath = HttpContext.Current.Server.MapPath(_imgImage.ImageUrl);

      string diskStoreDir = HttpContext.Current.Server.MapPath(StoreDirectory);
      if (!Directory.Exists(diskStoreDir)) Directory.CreateDirectory(diskStoreDir);
      if (File.Exists(oldPicFilePath)) File.Delete(oldPicFilePath);
      _fuImage.SaveAs(newPicFilePath);
      _imgImage.ImageUrl = newPicUrl;

      if (returnImageUrl) return newPicUrl;
      return newPicName;
    }
  }
}
