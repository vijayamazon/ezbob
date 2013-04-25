root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreButtonView extends Backbone.Marionette.ItemView
    events:
        "click .button": "clicked"

    template: "#store-button-template"

    initialize: (options) ->
        @name = options.name
        @logoText = options.logoText
        @shops = options.shops

        if @shops
            @shops.on("change reset", @render, this)

        @shopClass = options.name.toLowerCase().replace(' ', '')

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

    clicked: ->
        if @disabled
            EzBob.ShowMessage @disabledText  if @disabledText
            return false
        return false if not @isAddingAllowed()
        return  if @name is "bank-account" and @model.get("bankAccountAdded")
        @trigger "selected", @name
        false

    isAddingAllowed: -> return true

    update: ->
