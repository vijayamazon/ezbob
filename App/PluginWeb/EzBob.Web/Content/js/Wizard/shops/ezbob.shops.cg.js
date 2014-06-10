(function() {
  var root,
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.CgAccountInfoView = (function(_super) {

    __extends(CgAccountInfoView, _super);

    function CgAccountInfoView() {
      this.render = __bind(this.render, this);

      this.connect = __bind(this.connect, this);

      this.buildModel = __bind(this.buildModel, this);

      this.inputChanged = __bind(this.inputChanged, this);
      return CgAccountInfoView.__super__.constructor.apply(this, arguments);
    }

    CgAccountInfoView.prototype.events = {
      'click a.back': 'back',
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    CgAccountInfoView.prototype.initialize = function(options) {
      this.uploadFilesDlg = null;
      this.accountType = options.accountType;
      return this.template = '#' + this.accountType + 'AccountInfoTemplate';
    };

    CgAccountInfoView.prototype.inputChanged = function() {
      var enabled;
      enabled = EzBob.Validation.checkForm(this.validator);
      return this.$el.find('a.connect-account').toggleClass('disabled', !enabled);
    };

    CgAccountInfoView.prototype.getVendorInfo = function() {
      if (!this.vendorInfo) {
        this.vendorInfo = EzBob.CgVendors.pure()[this.accountType];
      }
      return this.vendorInfo;
    };

    CgAccountInfoView.prototype.buildModel = function(bUploadMode) {
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

    CgAccountInfoView.prototype.connect = function() {
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
      acc = new EzBob.CgAccountModel(accountModel);
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

    CgAccountInfoView.prototype.render = function() {
      var self;
      CgAccountInfoView.__super__.render.call(this);
      self = this;
      this.$el.find('a.connect-account').click(function(evt) {
        return self.connect();
      });
      this.validator = EzBob.validateCGShopForm(this.$el.find('form'), this.accountType);
      EzBob.UiAction.registerView(this);
      return this;
    };

    CgAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    CgAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return 'Link ' + this.getVendorInfo().DisplayName + ' Account';
    };

    return CgAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.CgAccountModel = (function(_super) {

    __extends(CgAccountModel, _super);

    function CgAccountModel() {
      return CgAccountModel.__super__.constructor.apply(this, arguments);
    }

    CgAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/CGMarketPlaces/Accounts";

    return CgAccountModel;

  })(Backbone.Model);

  EzBob.CgAccounts = (function(_super) {

    __extends(CgAccounts, _super);

    function CgAccounts() {
      return CgAccounts.__super__.constructor.apply(this, arguments);
    }

    CgAccounts.prototype.model = EzBob.CgAccountModel;

    CgAccounts.prototype.accountType = '';

    CgAccounts.prototype.url = function() {
      return ("" + window.gRootPath + "Customer/CGMarketPlaces/Accounts?atn=") + this.accountType;
    };

    CgAccounts.prototype.initialize = function(data, options) {
      return this.accountType = options.accountType;
    };

    return CgAccounts;

  })(Backbone.Collection);

}).call(this);
