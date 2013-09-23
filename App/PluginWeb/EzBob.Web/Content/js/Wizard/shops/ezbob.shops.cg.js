(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.CGAccountButtonView = (function(_super) {
    __extends(CGAccountButtonView, _super);

    function CGAccountButtonView() {
      _ref = CGAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    CGAccountButtonView.prototype.initialize = function(options) {
      return CGAccountButtonView.__super__.initialize.call(this, {
        name: options.accountType,
        logoText: '',
        shops: this.model
      });
    };

    CGAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return CGAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.CGAccountInfoView = (function(_super) {
    __extends(CGAccountInfoView, _super);

    function CGAccountInfoView() {
      this.render = __bind(this.render, this);
      this.connect = __bind(this.connect, this);
      this.inputChanged = __bind(this.inputChanged, this);      _ref1 = CGAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    CGAccountInfoView.prototype.events = {
      'click a.back': 'back',
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    CGAccountInfoView.prototype.initialize = function(options) {
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

    CGAccountInfoView.prototype.connect = function() {
      var acc, accountModel, elm, fi, func, propName, propVal, vendorInfo, xhr, _i, _len, _ref2,
        _this = this;

      if (!EzBob.Validation.checkForm(this.validator)) {
        this.validator.form();
        return false;
      }
      if (this.$el.find('a.connect-account').hasClass('disabled')) {
        return false;
      }
      accountModel = $.parseJSON($('div#cg-account-model-template').text());
      vendorInfo = this.getVendorInfo();
      accountModel.accountTypeName = this.accountType;
      _ref2 = vendorInfo.SecurityData.Fields;
      for (_i = 0, _len = _ref2.length; _i < _len; _i++) {
        fi = _ref2[_i];
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
      if (vendorInfo.ClientSide.LinkForm.OnBeforeLink.length) {
        func = new Function('accountModel', vendorInfo.ClientSide.LinkForm.OnBeforeLink.join("\n"));
        accountModel = func.call(null, accountModel);
        if (!accountModel) {
          EzBob.App.trigger('error', vendorInfo.DisplayName + ' Account Data Validation Error');
          return false;
        }
      }
      delete accountModel.id;
      acc = new EzBob.CGAccountModel(accountModel);
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', vendorInfo.DisplayName + ' Account Saving Error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        return EzBob.App.trigger('error', 'Failed to Save ' + vendorInfo.DisplayName + ' Account');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        try {
          _this.model.add(acc);
        } catch (_error) {}
        EzBob.App.trigger('info', vendorInfo.DisplayName + ' Account Added Successfully');
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

    CGAccountInfoView.prototype.render = function() {
      var oFieldStatusIcons, self;

      CGAccountInfoView.__super__.render.call(this);
      self = this;
      this.$el.find('a.connect-account').click(function(evt) {
        return self.connect();
      });
      oFieldStatusIcons = $('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
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
      _ref2 = CGAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    CGAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/CGMarketPlaces/Accounts";

    return CGAccountModel;

  })(Backbone.Model);

  EzBob.CGAccounts = (function(_super) {
    __extends(CGAccounts, _super);

    function CGAccounts() {
      _ref3 = CGAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
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
