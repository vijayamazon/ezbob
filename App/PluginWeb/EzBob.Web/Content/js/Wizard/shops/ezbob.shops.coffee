root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoView extends EzBob.StoreInfoBaseView
    attributes:
        class: "stores-view"

    initialize: ->
        @ebayStores = @model.get("ebayStores")
        @amazonMarketplaces = @model.get("amazonMarketplaces")
        @EbayButtonView = new EzBob.EbayButtonView(model: @ebayStores)
        @EbayStoreView = new EzBob.EbayStoreInfoView()
        @AmazonButtonView = new EzBob.AmazonButtonView(model: @amazonMarketplaces)
        @AmazonStoreInfoView = new EzBob.AmazonStoreInfoView()
        @amazonMarketplaces.on "reset change", @marketplacesChanged, this
        @ebayStores.on "reset change", @marketplacesChanged, this

        @ekmAccounts = new EzBob.EKMAccounts()
        @ekmAccounts.fetch()
        @ekmButtonView = new EzBob.EKMAccountButtonView(model: @ekmAccounts)
        @EKMAccountInfoView = new EzBob.EKMAccountInfoView(model: @ekmAccounts)
        
        @volusionAccounts = new EzBob.VolusionAccounts()
        @volusionAccounts.fetch()
        @volusionButtonView = new EzBob.VolusionAccountButtonView(model: @volusionAccounts)
        @volusionAccountInfoView = new EzBob.VolusionAccountInfoView(model: @volusionAccounts)

        @PayPointAccounts = new EzBob.PayPointAccounts()
        @PayPointAccounts.fetch()
        @PayPointButtonView = new EzBob.PayPointAccountButtonView(model: @PayPointAccounts)
        @PayPointAccountInfoView = new EzBob.PayPointAccountInfoView(model: @PayPointAccounts)

        @payPalAccounts = new EzBob.PayPalAccounts(@model.get("paypalAccounts"))
        #@payPalAccounts.on('fetch reset change', @accountChanged, this)
        @PayPalButtonView = new EzBob.PayPalButtonView model: @payPalAccounts 
        @PayPalInfoView = new EzBob.PayPalInfoView()

        @stores =
            "eBay":
                view: @EbayStoreView
                button: @EbayButtonView
                active: 0
            "Amazon":
                view: @AmazonStoreInfoView
                button: @AmazonButtonView
                active: 0
            "EKM":
                view: @EKMAccountInfoView
                button: @ekmButtonView
                active: 0
            "PayPoint":
                view: @PayPointAccountInfoView
                button: @PayPointButtonView
                active: 0
            "Volusion":
                view: @volusionAccountInfoView
                button: @volusionButtonView
                active: 0
            "paypal": 
                view: @PayPalInfoView
                button : @PayPalButtonView
                active: 1

         for j in EzBob.Config.ActiveMarketPlaces
             if @stores[j]
                 @stores[j].active = 1

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
