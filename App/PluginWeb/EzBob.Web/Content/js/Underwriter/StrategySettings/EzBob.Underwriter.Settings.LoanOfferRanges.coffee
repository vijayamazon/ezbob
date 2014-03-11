root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.LoanOfferRangesModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsLoanOfferRanges"

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

    removeRange: (rangeId) ->
        #TODO: implement
        return

    addRange: (e, range)->
        #TODO: implement
        return

    serializeData: ->
        debugger
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

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

