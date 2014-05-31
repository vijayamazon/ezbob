root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.StrategySettingsView extends Backbone.View
    initialize: ->
        @template = _.template($("#strategy-detail-settings").html())

    render: ->
        @$el.html @template()
        general = @$el.find "#general-settings"
        charges = @$el.find "#charges-settings"
        experian = @$el.find "#experian-settings"
        campaign = @$el.find "#campaign-settings"
        basicInterestRates = @$el.find "#basic-interest-rate-settings"
        loanOfferMultipliers = @$el.find "#loan-offer-multiplier-settings"
        pricingModel = @$el.find "#pricing-model-settings"
        euLoanMonthlyInterest = @$el.find "#eu-loan-monthly-interest-settings"
        defaultRateCompany = @$el.find "#default-rate-company-settings"
        defaultRateCustomer = @$el.find "#default-rate-customer-settings"

        @generalModel =  new EzBob.Underwriter.SettingsGeneralModel()
        @generalView =  new EzBob.Underwriter.SettingsGeneralView(
            el: general
            model: @generalModel
        )
        @chargesModel =  new EzBob.Underwriter.SettingsChargesModel()
        @chargesView =  new EzBob.Underwriter.SettingsChargesView(
            el: charges
            model: @chargesModel
        )
        @experianModel =  new EzBob.Underwriter.Settings.ExperianModel()
        @experianView =  new EzBob.Underwriter.Settings.ExperianView(
            el: experian
            model: @experianModel
        )
        @campaignModel = new EzBob.Underwriter.Settings.CampaignModel()
        @campaignView =  new EzBob.Underwriter.Settings.CampaignView(
            el: campaign
            model: @campaignModel
        )

        @basicInterestRateModel = new EzBob.Underwriter.Settings.BasicInterestRateModel()
        @basicInterestRateView =  new EzBob.Underwriter.Settings.BasicInterestRateView(
            el: basicInterestRates
            model: @basicInterestRateModel
        )

        @loanOfferMultiplierModel = new EzBob.Underwriter.Settings.LoanOfferMultiplierModel()
        @loanOfferMultiplierView =  new EzBob.Underwriter.Settings.LoanOfferMultiplierView(
            el: loanOfferMultipliers
            model: @loanOfferMultiplierModel
        )

        @pricingModelScenarios = new EzBob.Underwriter.PricingModelScenarios()
        @pricingModelScenarios.fetch()
        @pricingModelModel =  new EzBob.Underwriter.SettingsPricingModelModel()
        @pricingModelModel.fetch()
        @pricingModelView =  new EzBob.Underwriter.SettingsPricingModelView(
            el: pricingModel
            model: @pricingModelModel
            scenarios: @pricingModelScenarios
        )

        @euLoanMonthlyInterestModel = new EzBob.Underwriter.Settings.EuLoanMonthlyInterestModel()
        @euLoanMonthlyInterestView =  new EzBob.Underwriter.Settings.EuLoanMonthlyInterestView(
            el: euLoanMonthlyInterest
            model: @euLoanMonthlyInterestModel
        )

        @defaultRateCompanyModel = new EzBob.Underwriter.Settings.DefaultRateCompanyModel()
        @defaultRateCompanyView =  new EzBob.Underwriter.Settings.DefaultRateCompanyView(
            el: defaultRateCompany
            model: @defaultRateCompanyModel
        )

        @defaultRateCustomerModel = new EzBob.Underwriter.Settings.DefaultRateCustomerModel()
        @defaultRateCustomerView =  new EzBob.Underwriter.Settings.DefaultRateCustomerView(
            el: defaultRateCustomer
            model: @defaultRateCustomerModel
        )

        EzBob.handleUserLayoutSetting()

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()