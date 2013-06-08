﻿root = exports ? this
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
        @ekmAccounts.fetch().done => @render()
        @ekmButtonView = new EzBob.EKMAccountButtonView(model: @ekmAccounts)
        @EKMAccountInfoView = new EzBob.EKMAccountInfoView(model: @ekmAccounts)
        
        @freeAgentAccounts = new EzBob.FreeAgentAccounts()
        @freeAgentAccounts.fetch().done => @render()
        @freeAgentButtonView = new EzBob.FreeAgentAccountButtonView(model: @freeAgentAccounts)
        @FreeAgentAccountInfoView = new EzBob.FreeAgentAccountInfoView(model: @freeAgentAccounts)
        
        @volusionAccounts = new EzBob.VolusionAccounts()
        @volusionAccounts.fetch().done => @render()
        @volusionButtonView = new EzBob.VolusionAccountButtonView(model: @volusionAccounts)
        @volusionAccountInfoView = new EzBob.VolusionAccountInfoView(model: @volusionAccounts)

        @PayPointAccounts = new EzBob.PayPointAccounts()
        @PayPointAccounts.fetch().done => @render()
        @PayPointButtonView = new EzBob.PayPointAccountButtonView(model: @PayPointAccounts)
        @PayPointAccountInfoView = new EzBob.PayPointAccountInfoView(model: @PayPointAccounts)

        @YodleeAccounts = new EzBob.YodleeAccounts()
        @YodleeAccounts.fetch().done => @render()
        @YodleeButtonView = new EzBob.YodleeAccountButtonView(model: @YodleeAccounts)
        @YodleeAccountInfoView = new EzBob.YodleeAccountInfoView(model: @YodleeAccounts)

        @payPalAccounts = new EzBob.PayPalAccounts(@model.get("paypalAccounts"))
        @payPalAccounts.fetch().done => @render()
        @PayPalButtonView = new EzBob.PayPalButtonView(model: @payPalAccounts)
        @PayPalInfoView = new EzBob.PayPalInfoView(model: @payPalAccounts)

        @playAccounts = new EzBob.PlayAccounts()
        @playAccounts.fetch().done => @render()
        @playButtonView = new EzBob.PlayAccountButtonView(model: @playAccounts)
        @playAccountInfoView = new EzBob.PlayAccountInfoView(model: @playAccounts)

        @stores =
            "eBay":
                view: @EbayStoreView
                button: @EbayButtonView
                active: 0
                priority: 0
            "Amazon":
                view: @AmazonStoreInfoView
                button: @AmazonButtonView
                active: 0
                priority: 1
            "paypal": 
                view: @PayPalInfoView
                button : @PayPalButtonView
                active: 1
                priority: 2
            "EKM":
                view: @EKMAccountInfoView
                button: @ekmButtonView
                active: 0
                priority: 3
            "Volusion":
                view: @volusionAccountInfoView
                button: @volusionButtonView
                active: 0
                priority: 4
            "PayPoint":
                view: @PayPointAccountInfoView
                button: @PayPointButtonView
                active: 0
                priority: 5
            "Play":
                view: @playAccountInfoView
                button: @playButtonView
                active: 0
                priority: 6
            "Yodlee":
                view: @YodleeAccountInfoView
                button: @YodleeButtonView
                active: 0
                priority: 7
            "FreeAgent":
                view: @FreeAgentAccountInfoView
                button: @freeAgentButtonView
                active: 0
                priority: 8

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
