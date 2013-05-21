(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.YodleeAccountButtonView = (function(_super) {
    __extends(YodleeAccountButtonView, _super);

    function YodleeAccountButtonView() {
      _ref = YodleeAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    YodleeAccountButtonView.prototype.initialize = function() {
      return YodleeAccountButtonView.__super__.initialize.call(this, {
        name: 'Yodlee',
        logoText: '',
        shops: this.model
      });
    };

    YodleeAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return YodleeAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.YodleeAccountInfoView = (function(_super) {
    __extends(YodleeAccountInfoView, _super);

    function YodleeAccountInfoView() {
      _ref1 = YodleeAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    YodleeAccountInfoView.prototype.template = '#YodleeAccoutInfoTemplate';

    YodleeAccountInfoView.prototype.events = {
      'click a.back': 'back',
      'change input': 'inputChanged',
      'keyup input': 'inputChanged',
      'change input[name="Bank"]': 'bankChanged',
      'change input[type="radio"]': 'radioChanged',
      'click #yodleeContinueBtn': 'continueClicked'
    };

    YodleeAccountInfoView.prototype.initialize = function() {
      var that;

      that = this;
      window.AccountAdded = function(result) {
        EzBob.App.trigger('info', 'Congratulations. Yodlee account was added successfully.');
        that.trigger('completed');
        that.trigger('ready');
        return that.trigger('back');
      };
      return window.AccountAddingError = function(msg) {
        EzBob.App.trigger('error', msg);
        return that.trigger('back');
      };
    };

    YodleeAccountInfoView.prototype.radioChanged = function(el) {
      var checked, url;

      checked = this.$el.find("input[type='radio'][name!='Bank']:checked");
      if (checked.length > 0) {
        url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + (checked.val()) + "&bankName=" + (checked.attr('name'));
        this.$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled');
        console.log('remove');
      }
    };

    YodleeAccountInfoView.prototype.bankChanged = function() {
      var bank;

      this.$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked');
      this.$el.find(".SubBank:not([class*='hide'])").addClass('hide');
      bank = this.$el.find("input[type='radio'][name='Bank']:checked").val();
      this.$el.find("." + bank + "Container").removeClass('hide');
      console.log('add');
      return $("#yodleeContinueBtn:not([class*='disabled'])").addClass('disabled');
    };

    YodleeAccountInfoView.prototype.ui = {
      id: '#yodleeId',
      connect: 'a.connect-dag',
      form: 'form'
    };

    YodleeAccountInfoView.prototype.inputChanged = function() {
      var enabled;

      enabled = EzBob.Validation.checkForm(this.validator);
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    YodleeAccountInfoView.prototype.continueClicked = function() {
      if (this.$el.find('#yodleeContinueBtn').hasClass('disabled')) {
        return false;
      }
    };

    YodleeAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;

      if (!this.validator.form()) {
        return false;
      }
      if (this.$el.find('a.connect-yodlee').hasClass('disabled')) {
        return false;
      }
      acc = new EzBob.YodleeAccountModel({
        bankId: 1234,
        bankName: 'Santander'
      });
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', 'Yodlee Account Saving Error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        console.log(textStatus);
        return EzBob.App.trigger('error', 'Yodlee Account Saving Error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        _this.model.add(acc);
        EzBob.App.trigger('info', "Yodlee Account Added Successfully");
        _this.ui.mid.val("");
        _this.ui.vpnPassword.val("");
        _this.ui.remotePassword.val("");
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    YodleeAccountInfoView.prototype.render = function() {
      var oFieldStatusIcons;

      YodleeAccountInfoView.__super__.render.call(this);
      oFieldStatusIcons = $('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      this.validator = EzBob.validatePayPointShopForm(this.ui.form);
      return this;
    };

    YodleeAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    YodleeAccountInfoView.prototype.getDocumentTitle = function() {
      return "Link Yodlee Account";
    };

    YodleeAccountInfoView.prototype.AccountAdded = function() {
      return EzBob.App.trigger('ct:storebase.shop.connected');
    };

    return YodleeAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.YodleeAccountModel = (function(_super) {
    __extends(YodleeAccountModel, _super);

    function YodleeAccountModel() {
      _ref2 = YodleeAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    YodleeAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/YodleeMarketPlaces/Accounts";

    return YodleeAccountModel;

  })(Backbone.Model);

  EzBob.YodleeAccounts = (function(_super) {
    __extends(YodleeAccounts, _super);

    function YodleeAccounts() {
      _ref3 = YodleeAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    YodleeAccounts.prototype.model = EzBob.YodleeAccountModel;

    YodleeAccounts.prototype.url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/Accounts";

    return YodleeAccounts;

  })(Backbone.Collection);

}).call(this);
