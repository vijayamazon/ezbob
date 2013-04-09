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
      var j, _i, _len, _ref1;

      this.ebayStores = this.model.get("ebayStores");
      this.amazonMarketplaces = this.model.get("amazonMarketplaces");
      this.EbayButtonView = new EzBob.EbayButtonView({
        model: this.ebayStores
      });
      this.EbayStoreView = new EzBob.EbayStoreInfoView();
      this.AmazonButtonView = new EzBob.AmazonButtonView({
        model: this.amazonMarketplaces
      });
      this.AmazonStoreInfoView = new EzBob.AmazonStoreInfoView();
      this.amazonMarketplaces.on("reset change", this.marketplacesChanged, this);
      this.ebayStores.on("reset change", this.marketplacesChanged, this);
      this.ekmAccounts = new EzBob.EKMAccounts();
      this.ekmAccounts.fetch();
      this.ekmButtonView = new EzBob.EKMAccountButtonView({
        model: this.ekmAccounts
      });
      this.EKMAccountInfoView = new EzBob.EKMAccountInfoView({
        model: this.ekmAccounts
      });
<<<<<<< HEAD
      this.PayPointAccounts = new EzBob.PayPointAccounts();
      this.PayPointAccounts.fetch();
      this.PayPointButtonView = new EzBob.PayPointAccountButtonView({
        model: this.PayPointAccounts
      });
      this.PayPointAccountInfoView = new EzBob.PayPointAccountInfoView({
        model: this.PayPointAccounts
=======
      this.volusionAccounts = new EzBob.VolusionAccounts();
      this.volusionAccounts.fetch();
      this.volusionButtonView = new EzBob.VolusionAccountButtonView({
        model: this.volusionAccounts
      });
      this.volusionAccountInfoView = new EzBob.VolusionAccountInfoView({
        model: this.volusionAccounts
>>>>>>> Volusion Connector - first pre-alfa
      });
      this.stores = {
        "eBay": {
          view: this.EbayStoreView,
          button: this.EbayButtonView,
          active: 0
        },
        "Amazon": {
          view: this.AmazonStoreInfoView,
          button: this.AmazonButtonView,
          active: 0
        },
        "EKM": {
          view: this.EKMAccountInfoView,
          button: this.ekmButtonView,
          active: 0
        },
<<<<<<< HEAD
        "PayPoint": {
          view: this.PayPointAccountInfoView,
          button: this.PayPointButtonView,
=======
        "Volusion": {
          view: this.volusionAccountInfoView,
          button: this.volusionButtonView,
>>>>>>> Volusion Connector - first pre-alfa
          active: 0
        }
      };
      _ref1 = EzBob.Config.ActiveMarketPlaces;
      for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
        j = _ref1[_i];
        if (this.stores[j]) {
          this.stores[j].active = 1;
        }
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
