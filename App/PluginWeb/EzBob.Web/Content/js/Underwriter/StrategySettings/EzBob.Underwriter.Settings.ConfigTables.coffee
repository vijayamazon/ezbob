root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.LoanOfferMultiplierModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsLoanOfferMultiplier"

class EzBob.Underwriter.Settings.LoanOfferMultiplierView extends Backbone.Marionette.ItemView
    template: "#loan-offer-multiplier-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveLoanOfferMultiplierSettings": "saveLoanOfferMultiplierSettings"
        "click #CancelLoanOfferMultiplierSettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            newValue = parseInt(eventObject.target.value)

        ranges = @model.get('loanOfferMultipliers')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveLoanOfferMultiplierSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveLoanOfferMultiplier", serializedModels: JSON.stringify(@model.get('loanOfferMultipliers'))
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('loan-offer-multiplier-id')
        index = 0
        ranges = @model.get('loanOfferMultipliers')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#loanOfferMultiplierRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('loanOfferMultipliers').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = loanOfferMultipliers: @model.get('loanOfferMultipliers')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('loanOfferMultipliers')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.BasicInterestRateModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsBasicInterestRate"

class EzBob.Underwriter.Settings.BasicInterestRateView extends Backbone.Marionette.ItemView
    template: "#basic-interest-rate-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveBasicInterestRateSettings": "saveBasicInterestRateSettings"
        "click #CancelBasicInterestRateSettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            newValue = parseInt(eventObject.target.value)

        ranges = @model.get('basicInterestRates')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveBasicInterestRateSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveBasicInterestRate", serializedModels: JSON.stringify(@model.get('basicInterestRates'))
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('basic-interest-rate-id')
        index = 0
        ranges = @model.get('basicInterestRates')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#basicInterestRateRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('basicInterestRates').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = basicInterestRates: @model.get('basicInterestRates')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('basicInterestRates')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

