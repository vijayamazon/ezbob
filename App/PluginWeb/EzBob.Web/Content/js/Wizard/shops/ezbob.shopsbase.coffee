root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoBaseView extends Backbone.View
    isOffline: false

    initialize: ->
        @allowFinishOnlineWizardWithoutMarketplaces = $('#allowFinishWizardWithoutMarketplaces').attr('online').toLowerCase() == 'true'
        @allowFinishOfflineWizardWithoutMarketplaces = $('#allowFinishWizardWithoutMarketplaces').attr('offline').toLowerCase() == 'true'

        if typeof ordpi is 'undefined'
            ordpi = Math.random() * 10000000000000000
        @storeList = $(_.template($("#store-info").html(), {ordpi : ordpi}))
        
        @isReady = false

        for name, store of @stores
            store.button.on "selected", @connect, this
            store.view.on "completed", _.bind(@completed, this, store.button.name)
            store.view.on "back", @back, this
            store.button.on "ready", @ready, this

        EzBob.App.on "ct:storebase.shop.connected", @shopConnected, this
        EzBob.App.on "ct:storebase." + @name + ".connect", @connect, this

    completed: (name) ->
        @stores[name].button.update()
        @render()
        @trigger "completed"

    back: ->
        @$el.find(">div").hide()
        @storeList.show()
        $(document).attr "title", @oldTitle
        @updateEarnedPoints()
        false

    next: ->
        return if @$el.find(".next").hasClass("disabled")
        @trigger "next"
        EzBob.App.trigger "clear"
        false

    ready: (name) ->
        @trigger "ready", name
        unless @isReady
            @isReady = true
            @$el.find(".next").show()

    updateEarnedPoints: ->
        $.getJSON("#{window.gRootPath}Customer/Wizard/EarnedPointsStr").done (data) ->
            if data.EarnedPointsStr
                $('#EarnedPoints').text data.EarnedPointsStr

    shopConnected: ->
        @updateEarnedPoints()
        @render()

    render: ->
        hasHmrc = @stores.HMRC.button.model.length > 0

        if @isOffline
            if hasHmrc
                @$el.find('.entry_message').empty().append 'The more you link, the more funds you can get.'
            else
                @$el.find('.entry_message').empty()
                    .append('You must <strong>link</strong> or <strong>upload</strong> your ')
                    .append(
                        $('<span class="green btn">HM Revenue & Customs</span>')
                        .click(-> EzBob.App.trigger 'ct:storebase.shops.connect', 'HMRC' )
                    )
                    .append(' account data')
                    .append('<br />')
                    .append('to be approved for a loan.')

            @$el.find('.importantnumber').text '£200,000'

        that = this
        accountsList = @storeList.find(".accounts-list")

        sortedShopsByPriority = _.sortBy(@stores, (s) -> s.priority)
        sortedShopsByNumOfShops = _.sortBy(sortedShopsByPriority, (s) -> -s.button.model.length)

        hasFilledShops = sortedShopsByNumOfShops[0].button.model.length > 0

        hasEbay = @stores.eBay.button.model.length > 0
        hasPaypal = @stores.paypal.button.model.length > 0

        ebayPaypalRuleMessageVisible = hasEbay and !hasPaypal
        @$el.find(".eBayPaypalRule").toggleClass("hide", !ebayPaypalRuleMessageVisible)
                
        foundAllMandatories = true
        for key in Object.keys(@stores)
            if @stores[key].button.model.length == 0 && @stores[key].mandatory
                foundAllMandatories = false

        canContinue = (hasFilledShops and (!hasEbay or (hasEbay and hasPaypal)) and foundAllMandatories) or (@isOffline and @allowFinishOfflineWizardWithoutMarketplaces) or (!@isOffline and @allowFinishOnlineWizardWithoutMarketplaces)
        @$el.find('.next').toggleClass 'disabled', !canContinue
        @handleMandatoryText(hasFilledShops, canContinue, ebayPaypalRuleMessageVisible)

        for shop in sortedShopsByNumOfShops when shop.active 
            shop.button.render().$el.appendTo accountsList
            shop.view.render().$el.hide().appendTo that.$el

        @storeList.appendTo @$el

        this

    events:
        "click a.connect-store": "close"
        "click a.next": "next"
        "click a.back-step": "previousClick"

    previousClick: ->
        @trigger "previous"
        false

    handleMandatoryText: (hasFilledShops, canContinue, ebayPaypalRuleMessageVisible) ->
        shouldHide = !hasFilledShops or canContinue or ebayPaypalRuleMessageVisible
            
        if !shouldHide
            first = true
            text = 'Please add the following accounts in order to continue: '
            for key in Object.keys(@stores)
                if @stores[key].button.model.length == 0 && @stores[key].mandatory
                    foundAllMandatories = false
                    if !first
                        text += ', '
                    first = false
                    text += key

            for addMoreMsg in @$el.find('.AddMoreRule')
                addMoreMsg.innerText = text

        @$el.find('.AddMoreRule').toggleClass 'hide', shouldHide

    connect: (storeName) ->
        EzBob.CT.recordEvent "ct:storebase." + @name + ".connect", storeName
        @$el.find(">div").hide()
        storeView = @stores[storeName].view
        storeView.render()
        storeView.$el.show()
        @oldTitle = $(document).attr("title")
        @setDocumentTitle storeView
        @setFocus storeName
        false

    setFocus: (storeName) ->
        $.colorbox.close()

        switch storeName
            when "EKM"
                @$el.find("#ekm_login").focus()
            when "PayPoint"
                @$el.find("#payPoint_login").focus()
            else
                aryCGAccounts = $.parseJSON $('div#cg-account-list').text()
                if aryCGAccounts[storeName]
                    $('.form_field', '#' + storeName.toLowerCase() + 'Account').first().focus()

    setDocumentTitle: (view) ->
        title = view.getDocumentTitle()
        if title
            $(document).attr "title", "Step 2: #{title} | EZBOB"

    close: ->
        this