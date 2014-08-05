root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoView extends Backbone.View
    attributes:
        class: "stores-view"

    isOffline: -> @fromCustomer 'IsOffline'

    isProfile: -> @fromCustomer 'IsProfile'

    fromCustomer: (sPropName) ->
        oCustomer = @model.get 'customer'
        return false unless oCustomer
        return oCustomer.createdInProfile if sPropName == 'IsProfile'
        return oCustomer.get sPropName

    initialize: ->
        @renderExecuted = false

        @ebayStores = new EzBob.EbayStoreModels()
        @EbayStoreView = new EzBob.EbayStoreInfoView()
        @ebayStores.on "reset change", @marketplacesChanged, this

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
        @YodleeAccountInfoView = new EzBob.YodleeAccountInfoView(model: @YodleeAccounts, isProfile: @isProfile())

        @payPalAccounts = new EzBob.PayPalAccounts(@model.get("paypalAccounts"))
        @PayPalInfoView = new EzBob.PayPalInfoView(model: @payPalAccounts)

        @companyFilesAccounts = new EzBob.CompanyFilesAccounts(@model.get("companyUploadAccounts"))
        @companyFilesAccountInfoView =  new EzBob.CompanyFilesAccountInfoView(model: @companyFilesAccounts)

        for accountTypeName, ignore of EzBob.CgVendors.all()
            lc = accountTypeName.toLowerCase()

            acc = new EzBob.CgAccounts [], accountType: accountTypeName

            this[lc + 'Accounts'] = acc

            if lc is 'hmrc'
                this[lc + 'AccountInfoView'] = new EzBob.HmrcAccountInfoView
                    model: acc,
                    companyRefNum: (@fromCustomer('CompanyInfo') or {}).ExperianRefNum
            else
                this[lc + 'AccountInfoView'] = new EzBob.CgAccountInfoView model: acc, accountType: accountTypeName

        @stores =
            "eBay": view: @EbayStoreView
            "Amazon": view: @AmazonStoreInfoView
            "paypal": view: @PayPalInfoView
            "EKM": view: @EKMAccountInfoView
            "PayPoint": view: @PayPointAccountInfoView
            "Yodlee": view: @YodleeAccountInfoView
            "FreeAgent": view: @FreeAgentAccountInfoView
            "Sage": view: @sageAccountInfoView
            "CompanyFiles": view: @companyFilesAccountInfoView

        for accountTypeName, vendorInfo of EzBob.CgVendors.all()
            lc = accountTypeName.toLowerCase()

            @stores[accountTypeName] =
                view: this[lc + 'AccountInfoView']

        if typeof ordpi is 'undefined'
            ordpi = Math.random() * 10000000000000000
        @storeList = $(_.template($("#store-info").html(), {ordpi : ordpi}))

        EzBob.App.on 'ct:storebase.shops.connect', @connect, @

        @isReady = false
    # end of initialize

    events:
        'click a.connect-store': 'close'
        'click .btn-showmore': 'toggleShowMoreAccounts'
        'click .btn-go-to-link-accounts': 'showLinkAccountsForm'
        'click .btn-take-quick-offer': 'takeQuickOffer'
        'click .btn-back-to-quick-offer': 'backToQuickOffer'

    backToQuickOffer: ->
        if @shouldRemoveQuickOffer()
            @storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove()
        else
            @storeList.find('.link-accounts-form').addClass 'hide'
            @storeList.find('.quick-offer-form').removeClass 'hide'
    # end of backToQuickOffer

    takeQuickOffer: ->
        xhr = $.post window.gRootPath + 'CustomerDetails/TakeQuickOffer'

        xhr.done ->
            EzBob.App.trigger 'clear'
            setTimeout((-> window.location = window.gRootPath + 'Customer/Profile#GetCash'), 500)

        false
    # end of takeQuickOffer

    showLinkAccountsForm: ->
        @storeList.find('.quick-offer-form').addClass 'hide'
        @storeList.find('.link-accounts-form').removeClass 'hide'
    # end of showLinkAccountsForm

    render: ->
        UnBlockUi()
        @mpGroups = {}
        for grp in EzBob.Config.MarketPlaceGroups
            @mpGroups[grp.Id] = grp
            grp.ui = null
        
        for j in EzBob.Config.MarketPlaces
            storeTypeName = if j.Name == "Pay Pal" then "paypal" else j.Name
            if @stores[storeTypeName]
                @stores[storeTypeName].active = if @isProfile() then (if @isOffline() then j.ActiveDashboardOffline else j.ActiveDashboardOnline) else (if @isOffline() then j.ActiveWizardOffline else j.ActiveWizardOnline)
                @stores[storeTypeName].priority = if @isOffline() then j.PriorityOffline else j.PriorityOnline
                @stores[storeTypeName].ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].button = new EzBob.StoreButtonView({ name: storeTypeName, mpAccounts: @model, description: j.Description })
                @stores[storeTypeName].button.ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].mandatory = if @isOffline() then j.MandatoryOffline else j.MandatoryOnline
                @stores[storeTypeName].groupid = if j.Group? then j.Group.Id else 0

        for name, store of @stores
            store.button.on "selected", @connect, this
            store.view.on "completed", _.bind(@completed, this, store.button.name)
            store.view.on "back", @back, this
            store.button.on "ready", @ready, this

        @canContinue()

        if @renderExecuted
            if @shouldRemoveQuickOffer()
                @storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove()
                @storeList.find('.link-accounts-form').removeClass 'hide'
        else
            @storeList.find('.quick-offer-form, .link-accounts-form').addClass 'hide'

            if @shouldShowQuickOffer()
                @storeList.find('.quick-offer-form').removeClass 'hide'
                @renderQuickOfferForm()
            else
                @storeList.find('.link-accounts-form').removeClass 'hide'
                if @shouldRemoveQuickOffer()
                    @storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove()

        @renderExecuted = true

        @showOrRemove()

        accountsList = @storeList.find('.accounts-list')

        accountsList.empty()

        sActiveField = 'Active' + (if @isProfile() then 'Dashboard' else 'Wizard') + (if @isOffline() then 'Offline' else 'Online')
        sPriorityField = 'Priority' + (if @isOffline() then 'Offline' else 'Online')

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

        sortedShopsByPriority = _.sortBy(@stores, (s) -> s.priority)
        for shop in sortedShopsByPriority when shop.active
            oTarget = if @mpGroups[shop.groupid] and @mpGroups[shop.groupid].ui then @mpGroups[shop.groupid].ui else accountsList

            if @isProfile()
                sBtnClass = 'marketplace-button-profile'
            else
                sBtnClass = @extractBtnClass oTarget

            shop.button.render().$el.addClass('marketplace-button ' + sBtnClass).appendTo oTarget

        if @isOffline() and not @isProfile()
            @storeList.find('.marketplace-button-more, .marketplace-group.following').hide()

        if @storeList.find('.marketplace-group.following .marketplace-button-full, .marketplace-button-full.marketplace-button-more').length
            @showMoreAccounts()

        @storeList.appendTo @$el

        EzBob.UiAction.registerView @

        @amazonMarketplaces.trigger "reset"
        @ebayStores.trigger "reset"
        @$el.find("img[rel]").setPopover "left"
        @$el.find("li[rel]").setPopover "left"
        
        showMoreBtn = @$el.find('.btn-showmore')
        showMoreBtn.hover(
            ((evt) -> $('.onhover', this).animate({ top: 0,      opacity: 1 })),
            ((evt) -> $('.onhover', this).animate({ top: '60px', opacity: 0 }))
        )

        this
    # end of render

    renderQuickOfferForm: ->
        @storeList.find('.immediate-offer .amount, .quick-offer-amount').text EzBob.formatPoundsNoDecimals @quickOffer.Amount
        @storeList.find('.immediate-offer .term').text @quickOffer.ImmediateTerm + ' months'
        @storeList.find('.immediate-offer .interest-rate .value').text EzBob.formatPercentsWithDecimals @quickOffer.ImmediateInterestRate

        @setQuickOfferFormOnHover @storeList.find('.immediate-offer .btn-take-quick-offer')

        @storeList.find('.potential-offer .amount').text EzBob.formatPoundsNoDecimals @quickOffer.PotentialAmount
        @storeList.find('.potential-offer .term').text 'up to ' + @quickOffer.PotentialTerm + ' months'
        @storeList.find('.potential-offer .interest-rate .value').text EzBob.formatPercentsWithDecimals @quickOffer.PotentialInterestRate

        @setQuickOfferFormOnHover @storeList.find('.potential-offer .btn-go-to-link-accounts')
    # end of renderQuickOfferForm

    setQuickOfferFormOnHover: (oLinkBtn) ->
        oLinkBtn.hover(
            ((evt) ->
                oLinkBtn = $ @
                nHeight = oLinkBtn.outerHeight()
                nHeight = oLinkBtn.outerWidth()

                onHover = oLinkBtn.parent().find '.onhover'

                onHover.css
                    position: 'absolute'
                    top: nHeight
                    height: nHeight
                    left: 0
                    width: nHeight
                    display: 'block'

                onHover.animate { top: 0, opacity: 1 }
            ), # on hover in
            ((evt) ->
                oLinkBtn = $ @
                onHover = oLinkBtn.parent().find '.onhover'
                nHeight = oLinkBtn.outerHeight()
                onHover.animate { top: nHeight, opacity: 0 }
            ) # on hover out
        )
    # end of setQuickOfferFormOnHover

    shouldRemoveQuickOffer: () ->
        return true unless @quickOffer
        return moment.utc().diff(moment.utc(@quickOffer.ExpirationDate)) > 0
    # end of shouldRemoveQuickOffer

    shouldShowQuickOffer: () ->
        return false if @isProfile()

        @quickOffer = @fromCustomer 'QuickOffer'

        return false unless @quickOffer

        @requestedAmount = @fromCustomer 'RequestedAmount'

        return false unless @requestedAmount

        return moment.utc().diff(moment.utc(@quickOffer.ExpirationDate)) < 0
    # end of shouldShowQuickOffer

    showOrRemove: ->
        isOffline = @isOffline()
        isProfile = @isProfile()

        $(@storeList).find('.back-store').remove() # .hide() if not isProfile

        sShow = ''
        sRemove = ''

        @storeList.find('.marketplace-button.show-more').show()

        if isOffline
            sShow = '.offline_entry_message'
            sRemove = '.online_entry_message'

            @storeList.find('.importantnumber').text '£150,000'

            if isProfile
                @storeList.find('.marketplace-button.show-more').hide()
                @storeList.find('.AddMoreRuleBottom').removeClass 'hide'
            else
                @storeList.find('.btn-showmore').show()
        else
            sShow = '.online_entry_message'
            sRemove = '.offline_entry_message'

            @storeList.find('.marketplace-button.show-more').hide()
            @storeList.find('.AddMoreRuleBottom').removeClass 'hide'

        @storeList.find(sShow).show()
        @storeList.find(sRemove).remove()

        if isProfile
            sShow = '.profile_message'
            sRemove = '.wizard_message'
        else
            sShow = '.wizard_message'
            sRemove = '.profile_message'

        @storeList.find(sShow).show()
        @storeList.find(sRemove).remove()
    # end of showOrRemove

    toggleShowMoreAccounts: ->
        oBtn = @storeList.find('.btn-showmore')

        if oBtn.attr('data-current') is 'more'
            @showMoreAccounts()
        else
            @showLessAccounts()
    # end of toggleShowMoreAccounts

    showLessAccounts: ->
        oBtn = @storeList.find('.btn-showmore')

        oBtn.attr 'data-current', 'more'
        oBtn.find('.caption').text 'Show more account types'
        oBtn.find('.rotate90').html '&laquo;'
        oBtn.find('.onhover-cell').text 'Show more data source connectors'

        @storeList.find('.AddMoreRuleBottom').addClass 'hide'
        @storeList.find('.marketplace-button-more, .marketplace-group.following').hide()
        @storeList.find('.marketplace-button').not('.show-more, .marketplace-button-less').css 'display', 'none'
    # end of showLessAccounts

    showMoreAccounts: ->
        oBtn = @storeList.find('.btn-showmore')

        oBtn.attr 'data-current', 'less'
        oBtn.find('.caption').text 'Show less account types'
        oBtn.find('.rotate90').html '&raquo;'
        oBtn.find('.onhover-cell').text 'Show less data source connectors'

        @storeList.find('.AddMoreRuleBottom').removeClass 'hide'
        @storeList.find('.marketplace-button-more, .marketplace-group.following').show()
        @storeList.find('.marketplace-button').not('.show-more').css 'display', 'table'
    # end of showMoreAccounts

    canContinue: ->
        hasFilledShops = false
        for mpType, oStore of @stores
            if oStore.button.shops.length
                hasFilledShops = true
                break

        hasEbay = @stores.eBay.button.shops.length > 0
        hasPaypal = @stores.paypal.button.shops.length > 0

        @$el.find('.eBayPaypalRule').toggleClass 'hide', not (hasEbay and !hasPaypal)

        canContinue = false

        if @isProfile()
            canContinue = true
        else
            if hasFilledShops and (!hasEbay or (hasEbay and hasPaypal))
                canContinue = true
            else
                sAttrName = if @isOffline() then 'offline' else 'online'
                canContinue = $('#allowFinishWizardWithoutMarketplaces').data(sAttrName).toLowerCase() == 'true'

        @storeList.find('.continue').toggleClass 'disabled', not canContinue
        @storeList.find('.AddMoreRule').toggleClass 'hide', canContinue

        hasFilledShops
    # end of canContinue

    extractBtnClass: (jqTarget) ->
        sClass = 'pull-left'

        if $('.marketplace-button-less', jqTarget).length < 3
            sClass += ' marketplace-button-less'
        else
            sClass += ' marketplace-button-more'

            if @isOffline() and not @isProfile() and $('.marketplace-button-more', jqTarget).length is 0
                sClass += ' marketplace-button-more-first'

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
        false

    setFocus: (storeName) ->
        $.colorbox.close()

        switch storeName
            when "EKM"
                @$el.find("#ekm_login").focus()
            when "PayPoint"
                @$el.find("#payPoint_login").focus()
            else
                if EzBob.CgVendors.pure()[storeName]
                    $('.form_field', '#' + storeName.toLowerCase() + 'Account').first().focus()

    setDocumentTitle: (view) ->
        title = view.getDocumentTitle()
        if title
            $(document).attr "title", "Step 4: #{title} | EZBOB"

    close: ->
        this

    completed: (name) ->
        @shopConnected(name)
        @trigger 'completed'

    back: ->
        @$el.find(">div").hide()
        @storeList.show()
        $(document).attr "title", @oldTitle
        #@updateEarnedPoints()
        false

    ready: (name) ->
        @trigger "ready", name
        unless @isReady
            @isReady = true
            @$el.find(".continue").show()

    #updateEarnedPoints: ->
    #    $.getJSON("#{window.gRootPath}Customer/Wizard/EarnedPointsStr").done (data) ->
    #        if data.EarnedPointsStr
    #            $('#EarnedPoints').text data.EarnedPointsStr

    shopConnected: (name) ->
        @model.get('customer').safeFetch().done =>
            @stores[name].button.update(@fromCustomer 'mpAccounts')
            #@updateEarnedPoints()
            @render()
