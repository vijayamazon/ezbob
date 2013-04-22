root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.MarketPlaceModel extends Backbone.Model
    initialize: ->
        @on 'change reset', @recalculate, @
        @recalculate()

    recalculate: ->
        ai = @get 'AnalysisDataInfo'
        accountAge = @get 'AccountAge'
        age = if (accountAge isnt "-" and accountAge isnt 'undefined') then EzBob.SeniorityFormat(accountAge, 0) else "-"
        anualSales = (ai.TotalSumofOrders12M or ai.TotalSumofOrders6M or ai.TotalSumofOrders3M or ai.TotalSumofOrders1M or 0) * 1
        inventory = if not isNaN((ai.TotalValueofInventoryLifetime * 1)) then (ai.TotalValueofInventoryLifetime * 1) else "-"
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
        "click .reCheck-amazon": "reCheckmarketplaces"
        "click .reCheck-ebay": "reCheckmarketplaces"
        "click tbody tr": "rowClick"
        "click .mp-error-description" : "showMPError"
        "click .renew-token": "renewTokenClicked"

    rowClick: (e) ->
        return if e.target.getAttribute('href')
        return if e.target.tagName is 'I'
        id = e.currentTarget.getAttribute("data-id")
        return unless id
        shop = @model.at(id)
        return if shop.get('Name') is 'EKM'

        @detailView = new EzBob.Underwriter.MarketPlaceDetailsView el: @$el.find('#marketplace-details'), model: @model, currentId: id, customerId: @model.customerId
        @detailView.on "reCheck", @reCheckmarketplaces, @
        @detailView.on("recheck-token", @renewToken)
        @detailView.render()

    showMPError: -> false

    serializeData: ->
        data = 
            customerId: @model.customerId
            marketplaces: @model.toJSON()
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

    renewTokenClicked: (e)->
        umi = $(e.currentTarget).data "umi"
        @renewToken(umi);
        false

    renewToken: (umi) ->
        xhr = $.post "#{window.gRootPath}Underwriter/MarketPlaces/RenewEbayToken", umi: umi
        xhr.done ->
            EzBob.ShowMessage "Renew started successfully", "Successfully"