(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.HMRCAccountInfoView = (function(_super) {
    __extends(HMRCAccountInfoView, _super);

    function HMRCAccountInfoView() {
      _ref = HMRCAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    HMRCAccountInfoView.prototype.events = {
      'change input': 'inputChanged',
      'keyup input': 'inputChanged',
      'click a.hmrcBack': 'back',
      'click div.hmrcUploadButton': 'uploadFiles',
      'click a.linkAccountBack': 'linkAccountBack',
      'click a.uploadFilesBack': 'uploadFilesBack',
      'click a.connect-account': 'connect',
      'click a.connect-account-help': 'connect',
      'click a.select-vat': 'selectVatFiles',
      'click #linkHelpButton': 'getLinkHelp',
      'click #uploadHelpButton': 'getUploadHelp',
      'click div.hmrcLinkButton': 'linkAccount'
    };

    HMRCAccountInfoView.prototype.initialize = function(options) {
      this.uploadFilesDlg = null;
      this.accountType = 'HMRC';
      this.template = '#' + this.accountType + 'AccountInfoTemplate';
      return this.activeForm = void 0;
    };

    HMRCAccountInfoView.prototype.render = function() {
      HMRCAccountInfoView.__super__.render.call(this);
      this.$el.find('#hmrcAccountUpload').dropzone();
      return this;
    };

    HMRCAccountInfoView.prototype.inputChanged = function() {
      var enabled;

      if (this.activeForm === void 0) {
        this.activeForm = this.$el.find('#hmrcLinkAccountForm');
        this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
      }
      enabled = EzBob.Validation.checkForm(this.validator);
      this.$el.find('a.connect-account').toggleClass('disabled', !enabled);
      return this.$el.find('a.connect-account-help').toggleClass('disabled', !enabled);
    };

    HMRCAccountInfoView.prototype.linkAccount = function() {
      this.activeForm = this.$el.find('#hmrcLinkAccountForm');
      this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
      this.$el.find('#linkAccountDiv').show();
      return this.$el.find('#initialDiv').hide();
    };

    HMRCAccountInfoView.prototype.uploadFiles = function() {
      this.$el.find('#uploadFilesDiv').show();
      return this.$el.find('#initialDiv').hide();
    };

    HMRCAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    HMRCAccountInfoView.prototype.linkAccountBack = function() {
      this.$el.find('#linkAccountDiv').hide();
      this.$el.find('#initialDiv').show();
      return false;
    };

    HMRCAccountInfoView.prototype.uploadFilesBack = function() {
      this.$el.find('#uploadFilesDiv').hide();
      this.$el.find('#initialDiv').show();
      return false;
    };

    HMRCAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return 'Link HMRC Account';
    };

    HMRCAccountInfoView.prototype.connect = function() {
      var acc, accountModel, xhr,
        _this = this;

      if (this.activeForm === void 0) {
        this.activeForm = this.$el.find('#hmrcLinkAccountForm');
        this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
      }
      if (!EzBob.Validation.checkForm(this.validator)) {
        this.validator.form();
        return false;
      }
      if (this.$el.find('a.connect-account').hasClass('disabled')) {
        return false;
      }
      accountModel = this.buildModel();
      if (!accountModel) {
        EzBob.App.trigger('error', 'HMRC Account Data Validation Error');
        return false;
      }
      acc = new EzBob.CGAccountModel(accountModel);
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', 'HMRC Account Saving Error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        return EzBob.App.trigger('error', 'Failed to Save HMRC Account');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        try {
          _this.model.add(acc);
        } catch (_error) {}
        EzBob.App.trigger('info', 'HMRC Account Added Successfully');
        _this.$el.find('#hmrc_user_id').val("");
        _this.$el.find('#hmrc_password').val("");
        _this.$el.find('#linkAccountDiv').hide();
        _this.$el.find('#initialDiv').show();
        _this.activeForm = _this.$el.find('#hmrcLinkAccountForm');
        _this.validator = EzBob.validateHmrcLinkForm(_this.activeForm);
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    HMRCAccountInfoView.prototype.buildModel = function() {
      var accountModel;

      accountModel = $.parseJSON($('div#cg-account-model-template').text());
      accountModel.accountTypeName = 'HMRC';
      accountModel['login'] = this.$el.find('#hmrc_user_id').val();
      accountModel['name'] = this.$el.find('#hmrc_user_id').val();
      accountModel['password'] = this.$el.find('#hmrc_password').val();
      delete accountModel.id;
      return accountModel;
    };

    HMRCAccountInfoView.prototype.selectVatFiles = function(evt) {
      var sKey, sModelKey,
        _this = this;

      evt.preventDefault();
      sKey = 'f' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000);
      sModelKey = 'model' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000);
      while (window[sKey]) {
        sKey += Math.floor(Math.random() * 1000);
      }
      while (window[sModelKey]) {
        sModelKey += Math.floor(Math.random() * 1000);
      }
      window[sModelKey] = function() {
        return _this.buildModel();
      };
      window[sKey] = function(sResult) {
        var oResult;

        delete window[sKey];
        delete window[sModelKey];
        _this.uploadFileDlg.dialog('close');
        _this.uploadFileDlg = null;
        oResult = JSON.parse(sResult);
        if (oResult.error) {
          EzBob.App.trigger('error', 'Problem Linking HMRC Account: ' + oResult.error.Data.error);
        } else {
          if (oResult.submitted) {
            EzBob.App.trigger('info', 'HMRC Account Added Successfully');
          }
        }
        _this.trigger('completed');
        return _this.trigger('back');
      };
      $('iframe', this.$el.find('div#upload-files-form')).each(function(idx, iframe) {
        iframe.setAttribute('width', 570);
        iframe.setAttribute('height', 515);
        return iframe.setAttribute('src', ("" + window.gRootPath + "Customer/CGMarketPlaces/UploadFilesDialog?key=") + sKey + "&handler=HandleUploadedHmrcVatReturn&modelkey=" + sModelKey);
      });
      this.uploadFileDlg = this.$el.find('div#upload-files-form').dialog({
        height: 600,
        width: 600,
        modal: true,
        title: 'Please upload the VAT returns',
        resizable: false,
        dialogClass: 'upload-files-dialog',
        closeOnEscape: false
      });
      return false;
    };

    HMRCAccountInfoView.prototype.getLinkHelp = function() {
      var oDialog;

      oDialog = $('#hmrcLinkHelpPopup');
      if (oDialog.length > 0) {
        return $.colorbox({
          inline: true,
          transition: 'none',
          open: true,
          href: oDialog,
          width: '20%'
        });
      }
    };

    HMRCAccountInfoView.prototype.getUploadHelp = function() {
      var oDialog;

      oDialog = $('#hmrcUploadHelpPopup');
      if (oDialog.length > 0) {
        return $.colorbox({
          inline: true,
          transition: 'none',
          open: true,
          href: oDialog,
          width: '35%'
        });
      }
    };

    return HMRCAccountInfoView;

  })(Backbone.Marionette.ItemView);

}).call(this);
