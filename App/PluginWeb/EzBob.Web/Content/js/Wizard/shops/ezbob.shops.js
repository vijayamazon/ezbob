(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoView = (function(_super) {

    __extends(StoreInfoView, _super);

    function StoreInfoView() {
      return StoreInfoView.__super__.constructor.apply(this, arguments);
    }

    StoreInfoView.prototype.attributes = {
      "class": "stores-view"
    };

    StoreInfoView.prototype.initialize = function() {
      var acc, accountTypeName, aryCGAccounts, ignore, j, lc, storeTypeName, vendorInfo, _i, _len, _ref,
        _this = this;
      this.ebayStores = new EzBob.EbayStoreModels();
      this.EbayButtonView = new EzBob.EbayButtonView({
        model: this.ebayStores
      });
      this.EbayStoreView = new EzBob.EbayStoreInfoView();
      this.ebayStores.on("reset change", this.marketplacesChanged, this);
      this.ebayStores.on("sync", this.render, this);
      this.amazonMarketplaces = new EzBob.AmazonStoreModels();
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
          button: this.EbayButtonView
        },
        "Amazon": {
          view: this.AmazonStoreInfoView,
          button: this.AmazonButtonView
        },
        "paypal": {
          view: this.PayPalInfoView,
          button: this.PayPalButtonView
        },
        "EKM": {
          view: this.EKMAccountInfoView,
          button: this.ekmButtonView
        },
        "PayPoint": {
          view: this.PayPointAccountInfoView,
          button: this.PayPointButtonView
        },
        "Yodlee": {
          view: this.YodleeAccountInfoView,
          button: this.YodleeButtonView
        },
        "FreeAgent": {
          view: this.FreeAgentAccountInfoView,
          button: this.freeAgentButtonView
        },
        "Sage": {
          view: this.sageAccountInfoView,
          button: this.sageButtonView
        }
      };
      for (accountTypeName in aryCGAccounts) {
        vendorInfo = aryCGAccounts[accountTypeName];
        lc = accountTypeName.toLowerCase();
        this.stores[accountTypeName] = {
          view: this[lc + 'AccountInfoView'],
          button: this[lc + 'ButtonView']
        };
      }
      this.isOffline = this.model.get('isOffline');
      this.isProfile = this.model.get('isProfile');
      _ref = EzBob.Config.MarketPlaces;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        j = _ref[_i];
        storeTypeName = j.Name === "Pay Pal" ? "paypal" : j.Name;
        if (this.stores[storeTypeName]) {
          this.stores[storeTypeName].active = this.isProfile ? (this.isOffline ? j.ActiveDashboardOffline : j.ActiveDashboardOnline) : (this.isOffline ? j.ActiveWizardOffline : j.ActiveWizardOnline);
          this.stores[storeTypeName].priority = this.isOffline ? j.PriorityOffline : j.PriorityOnline;
          this.stores[storeTypeName].ribbon = j.Ribbon ? j.Ribbon : "";
          this.stores[storeTypeName].button.ribbon = j.Ribbon ? j.Ribbon : "";
          this.stores[storeTypeName].mandatory = this.isOffline ? j.MandatoryOffline : j.MandatoryOnline;
        }
      }
      this.name = "shops";
      return StoreInfoView.__super__.initialize.call(this);
    };

    StoreInfoView.prototype.render = function() {
      StoreInfoView.__super__.render.call(this);
      console.log("EzBob.StoreInfoView render");
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
