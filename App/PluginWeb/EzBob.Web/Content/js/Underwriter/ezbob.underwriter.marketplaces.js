(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.MarketPlaceModel = (function(_super) {

    __extends(MarketPlaceModel, _super);

    function MarketPlaceModel() {
      return MarketPlaceModel.__super__.constructor.apply(this, arguments);
    }

    MarketPlaceModel.prototype.initialize = function() {
      this.on('change reset', this.recalculate, this);
      return this.recalculate();
    };

    MarketPlaceModel.prototype.recalculate = function() {
      var accountAge, age, ai, anualSales, inventory;
      ai = this.get('AnalysisDataInfo');
      accountAge = this.get('AccountAge');
      age = accountAge !== "-" && accountAge !== 'undefined' ? EzBob.SeniorityFormat(accountAge, 0) : "-";
      anualSales = (ai.TotalSumofOrders12M || ai.TotalSumofOrders6M || ai.TotalSumofOrders3M || ai.TotalSumofOrders1M || 0) * 1;
      inventory = !isNaN(ai.TotalValueofInventoryLifetime * 1) ? ai.TotalValueofInventoryLifetime * 1 : "-";
      return this.set({
        age: age,
        anualSales: anualSales,
        inventory: inventory
      }, {
        silent: true
      });
    };

    return MarketPlaceModel;

  })(Backbone.Model);

  EzBob.Underwriter.MarketPlaces = (function(_super) {

    __extends(MarketPlaces, _super);

    function MarketPlaces() {
      return MarketPlaces.__super__.constructor.apply(this, arguments);
    }

    MarketPlaces.prototype.model = EzBob.Underwriter.MarketPlaceModel;

    MarketPlaces.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/MarketPlaces/Index/" + this.customerId;
    };

    return MarketPlaces;

  })(Backbone.Collection);

  EzBob.Underwriter.MarketPlacesView = (function(_super) {

    __extends(MarketPlacesView, _super);

    function MarketPlacesView() {
      return MarketPlacesView.__super__.constructor.apply(this, arguments);
    }

    MarketPlacesView.prototype.template = "#marketplace-template";

    MarketPlacesView.prototype.initialize = function() {
      this.model.on("reset", this.render, this);
      return this.rendered = false;
    };

    MarketPlacesView.prototype.onRender = function() {
      this.$el.find('.mp-error-description').tooltip({
        placement: "bottom"
      });
      return this.$el.find('a[data-bug-type]').tooltip({
        title: 'Report bug'
      });
    };

    MarketPlacesView.prototype.events = {
      "click .reCheck-amazon": "reCheckmarketplaces",
      "click .reCheck-ebay": "reCheckmarketplaces",
      "click tbody tr": "rowClick",
      "click .mp-error-description": "showMPError",
      "click .renew-token": "renewTokenClicked"
    };

    MarketPlacesView.prototype.rowClick = function(e) {
      var id, shop, view;
      if (e.target.getAttribute('href')) {
        return;
      }
      if (e.target.tagName === 'I') {
        return;
      }
      id = e.currentTarget.getAttribute("data-id");
      if (!id) {
        return;
      }
      shop = this.model.at(id);
      if (shop.get('Name') === 'EKM') {
        return;
      }
      view = new EzBob.Underwriter.MarketPlaceDetailsView({
        el: this.$el.find('#marketplace-details'),
        model: shop,
        customerId: this.customerId
      });
      view.on("reCheck", this.reCheckmarketplaces);
      view.on("recheck-token", this.renewToken);
      view.customerId = this.model.customerId;
      return view.render();
    };

    MarketPlacesView.prototype.showMPError = function() {
      return false;
    };

    MarketPlacesView.prototype.serializeData = function() {
      var data, m, total, _i, _len, _ref;
      data = {
        customerId: this.model.customerId,
        marketplaces: this.model.toJSON(),
        summary: {
          anualSales: 0,
          inventory: 0,
          positive: 0,
          negative: 0,
          neutral: 0
        }
      };
      _ref = data.marketplaces;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        m = _ref[_i];
        data.summary.anualSales += m.anualSales;
        data.summary.inventory += m.inventory;
        data.summary.positive += m.PositiveFeedbacks;
        data.summary.negative += m.NegativeFeedbacks;
        data.summary.neutral += m.NeutralFeedbacks;
      }
      total = data.summary.positive + data.summary.negative + data.summary.neutral;
      data.summary.rating = total > 0 ? data.summary.positive / total : 0;
      return data;
    };

    MarketPlacesView.prototype.reCheckmarketplaces = function(e) {
      var that;
      that = this;
      EzBob.ShowMessage("", "Are you sure?", (function() {
        var el;
        el = $(e.currentTarget);
        return $.post(window.gRootPath + "Underwriter/MarketPlaces/ReCheckMarketplaces", {
          customerId: that.model.customerId,
          umi: el.attr("umi"),
          marketplaceType: el.attr("marketplaceType")
        }).done(function() {
          EzBob.ShowMessage("Wait a few minutes", "The marketplace recheck was starting. ", null, "OK");
          return that.trigger("rechecked", {
            umi: el.attr("umi"),
            el: el
          });
        }).fail(function(data) {
          return console.error(data.responseText);
        });
      }), "Yes", null, "No");
      return false;
    };

    MarketPlacesView.prototype.renewTokenClicked = function(e) {
      var umi;
      umi = $(e.currentTarget).data("umi");
      this.renewToken(umi);
      return false;
    };

    MarketPlacesView.prototype.renewToken = function(umi) {
      var req;
      req = $.post("" + window.gRootPath + "Underwriter/MarketPlaces/RenewEbayToken", {
        umi: umi
      });
      return req.done(function() {
        return EzBob.ShowMessage("Renew started successfully", "Successfully");
      });
    };

    return MarketPlacesView;

  })(Backbone.Marionette.ItemView);

}).call(this);
