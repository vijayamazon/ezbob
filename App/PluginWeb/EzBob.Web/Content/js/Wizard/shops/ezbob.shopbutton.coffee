root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreButtonView extends Backbone.Marionette.ItemView
    template: "#store-button-template"

    initialize: (options) ->
        @name = options.name
        @mpAccounts = options.mpAccounts.get('mpAccounts')
        @shops = if @mpAccounts then @shops = _.where(@mpAccounts, {MpName: @name}) else []
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
            data.shops = @shops
            data.shopNames = _.pluck(@shops,"displayName").join(", ")
        return data

    onRender: ->
        @$el.find('.tooltipdiv').tooltip()
        @$el.find('.source_help').colorbox({ inline:true, transition: 'none' });

    isAddingAllowed: -> return true

    update: (data) ->
        @shops = if data then @shops = _.where(data, {MpName: @name}) else []

