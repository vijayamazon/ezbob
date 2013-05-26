(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4, _ref5,
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
      return this.model.fetch().done(function() {
        return EzBob.App.trigger('ct:storebase.shop.connected');
      });
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
      'click #yodleeContinueBtn': 'continueClicked',
      'click .radio-fx': 'parentBankSelected',
      'click img': 'parentBankImageClicked',
      'change select': "subBankSelectionChanged"
    };

    YodleeAccountInfoView.prototype.initialize = function(options) {
      var that,
        _this = this;

      that = this;
      this.YodleeBanks = new EzBob.YodleeBanks();
      this.YodleeBanks.fetch().done(function() {
        if (_this.YodleeBanks.length > 0) {
          return _this.render;
        }
      });
      window.YodleeAccountAdded = function(result) {
        if (result.error) {
          EzBob.App.trigger('error', result.error);
        } else {
          EzBob.App.trigger('info', 'Congratulations. Yodlee account was added successfully.');
        }
        that.trigger('completed');
        that.trigger('ready');
        return that.trigger('back');
      };
      return window.AccountAddingError = function(msg) {
        EzBob.App.trigger('error', msg);
        return that.trigger('back');
      };
    };

    YodleeAccountInfoView.prototype.parentBankImageClicked = function(el) {
      var baseName, currentVal, img, inp;

      img = el.target;
      currentVal = img.getAttribute('class');
      baseName = '#Bank_' + currentVal.split(" ")[0];
      inp = this.$el.find(baseName);
      return inp.trigger('click');
    };

    YodleeAccountInfoView.prototype.subBankSelectionChanged = function(el) {
      var url;

      url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + (this.$el.find("option:selected").val()) + "&bankName=" + (this.$el.find("input[type='radio'][name='Bank']:checked").attr('value'));
      return this.$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled');
    };

    YodleeAccountInfoView.prototype.bankChanged = function() {
      var arr, baseName, currentSubBanks, currentVal, element, i, length;

      this.$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked');
      currentSubBanks = this.$el.find(".SubBank:not([class*='hide'])");
      currentSubBanks.addClass('hide');
      currentSubBanks.find('option').removeAttr('selected');
      arr = this.$el.find("input[type='radio'][name='Bank']:not(checked)").next();
      length = arr.length;
      i = 0;
      while (i < length) {
        currentVal = arr[i].getAttribute('class');
        baseName = currentVal.split(" ")[0];
        arr[i].setAttribute('class', arr[i].getAttribute('class').replace(baseName + '-on', baseName + '-off'));
        i++;
      }
      element = this.$el.find("input[type='radio'][name='Bank']:checked").next();
      currentVal = element.attr('class');
      baseName = currentVal.split(" ")[0];
      element.removeClass(baseName + '-off').addClass(baseName + '-on');
      this.$el.find("." + this.$el.find("input[type='radio'][name='Bank']:checked").attr('value') + "Container").removeClass('hide');
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

    YodleeAccountInfoView.prototype.parentBankSelected = function() {
      var $check, unique;

      $check = this.$el.prev(".BankContainer input:radio");
      unique = "." + this.className.split(" ")[1] + " span";
      $(unique).attr("class", "radio");
      this.$el.find("span").attr("class", "radio-checked");
      $check.attr("checked", true);
      return false;
    };

    YodleeAccountInfoView.prototype.serializeData = function() {
      return {
        YodleeBanks: this.YodleeBanks.toJSON()
      };
    };

    YodleeAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    YodleeAccountInfoView.prototype.getDocumentTitle = function() {
      return "Link Yodlee Account";
    };

    YodleeAccountInfoView.prototype.YodleeAccountAdded = function(model) {
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

  EzBob.YodleeBankModel = (function(_super) {
    __extends(YodleeBankModel, _super);

    function YodleeBankModel() {
      _ref4 = YodleeBankModel.__super__.constructor.apply(this, arguments);
      return _ref4;
    }

    YodleeBankModel.prototype.urlRoot = "" + window.gRootPath + "Customer/YodleeMarketPlaces/Banks";

    return YodleeBankModel;

  })(Backbone.Model);

  EzBob.YodleeBanks = (function(_super) {
    __extends(YodleeBanks, _super);

    function YodleeBanks() {
      _ref5 = YodleeBanks.__super__.constructor.apply(this, arguments);
      return _ref5;
    }

    YodleeBanks.prototype.model = EzBob.YodleeBankModel;

    YodleeBanks.prototype.url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/Banks";

    return YodleeBanks;

  })(Backbone.Collection);

}).call(this);
