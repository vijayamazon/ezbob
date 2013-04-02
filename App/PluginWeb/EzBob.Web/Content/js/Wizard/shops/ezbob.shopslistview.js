(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreListView = (function(_super) {

    __extends(StoreListView, _super);

    function StoreListView() {
      return StoreListView.__super__.constructor.apply(this, arguments);
    }

    StoreListView.prototype.template = "#ebay-store-list";

    StoreListView.prototype.serializeData = function() {
      return {
        stores: this.model.toJSON()
      };
    };

    StoreListView.prototype.initialize = function() {
      return this.model.on("all", this.render, this);
    };

    return StoreListView;

  })(Backbone.Marionette.ItemView);

}).call(this);
