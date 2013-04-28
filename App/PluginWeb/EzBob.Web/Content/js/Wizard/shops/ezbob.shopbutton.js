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
      "click .addAccount": "addAccount"
    };

    StoreButtonView.prototype.template = "#store-button-template";

    StoreButtonView.prototype.initialize = function(options) {
      this.name = options.name;
      this.logoText = options.logoText;
      this.shops = options.shops;
      if (this.shops) {
        this.shops.on("change reset", this.render, this);
      }
      return this.shopClass = options.name.toLowerCase().replace(' ', '');
    };

    StoreButtonView.prototype.serializeData = function() {
      var data;

      data = {
        name: this.name,
        logoText: this.logoText,
        shopClass: this.shopClass,
        shops: [],
        shopNames: ""
      };
      if (this.shops) {
        data.shops = this.shops.toJSON();
        data.shopNames = this.shops.pluck("displayName").join(", ");
      }
      return data;
    };

    StoreButtonView.prototype.onRender = function() {
      this.$el.find('.tooltipdiv').tooltip();
      return this.$el.find('.source_help').colorbox({
        inline: true
      });
    };

    StoreButtonView.prototype.addAccount = function() {
      console.log('continue');
      if (this.disabled) {
        console.log('continue dis');
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
      console.log('continue sel');
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
