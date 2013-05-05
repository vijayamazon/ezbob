root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreButtonView extends Backbone.Marionette.ItemView
    template: "#store-button-template"

    initialize: (options) ->
        @name = options.name
        @logoText = options.logoText
        @shops = options.shops

        if @shops
            @shops.on("change reset", @render, this)

        @shopClass = options.name.replace(' ', '')

    serializeData: ->
        data = 
            name: @name
            logoText: @logoText
            shopClass: @shopClass
            shops: []
            shopNames: ""

        if @shops
            data.shops = @shops.toJSON()
            data.shopNames = @shops.pluck("displayName").join(", ")
        return data

    onRender: ->
        @$el.find('.tooltipdiv').tooltip()
        @$el.find('.source_help').colorbox({ inline:true });

    isAddingAllowed: -> return true

    update: ->
