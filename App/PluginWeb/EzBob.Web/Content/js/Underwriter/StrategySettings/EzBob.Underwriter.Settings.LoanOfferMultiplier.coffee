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
            id = eventObject.target.id.substring(9)
            newValue = parseInt(eventObject.target.value)
        if (typeIdentifier == "sta")
            id = eventObject.target.id.substring(11)
            newValue = parseInt(eventObject.target.value)
        if (typeIdentifier == "mul")
            id = eventObject.target.id.substring(11)
            newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('loanOfferMultipliers')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.EndScore = newValue
                if (typeIdentifier == "sta")
                    row.StartScore = newValue
                if (typeIdentifier == "mul")
                    row.Multiplier = newValue
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

        this.model.get('loanOfferMultipliers').push( {StartScore: 0, Id: freeId, EndScore: 0, Multiplier:0.0})
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
            startScoreObject = @$el.find('#startScore_' + row.Id)
            if (startScoreObject.length == 1)
                startScoreObject.numericOnly()
            endScoreObject = @$el.find('#endScore_' + row.Id)
            if (endScoreObject.length == 1)
                endScoreObject.numericOnly()
            multiplierObject = @$el.find('#multiplier_' + row.Id)
            if (multiplierObject.length == 1)
                multiplierObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

