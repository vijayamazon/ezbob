(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.MarketPlacesHistoryModel = (function(_super) {

    __extends(MarketPlacesHistoryModel, _super);

    function MarketPlacesHistoryModel() {
      return MarketPlacesHistoryModel.__super__.constructor.apply(this, arguments);
    }

    MarketPlacesHistoryModel.prototype.idAttribute = "Id";

    return MarketPlacesHistoryModel;

  })(Backbone.Model);

  EzBob.Underwriter.MarketPlacesHistory = (function(_super) {

    __extends(MarketPlacesHistory, _super);

    function MarketPlacesHistory() {
      return MarketPlacesHistory.__super__.constructor.apply(this, arguments);
    }

    MarketPlacesHistory.prototype.model = EzBob.Underwriter.MarketPlacesHistoryModel;

    MarketPlacesHistory.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/MarketPlaces/GetCustomerMarketplacesHistory/?customerId=" + this.customerId;
    };

    return MarketPlacesHistory;

  })(Backbone.Collection);

  EzBob.Underwriter.MarketPlacesHistoryView = (function(_super) {

    __extends(MarketPlacesHistoryView, _super);

    function MarketPlacesHistoryView() {
      return MarketPlacesHistoryView.__super__.constructor.apply(this, arguments);
    }

    MarketPlacesHistoryView.prototype.template = "#marketplace-history-template";

    MarketPlacesHistoryView.prototype.initialize = function(options) {
      this.model.on("reset change sync", this.render, this);
      return this.loadMarketPlacesHistory();
    };

    MarketPlacesHistoryView.prototype.loadMarketPlacesHistory = function() {
      var that,
        _this = this;
      that = this;
      return this.model.fetch().done(function() {
        if (that.model.length > 0) {
          return that.render();
        }
      });
    };

    MarketPlacesHistoryView.prototype.events = {
      "click .showHistoryMarketPlaces": "showHistoryMarketPlacesClicked",
      "click .showCurrentMarketPlaces": "showCurrentMarketPlacesClicked"
    };

    MarketPlacesHistoryView.prototype.serializeData = function() {
      return {
        MarketPlacesHistory: this.model
      };
    };

    MarketPlacesHistoryView.prototype.showHistoryMarketPlacesClicked = function() {
      var date;
      date = this.$el.find("#mpHistoryDdl :selected").val();
      return EzBob.App.vent.trigger('ct:marketplaces.history', date);
    };

    MarketPlacesHistoryView.prototype.showCurrentMarketPlacesClicked = function() {
      return EzBob.App.vent.trigger('ct:marketplaces.history', null);
    };

    return MarketPlacesHistoryView;

  })(Backbone.Marionette.ItemView);

}).call(this);
