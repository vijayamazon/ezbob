(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.CGAccountButtonView = (function(_super) {

    __extends(CGAccountButtonView, _super);

    function CGAccountButtonView() {
      return CGAccountButtonView.__super__.constructor.apply(this, arguments);
    }

    CGAccountButtonView.prototype.initialize = function(options) {
      return CGAccountButtonView.__super__.initialize.call(this, {
        name: options.accountType,
        logoText: '',
        shops: this.model
      });
    };

    CGAccountButtonView.prototype.update = function() {
      return this.model.fetch().done(function() {
        return EzBob.App.trigger('ct:storebase.shop.connected');
      });
    };

    return CGAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.CGAccountInfoView = (function(_super) {

    __extends(CGAccountInfoView, _super);

    function CGAccountInfoView() {
      this.render = __bind(this.render, this);

      this.uploadFiles = __bind(this.uploadFiles, this);

      this.connect = __bind(this.connect, this);

      this.buildModel = __bind(this.buildModel, this);

      this.inputChanged = __bind(this.inputChanged, this);
      return CGAccountInfoView.__super__.constructor.apply(this, arguments);
    }

    CGAccountInfoView.prototype.events = {
      'click a.back': 'back',
      'change input': 'inputChanged',
      'keyup input': 'inputChanged',
      'click .upload-files': 'uploadFiles'
    };

    CGAccountInfoView.prototype.initialize = function(options) {
      this.uploadFilesDlg = null;
      this.accountType = options.accountType;
      return this.template = '#' + this.accountType + 'AccountInfoTemplate';
    };

    CGAccountInfoView.prototype.inputChanged = function() {
      var enabled;
      enabled = EzBob.Validation.checkForm(this.validator);
      return this.$el.find('a.connect-account').toggleClass('disabled', !enabled);
    };

    CGAccountInfoView.prototype.getVendorInfo = function() {
      var lst;
      if (!this.vendorInfo) {
        lst = $.parseJSON($('div#cg-account-list').text());
        this.vendorInfo = lst[this.accountType];
      }
      return this.vendorInfo;
    };

    CGAccountInfoView.prototype.buildModel = function(bUploadMode) {
      var accountModel, elm, fi, func, oVendorInfo, propName, propVal, _i, _len, _ref;
      accountModel = $.parseJSON($('div#cg-account-model-template').text());
      oVendorInfo = this.getVendorInfo();
      accountModel.accountTypeName = this.accountType;
      _ref = oVendorInfo.SecurityData.Fields;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        fi = _ref[_i];
        if (fi.Default) {
          accountModel[fi.NodeName] = fi.Default;
        }
      }
      for (propName in accountModel) {
        propVal = accountModel[propName];
        elm = this.$el.find('#' + this.accountType.toLowerCase() + '_' + propName.toLowerCase());
        if (elm.length > 0) {
          accountModel[propName] = elm.val();
        }
      }
      if (oVendorInfo.ClientSide.LinkForm.OnBeforeLink.length) {
        func = new Function('accountModel', 'bUploadMode', oVendorInfo.ClientSide.LinkForm.OnBeforeLink.join("\n"));
        accountModel = func.call(null, accountModel, bUploadMode);
        if (!accountModel) {
          return null;
        }
      }
      delete accountModel.id;
      return accountModel;
    };

    CGAccountInfoView.prototype.connect = function() {
      var acc, accountModel, oVendorInfo, xhr,
        _this = this;
      if (!EzBob.Validation.checkForm(this.validator)) {
        this.validator.form();
        return false;
      }
      if (this.$el.find('a.connect-account').hasClass('disabled')) {
        return false;
      }
      accountModel = this.buildModel(false);
      oVendorInfo = this.getVendorInfo();
      if (!accountModel) {
        EzBob.App.trigger('error', oVendorInfo.DisplayName + ' Account Data Validation Error');
        return false;
      }
      acc = new EzBob.CGAccountModel(accountModel);
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', oVendorInfo.DisplayName + ' Account Saving Error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        return EzBob.App.trigger('error', 'Failed to Save ' + oVendorInfo.DisplayName + ' Account');
      });
      xhr.done(function(res) {
        var elm, propName, propVal;
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        try {
          _this.model.add(acc);
        } catch (_error) {}
        EzBob.App.trigger('info', oVendorInfo.DisplayName + ' Account Added Successfully');
        for (propName in accountModel) {
          propVal = accountModel[propName];
          elm = _this.$el.find('#' + _this.accountType.toLowerCase() + '_' + propName.toLowerCase());
          if (elm.length > 0) {
            elm.val("");
          }
        }
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    CGAccountInfoView.prototype.uploadFiles = function() {
      var oVendorInfo, sKey, sModelKey,
        _this = this;
      sKey = 'f' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000);
      sModelKey = 'model' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000);
      while (window[sKey]) {
        sKey += Math.floor(Math.random() * 1000);
      }
      while (window[sModelKey]) {
        sModelKey += Math.floor(Math.random() * 1000);
      }
      oVendorInfo = this.getVendorInfo();
      window[sModelKey] = function() {
        return _this.buildModel(true);
      };
      window[sKey] = function(sResult) {
        var oResult;
        delete window[sKey];
        delete window[sModelKey];
        _this.uploadFileDlg.dialog('close');
        _this.uploadFileDlg = null;
        oResult = JSON.parse(sResult);
        if (oResult.error) {
          EzBob.App.trigger('error', 'Problem Linking ' + oVendorInfo.DisplayName + ' Account: ' + oResult.error.Data.error);
        } else {
          if (oResult.submitted) {
            EzBob.App.trigger('info', oVendorInfo.DisplayName + ' Account Added Successfully');
          }
        }
        _this.trigger('completed');
        return _this.trigger('back');
      };
      $('iframe', this.$el.find('div#upload-files-form')).each(function(idx, iframe) {
        iframe.setAttribute('width', 570);
        iframe.setAttribute('height', 515);
        return iframe.setAttribute('src', ("" + window.gRootPath + "Customer/CGMarketPlaces/UploadFilesDialog?key=") + sKey + "&handler=" + oVendorInfo.ClientSide.LinkForm.UploadFilesHandler + '&modelkey=' + sModelKey);
      });
      return this.uploadFileDlg = this.$el.find('div#upload-files-form').dialog({
        height: 600,
        width: 600,
        modal: true,
        title: 'Please upload the VAT returns',
        resizable: false,
        dialogClass: 'upload-files-dialog',
        closeOnEscape: false
      });
    };

    CGAccountInfoView.prototype.render = function() {
      var self;
      CGAccountInfoView.__super__.render.call(this);
      self = this;
      this.$el.find('a.connect-account').click(function(evt) {
        return self.connect();
      });
      this.validator = EzBob.validateCGShopForm(this.$el.find('form'), this.accountType);
      return this;
    };

    CGAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    CGAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return 'Link ' + this.getVendorInfo().DisplayName + ' Account';
    };

    return CGAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.CGAccountModel = (function(_super) {

    __extends(CGAccountModel, _super);

    function CGAccountModel() {
      return CGAccountModel.__super__.constructor.apply(this, arguments);
    }

    CGAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/CGMarketPlaces/Accounts";

    return CGAccountModel;

  })(Backbone.Model);

  EzBob.CGAccounts = (function(_super) {

    __extends(CGAccounts, _super);

    function CGAccounts() {
      return CGAccounts.__super__.constructor.apply(this, arguments);
    }

    CGAccounts.prototype.model = EzBob.CGAccountModel;

    CGAccounts.prototype.accountType = '';

    CGAccounts.prototype.url = function() {
      return ("" + window.gRootPath + "Customer/CGMarketPlaces/Accounts?atn=") + this.accountType;
    };

    CGAccounts.prototype.initialize = function(data, options) {
      return this.accountType = options.accountType;
    };

    return CGAccounts;

  })(Backbone.Collection);

}).call(this);
