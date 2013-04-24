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
        @shopNames = ""

        if @shops
            @shops.on("change reset", @updateShopNames, this)

        @shopClass = options.name.toLowerCase().replace(' ', '')

    serializeData: ->
        name: @name
        logoText: @logoText
        shopClass: @shopClass
        shops: if @shops then @shops.toJSON() else []
        shopNames: @shopNames

    updateShopNames: ->
        if @shops
            s = ""
            _.each @shops.models, (sh, idx) ->
                if s != ""
                    s += ", "
                s += sh.attributes.displayName
            @shopNames = s
        @render()

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
