(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreButtonView = (function(_super) {

    __extends(StoreButtonView, _super);

    function StoreButtonView() {
      return StoreButtonView.__super__.constructor.apply(this, arguments);
    }

    StoreButtonView.prototype.attributes = {
      "class": "span6"
    };

    StoreButtonView.prototype.events = {
      "click .store-logo": "clicked"
    };

    StoreButtonView.prototype.template = "#store-button-template";

    StoreButtonView.prototype.initialize = function(options) {
      this.name = options.name;
      return this.logoText = options.logoText;
    };

    StoreButtonView.prototype.serializeData = function() {
      return {
        name: this.name,
        logoText: this.logoText
      };
    };

    StoreButtonView.prototype.clicked = function() {
      if (this.disabled) {
        if (this.disabledText) {
          EzBob.ShowMessage(this.disabledText);
        }
        return false;
      }
      if (this.name === "bank-account" && this.model.get("bankAccountAdded")) {
        return;
      }
      this.trigger("selected", this.name);
      return false;
    };

    StoreButtonView.prototype.update = function() {};

    return StoreButtonView;

  })(Backbone.Marionette.ItemView);

}).call(this);
