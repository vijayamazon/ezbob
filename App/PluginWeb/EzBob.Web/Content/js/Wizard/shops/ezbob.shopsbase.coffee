root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoBaseView extends Backbone.View
    initialize: ->
        that = this
        @storeList = $($("#store-info").html())
        @isReady = false

        _.each @stores, (store) ->
            store.button.on "selected", that.connect, that
            store.view.on "completed", _.bind(that.completed, that, store.button.name)
            store.view.on "back", that.back, that
            store.button.on "ready", that.ready, that

        EzBob.App.on "ct:storebase." + @name + ".connect", @connect, this
        

    completed: (name) ->
        @stores[name].button.update()
        @$el.empty()
        @render()
        @trigger "completed"

    back: ->
        @$el.find(">div").hide()
        @storeList.show()
        $(document).attr "title", @oldTitle
        false

    next: ->
        @trigger "next"
        EzBob.App.trigger "clear"
        false

    ready: (name) ->
        @trigger "ready", name
        unless @isReady
            @isReady = true
            @$el.find(".next").show()

    render: ->
        that = this
        shopsList = @storeList.find(".shops-list")
        accountsList = @storeList.find(".accounts-list")

        sortedShops = _.sortBy(@stores, (s) -> s.button.model.length).reverse()

        for shop in sortedShops when shop.active and shop.isShop is 1
            shop.button.render().$el.appendTo shopsList
            shop.view.render().$el.hide().appendTo that.$el

        for shop in sortedShops when shop.active and shop.isShop is 0
            shop.button.render().$el.appendTo accountsList
            shop.view.render().$el.hide().appendTo that.$el

        @storeList.appendTo @$el
        that.ready() if @stores["bank-account"].button.model.get("bankAccountAdded")    if @stores["bank-account"]?
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
        @stores[storeName].view.$el.show()
        @oldTitle = $(document).attr("title")
        @setDocumentTitle storeName
        @setFocus storeName
        false

    setFocus: (storeName) ->
        switch storeName
            when "ekm"
                @$el.find("#ekm_login").focus()
            when "volusion"
                @$el.find("#volusion_login").focus()
            when "payPoint"
                @$el.find("#payPoint_login").focus()
            when "bank-account"
                @$el.find("#AccountNumber").focus()
            else

    setDocumentTitle: (storeName) ->
        switch storeName
            when "amazon"
                $(document).attr "title", "Wizard 2 Amazon: Link Your Amazon Shop | EZBOB"
            when "ebay"
                $(document).attr "title", "Wizard 2 Ebay: Link Your Ebay Shop | EZBOB"
            when "bank-account"
                $(document).attr "title", "Wizard 3 Bank: Bank Account Details | EZBOB"
            when "paypal"
                $(document).attr "title", "Wizard 3 PayPal: Link Your PayPal Account | EZBOB"
            else

    close: ->
        this