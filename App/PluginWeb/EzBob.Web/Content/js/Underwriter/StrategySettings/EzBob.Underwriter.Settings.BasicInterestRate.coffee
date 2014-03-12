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
            id = eventObject.target.id.substring(9)
            newValue = parseInt(eventObject.target.value)
        if (typeIdentifier == "sta")
            id = eventObject.target.id.substring(11)
            newValue = parseInt(eventObject.target.value)
        if (typeIdentifier == "int")
            id = eventObject.target.id.substring(9)
            newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('basicInterestRates')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.ToScore = newValue
                if (typeIdentifier == "sta")
                    row.FromScore = newValue
                if (typeIdentifier == "int")
                    row.LoanInterestBase = newValue
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

        this.model.get('basicInterestRates').push( {FromScore: 0, Id: freeId, ToScore: 0, LoanInterestBase:0.0})
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
            fromScoreObject = @$el.find('#startValue_' + row.Id)
            if (fromScoreObject.length == 1)
                fromScoreObject.numericOnly()
            toScoreObject = @$el.find('#endValue_' + row.Id)
            if (toScoreObject.length == 1)
                toScoreObject.numericOnly()
            loanInterestBaseObject = @$el.find('#interest_' + row.Id)
            if (loanInterestBaseObject.length == 1)
                loanInterestBaseObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

