(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4, _ref5, _ref6,
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
      'change input[name="Bank"]': 'bankChanged',
      'click #yodleeContinueBtn': 'continueClicked',
      'click .radio-fx': 'parentBankSelected',
      'change .SubBank': 'subBankSelectionChanged'
    };

    YodleeAccountInfoView.prototype.loadBanks = function() {
      var _this = this;

      return this.YodleeBanks.fetch().done(function() {
        if (_this.YodleeBanks.length > 0) {
          return _this.render;
        }
      });
    };

    YodleeAccountInfoView.prototype.initialize = function(options) {
      var that;

      that = this;
      this.YodleeBanks = new EzBob.YodleeBanks();
      this.loadBanks();
      window.YodleeAccountAdded = function(result) {
        if (result.error) {
          EzBob.App.trigger('error', result.error);
        } else {
          EzBob.App.trigger('info', 'Congratulations. Bank account was added successfully.');
        }
        that.trigger('completed');
        that.trigger('ready');
        return that.trigger('back');
      };
      window.YodleeAccountAddingError = function(msg) {
        EzBob.App.trigger('error', msg);
        return that.trigger('back');
      };
      window.YodleeAccountRetry = function() {
        that.attemptsLeft = that.attemptsLeft - 1;
        console.log(that.$el.find("#yodleeContinueBtn"));
        return {
          url: that.$el.find('#yodleeContinueBtn').attr('href'),
          attemptsLeft: that.attemptsLeft
        };
      };
      return false;
    };

    YodleeAccountInfoView.prototype.subBankSelectionChanged = function(el) {
      var url;

      if (this.$el.find(".SubBank option:selected").length === 0) {
        return false;
      }
      url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + (this.$el.find("option:selected").val()) + "&bankName=" + (this.$el.find("input[type='radio'][name='Bank']:checked").attr('value'));
      return this.$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled');
    };

    YodleeAccountInfoView.prototype.bankChanged = function() {
      var bank, currentSubBanks;

      this.$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked');
      currentSubBanks = this.$el.find(".SubBank:not([class*='hide'])");
      currentSubBanks.addClass('hide');
      this.$el.find("#subTypeHeader[class*='hide']").removeClass('hide');
      currentSubBanks.find('option').removeAttr('selected');
      bank = this.$el.find("input[type='radio'][name='Bank']:checked").val();
      this.$el.find("." + bank + "Container").removeClass('hide');
      return $("#yodleeContinueBtn:not([class*='disabled'])").addClass('disabled');
    };

    YodleeAccountInfoView.prototype.continueClicked = function(e) {
      var that, xhr,
        _this = this;

      if (this.$el.find('#yodleeContinueBtn').hasClass('disabled')) {
        return false;
      }
      e.preventDefault();
      xhr = $.post("" + window.gRootPath + "Customer/YodleeMarketPlaces/CheckYodleeUniqueness", {
        csId: this.$el.find("option:selected").val()
      });
      that = this;
      return xhr.done(function(result) {
        var win;

        if (result.error) {
          EzBob.App.trigger('error', result.error);
          return false;
        } else {
          that.attemptsLeft = 5;
          win = window.open(that.$el.find('#yodleeContinueBtn').attr('href'), '_blank');
          return win.focus();
        }
      });
    };

    YodleeAccountInfoView.prototype.parentBankSelected = function(evt) {
      evt.preventDefault();
      this.$el.find('#Bank_' + evt.currentTarget.id).click();
      this.$el.find('span.on').removeClass('on').addClass('off');
      $(evt.currentTarget).find('span.off').removeClass('off').addClass('on');
    };

    YodleeAccountInfoView.prototype.render = function() {
      YodleeAccountInfoView.__super__.render.call(this);
      return this;
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

  EzBob.YodleeUniqunessModel = (function(_super) {
    __extends(YodleeUniqunessModel, _super);

    function YodleeUniqunessModel() {
      _ref6 = YodleeUniqunessModel.__super__.constructor.apply(this, arguments);
      return _ref6;
    }

    YodleeUniqunessModel.prototype.urlRoot = "" + window.gRootPath + "Customer/YodleeMarketPlaces/CheckYodleeUniqueness";

    return YodleeUniqunessModel;

  })(Backbone.Model);

}).call(this);
