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

    MarketPlaceModel.prototype.idAttribute = "Id";

    MarketPlaceModel.prototype.initialize = function() {
      this.on('change reset', this.recalculate, this);
      return this.recalculate();
    };

    MarketPlaceModel.prototype.recalculate = function() {
      var accountAge, age, ai, anualSales, inventory, monthAnnualizedSales, monthSales, pp;
      ai = this.get('AnalysisDataInfo');
      accountAge = this.get('AccountAge');
      monthSales = ai ? (ai.TotalSumofOrders1M || 0) * 1 : 0;
      monthAnnualizedSales = ai ? (ai.TotalSumofOrdersAnnualized1M || 0) * 1 : 0;
      anualSales = ai ? (ai.TotalSumofOrders12M || ai.TotalSumofOrders6M || ai.TotalSumofOrders3M || ai.TotalSumofOrders1M || 0) * 1 : 0;
      inventory = ai && !isNaN(ai.TotalValueofInventoryLifetime * 1) ? ai.TotalValueofInventoryLifetime * 1 : "-";
      pp = this.get("PayPal");
      if (pp) {
        monthSales = pp.GeneralInfo.MonthInPayments;
        monthAnnualizedSales = pp.GeneralInfo.MonthInPaymentsAnnualized;
        anualSales = pp.GeneralInfo.TotalNetInPayments;
      }
      age = accountAge !== "-" && accountAge !== 'undefined' ? EzBob.SeniorityFormat(accountAge, 0) : "-";
      return this.set({
        age: age,
        monthSales: monthSales,
        monthAnnualizedSales: monthAnnualizedSales,
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
      return "" + window.gRootPath + "Underwriter/MarketPlaces/Index/?id=" + this.customerId + "&history=" + this.history;
    };

    return MarketPlaces;

  })(Backbone.Collection);

  EzBob.Underwriter.Affordability = (function(_super) {

    __extends(Affordability, _super);

    function Affordability() {
      return Affordability.__super__.constructor.apply(this, arguments);
    }

    Affordability.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/MarketPlaces/GetAffordabilityData/?id=" + this.customerId;
    };

    return Affordability;

  })(Backbone.Model);

  EzBob.Underwriter.MarketPlacesView = (function(_super) {

    __extends(MarketPlacesView, _super);

    function MarketPlacesView() {
      return MarketPlacesView.__super__.constructor.apply(this, arguments);
    }

    MarketPlacesView.prototype.template = "#marketplace-template";

    MarketPlacesView.prototype.initialize = function() {
      var _this = this;
      this.model.on("reset change sync", this.render, this);
      this.rendered = false;
      window.YodleeTryRecheck = function(result) {
        if (result.error) {
          return EzBob.ShowMessage(result.error, "Yodlee Recheck Error", "OK");
        } else {
          return EzBob.ShowMessage('Yodlee recheked successfully, refresh the page', null, "OK");
        }
      };
      EzBob.App.vent.on('ct:marketplaces.history', function() {
        return _this.$el.find('#hmrc-upload-container').hide().empty();
      });
      EzBob.App.vent.on('ct:marketplaces.uploadHmrc', function() {
        var oUploader, uploadHmrcView;
        oUploader = $('<div class="box-content"></div>');
        _this.$el.find('#hmrc-upload-container').empty().append(oUploader);
        uploadHmrcView = new EzBob.Underwriter.UploadHmrcView({
          el: oUploader,
          customerId: _this.model.customerId,
          companyRefNum: _this.options.personalInfoModel.get('CompanyExperianRefNum')
        });
        uploadHmrcView.render();
        _this.$el.find('#hmrc-upload-container').show();
        return $(".mps-tables").hide();
      });
      EzBob.App.vent.on('ct:marketplaces.uploadHmrcBack', function() {
        $(".mps-tables").show();
        return _this.$el.find('#hmrc-upload-container').hide().empty();
      });
      EzBob.App.vent.on('ct:marketplaces.enterHmrc', function() {
        return EzBob.Underwriter.EnterHmrcView.execute(_this.model.customerId, _this.model);
      });
      EzBob.App.vent.on('ct:marketplaces.parseYodlee', function() {
        return _this.parseYodlee();
      });
      EzBob.App.vent.on('ct:marketplaces.parseYodleeBack', function() {
        _this.$el.find(".mps-tables").show();
        return _this.$el.find('#parse-yodlee-container').hide().empty();
      });
      EzBob.App.vent.on('ct:marketplaces.addedFile', function() {
        return _this.model.fetch().done(function() {
          return _this.parseYodlee();
        });
      });
      return this;
    };

    MarketPlacesView.prototype.onRender = function() {
      var marketplacesHistoryDiv;
      this.$el.find('.mp-error-description').tooltip({
        placement: "bottom"
      });
      this.$el.find('a[data-bug-type]').tooltip({
        title: 'Report bug'
      });
      _.each(this.$el.find('[data-original-title]'), function(elem) {
        return $(elem).tooltip({
          title: elem.getAttribute('data-original-title')
        });
      });
      if (this.detailView) {
        this.detailView.render();
      }
      marketplacesHistoryDiv = this.$el.find("#marketplaces-history");
      this.marketPlacesHistory = new EzBob.Underwriter.MarketPlacesHistory();
      this.marketPlacesHistory.customerId = this.model.customerId;
      this.marketPlacesHistory.silent = true;
      this.marketPlaceHistoryView = new EzBob.Underwriter.MarketPlacesHistoryView({
        model: this.marketPlacesHistory,
        el: marketplacesHistoryDiv,
        customerId: this.model.customerId
      });
      return this;
    };

    MarketPlacesView.prototype.events = {
      "click .tryRecheckYodlee": "tryRecheckYodlee",
      "click .reCheckMP": "reCheckmarketplaces",
      "click tbody tr": "rowClick",
      "click .mp-error-description": "showMPError",
      "click .renew-token": "renewTokenClicked",
      "click .disable-shop": "disableShop",
      "click .enable-shop": "enableShop"
    };

    MarketPlacesView.prototype.rowClick = function(e) {
      var id, shop;
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
      shop = this.model.get(id);
      this.detailView = new EzBob.Underwriter.MarketPlaceDetailsView({
        model: this.model,
        currentId: id,
        customerId: this.model.customerId,
        personalInfoModel: this.options.personalInfoModel
      });
      EzBob.App.jqmodal.show(this.detailView);
      this.detailView.on("reCheck", this.reCheckmarketplaces, this);
      this.detailView.on("disable-shop", this.disableShop, this);
      this.detailView.on("enable-shop", this.enableShop, this);
      this.detailView.on("recheck-token", this.renewToken);
      this.detailView.customerId = this.model.customerId;
      return this.detailView.render();
    };

    MarketPlacesView.prototype.showMPError = function() {
      return false;
    };

    MarketPlacesView.prototype.serializeData = function() {
      var data, isMarketplace, m, total, _i, _len, _ref;
      isMarketplace = function(x) {
        var cg;
        if (!(EzBob.CgVendors.all()[x.get('Name')])) {
          return !x.get('IsPaymentAccount');
        }
        cg = EzBob.CgVendors.all()[x.get('Name')];
        return (cg.Behaviour === 0) && !cg.HasExpenses;
      };
      data = {
        customerId: this.model.customerId,
        marketplaces: _.sortBy(_.pluck(_.filter(this.model.models, function(x) {
          return x && isMarketplace(x);
        }), "attributes"), "UWPriority"),
        accounts: _.sortBy(_.pluck(_.filter(this.model.models, function(x) {
          return x && !isMarketplace(x);
        }), "attributes"), "UWPriority"),
        hideAccounts: false,
        hideMarketplaces: false,
        summary: {
          monthSales: 0,
          anualSales: 0,
          inventory: 0,
          positive: 0,
          negative: 0,
          neutral: 0,
          monthAnnualizedSales: 0
        }
      };
      _ref = data.marketplaces;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        m = _ref[_i];
        if (m.Disabled === false) {
          data.summary.monthSales += m.monthSales;
        }
        if (m.Disabled === false) {
          data.summary.anualSales += m.anualSales;
        }
        if (m.Disabled === false) {
          data.summary.monthAnnualizedSales += m.monthAnnualizedSales;
        }
        if (m.Disabled === false) {
          data.summary.inventory += m.inventory;
        }
        data.summary.positive += m.PositiveFeedbacks;
        data.summary.negative += m.NegativeFeedbacks;
        data.summary.neutral += m.NeutralFeedbacks;
      }
      total = data.summary.positive + data.summary.negative + data.summary.neutral;
      data.summary.rating = total > 0 ? data.summary.positive / total : 0;
      return data;
    };

    MarketPlacesView.prototype.disableShop = function(e) {
      var $el, umi,
        _this = this;
      $el = $(e.currentTarget);
      umi = $el.attr("umi");
      EzBob.ShowMessage("Disable shop", "Are you sure?", (function() {
        return _this.doEnableShop(umi, false);
      }), "Yes", null, "No");
      return false;
    };

    MarketPlacesView.prototype.doEnableShop = function(umi, enabled) {
      var url, xhr,
        _this = this;
      url = enabled ? "" + window.gRootPath + "Underwriter/MarketPlaces/Enable" : "" + window.gRootPath + "Underwriter/MarketPlaces/Disable";
      xhr = $.post(url, {
        umi: umi
      });
      return xhr.done(function(response) {
        return _this.model.fetch();
      });
    };

    MarketPlacesView.prototype.enableShop = function(e) {
      var $el, umi,
        _this = this;
      $el = $(e.currentTarget);
      umi = $el.attr("umi");
      EzBob.ShowMessage("Enable shop", "Are you sure?", (function() {
        return _this.doEnableShop(umi, true);
      }), "Yes", null, "No");
      return false;
    };

    MarketPlacesView.prototype.tryRecheckYodlee = function(e) {};

    MarketPlacesView.prototype.reCheckmarketplaces = function(e) {
      var $el, customerId, mpType, okFn, umi,
        _this = this;
      $el = $(e.currentTarget);
      umi = $el.attr("umi");
      mpType = $el.attr("marketplaceType");
      customerId = this.model.customerId;
      okFn = function() {
        var xhr;
        xhr = $.post("" + window.gRootPath + "Underwriter/MarketPlaces/ReCheckMarketplaces", {
          customerId: customerId,
          umi: umi,
          marketplaceType: mpType
        });
        xhr.done(function(response) {
          if (response && response.error !== void 0) {
            EzBob.ShowMessage(response.error, "Error occured");
          } else {
            EzBob.ShowMessage("Wait a few minutes", "The marketplace recheck is running. ", null, "OK");
          }
          return _this.trigger("rechecked", {
            umi: umi,
            el: $el
          });
        });
        return xhr.fail(function(data) {
          return console.error(data.responseText);
        });
      };
      EzBob.ShowMessage("", "Are you sure?", okFn, "Yes", null, "No");
      return false;
    };

    MarketPlacesView.prototype.renewTokenClicked = function(e) {
      var umi;
      umi = $(e.currentTarget).data("umi");
      this.renewToken(umi);
      return false;
    };

    MarketPlacesView.prototype.renewToken = function(umi) {
      var xhr;
      xhr = $.post("" + window.gRootPath + "Underwriter/MarketPlaces/RenewEbayToken", {
        umi: umi
      });
      return xhr.done(function() {
        return EzBob.ShowMessage("Renew started successfully", "Successfully");
      });
    };

    MarketPlacesView.prototype.parseYodlee = function() {
      var parseYodleeView;
      parseYodleeView = new EzBob.Underwriter.ParseYodleeView({
        el: this.$el.find('#parse-yodlee-container'),
        customerId: this.model.customerId,
        model: this.model
      });
      parseYodleeView.render();
      this.$el.find('#parse-yodlee-container').show();
      $(".mps-tables").hide();
      return this;
    };

    return MarketPlacesView;

  })(Backbone.Marionette.ItemView);

}).call(this);
