root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreButtonView extends Backbone.Marionette.ItemView
    template: "#store-button-template"

    initialize: (options) ->
        @name = options.name
        @description = options.description
        @mpAccounts = options.mpAccounts.get('customer').get('mpAccounts')
        @shops = if @mpAccounts then @shops = _.where(@mpAccounts, {MpName: @name}) else []
        @shopClass = options.name.replace(' ', '')

    serializeData: ->
        data = 
            name: @name
            shopClass: @shopClass
            shopDescription : @description
        return data

    onRender: ->
        btn = @$el.find '.marketplace-button-account-' + @shopClass

        @$el.removeClass 'marketplace-button-full marketplace-button-empty'

        sTitle = (if @shops.length then 'Some' else 'No') + ' accounts linked. Click to link '

        if @shops.length
            @$el.addClass('marketplace-button-full')
            sTitle += 'more.'
        else
            @$el.addClass 'marketplace-button-empty'
            sTitle += 'one.'

        @$el.attr 'title', sTitle

        switch @shopClass
            when 'eBay', 'paypal', 'FreeAgent', 'Sage'
                oHelpWindowTemplate = _.template($('#store-button-help-window-template').html());
                @$el.append oHelpWindowTemplate @serializeData()

                oLinks = JSON.parse $('#store-button-help-window-links').html()
                @$el.find('.help-window-continue-link').attr 'href', oLinks[@shopClass]

                btn.attr 'href', '#' + @shopClass + '_help'

                btn.colorbox({ inline:true, transition: 'elastic', onClosed: ->
                    oBackLink = $ '#link_account_implicit_back'

                    if oBackLink.length
                        EzBob.UiAction.saveOne 'click', oBackLink
                });
            else
                btn.click(
                    ((arg) ->
                        return -> EzBob.App.trigger 'ct:storebase.shops.connect', arg
                    )(@shopClass)
                )
        # end of switch

        if $.browser.name.toLowerCase() is 'firefox'
            sTop = '1px'
        else if document.all
            sTop = '2px'
        else
            sTop = 0

        btn.hoverIntent(
            ((evt) -> $('.onhover', this).animate({ top:   sTop, opacity: 1 })),
            ((evt) -> $('.onhover', this).animate({ top: '60px', opacity: 0 }))
        )
    # end of onRender

    isAddingAllowed: -> return true

    update: (data) ->
        @shops = if data then @shops = _.where(data, {MpName: @name}) else []
