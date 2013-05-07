﻿root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.AmazonStoreInfoView extends Backbone.View
    initialize: ->
        EzBob.CT.bindShopToCT this, "amazon"

    render: ->
        @$el.html $("#amazon-store-info").html()
        @form = @$el.find(".AmazonForm")
        @validator = EzBob.validateAmazonForm(@form)
        @marketplaceId = @$el.find("#amazonMarketplaceId")
        @merchantId = @$el.find("#amazonMerchantId")

        oFieldStatusIcons = $ "IMG.field_status"
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        this

    events:
        "click a.go-to-amazon": "enableControls"
        "click a.connect-amazon": "connect"
        "click a.back": "back"
        "click .screenshots": "runTutorial"
        "click a.print": "print"

        'cut    #amazonMarketplaceId': 'marketplaceIdChanged'
        'change #amazonMarketplaceId': 'marketplaceIdChanged'
        'paste  #amazonMarketplaceId': 'marketplaceIdChanged'

        'cut    #amazonMerchantId': 'merchantIdChanged'
        'change #amazonMerchantId': 'merchantIdChanged'
        'paste  #amazonMerchantId': 'merchantIdChanged'

        'change input': 'inputChanged'

    enableControls: ->
        @$el.find('#amazonMarketplaceId, #amazonMerchantId').removeAttr('disabled')

    marketplaceIdChanged: ->
        @$el.find('#amazonMarketplaceIdImage').field_status('set', if EzBob.Validation.element(@validator, @marketplaceId) then 'ok' else 'fail')

    merchantIdChanged: ->
        @$el.find('#amazonMerchantIdImage').field_status('set', if EzBob.Validation.element(@validator, @merchantId) then 'ok' else 'fail')

    inputChanged: (sIcon) ->
        enabled =  EzBob.Validation.checkForm(@validator) 
        #enabled = EzBob.Validation.element(@validator, @marketplaceId) and @merchantId.val().length > 10 and @merchantId.val().length < 15
        @$el.find("a.connect-amazon").toggleClass('disabled', !enabled)

    runTutorial: ->
        div = $("<div/>")
        content = $("#amazon-gallery-container")
        div.html content.html()
        div.find(".amazon-tutorial-slider").attr("id", "amazon-tutorial-slider" + (new Date().getTime())).show()
        div.dialog
            width: "960"
            height: "573"
            modal: true
            draggable: false
            resizable: false
            close: ->
                div.empty()

            dialogClass: "amazon-tutor-dlg"
            title: "Link Your Amazon Shop to EZBOB"

        div.find(".amazon-tutorial-slider").coinslider
            width: 930
            height: 471
            delay: 1000000
            effect: "rain"
            sDelay: 30
            titleSpeed: 50
            links: false

        false

    back: ->
        @trigger "back"
        false

    connect: (e) ->
        unless @validator.form()
            EzBob.App.trigger "error", "Please enter a valid Merchant ID"
            return false

        marketplaceId = @$el.find("#amazonMarketplaceId")
        merchantId = @$el.find("#amazonMerchantId")
        
        return false if @$el.find("a.connect-amazon").hasClass("disabled")
        
        @blockBtn true

        $.post(window.gRootPath + "Customer/AmazonMarketplaces/ConnectAmazon",
            marketplaceId: marketplaceId.val()
            merchantId: merchantId.val()
        ).success((result) =>
            @blockBtn false
            if result.error
                EzBob.App.trigger "error", result.error
                @trigger "back"
                return

            EzBob.App.trigger "info", result.msg
            @trigger "completed"
            @trigger 'back'
            marketplaceId.val ""
            merchantId.val ""
        ).error ->
            EzBob.App.trigger "error", "Amazon Account Adding Failed"

        false

    print: ->
        window.print()
        false

    blockBtn: (isBlock) ->
        BlockUi (if isBlock then "on" else "off")
        @$el.find("connect-amazon").toggleClass "disabled", isBlock

    getDocumentTitle: ->
        "Link Amazon Account"

class EzBob.AmazonStoreModel extends Backbone.Model
    defaults:
        marketplaceId: null

class EzBob.AmazonStoreModels extends Backbone.Collection
    model: EzBob.AmazonStoreModel
    url: "#{window.gRootPath}Customer/AmazonMarketPlaces"

class EzBob.AmazonButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: "Amazon", logoText: "", shops: @model})

    update: ->
        @model.fetch().done -> EzBob.App.trigger 'ct:storebase.shop.connected'
