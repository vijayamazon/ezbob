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
        @render()
        @trigger "completed"

    back: ->
        @$el.find(">div").hide()
        @storeList.show()
        $(document).attr "title", @oldTitle
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

    render: ->
        that = this
        accountsList = @storeList.find(".accounts-list")

        sortedShopsByPriority = _.sortBy(@stores, (s) -> s.priority)
        sortedShopsByNumOfShops = _.sortBy(sortedShopsByPriority, (s) -> -s.button.model.length)

        hasFilledShops = sortedShopsByNumOfShops[0].button.model.length > 0
        @$el.find(".next").toggleClass("disabled", !hasFilledShops)

        for shop in sortedShopsByNumOfShops when shop.active 
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
        $.colorbox.close()

        switch storeName
            when "EKM"
                @$el.find("#ekm_login").focus()
            when "Volusion"
                sText = $("#header_description").text().trim()
                if "" == @$el.find("#volusion_login").val()
                    @$el.find("#volusion_login").val(sText.substr(0, sText.indexOf(" ")))
                @$el.find("#volusion_shopname").focus()
            when "PayPoint"
                @$el.find("#payPoint_login").focus()
            when "bank-account"
                @$el.find("#AccountNumber").focus()
            else

    setDocumentTitle: (storeName) ->
        switch storeName
            when "Amazon"
                $(document).attr "title", "Wizard 2 Amazon: Link Your Amazon Shop | EZBOB"
            when "eBay"
                $(document).attr "title", "Wizard 2 Ebay: Link Your Ebay Shop | EZBOB"
            when "bank-account"
                $(document).attr "title", "Wizard 3 Bank: Bank Account Details | EZBOB"
            when "paypal"
                $(document).attr "title", "Wizard 3 PayPal: Link Your PayPal Account | EZBOB"
            when "EKM"
                $(document).attr "title", "Wizard 3 EKM: Link Your EKM Account | EZBOB"
            when "PayPoint"
                $(document).attr "title", "Wizard 3 PayPoint: Link Your PayPoint Account | EZBOB"
            when "Volusion"
                $(document).attr "title", "Wizard 3 Volusion: Link Your Volusion Account | EZBOB"
            else

    close: ->
        this