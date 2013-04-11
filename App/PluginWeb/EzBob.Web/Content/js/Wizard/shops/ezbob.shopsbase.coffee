root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoBaseView extends Backbone.View
    initialize: ->
        that = this
        _.each @stores, (store) ->
            store.button.on "selected", that.connect, that
            store.view.on "completed", _.bind(that.completed, that, store.button.name)
            store.view.on "back", that.back, that
            store.button.on "ready", that.ready, that

        @storeList = $($("#store-info").html())
        EzBob.App.on "ct:storebase." + @name + ".connect", @connect, this
        @isReady = false

    completed: (name) ->
        @stores[name].button.update()
        @$el.find(">div").hide()
        @storeList.show()
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
        buttonList = @storeList.find(".buttons-list")
        row = null
        _.each @stores, (store) ->
            return unless store.active
            store.button.render().$el.appendTo buttonList
            store.view.render().$el.hide().appendTo that.$el

        @storeList.appendTo @$el
        that.ready()    if @stores["bank-account"].button.model.get("bankAccountAdded")    if @stores["bank-account"]?
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