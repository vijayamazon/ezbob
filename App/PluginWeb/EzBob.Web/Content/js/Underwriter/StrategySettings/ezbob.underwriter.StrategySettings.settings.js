var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.StrategySettingsView = Backbone.View.extend({
  initialize: function() {
    this.template = _.template($("#strategy-detail-settings").html());
  },
  render: function() {
    var basicInterestRates, campaign, charges, defaultRateCompany, defaultRateCustomer, euLoanMonthlyInterest, experian, general, loanOfferMultipliers, pricingModel, product, openPlatform;
    this.$el.html(this.template());
    general = this.$el.find("#general-settings");
    charges = this.$el.find("#charges-settings");
    experian = this.$el.find("#experian-settings");
    campaign = this.$el.find("#campaign-settings");
    basicInterestRates = this.$el.find("#basic-interest-rate-settings");
    loanOfferMultipliers = this.$el.find("#loan-offer-multiplier-settings");
    pricingModel = this.$el.find("#pricing-model-settings");
    euLoanMonthlyInterest = this.$el.find("#eu-loan-monthly-interest-settings");
    defaultRateCompany = this.$el.find("#default-rate-company-settings");
    defaultRateCustomer = this.$el.find("#default-rate-customer-settings");
    product = this.$el.find("#product-settings");
    openPlatform = this.$el.find("#open-platform-settings");
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


    this.basicInterestRateModel = new EzBob.Underwriter.Settings.RangeModel({ type: "BasicInterestRate", typeName: "Basic interest rate" });
    this.basicInterestRateView = new EzBob.Underwriter.Settings.RangeView({
      el: basicInterestRates,
      model: this.basicInterestRateModel
    });
    this.loanOfferMultiplierModel = new EzBob.Underwriter.Settings.RangeModel({ type: "LoanOfferMultiplier", typeName: "Loan offer multiplier" });
    this.loanOfferMultiplierView = new EzBob.Underwriter.Settings.RangeView({
      el: loanOfferMultipliers,
      model: this.loanOfferMultiplierModel
    });
    this.euLoanMonthlyInterestModel = new EzBob.Underwriter.Settings.RangeModel({ type: "EuLoanMonthlyInterest", typeName: "EU loan monthly interest" });
    this.euLoanMonthlyInterestView = new EzBob.Underwriter.Settings.RangeView({
    	el: euLoanMonthlyInterest,
    	model: this.euLoanMonthlyInterestModel
    });
    this.defaultRateCompanyModel = new EzBob.Underwriter.Settings.RangeModel({ type: "DefaultRateCompany", typeName: "Default rate company" });
    this.defaultRateCompanyView = new EzBob.Underwriter.Settings.RangeView({
    	el: defaultRateCompany,
    	model: this.defaultRateCompanyModel
    });
    this.defaultRateCustomerModel = new EzBob.Underwriter.Settings.RangeModel({ type: "DefaultRateCustomer", typeName: "Default rate customer" });
    this.defaultRateCustomerView = new EzBob.Underwriter.Settings.RangeView({
    	el: defaultRateCustomer,
    	model: this.defaultRateCustomerModel
    });
    this.productModel = new EzBob.Underwriter.Settings.ProductModel();
    this.productView = new EzBob.Underwriter.Settings.ProductView({
    	el: product,
    	model: this.productModel
    });
    this.openPlatformModel = new EzBob.Underwriter.Settings.OpenPlatformModel();
    this.openPlatformView = new EzBob.Underwriter.Settings.OpenPlatformView({
    	el: openPlatform,
    	model: this.openPlatformModel
    });

    this.pricingModelView = new EzBob.Underwriter.SettingsPricingModelView({
      el: pricingModel,
    });
    
    return EzBob.handleUserLayoutSetting();
  },
  show: function(type) {
    return this.$el.show();
  },
  hide: function() {
    return this.$el.hide();
  },
  onClose: function() {
    return this.modelBinder.unbind();
  }
});
