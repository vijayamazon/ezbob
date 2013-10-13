(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoView = (function(_super) {
    __extends(StoreInfoView, _super);

    function StoreInfoView() {
      _ref = StoreInfoView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StoreInfoView.prototype.attributes = {
      "class": "stores-view"
    };

    StoreInfoView.prototype.initialize = function() {
      var acc, accountTypeName, aryCGAccounts, ignore, j, lc, offlineMarketPlaces, storeTypeName, vendorInfo, _i, _j, _len, _len1, _ref1, _ref2,
        _this = this;

      this.ebayStores = this.model.get("ebayStores");
      this.EbayButtonView = new EzBob.EbayButtonView({
        model: this.ebayStores
      });
      this.EbayStoreView = new EzBob.EbayStoreInfoView();
      this.ebayStores.on("reset change", this.marketplacesChanged, this);
      this.ebayStores.on("sync", this.render, this);
      this.amazonMarketplaces = this.model.get("amazonMarketplaces");
      this.AmazonButtonView = new EzBob.AmazonButtonView({
        model: this.amazonMarketplaces
      });
      this.AmazonStoreInfoView = new EzBob.AmazonStoreInfoView();
      this.amazonMarketplaces.on("reset change", this.marketplacesChanged, this);
      this.amazonMarketplaces.on("sync", this.render, this);
      this.ekmAccounts = new EzBob.EKMAccounts();
      this.ekmAccounts.safeFetch().done(function() {
        return _this.render();
      });
      this.ekmButtonView = new EzBob.EKMAccountButtonView({
        model: this.ekmAccounts
      });
      this.EKMAccountInfoView = new EzBob.EKMAccountInfoView({
        model: this.ekmAccounts
      });
      this.freeAgentAccounts = new EzBob.FreeAgentAccounts();
      this.freeAgentAccounts.safeFetch().done(function() {
        return _this.render();
      });
      this.freeAgentButtonView = new EzBob.FreeAgentAccountButtonView({
        model: this.freeAgentAccounts
      });
      this.FreeAgentAccountInfoView = new EzBob.FreeAgentAccountInfoView({
        model: this.freeAgentAccounts
      });
      this.sageAccounts = new EzBob.SageAccounts();
      this.sageAccounts.safeFetch().done(function() {
        return _this.render();
      });
      this.sageButtonView = new EzBob.SageAccountButtonView({
        model: this.sageAccounts
      });
      this.sageAccountInfoView = new EzBob.SageAccountInfoView({
        model: this.sageAccounts
      });
      this.PayPointAccounts = new EzBob.PayPointAccounts();
      this.PayPointAccounts.safeFetch().done(function() {
        return _this.render();
      });
      this.PayPointButtonView = new EzBob.PayPointAccountButtonView({
        model: this.PayPointAccounts
      });
      this.PayPointAccountInfoView = new EzBob.PayPointAccountInfoView({
        model: this.PayPointAccounts
      });
      this.YodleeAccounts = new EzBob.YodleeAccounts();
      this.YodleeAccounts.safeFetch().done(function() {
        return _this.render();
      });
      this.YodleeButtonView = new EzBob.YodleeAccountButtonView({
        model: this.YodleeAccounts
      });
      this.YodleeAccountInfoView = new EzBob.YodleeAccountInfoView({
        model: this.YodleeAccounts
      });
      this.payPalAccounts = new EzBob.PayPalAccounts(this.model.get("paypalAccounts"));
      this.payPalAccounts.safeFetch().done(function() {
        return _this.render();
      });
      this.PayPalButtonView = new EzBob.PayPalButtonView({
        model: this.payPalAccounts
      });
      this.PayPalInfoView = new EzBob.PayPalInfoView({
        model: this.payPalAccounts
      });
      aryCGAccounts = $.parseJSON($('div#cg-account-list').text());
      for (accountTypeName in aryCGAccounts) {
        ignore = aryCGAccounts[accountTypeName];
        lc = accountTypeName.toLowerCase();
        acc = new EzBob.CGAccounts([], {
          accountType: accountTypeName
        });
        acc.safeFetch().done(function() {
          return _this.render();
        });
        this[lc + 'Accounts'] = acc;
        this[lc + 'ButtonView'] = new EzBob.CGAccountButtonView({
          model: acc,
          accountType: accountTypeName
        });
        this[lc + 'AccountInfoView'] = new EzBob.CGAccountInfoView({
          model: acc,
          accountType: accountTypeName
        });
      }
      this.stores = {
        "eBay": {
          view: this.EbayStoreView,
          button: this.EbayButtonView,
          active: 0,
          priority: 0
        },
        "Amazon": {
          view: this.AmazonStoreInfoView,
          button: this.AmazonButtonView,
          active: 0,
          priority: 1
        },
        "paypal": {
          view: this.PayPalInfoView,
          button: this.PayPalButtonView,
          active: 0,
          priority: 2
        },
        "EKM": {
          view: this.EKMAccountInfoView,
          button: this.ekmButtonView,
          active: 0,
          priority: 3
        },
        "PayPoint": {
          view: this.PayPointAccountInfoView,
          button: this.PayPointButtonView,
          active: 0,
          priority: 5
        },
        "Yodlee": {
          view: this.YodleeAccountInfoView,
          button: this.YodleeButtonView,
          active: 0,
          priority: 7
        },
        "FreeAgent": {
          view: this.FreeAgentAccountInfoView,
          button: this.freeAgentButtonView,
          active: 0,
          priority: 8
        },
        "Sage": {
          view: this.sageAccountInfoView,
          button: this.sageButtonView,
          active: 0,
          priority: 14
        }
      };
      for (accountTypeName in aryCGAccounts) {
        vendorInfo = aryCGAccounts[accountTypeName];
        lc = accountTypeName.toLowerCase();
        this.stores[accountTypeName] = {
          view: this[lc + 'AccountInfoView'],
          button: this[lc + 'ButtonView'],
          active: 0,
          priority: vendorInfo.ClientSide.SortPriority
        };
      }
      this.isOffline = this.model.get('isOffline');
      offlineMarketPlaces = {};
      _ref1 = EzBob.Config.OfflineMarketPlaces;
      for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
        j = _ref1[_i];
        offlineMarketPlaces[j] = j;
      }
      _ref2 = EzBob.Config.ActiveMarketPlaces;
      for (_j = 0, _len1 = _ref2.length; _j < _len1; _j++) {
        j = _ref2[_j];
        storeTypeName = j === "Pay Pal" ? "paypal" : j;
        if (this.stores[storeTypeName] && ((!this.isOffline) || offlineMarketPlaces[storeTypeName])) {
          this.stores[storeTypeName].active = 1;
        }
      }
      if (!this.isOffline && this.stores['HMRC']) {
        this.stores['HMRC'].active = 0;
      }
      this.name = "shops";
      return StoreInfoView.__super__.initialize.call(this);
    };

    StoreInfoView.prototype.render = function() {
      StoreInfoView.__super__.render.call(this);
      this.amazonMarketplaces.trigger("reset");
      this.ebayStores.trigger("reset");
      this.$el.find("img[rel]").setPopover("left");
      this.$el.find("li[rel]").setPopover("left");
      return this;
    };

    StoreInfoView.prototype.marketplacesChanged = function() {
      if (this.ebayStores.length > 0 || this.amazonMarketplaces.length > 0) {
        return this.$el.find(".wizard-top-notification h2").text("Add more shops to get more cash!");
      }
    };

    return StoreInfoView;

  })(EzBob.StoreInfoBaseView);

}).call(this);
