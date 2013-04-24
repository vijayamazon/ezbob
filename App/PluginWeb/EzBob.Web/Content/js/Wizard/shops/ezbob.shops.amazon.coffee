root = exports ? this
root.EzBob = root.EzBob or {}

EzBob.AmazonStoreInfoView = Backbone.View.extend(
    initialize: ->
        EzBob.CT.bindShopToCT this, "amazon"

    render: ->
        @$el.html $("#amazon-store-info").html()
        @form = @$el.find(".AmazonForm")
        @validator = EzBob.validateAmazonForm(@form)
        @inputChanged()
        this

    events:
        "click a.connect-amazon": "connect"
        "click a.back": "back"
        "click .screenshots": "runTutorial"
        "keyup input[type='text']": "inputChanged"
        "change input[type='text']": "inputChanged"
        "click a.print": "print"

    inputChanged: ->
        marketplaceId = @$el.find("#amazonMarketplaceId").val()
        merchantId = @$el.find("#amazonMerchantId").val()
        if marketplaceId.length < 10 or marketplaceId.length > 15 or merchantId.length < 10 or merchantId.length > 15 or not @validator.form()
            @$el.find("a.connect-amazon").addClass "disabled"
            return
        @$el.find("a.connect-amazon").removeClass "disabled"

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
            EzBob.App.trigger "error", "The fields Merchant ID or Marketplace ID are not filled"
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
            EzBob.App.trigger "error", "Amazon account adding failed"

        false

    print: ->
        window.print()
        false

    blockBtn: (isBlock) ->
        BlockUi (if isBlock then "on" else "off")
        @$el.find("connect-amazon").toggleClass "disabled", isBlock
)


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
        @model.fetch()