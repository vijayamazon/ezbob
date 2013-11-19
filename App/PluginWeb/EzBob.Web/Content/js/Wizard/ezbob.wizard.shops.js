(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoStepModel = (function(_super) {

    __extends(StoreInfoStepModel, _super);

    function StoreInfoStepModel() {
      return StoreInfoStepModel.__super__.constructor.apply(this, arguments);
    }

    StoreInfoStepModel.prototype.url = "" + window.gRootPath + "Customer/MarketPlaces/Accounts";

    StoreInfoStepModel.prototype.initialize = function(options) {
      return this.set({
        isOffline: options.isOffline,
        isProfile: options.isProfile,
        stores: options.mpAccounts
      });
    };

    StoreInfoStepModel.prototype.getStores = function() {
      var mpAccounts, shop, stores, _i, _len;
      stores = [];
      mpAccounts = this.get('mpAccounts');
      if (mpAccounts) {
        for (_i = 0, _len = mpAccounts.length; _i < _len; _i++) {
          shop = mpAccounts[_i];
          if (shop.MpName === "Pay Pal") {
            shop.MpName = "paypal";
          }
          stores.push({
            displayName: shop.displayName,
            type: shop.MpName
          });
        }
      }
      return stores;
    };

    return StoreInfoStepModel;

  })(EzBob.WizardStepModel);

  EzBob.StoreInfoStepView = (function(_super) {

    __extends(StoreInfoStepView, _super);

    function StoreInfoStepView() {
      return StoreInfoStepView.__super__.constructor.apply(this, arguments);
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
      this.ready();
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
