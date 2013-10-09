root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoBaseView extends Backbone.View
    isOffline: false

    initialize: ->
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
        $.colorbox.close()

        if @isOffline
            @$el.find('.entry_message').empty()
                .append('You must link your ')
                .append('<span class=green>HM Revenue & Customs</span>')
                .append(' account to be approved for a loan.')
                .append('<div>Please note, the more accounts you link, the more funds you can get.</div>')

            @$el.find('.importantnumber').text '£200,000'

            _.each @stores, (s, sShopName) ->
                switch sShopName
                    when "HMRC" then s.priority = 1
                    when "Yodlee" then s.priority = 2
                    else s.priority += 4

        that = this
        accountsList = @storeList.find(".accounts-list")

        sortedShopsByPriority = _.sortBy(@stores, (s) -> s.priority)
        sortedShopsByNumOfShops = _.sortBy(sortedShopsByPriority, (s) -> -s.button.model.length)

        hasFilledShops = sortedShopsByNumOfShops[0].button.model.length > 0

        hasEbay = @stores.eBay.button.model.length > 0
        hasPaypal = @stores.paypal.button.model.length > 0

        hasOtherThanYodleeAndFreeAgentAndSage = false
        for shopName, shopInfo of @stores
            if shopName is 'Yodlee' or shopName is 'FreeAgent' or shopName is 'Sage'
                continue
            if shopInfo.button.model.length > 0
                hasOtherThanYodleeAndFreeAgentAndSage = true
                break

        hasOnlyYodleeAndFreeAgentAndSage = (@stores.Yodlee.button.model.length > 0 or @stores.FreeAgent.button.model.length > 0 or @stores.Sage.button.model.length > 0) and not hasOtherThanYodleeAndFreeAgentAndSage

        @$el.find(".eBayPaypalRule").toggleClass("hide", not hasEbay or hasPaypal)
        @$el.find(".YodleeAndFreeAgentAndSageRule").toggleClass("hide", not hasOnlyYodleeAndFreeAgentAndSage)

        if @isOffline
            hasHmrc = @stores.HMRC.button.model.length > 0
            @$el.find(".next").toggleClass("disabled", !hasHmrc)
        else
            @$el.find(".next").toggleClass("disabled", !hasFilledShops or hasOnlyYodleeAndFreeAgentAndSage or (hasEbay and not hasPaypal))

        for shop in sortedShopsByNumOfShops when shop.active 
            shop.button.render().$el.appendTo accountsList
            shop.view.render().$el.hide().appendTo that.$el

        @storeList.appendTo @$el
        #that.ready() if @stores["bank-account"].button.model.get("bankAccountAdded") if @stores["bank-account"]?

        this

    events:
        "click a.connect-store": "close"
        "click a.next": "next"
        "click a.back-step": "previousClick"

    previousClick: ->
        @trigger "previous"
        false

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