root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsPricingModelModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsPricingModel"

class EzBob.Underwriter.SettingsPricingModelView extends Backbone.Marionette.ItemView
    template: "#pricing-model-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        PricingModelTenurePercents:
            selector: "input[name='PricingModelTenurePercents']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelDefaultRateCompanyShare:
            selector: "input[name='PricingModelDefaultRateCompanyShare']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelCollectionRate:
            selector: "input[name='PricingModelCollectionRate']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelEuCollectionRate:
            selector: "input[name='PricingModelEuCollectionRate']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelDebtOutOfTotalCapital:
            selector: "input[name='PricingModelDebtOutOfTotalCapital']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelCostOfDebtPA:
            selector: "input[name='PricingModelCostOfDebtPA']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelProfitMarkupPercentsOfRevenue:
            selector: "input[name='PricingModelProfitMarkupPercentsOfRevenue']"
            converter: EzBob.BindingConverters.percentsFormat

        PricingModelSetupFee:
            selector: "input[name='PricingModelSetupFee']"
            converter: EzBob.BindingConverters.percentsFormat
        
        PricingModelOpexAndCapex:
            selector: "input[name='PricingModelOpexAndCapex']"
        
        PricingModelCogs:
            selector: "input[name='PricingModelCogs']"
        
        PricingModelInterestOnlyPeriod:
            selector: "input[name='PricingModelInterestOnlyPeriod']"

    events:
        "click button[name='SavePricingModelSettings']":     "saveSettings"
        "click button[name='CancelPricingModelSettings']":   "cancelSettings"

    saveSettings: ->
        BlockUi "on"
        console.log
        @model.save().done -> EzBob.ShowMessage  "Saved successfully", "Successful"
        @model.save().complete -> BlockUi "off"
        @model.save()
        false

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    cancelSettings: ->
        @update()

    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        
        @$el.find("input[name='PricingModelTenurePercents']").percentFormat()
        @$el.find("input[name='PricingModelDefaultRateCompanyShare']").percentFormat()
        @$el.find("input[name='PricingModelCollectionRate']").percentFormat()
        @$el.find("input[name='PricingModelEuCollectionRate']").percentFormat()
        @$el.find("input[name='PricingModelDebtOutOfTotalCapital']").percentFormat()
        @$el.find("input[name='PricingModelCostOfDebtPA']").percentFormat()
        @$el.find("input[name='PricingModelProfitMarkupPercentsOfRevenue']").percentFormat()
        @$el.find("input[name='PricingModelOpexAndCapex']").numericOnlyWithDecimal()
        @$el.find("input[name='PricingModelCogs']").numericOnlyWithDecimal()
        @$el.find("input[name='PricingModelInterestOnlyPeriod']").numericOnly(2)

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()
