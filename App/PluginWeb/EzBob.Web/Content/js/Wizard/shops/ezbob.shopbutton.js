(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreButtonView = (function(_super) {
    __extends(StoreButtonView, _super);

    function StoreButtonView() {
      _ref = StoreButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StoreButtonView.prototype.events = {
      "click .button": "clicked"
    };

    StoreButtonView.prototype.template = "#store-button-template";

    StoreButtonView.prototype.initialize = function(options) {
      this.name = options.name;
      this.logoText = options.logoText;
      this.shops = options.shops;
      this.shopNames = "";
      if (this.shops) {
        this.shops.on("change reset", this.updateShopNames, this);
      }
      return this.shopClass = options.name.toLowerCase().replace(' ', '');
    };

    StoreButtonView.prototype.serializeData = function() {
      return {
        name: this.name,
        logoText: this.logoText,
        shopClass: this.shopClass,
        shops: this.shops ? this.shops.toJSON() : [],
        shopNames: this.shopNames
      };
    };

    StoreButtonView.prototype.updateShopNames = function() {
      var s;

      if (this.shops) {
        s = "";
        _.each(this.shops.models, function(sh, idx) {
          if (s !== "") {
            s += ", ";
          }
          return s += sh.attributes.displayName;
        });
        this.shopNames = s;
      }
      return this.render;
    };

    StoreButtonView.prototype.clicked = function() {
      if (this.disabled) {
        if (this.disabledText) {
          EzBob.ShowMessage(this.disabledText);
        }
        return false;
      }
      if (!this.isAddingAllowed()) {
        return false;
      }
      if (this.name === "bank-account" && this.model.get("bankAccountAdded")) {
        return;
      }
      this.trigger("selected", this.name);
      return false;
    };

    StoreButtonView.prototype.isAddingAllowed = function() {
      return true;
    };

    StoreButtonView.prototype.update = function() {};

    return StoreButtonView;

  })(Backbone.Marionette.ItemView);

}).call(this);
