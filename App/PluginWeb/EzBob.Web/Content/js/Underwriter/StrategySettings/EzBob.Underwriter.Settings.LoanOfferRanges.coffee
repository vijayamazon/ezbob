root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.LoanOfferRangesModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsBasicInterestRate"

class EzBob.Underwriter.Settings.LoanOfferRangesView extends Backbone.Marionette.ItemView
    template: "#loan-offer-ranges-settings-template"

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

        ranges = @model.get('loanOfferRanges')
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
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveBasicInterestRate", serializedModels: JSON.stringify(@model.get('loanOfferRanges'))
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('data-loan-offer-range-id')
        index = 0
        ranges = @model.get('loanOfferRanges')
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

        this.model.get('loanOfferRanges').push( {FromScore: 0, Id: freeId, ToScore: 0, LoanInterestBase:0.0})
        @render()
        return

    serializeData: ->
        data = 
            loanOfferRanges: @model.get('loanOfferRanges')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            
        counter = 0
        found = true
        while (found)
            x = @$el.find('#startValue_' + counter)
            if (x.length == 1)
                x.autoNumeric()
            else
                found = false
            counter++
        
        counter = 0
        found = true
        while (found)
            x = @$el.find('#endValue_' + counter)
            if (x.length == 1)
                x.autoNumeric()
            else
                found = false
            counter++
        
        counter = 0
        found = true
        while (found)
            x = @$el.find('#interest_' + counter)
            if (x.length == 1)
                x.autoNumeric()
            else
                found = false
            counter++

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

