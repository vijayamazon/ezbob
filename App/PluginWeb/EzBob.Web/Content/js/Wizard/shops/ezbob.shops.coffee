root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoView extends EzBob.StoreInfoBaseView
    attributes:
        class: "stores-view"

    initialize: ->
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
        for j in EzBob.Config.MarketPlaces
            storeTypeName = if j.Name == "Pay Pal" then "paypal" else j.Name
            if @stores[storeTypeName]
                @stores[storeTypeName].active = if @isProfile then (if @isOffline then j.ActiveDashboardOffline else j.ActiveDashboardOnline) else (if @isOffline then j.ActiveWizardOffline else j.ActiveWizardOnline)
                @stores[storeTypeName].priority = if @isOffline then j.PriorityOffline else j.PriorityOnline
                @stores[storeTypeName].ribbon = if j.Ribbon then j.Ribbon else ""
                @stores[storeTypeName].button = new EzBob.StoreButtonView({ name: storeTypeName, mpAccounts: @model })
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
