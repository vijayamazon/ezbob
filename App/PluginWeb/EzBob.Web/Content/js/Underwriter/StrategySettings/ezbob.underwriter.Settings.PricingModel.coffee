root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsPricingModelModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsPricingModel"

class EzBob.Underwriter.SettingsPricingModelView extends Backbone.Marionette.ItemView
    template: "#pricing-model-settings-template"

    initialize: (options) ->
        @scenarios = options.scenarios;
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        TenurePercents:
            selector: "input[name='TenurePercents']"
            converter: EzBob.BindingConverters.percentsFormat

        DefaultRateCompanyShare:
            selector: "input[name='DefaultRateCompanyShare']"
            converter: EzBob.BindingConverters.percentsFormat

        CollectionRate:
            selector: "input[name='CollectionRate']"
            converter: EzBob.BindingConverters.percentsFormat

        EuCollectionRate:
            selector: "input[name='EuCollectionRate']"
            converter: EzBob.BindingConverters.percentsFormat

        DebtPercentOfCapital:
            selector: "input[name='DebtPercentOfCapital']"
            converter: EzBob.BindingConverters.percentsFormat

        CostOfDebt:
            selector: "input[name='CostOfDebt']"
            converter: EzBob.BindingConverters.percentsFormat

        ProfitMarkup:
            selector: "input[name='ProfitMarkup']"
            converter: EzBob.BindingConverters.percentsFormat

        SetupFeePercents:
            selector: "input[name='SetupFeePercents']"
            converter: EzBob.BindingConverters.percentsFormat

        BrokerSetupFeePercents:
            selector: "input[name='BrokerSetupFeePercents']"
            converter: EzBob.BindingConverters.percentsFormat
        
        OpexAndCapex:
            selector: "input[name='OpexAndCapex']"
        
        Cogs:
            selector: "input[name='Cogs']"
        
        InterestOnlyPeriod:
            selector: "input[name='InterestOnlyPeriod']"

    events:
        "click button[name='SavePricingModelSettings']":     "saveSettings"
        "click button[name='CancelPricingModelSettings']":   "cancelSettings"
        "change #PricingModelScenarioSettings": "scenarioChanged"
        
    scenarioChanged: ->
        @selectedScenario = @$el.find('#PricingModelScenarioSettings').val()
        BlockUi()
        that = this
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SettingsPricingModelForScenario", scenarioName: @selectedScenario
        xhr.done (res) =>
            that.model.set(res)
            UnBlockUi()
            that.render()

    saveSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SettingsSavePricingModelScenario", 
            scenarioName: @$el.find('#PricingModelScenarioSettings').val()
            model: JSON.stringify(this.model.toJSON())
        xhr.done () =>
            EzBob.ShowMessage  "Saved successfully", "Successful"
        xhr.complete () =>
            BlockUi "off"
            
        false

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    cancelSettings: ->
        @update()

    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        
        @$el.find("input[name='TenurePercents']").percentFormat()
        @$el.find("input[name='DefaultRateCompanyShare']").percentFormat()
        @$el.find("input[name='CollectionRate']").percentFormat()
        @$el.find("input[name='EuCollectionRate']").percentFormat()
        @$el.find("input[name='DebtPercentOfCapital']").percentFormat()
        @$el.find("input[name='CostOfDebt']").percentFormat()
        @$el.find("input[name='ProfitMarkup']").percentFormat()
        @$el.find("input[name='OpexAndCapex']").numericOnlyWithDecimal()
        @$el.find("input[name='Cogs']").numericOnlyWithDecimal()
        @$el.find("input[name='InterestOnlyPeriod']").numericOnly(2)

        if !$("body").hasClass("role-manager") 
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()

        if (@selectedScenario)
            @$el.find('#PricingModelScenarioSettings').val(this.selectedScenario);

    show: (type) ->
        @$el.show()

    hide: () ->
        @$el.hide()

    onClose: ->
        @modelBinder.unbind()
        
    serializeData: ->
        return { model: @model.toJSON(), scenarios: @scenarios.toJSON() }
