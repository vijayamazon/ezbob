root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.MarketPlaceModel extends Backbone.Model
    idAttribute: "Id"

    initialize: ->
        @on 'change reset', @recalculate, @
        @recalculate()

    recalculate: ->
        ai = @get 'AnalysisDataInfo'
        accountAge = @get 'AccountAge'
        anualSales = if ai then (ai.TotalSumofOrders12M or ai.TotalSumofOrders6M or ai.TotalSumofOrders3M or ai.TotalSumofOrders1M or 0) * 1 else 0
        inventory = if ai and not isNaN((ai.TotalValueofInventoryLifetime * 1)) then (ai.TotalValueofInventoryLifetime * 1) else "-"

        pp = @get("PayPal")
        if pp
            accountAge = pp.GeneralInfo.Seniority
            anualSales = pp.GeneralInfo.TotalNetInPayments

        age = if (accountAge isnt "-" and accountAge isnt 'undefined') then EzBob.SeniorityFormat(accountAge, 0) else "-"

        @set {age: age, anualSales: anualSales, inventory: inventory}, {silent: true}

class EzBob.Underwriter.MarketPlaces extends Backbone.Collection
    model: EzBob.Underwriter.MarketPlaceModel
    url: -> "#{window.gRootPath}Underwriter/MarketPlaces/Index/#{@customerId}"

class EzBob.Underwriter.MarketPlacesView extends Backbone.Marionette.ItemView
    template: "#marketplace-template"

    initialize: ->
        @model.on "reset change", @render, this
        @rendered = false

    onRender: ->
        @$el.find('.mp-error-description').tooltip(({placement: "bottom"}));
        @$el.find('a[data-bug-type]').tooltip({title: 'Report bug'});
        if @detailView!= undefined
            @detailView.render()

    events:
        "click .reCheckMP": "reCheckmarketplaces"
        "click .reCheck-paypal" : "reCheckPaypal"
        "click tbody tr": "rowClick"
        "click .mp-error-description" : "showMPError"
        "click .renew-token": "renewTokenClicked"

    rowClick: (e) ->
        return if e.target.getAttribute('href')
        return if e.target.tagName is 'I'
        id = e.currentTarget.getAttribute("data-id")
        return unless id
        shop = @model.get(id)

        @detailView = new EzBob.Underwriter.MarketPlaceDetailsView model: @model, currentId: id, customerId: @model.customerId

        EzBob.App.jqmodal.show(@detailView)

        @detailView.on "reCheck", @reCheckmarketplaces, @
        @detailView.on "reCheck-PayPal", @reCheckPaypal, @
        @detailView.on("recheck-token", @renewToken)
        @detailView.customerId = @model.customerId
        @detailView.render()

    showMPError: -> false

    serializeData: ->
        data = 
            customerId: @model.customerId
            marketplaces: _.sortBy _.pluck(@model.where(IsPaymentAccount: false), "attributes"), "UWPriority"
            accounts: _.sortBy _.pluck(@model.where(IsPaymentAccount: true), "attributes"), "UWPriority"
            hideAccounts: false
            hideMarketplaces: false
            summary:
                anualSales : 0
                inventory : 0
                positive : 0
                negative : 0
                neutral : 0


        for m in data.marketplaces
            data.summary.anualSales += m.anualSales
            data.summary.inventory += m.inventory
            data.summary.positive += m.PositiveFeedbacks
            data.summary.negative += m.NegativeFeedbacks
            data.summary.neutral += m.NeutralFeedbacks

        total = data.summary.positive + data.summary.negative + data.summary.neutral

        data.summary.rating = if total > 0 then data.summary.positive / total else 0

        return data

    reCheckmarketplaces: (e) ->
        $el = $(e.currentTarget)
        umi = $el.attr "umi"
        mpType = $el.attr "marketplaceType"
        customerId = @model.customerId
        okFn = =>
            xhr = $.post "#{window.gRootPath}Underwriter/MarketPlaces/ReCheckMarketplaces",
                customerId: customerId
                umi: umi
                marketplaceType: mpType
            xhr.done (response)=>
                if response and response.error != undefined
                    EzBob.ShowMessage response.error, "Error occured"
                else
                    EzBob.ShowMessage "Wait a few minutes", "The marketplace recheck is running. ", null, "OK"
                @trigger "rechecked",
                    umi: umi
                    el: $el
            xhr.fail (data) ->
                console.error data.responseText
        EzBob.ShowMessage "", "Are you sure?", okFn, "Yes", null, "No"
        false

    reCheckPaypal: (e) ->
        el = $(e.currentTarget)
        umi = el.attr("umi")
        EzBob.ShowMessage "", "Are you sure?", (=> @doReCheck(umi, el)), "Yes", null, "No"
        false

    doReCheck: (umi, el) ->
        xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/ReCheckPaypal", { customerId: @model.customerId, umi: umi}
        xhr.done(=>
            EzBob.ShowMessage "Wait a few minutes", "The marketplace recheck has been started. ", null, "OK"
            @.trigger "rechecked", {umi: umi, el: el}
        )
        xhr.fail (data) ->
            console.error data.responseText

    renewTokenClicked: (e)->
        umi = $(e.currentTarget).data "umi"
        @renewToken(umi);
        false

    renewToken: (umi) ->
        xhr = $.post "#{window.gRootPath}Underwriter/MarketPlaces/RenewEbayToken", umi: umi
        xhr.done ->
            EzBob.ShowMessage "Renew started successfully", "Successfully"