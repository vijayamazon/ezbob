root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoView extends Backbone.View
    attributes:
        class: "stores-view"

    isOffline: false

    initialize: ->
        @ebayStores = new EzBob.EbayStoreModels()
        @EbayStoreView = new EzBob.EbayStoreInfoView()
        @ebayStores.on "reset 
        change", @marketplacesChanged, this

        @amazonMarketplaces = new EzBob.AmazonStoreModels()
        @AmazonStoreInfoView = new EzBob.AmazonStoreInfoView()
        @amazonMarketplaces.on "reset change", @marketplacesChanged, this

        @ekmAccounts = new EzBob.EKMAccounts()
        @EKMAccountInfoView = new EzBob.EKMAccountInfoView(model: @ekmAccounts)

        @freeAgentAccounts = new EzBob.FreeAgentAccounts()
        @FreeAgentAccountInfoView = new EzBob.FreeAgentAccountInfoView(model: @freeAgentAccounts)

        @sageAccounts = new EzBob.SageAccounts()
        @sageAccountInfoView = new EzBob.SageAccountInfoView(model: @sageAccounts)

        @PayPointAccounts = new EzBob.PayPointAccounts()
        @PayPointAccountInfoView = new EzBob.PayPointAccountInfoView(model: @PayPointAccounts)

        @YodleeAccounts = new EzBob.YodleeAccounts()
        @YodleeAccountInfoView = new EzBob.YodleeAccountInfoView(model: @YodleeAccounts)

        @payPalAccounts = new EzBob.PayPalAccounts(@model.get("paypalAccounts"))
        @PayPalInfoView = new EzBob.PayPalInfoView(model: @payPalAccounts)

        aryCGAccounts = $.parseJSON $('div#cg-account-list').text()

        for accountTypeName, ignore of aryCGAccounts
            lc = accountTypeName.toLowerCase()
            acc = new EzBob.CGAccounts [], accountType: accountTypeName
            this[lc + 'Accounts'] = acc
            this[lc + 'AccountInfoView'] = new EzBob.CGAccountInfoView model: acc, accountType: accountTypeName

        @stores =
            "eBay": view: @EbayStoreView
            "Amazon": view: @AmazonStoreInfoView
            "paypal": view: @PayPalInfoView
            "EKM": view: @EKMAccountInfoView
            "PayPoint": view: @PayPointAccountInfoView
            "Yodlee": view: @YodleeAccountInfoView
            "FreeAgent": view: @FreeAgentAccountInfoView
            "Sage": view: @sageAccountInfoView

        for accountTypeName, vendorInfo of aryCGAccounts
            lc = accountTypeName.toLowerCase()

            @stores[accountTypeName] =
                view: this[lc + 'AccountInfoView']

        @isOffline = @model.get 'isOffline'
        @isProfile = @model.get 'isProfile'

        @mpGroups = {}
        for grp in EzBob.Config.MarketPlaceGroups
            @mpGroups[grp.Id] = grp
            grp.ui = null

        for j in EzBob.Config.MarketPlaces
            storeTypeName = if j.Name == "Pay Pal" then "paypal" else j.Name
            if @stores[storeTypeName]
                @stores[storeTypeName].active = if @isProfile then (if @isOffline then j.ActiveDashboardOffline else j.ActiveDashboardOnline) else (if @isOffline then j.ActiveWizardOffline else j.ActiveWizardOnline)
                @stores[storeTypeName].priority = if @isOffline then j.PriorityOffline else j.PriorityOnline
                @stores[storeTypeName].ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].button = new EzBob.StoreButtonView({ name: storeTypeName, mpAccounts: @model })
                @stores[storeTypeName].button.ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].mandatory = if @isOffline then j.MandatoryOffline else j.MandatoryOnline
                @stores[storeTypeName].groupid = if j.Group? then j.Group.Id else 0

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

        EzBob.App.on 'ct:storebase.shops.connect', @connect, @
    # end of initialize

    events:
        'click a.connect-store': 'close'
        'click a.continue': 'next'
        'click .btn-showmore': 'showMoreAccounts'

    render: ->
        hasHmrc = @stores.HMRC.button.shops.length > 0

        sShow = ''
        sRemove = ''

        sortedShopsByPriority = _.sortBy(@stores, (s) -> s.priority)
        sortedShopsByNumOfShops = _.sortBy(sortedShopsByPriority, (s) -> -s.button.shops.length)

        hasFilledShops = sortedShopsByNumOfShops[0].button.shops.length > 0

        hasEbay = @stores.eBay.button.shops.length > 0
        hasPaypal = @stores.paypal.button.shops.length > 0

        ebayPaypalRuleMessageVisible = hasEbay and !hasPaypal
        @$el.find(".eBayPaypalRule").toggleClass("hide", !ebayPaypalRuleMessageVisible)

        foundAllMandatories = true
        for key in Object.keys(@stores)
            if @stores[key].button.shops.length == 0 && @stores[key].mandatory
                foundAllMandatories = false

        $(@storeList).find(".back-store").remove() # .hide() if not @isProfile

        canContinue = @isProfile or ((hasFilledShops and (!hasEbay or (hasEbay and hasPaypal)) and foundAllMandatories) or (@isOffline and @allowFinishOfflineWizardWithoutMarketplaces) or (!@isOffline and @allowFinishOnlineWizardWithoutMarketplaces))
        @storeList.find('.continue').toggleClass 'disabled', !canContinue
        @handleMandatoryText(hasFilledShops, canContinue, ebayPaypalRuleMessageVisible)
        
        if @isOffline
            sShow = '.offline_entry_message'
            sRemove = '.online_entry_message'

            @storeList.find('.importantnumber').text '£150,000'

            if @isProfile
                @storeList.find('.btn-showmore').remove()
            else
                @storeList.find('.btn-showmore').show()
        else
            sShow = '.online_entry_message'
            sRemove = '.offline_entry_message'

            @storeList.find('.btn-showmore').remove()

        @storeList.find(sShow).show()
        @storeList.find(sRemove).remove()

        if @isProfile
            sShow = ".profile_message"
            sRemove = ".wizard_message"
        else
            sShow = ".wizard_message"
            sRemove = ".profile_message"

        @storeList.find(sShow).show()
        @storeList.find(sRemove).remove()

        accountsList = @storeList.find('.accounts-list')

        accountsList.empty()

        sActiveField = 'Active' + (if @isProfile then 'Dashboard' else 'Wizard') + (if @isOffline then 'Offline' else 'Online')
        sPriorityField = 'Priority' + (if @isOffline then 'Offline' else 'Online')

        relevantMpGroups = []

        for grpid, grp of @mpGroups when grp[sActiveField]
            relevantMpGroups.push grp

        relevantMpGroups = _.sortBy relevantMpGroups, (g) -> g[sPriorityField]

        bFirst = true

        for grp in relevantMpGroups
            if bFirst
                bFirst = false
                sGroupClass = 'first'
            else
                sGroupClass = 'following'

            grpui = @storeList
                .find('.marketplace-group-template')
                .clone()
                .removeClass('marketplace-group-template hide')
                .addClass(sGroupClass)
                .appendTo(accountsList)

            $('.group-title', grpui).text grp.DisplayName
            @mpGroups[grp.Id].ui = grpui

        for shop in sortedShopsByNumOfShops when shop.active
            oTarget = if @mpGroups[shop.groupid] and @mpGroups[shop.groupid].ui then @mpGroups[shop.groupid].ui else accountsList

            if @isProfile
                sBtnClass = 'marketplace-button-profile'
            else
                sBtnClass = @extractBtnClass oTarget

            shop.button.render().$el.addClass('marketplace-button ' + sBtnClass).appendTo oTarget

        if @isOffline and not @isProfile
            @storeList.find('.marketplace-button-more, .marketplace-group.following').hide()

        @storeList.appendTo @$el

        EzBob.UiAction.registerView @

        @amazonMarketplaces.trigger "reset"
        @ebayStores.trigger "reset"
        @$el.find("img[rel]").setPopover "left"
        @$el.find("li[rel]").setPopover "left"
        this
    # end of render

    showMoreAccounts: ->
        @storeList.find('.btn-showmore').remove()
        @storeList.find('.marketplace-button-more, .marketplace-group.following').show()
    # end of showMoreAccounts

    extractBtnClass: (jqTarget) ->
        sClass = jqTarget.attr('data-last-button-class') || 'pull-right'

        sClass = if sClass == 'pull-right' then 'pull-left' else 'pull-right'

        jqTarget.attr 'data-last-button-class', sClass

        sClass += ' marketplace-button-' + if $('.marketplace-button-less', jqTarget).length < 2 then 'less' else 'more'

        sClass
    # end of extractBtnClass

    marketplacesChanged: ->
        @$el.find(".wizard-top-notification h2").text "Add more shops to get more cash!"  if @ebayStores.length > 0 or @amazonMarketplaces.length > 0

    connect: (storeName) ->
        EzBob.CT.recordEvent "ct:storebase.shops.connect", storeName
        @$el.find(">div").hide()
        storeView = @stores[storeName].view
        storeView.render().$el.appendTo @$el

        oFieldStatusIcons = storeView.$el.find 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        storeView.$el.show()
        @oldTitle = $(document).attr("title")
        @setDocumentTitle storeView
        @setFocus storeName

        event.preventDefault()
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

    completed: (name) ->
        @shopConnected(name)
        @render()
        @trigger "completed"

    back: ->
        @$el.find(">div").hide()
        @storeList.show()
        $(document).attr "title", @oldTitle
        @updateEarnedPoints()
        false

    next: ->
        return if @$el.find(".continue").hasClass("disabled")
        $.post window.gRootPath + 'CustomerDetails/LinkAccountsComplete'
        @trigger "next"
        EzBob.App.trigger "clear"
        false

    ready: (name) ->
        @trigger "ready", name
        unless @isReady
            @isReady = true
            @$el.find(".continue").show()

    updateEarnedPoints: ->
        $.getJSON("#{window.gRootPath}Customer/Wizard/EarnedPointsStr").done (data) ->
            if data.EarnedPointsStr
                $('#EarnedPoints').text data.EarnedPointsStr

    shopConnected: (name) ->
        @model.safeFetch().done =>
            @stores[name].button.update(@model.get('mpAccounts'))
            @updateEarnedPoints()
            @render()

    handleMandatoryText: (hasFilledShops, canContinue, ebayPaypalRuleMessageVisible) ->
        shouldHide = !hasFilledShops or canContinue or ebayPaypalRuleMessageVisible

        if !shouldHide
            first = true
            text = 'Please add the following accounts in order to continue: '
            for key in Object.keys(@stores)
                if @stores[key].button.shops.length == 0 && @stores[key].mandatory
                    foundAllMandatories = false
                    if !first
                        text += ', '
                    first = false
                    text += key

            @storeList.find('.AddMoreRule').text text

        @storeList.find('.AddMoreRule').toggleClass 'hide', shouldHide
