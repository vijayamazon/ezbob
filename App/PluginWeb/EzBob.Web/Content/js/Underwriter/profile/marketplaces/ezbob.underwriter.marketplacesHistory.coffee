root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.MarketPlacesHistoryModel extends Backbone.Model
    idAttribute: "Id"

class EzBob.Underwriter.MarketPlacesHistory extends Backbone.Collection
    model: EzBob.Underwriter.MarketPlacesHistoryModel
    url: -> "#{window.gRootPath}Underwriter/MarketPlaces/GetCustomerMarketplacesHistory/?customerId=#{@customerId}"

class EzBob.Underwriter.MarketPlacesHistoryView extends Backbone.Marionette.ItemView
    template: "#marketplace-history-template"

    initialize :(options) ->
        @model.on "reset change sync", @render, this
        @loadMarketPlacesHistory()

    loadMarketPlacesHistory: () ->
        that = @
        @model.fetch().done =>
            if that.model.length > 0
                that.render()

    events:
        "click .showHistoryMarketPlaces": "showHistoryMarketPlacesClicked"
        "click .showCurrentMarketPlaces": "showCurrentMarketPlacesClicked"
        "click .parseYodleeMp": "parseYodleeClicked"
        "click .uploadHmrcMp": "uploadHmrcClicked"
        "click .enterHmrcMp": "enterHmrcClicked"

    serializeData: ->
        return {MarketPlacesHistory: @model}

    showHistoryMarketPlacesClicked: ->
        date = @$el.find("#mpHistoryDdl :selected").val()
        EzBob.App.vent.trigger 'ct:marketplaces.history', date

    showCurrentMarketPlacesClicked: ->
        EzBob.App.vent.trigger 'ct:marketplaces.history', null
        
    parseYodleeClicked: (event) ->
        event.preventDefault()
        event.stopPropagation()

        EzBob.App.vent.trigger 'ct:marketplaces.parseYodlee'
        
    uploadHmrcClicked: (event) ->
        event.preventDefault()
        event.stopPropagation()

        EzBob.App.vent.trigger 'ct:marketplaces.uploadHmrc'

    enterHmrcClicked: (event) ->
        event.preventDefault()
        event.stopPropagation()

        EzBob.App.vent.trigger 'ct:marketplaces.enterHmrc'
