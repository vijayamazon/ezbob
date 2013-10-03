root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.MarketPlacesHistoryModel extends Backbone.Model
    idAttribute: "Id"

class EzBob.Underwriter.MarketPlacesHistory extends Backbone.Collection
    model: EzBob.Underwriter.MarketPlacesHistoryModel
    url: -> 
        console.log "url", @, @customerId
        "#{window.gRootPath}Underwriter/MarketPlaces/GetCustomerMarketplacesHistory/?customerId=#{@customerId}"

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

    serializeData: ->
        return {MarketPlacesHistory: @model}

    showHistoryMarketPlacesClicked: ->
        date = @$el.find("#mpHistoryDdl :selected").text()
        console.log('show history', date)
