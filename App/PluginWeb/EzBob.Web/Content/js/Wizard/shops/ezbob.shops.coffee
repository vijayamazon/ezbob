root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoView extends EzBob.StoreInfoBaseView
    attributes:
        class: "stores-view"

    initialize: ->
        @ebayStores = @model.get("ebayStores")
        @EbayButtonView = new EzBob.EbayButtonView(model: @ebayStores)
        @EbayStoreView = new EzBob.EbayStoreInfoView()
        @ebayStores.on "reset change", @marketplacesChanged, this
        @ebayStores.on "sync", @render, this

        @amazonMarketplaces = @model.get("amazonMarketplaces")
        @AmazonButtonView = new EzBob.AmazonButtonView(model: @amazonMarketplaces)
        @AmazonStoreInfoView = new EzBob.AmazonStoreInfoView()
        @amazonMarketplaces.on "reset change", @marketplacesChanged, this
        @amazonMarketplaces.on "sync", @render, this

        @ekmAccounts = new EzBob.EKMAccounts()
        @ekmAccounts.safeFetch().done => @render()
        @ekmButtonView = new EzBob.EKMAccountButtonView(model: @ekmAccounts)
        @EKMAccountInfoView = new EzBob.EKMAccountInfoView(model: @ekmAccounts)

        @freeAgentAccounts = new EzBob.FreeAgentAccounts()
        @freeAgentAccounts.safeFetch().done => @render()
        @freeAgentButtonView = new EzBob.FreeAgentAccountButtonView(model: @freeAgentAccounts)
        @FreeAgentAccountInfoView = new EzBob.FreeAgentAccountInfoView(model: @freeAgentAccounts)

        @sageAccounts = new EzBob.SageAccounts()
        @sageAccounts.safeFetch().done => @render()
        @sageButtonView = new EzBob.SageAccountButtonView(model: @sageAccounts)
        @sageAccountInfoView = new EzBob.SageAccountInfoView(model: @sageAccounts)

        @PayPointAccounts = new EzBob.PayPointAccounts()
        @PayPointAccounts.safeFetch().done => @render()
        @PayPointButtonView = new EzBob.PayPointAccountButtonView(model: @PayPointAccounts)
        @PayPointAccountInfoView = new EzBob.PayPointAccountInfoView(model: @PayPointAccounts)

        @YodleeAccounts = new EzBob.YodleeAccounts()
        @YodleeAccounts.safeFetch().done => @render()
        @YodleeButtonView = new EzBob.YodleeAccountButtonView(model: @YodleeAccounts)
        @YodleeAccountInfoView = new EzBob.YodleeAccountInfoView(model: @YodleeAccounts)

        @payPalAccounts = new EzBob.PayPalAccounts(@model.get("paypalAccounts"))
        @payPalAccounts.safeFetch().done => @render()
        @PayPalButtonView = new EzBob.PayPalButtonView(model: @payPalAccounts)
        @PayPalInfoView = new EzBob.PayPalInfoView(model: @payPalAccounts)

        aryCGAccounts = $.parseJSON $('div#cg-account-list').text()

        for accountTypeName, ignore of aryCGAccounts
            lc = accountTypeName.toLowerCase()

            acc = new EzBob.CGAccounts [], accountType: accountTypeName
            acc.safeFetch().done => @render()

            this[lc + 'Accounts'] = acc
            this[lc + 'ButtonView'] = new EzBob.CGAccountButtonView model: acc, accountType: accountTypeName
            this[lc + 'AccountInfoView'] = new EzBob.CGAccountInfoView model: acc, accountType: accountTypeName

        @stores =
            "eBay":
                view: @EbayStoreView
                button: @EbayButtonView
            "Amazon":
                view: @AmazonStoreInfoView
                button: @AmazonButtonView
            "paypal": 
                view: @PayPalInfoView
                button : @PayPalButtonView
            "EKM":
                view: @EKMAccountInfoView
                button: @ekmButtonView
            "PayPoint":
                view: @PayPointAccountInfoView
                button: @PayPointButtonView
            "Yodlee":
                view: @YodleeAccountInfoView
                button: @YodleeButtonView
            "FreeAgent":
                view: @FreeAgentAccountInfoView
                button: @freeAgentButtonView
            "Sage":
                view: @sageAccountInfoView
                button: @sageButtonView

        for accountTypeName, vendorInfo of aryCGAccounts
            lc = accountTypeName.toLowerCase()

            @stores[accountTypeName] =
                view: this[lc + 'AccountInfoView']
                button: this[lc + 'ButtonView']

        @isOffline = @model.get 'isOffline'
        @isProfile = @model.get 'isProfile'
        for j in EzBob.Config.MarketPlaces
            storeTypeName = if j.Name == "Pay Pal" then "paypal" else j.Name
            if @stores[storeTypeName]
                @stores[storeTypeName].active = if @isProfile then (if @isOffline then j.ActiveDashboardOffline else j.ActiveDashboardOnline) else (if @isOffline then j.ActiveWizardOffline else j.ActiveWizardOnline)
                @stores[storeTypeName].priority = if @isOffline then j.PriorityOffline else j.PriorityOnline
                @stores[storeTypeName].ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].button.ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].mandatory = if @isOffline then j.MandatoryOffline else j.MandatoryOnline

        @name = "shops"
        super()

    render: ->
        super()
        @amazonMarketplaces.trigger "reset"
        @ebayStores.trigger "reset"
        @$el.find("img[rel]").setPopover "left"
        @$el.find("li[rel]").setPopover "left"
        this

    marketplacesChanged: ->
        @$el.find(".wizard-top-notification h2").text "Add more shops to get more cash!"  if @ebayStores.length > 0 or @amazonMarketplaces.length > 0
