(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.ProfilePopupModel = (function(_super) {

    __extends(ProfilePopupModel, _super);

    function ProfilePopupModel() {
      return ProfilePopupModel.__super__.constructor.apply(this, arguments);
    }

    ProfilePopupModel.prototype.idAttribute = "Id";

    ProfilePopupModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/Profile/GetRegisteredCustomerInfo/?customerId=" + this.customerId;
    };

    ProfilePopupModel.prototype.defaults = {
      title: "Customer`s Marketplaces"
    };

    ProfilePopupModel.prototype.initialize = function() {
      var interval,
        _this = this;
      interval = setInterval((function() {
        return _this.fetch();
      }), 2000);
      return this.set("interval", interval);
    };

    return ProfilePopupModel;

  })(Backbone.Model);

  EzBob.Underwriter.ProfilePopupView = (function(_super) {

    __extends(ProfilePopupView, _super);

    function ProfilePopupView() {
      return ProfilePopupView.__super__.constructor.apply(this, arguments);
    }

    ProfilePopupView.prototype.initialize = function(options) {
      this.model = new EzBob.Underwriter.ProfilePopupModel();
      this.model.customerId = options.customerId;
      this.bindTo(this.model, 'change reset fetch', this.render, this);
      return this.model.fetch();
    };

    ProfilePopupView.prototype.serializeData = function() {
      return {
        mps: this.model.get("mps"),
        title: this.model.get("title"),
        strategyStatus: this.model.get("strategyStatus"),
        isWizardFinished: this.model.get("isWizardFinished")
      };
    };

    ProfilePopupView.prototype.template = "#profile-popup-view-template";

    ProfilePopupView.prototype.events = {
      "click .recheck-mp": "recheckMpClicked",
      "click #recheck-main-strat": "recheckMainStrat"
    };

    ProfilePopupView.prototype.recheckMpClicked = function(e) {
      var model, xhr;
      if ($(e.currentTarget).hasClass("disabled")) {
        return;
      }
      model = {
        umi: $(e.currentTarget).data('mp-id')
      };
      return xhr = $.post("" + window.gRootPath + "Underwriter/MarketPlaces/ReCheckMarketplaces", model);
    };

    ProfilePopupView.prototype.recheckMainStrat = function(e) {
      var model, xhr;
      if ($(e.currentTarget).hasClass("disabled")) {
        return;
      }
      $(e.currentTarget).addClass("disabled");
      model = {
        customerId: this.model.customerId
      };
      return xhr = $.post("" + window.gRootPath + "Underwriter/Profile/StartMainStrategy", model);
    };

    ProfilePopupView.prototype.onClose = function() {
      return clearInterval(this.model.get('interval'));
    };

    return ProfilePopupView;

  })(Backbone.Marionette.ItemView);

}).call(this);
