(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.CompanyFilesAccountModel = (function(_super) {

    __extends(CompanyFilesAccountModel, _super);

    function CompanyFilesAccountModel() {
      return CompanyFilesAccountModel.__super__.constructor.apply(this, arguments);
    }

    CompanyFilesAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/CompanyFilesMarketPlaces/Accounts";

    return CompanyFilesAccountModel;

  })(Backbone.Model);

  EzBob.CompanyFilesAccounts = (function(_super) {

    __extends(CompanyFilesAccounts, _super);

    function CompanyFilesAccounts() {
      return CompanyFilesAccounts.__super__.constructor.apply(this, arguments);
    }

    CompanyFilesAccounts.prototype.model = EzBob.CompanyFilesAccountModel;

    CompanyFilesAccounts.prototype.url = "" + window.gRootPath + "Customer/CompanyFilesMarketPlaces/Accounts";

    return CompanyFilesAccounts;

  })(Backbone.Collection);

  EzBob.CompanyFilesAccountInfoView = (function(_super) {

    __extends(CompanyFilesAccountInfoView, _super);

    function CompanyFilesAccountInfoView() {
      return CompanyFilesAccountInfoView.__super__.constructor.apply(this, arguments);
    }

    CompanyFilesAccountInfoView.prototype.events = {
      'click a.hmrcBack': 'back',
      'click a.connect-account': 'connect'
    };

    CompanyFilesAccountInfoView.prototype.initialize = function(options) {
      this.accountType = 'CompanyFiles';
      this.template = '#' + this.accountType + 'AccountInfoTemplate';
      this.isOldInternetExplorer = 'Microsoft Internet Explorer' === navigator.appName && navigator.appVersion.indexOf("MSIE 1") === -1;
      return this.Dropzone = null;
    };

    CompanyFilesAccountInfoView.prototype.ui = {
      companyFilesUploadZone: "#companyFilesUploadZone",
      uploadButton: ".connect-account"
    };

    CompanyFilesAccountInfoView.prototype.render = function() {
      var that;
      CompanyFilesAccountInfoView.__super__.render.call(this);
      that = this;
      this.initDropzone();
      return this;
    };

    CompanyFilesAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    CompanyFilesAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return 'Upload Company Files';
    };

    CompanyFilesAccountInfoView.prototype.initDropzone = function() {
      var self;
      this.clearDropzone();
      Dropzone.options.customerFilesUploader = false;
      self = this;
      return this.Dropzone = new Dropzone(self.ui.companyFilesUploadZone[0], {
        init: function() {
          var oDropzone;
          oDropzone = this;
          ({
            maxFilesize: 10,
            maxFiles: 10,
            dictFileTooBig: "File is too big, max file size is 10MB"
          });
          oDropzone.on("success", function(oFile, oResponse) {
            var enabled;
            if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
              enabled = this.getAcceptedFiles() !== 0;
              self.ui.uploadButton.toggleClass('disabled', !enabled);
            }
            if (oResponse.success) {
              EzBob.App.trigger("info", "Upload successful: " + oFile.name);
            } else if (oResponse.error) {
              EzBob.App.trigger("error", oResponse.error);
            }
          });
          oDropzone.on("error", function(oFile, sErrorMsg, oXhr) {
            EzBob.App.trigger("error", "Error uploading " + oFile.name);
            oDropzone.removeFile(oFile);
          });
          return oDropzone.on("maxfilesexceeded", function(o) {
            EzBob.App.trigger("error", "You can upload up to 10 files");
          });
        }
      });
    };

    CompanyFilesAccountInfoView.prototype.clearDropzone = function() {
      if (this.Dropzone) {
        this.Dropzone.destroy();
        return this.Dropzone = null;
      }
    };

    CompanyFilesAccountInfoView.prototype.connect = function() {
      var that, xhr;
      if (this.ui.uploadButton.hasClass('disabled')) {
        return false;
      }
      that = this;
      BlockUi('on');
      xhr = $.post(window.gRootPath + "CompanyFilesMarketPlaces/Connect", {
        customerId: this.customerId
      });
      xhr.done(function(res) {
        if (res.error !== void 0) {
          return EzBob.App.trigger('error', 'Failed to upload company files');
        } else {
          return EzBob.App.trigger('info', 'Company files uploaded successfully');
        }
      });
      xhr.always(function() {
        return BlockUi('off');
      });
      this.trigger('completed');
      this.trigger('back');
      return false;
    };

    return CompanyFilesAccountInfoView;

  })(Backbone.Marionette.ItemView);

}).call(this);
