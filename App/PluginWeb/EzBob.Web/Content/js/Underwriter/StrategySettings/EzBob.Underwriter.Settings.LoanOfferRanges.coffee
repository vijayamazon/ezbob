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

    saveBasicInterestRateSettings: ->
        BlockUi "on"
        debugger
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveBasicInterestRate", serializedModels: JSON.stringify(@model.get('loanOfferRanges'))
        # @model
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (rangeId) ->
        #TODO: implement
        return

    addRange: (e, range)->
        #TODO: implement
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

