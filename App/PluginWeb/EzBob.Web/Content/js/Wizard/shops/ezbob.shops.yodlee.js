(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4,
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
      'click .radio-fx': 'parentBankSelected',
      'change .SubBank': 'subBankSelectionChanged',
      'click #yodleeLinkAccountBtn': 'linkAccountClicked',
      'click #OtherYodleeBanks': 'OtherYodleeBanksClicked',
      'change #OtherYodleeBanks': 'OtherYodleeBanksClicked'
    };

    YodleeAccountInfoView.prototype.loadBanks = function() {
      var _this = this;

      return this.YodleeBanks.safeFetch().done(function() {
        if (_this.YodleeBanks.length > 0) {
          return _this.render;
        }
      });
    };

    YodleeAccountInfoView.prototype.initialize = function(options) {
      var that;

      that = this;
      this.YodleeBanks = new EzBob.YodleeBanksModel();
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
        that.attemptsLeft = (that.attemptsLeft || 5) - 1;
        return {
          url: that.$el.find('#yodleeContinueBtn').attr('href'),
          attemptsLeft: that.attemptsLeft
        };
      };
      return false;
    };

    YodleeAccountInfoView.prototype.OtherYodleeBanksClicked = function(el) {
      var selectedId, selectedName, url;

      selectedId = $(el.currentTarget).find(':selected').val();
      selectedName = $(el.currentTarget).find(':selected').text();
      if (selectedId) {
        this.$el.find("input[type='radio'][name='Bank']:checked").removeAttr('checked');
        this.$el.find(".SubBank:not(.hide)").addClass('hide');
        this.$el.find("a.radio-fx .on").addClass('off').removeClass('on');
        url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + selectedId + "&bankName=" + selectedName;
        this.$el.find("#yodleeContinueBtn").attr("href", url);
        return this.$el.find("#yodleeLinkAccountBtn").removeClass('disabled');
      } else {
        return this.$el.find("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled');
      }
    };

    YodleeAccountInfoView.prototype.subBankSelectionChanged = function(el) {
      var url;

      if (this.$el.find(".SubBank option:selected").length === 0) {
        return false;
      }
      url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + (this.$el.find("option:selected").val()) + "&bankName=" + (this.$el.find("input[type='radio'][name='Bank']:checked").attr('value'));
      this.$el.find("#yodleeContinueBtn").attr("href", url);
      return this.$el.find("#yodleeLinkAccountBtn").removeClass('disabled');
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
      return $("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled');
    };

    YodleeAccountInfoView.prototype.linkAccountClicked = function() {
      if (this.$el.find('#yodleeLinkAccountBtn').hasClass('disabled')) {
        return false;
      }
      return this.$el.find('.yodlee_help').colorbox({
        inline: true,
        transition: 'none'
      });
    };

    YodleeAccountInfoView.prototype.parentBankSelected = function(evt) {
      evt.preventDefault();
      this.$el.find('#Bank_' + evt.currentTarget.id).click();
      this.$el.find('span.on').removeClass('on').addClass('off');
      $(evt.currentTarget).find('span.off').removeClass('off').addClass('on');
      this.$el.find(".SubBank:not(.hide) option:selected").prop('selected', false);
      this.$el.find("#OtherYodleeBanks option").eq(0).prop('selected', true);
      this.$el.find("#OtherYodleeBanks").change();
    };

    YodleeAccountInfoView.prototype.render = function() {
      YodleeAccountInfoView.__super__.render.call(this);
      $.colorbox.close();
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
      EzBob.App.trigger('clear');
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

  EzBob.YodleeBanksModel = (function(_super) {
    __extends(YodleeBanksModel, _super);

    function YodleeBanksModel() {
      _ref4 = YodleeBanksModel.__super__.constructor.apply(this, arguments);
      return _ref4;
    }

    YodleeBanksModel.prototype.urlRoot = "" + window.gRootPath + "Customer/YodleeMarketPlaces/Banks";

    return YodleeBanksModel;

  })(Backbone.Model);

}).call(this);
