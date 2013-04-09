(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoStepModel = (function(_super) {
    __extends(StoreInfoStepModel, _super);

    function StoreInfoStepModel() {
      _ref = StoreInfoStepModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StoreInfoStepModel.prototype.initialize = function(options) {
      return this.set({
        ebayStores: new EzBob.EbayStoreModels(options.ebayMarketPlaces),
        amazonMarketplaces: new EzBob.AmazonStoreModels(options.amazonMarketPlaces)
      });
    };

    StoreInfoStepModel.prototype.getStores = function() {
      var amazons, ebays, ekms, payPoints, shop, stores, _i, _j, _k, _l, _len, _len1, _len2, _len3;

      stores = [];
      ebays = this.get("ebayStores").toJSON();
      amazons = this.get("amazonMarketplaces").toJSON();
      ekms = this.get("ekmShops");
      payPoints = this.get("payPointAccounts");
      for (_i = 0, _len = ebays.length; _i < _len; _i++) {
        shop = ebays[_i];
        stores.push({
          displayName: shop.displayName,
          type: "Ebay"
        });
      }
      for (_j = 0, _len1 = amazons.length; _j < _len1; _j++) {
        shop = amazons[_j];
        stores.push({
          displayName: shop.displayName,
          type: "Amazon"
        });
      }
      for (_k = 0, _len2 = ekms.length; _k < _len2; _k++) {
        shop = ekms[_k];
        stores.push({
          displayName: shop.displayName,
          type: "EKM"
        });
      }
      for (_l = 0, _len3 = payPoints.length; _l < _len3; _l++) {
        shop = payPoints[_l];
        stores.push({
          displayName: shop.displayName,
          type: "PayPoint"
        });
      }
      return stores;
    };

    return StoreInfoStepModel;

  })(EzBob.WizardStepModel);

  EzBob.StoreInfoStepView = (function(_super) {
    __extends(StoreInfoStepView, _super);

    function StoreInfoStepView() {
      _ref1 = StoreInfoStepView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    StoreInfoStepView.prototype.initialize = function() {
      this.StoreInfoView = new EzBob.StoreInfoView({
        model: this.model
      });
      this.StoreInfoView.on("completed", this.completed, this);
      this.StoreInfoView.on("ready", this.ready, this);
      this.StoreInfoView.on("next", this.next, this);
      return this.StoreInfoView.on("previous", this.previous, this);
    };

    StoreInfoStepView.prototype.completed = function() {
      return this.trigger("completed");
    };

    StoreInfoStepView.prototype.ready = function() {
      return this.trigger("ready");
    };

    StoreInfoStepView.prototype.next = function() {
      return this.trigger("next");
    };

    StoreInfoStepView.prototype.previous = function() {
      return this.trigger("previous");
    };

    StoreInfoStepView.prototype.render = function() {
      this.StoreInfoView.render().$el.appendTo(this.$el);
      return this;
    };

    return StoreInfoStepView;

  })(Backbone.View);

}).call(this);
