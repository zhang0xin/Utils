// namespace utils
var utils = window.utils || {};

// class ImageUpload
utils.ImageUpload = function(imgTag, srcTag, uploadTag) {
    this.imgId = imgTag;
    this.uploadId = uploadTag;
    this.srcId = srcTag;
};  
utils.ImageUpload.prototype.changeUpload = function (fileInput) {
    var reader = new FileReader();
    var self = this;
    reader.onload = function () {
        document.getElementById(self.imgId).src = reader.result;
    };
    reader.readAsDataURL(fileInput.files[0]); 
};
utils.ImageUpload.prototype.clearUpload = function() {
    document.getElementById(this.uploadId).value = '';
    document.getElementById(this.imgId).src =
        document.getElementById(this.srcId).value;
};

// class Popup
utils.Popup = function (contentId, maskId) {
    this.contentId = contentId;
    this.maskId = maskId;
}
utils.Popup.prototype.show = function() {
    document.getElementById(this.contentId).style.display = "block";
    document.getElementById(this.maskId).style.display = "block";
}
utils.Popup.prototype.hide = function() {
    document.getElementById(this.contentId).style.display = "none";
    document.getElementById(this.maskId).style.display = "none";
}
