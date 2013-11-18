root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreButtonView extends Backbone.Marionette.ItemView
    template: "#store-button-template"

    initialize: (options) ->
        @name = options.name
        @logoText = options.logoText
        @shops = options.shops
        if @shops
            @shops.on("change reset sync", @render, this)

        @shopClass = options.name.replace(' ', '')

    serializeData: ->
        data = 
            name: @name
            logoText: @logoText
            shopClass: @shopClass
            shops: []
            ribbon: @ribbon
            shopNames: ""

        if @shops
            data.shops = @shops.toJSON()
            data.shopNames = @shops.pluck("displayName").join(", ")
        return data

    onRender: ->
        console.log("EzBob.StoreButtonView render")
        @$el.find('.tooltipdiv').tooltip()
        @$el.find('.source_help').colorbox({ inline:true, transition: 'none' });

    isAddingAllowed: -> return true

    update: ->
