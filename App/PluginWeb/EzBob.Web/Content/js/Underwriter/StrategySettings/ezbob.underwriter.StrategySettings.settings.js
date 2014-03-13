(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.StrategySettingsView = (function(_super) {
    __extends(StrategySettingsView, _super);

    function StrategySettingsView() {
      _ref = StrategySettingsView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StrategySettingsView.prototype.initialize = function() {
      return this.template = _.template($("#strategy-detail-settings").html());
    };

    StrategySettingsView.prototype.render = function() {
      var basicInterestRates, campaign, charges, experian, general, loanOfferMultipliers;

      this.$el.html(this.template());
      general = this.$el.find("#general-settings");
      charges = this.$el.find("#charges-settings");
      experian = this.$el.find("#experian-settings");
      campaign = this.$el.find("#campaign-settings");
      basicInterestRates = this.$el.find("#basic-interest-rate-settings");
      loanOfferMultipliers = this.$el.find("#loan-offer-multiplier-settings");
      this.generalModel = new EzBob.Underwriter.SettingsGeneralModel();
      this.generalView = new EzBob.Underwriter.SettingsGeneralView({
        el: general,
        model: this.generalModel
      });
      this.chargesModel = new EzBob.Underwriter.SettingsChargesModel();
      this.chargesView = new EzBob.Underwriter.SettingsChargesView({
        el: charges,
        model: this.chargesModel
      });
      this.experianModel = new EzBob.Underwriter.Settings.ExperianModel();
      this.experianView = new EzBob.Underwriter.Settings.ExperianView({
        el: experian,
        model: this.experianModel
      });
      this.campaignModel = new EzBob.Underwriter.Settings.CampaignModel();
      this.campaignView = new EzBob.Underwriter.Settings.CampaignView({
        el: campaign,
        model: this.campaignModel
      });
      this.basicInterestRateModel = new EzBob.Underwriter.Settings.BasicInterestRateModel();
      this.basicInterestRateView = new EzBob.Underwriter.Settings.BasicInterestRateView({
        el: basicInterestRates,
        model: this.basicInterestRateModel
      });
      this.loanOfferMultiplierModel = new EzBob.Underwriter.Settings.LoanOfferMultiplierModel();
      this.loanOfferMultiplierView = new EzBob.Underwriter.Settings.LoanOfferMultiplierView({
        el: loanOfferMultipliers,
        model: this.loanOfferMultiplierModel
      });
      return EzBob.handleUserLayoutSetting();
    };

    StrategySettingsView.prototype.show = function(type) {
      return this.$el.show();
    };

    StrategySettingsView.prototype.hide = function() {
      return this.$el.hide();
    };

    StrategySettingsView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return StrategySettingsView;

  })(Backbone.View);

}).call(this);
