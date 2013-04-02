root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreButtonView extends Backbone.Marionette.ItemView
    attributes:
        class: "span6"

    events:
        "click .store-logo": "clicked"

    template: "#store-button-template"

    initialize: (options) ->
        @name = options.name
        @logoText = options.logoText

    serializeData: ->
        name: @name
        logoText: @logoText

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