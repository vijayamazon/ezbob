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
        monthSales = if ai then (ai.TotalSumofOrders1M or 0) * 1 else 0
        anualSales = if ai then (ai.TotalSumofOrders12M or ai.TotalSumofOrders6M or ai.TotalSumofOrders3M or ai.TotalSumofOrders1M or 0) * 1 else 0
        inventory = if ai and not isNaN((ai.TotalValueofInventoryLifetime * 1)) then (ai.TotalValueofInventoryLifetime * 1) else "-"

        pp = @get("PayPal")
        if pp
            monthSales = pp.GeneralInfo.MonthInPayments
            anualSales = pp.GeneralInfo.TotalNetInPayments

        age = if (accountAge isnt "-" and accountAge isnt 'undefined') then EzBob.SeniorityFormat(accountAge, 0) else "-"

        @set {age: age, monthSales: monthSales, anualSales: anualSales, inventory: inventory}, {silent: true}

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
        "click .disable-shop": "disableShop"
        "click .enable-shop": "enableShop"
        
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
        @detailView.on "disable-shop", @disableShop, @
        @detailView.on "enable-shop", @enableShop, @
        @detailView.on("recheck-token", @renewToken)
        @detailView.customerId = @model.customerId
        @detailView.render()

    showMPError: -> false

    serializeData: ->
        aryCGAccounts = $.parseJSON $('div#cg-account-list').text()

        isMarketplace = (x) ->
            unless (aryCGAccounts[x.get('Name')])
                return not x.get 'IsPaymentAccount'
            cg = aryCGAccounts[x.get('Name')]
            (cg.Behaviour == 0) and not cg.HasExpenses

        data = 
            customerId: @model.customerId
            marketplaces: _.sortBy _.pluck(_.filter(@model.models, (x) -> x and isMarketplace(x) ), "attributes"), "UWPriority"
            accounts: _.sortBy _.pluck(_.filter(@model.models, (x) -> x and !isMarketplace(x) ), "attributes"), "UWPriority"
            hideAccounts: false
            hideMarketplaces: false
            summary:
                monthSales : 0
                anualSales : 0
                inventory : 0
                positive : 0
                negative : 0
                neutral : 0

        for m in data.marketplaces
            data.summary.monthSales += m.monthSales if m.Disabled == false
            data.summary.anualSales += m.anualSales if m.Disabled == false
            data.summary.inventory += m.inventory if m.Disabled == false
            data.summary.positive += m.PositiveFeedbacks
            data.summary.negative += m.NegativeFeedbacks
            data.summary.neutral += m.NeutralFeedbacks

        total = data.summary.positive + data.summary.negative + data.summary.neutral

        data.summary.rating = if total > 0 then data.summary.positive / total else 0

        return data

    disableShop: (e) ->
        $el = $(e.currentTarget)
        umi = $el.attr "umi"
        EzBob.ShowMessage "Disable shop", "Are you sure?", (=> @doEnableShop(umi, false)), "Yes", null, "No"
        return false

    doEnableShop: (umi, enabled) ->
        url = if enabled then "#{window.gRootPath}Underwriter/MarketPlaces/Enable" else "#{window.gRootPath}Underwriter/MarketPlaces/Disable"
        xhr = $.post url, umi: umi
        xhr.done (response) =>
            @model.fetch()

    enableShop: (e) ->
        $el = $(e.currentTarget)
        umi = $el.attr "umi"
        EzBob.ShowMessage "Enable shop", "Are you sure?", (=> @doEnableShop(umi, true)), "Yes", null, "No"
        return false

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