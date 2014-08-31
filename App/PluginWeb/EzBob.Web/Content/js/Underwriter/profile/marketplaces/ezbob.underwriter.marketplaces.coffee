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
        monthAnnualizedSales = if ai then (ai.TotalSumofOrdersAnnualized1M or 0) * 1 else 0
        anualSales = if ai then (ai.TotalSumofOrders12M or ai.TotalSumofOrders6M or ai.TotalSumofOrders3M or ai.TotalSumofOrders1M or 0) * 1 else 0
        inventory = if ai and not isNaN((ai.TotalValueofInventoryLifetime * 1)) then (ai.TotalValueofInventoryLifetime * 1) else "-"

        pp = @get("PayPal")
        if pp
            monthSales = pp.GeneralInfo.MonthInPayments
            monthAnnualizedSales = pp.GeneralInfo.MonthInPaymentsAnnualized
            anualSales = pp.GeneralInfo.TotalNetInPayments

        age = if (accountAge isnt "-" and accountAge isnt 'undefined') then EzBob.SeniorityFormat(accountAge, 0) else "-"

        @set {age: age, monthSales: monthSales, monthAnnualizedSales: monthAnnualizedSales, anualSales: anualSales, inventory: inventory}, {silent: true}

class EzBob.Underwriter.MarketPlaces extends Backbone.Collection
    model: EzBob.Underwriter.MarketPlaceModel
    url: -> "#{window.gRootPath}Underwriter/MarketPlaces/Index/?id=#{@customerId}&history=#{@history}"

class EzBob.Underwriter.Affordability extends Backbone.Model
    url: -> "#{window.gRootPath}Underwriter/MarketPlaces/GetAffordabilityData/?id=#{@customerId}"

class EzBob.Underwriter.MarketPlacesView extends Backbone.Marionette.ItemView
    template: "#marketplace-template"

    initialize: ->
        @model.on "reset change sync", @render, this
        @rendered = false

        window.YodleeTryRecheck = (result) ->
            if (result.error)
                EzBob.ShowMessage result.error, "Yodlee Recheck Error", "OK"
            else
                EzBob.ShowMessage 'Yodlee recheked successfully, refresh the page', null, "OK"
        # end of YodleeTryRecheck

        EzBob.App.vent.on 'ct:marketplaces.history', () =>
            @$el.find('#hmrc-upload-container').hide().empty()
        # end of on history

        EzBob.App.vent.on 'ct:marketplaces.uploadHmrc', () =>
            oUploader = $('<div class="box-content"></div>')

            @$el.find('#hmrc-upload-container').empty().append(oUploader)

            uploadHmrcView = new EzBob.Underwriter.UploadHmrcView
                el: oUploader
                customerId: @model.customerId
                companyRefNum: @options.personalInfoModel.get('CompanyExperianRefNum')

            uploadHmrcView.render()

            @$el.find('#hmrc-upload-container').show()

            $(".mps-tables").hide()
        # end of on uploadHmrc

        EzBob.App.vent.on 'ct:marketplaces.uploadHmrcBack', () =>
            $(".mps-tables").show()
            @$el.find('#hmrc-upload-container').hide().empty()
        # end of on uploadHmrcBack

        EzBob.App.vent.on 'ct:marketplaces.enterHmrc', () =>
            EzBob.Underwriter.EnterHmrcView.execute @model.customerId, @model
        # end of on enterHmrc

        EzBob.App.vent.on 'ct:marketplaces.parseYodlee', () =>
            @parseYodlee()
            
        EzBob.App.vent.on 'ct:marketplaces.parseYodleeBack', () =>
            @$el.find(".mps-tables").show()
            @$el.find('#parse-yodlee-container').hide().empty()


        @
    # end of initialize

    onRender: ->
        @$el.find('.mp-error-description').tooltip(({placement: "bottom"}));

        @$el.find('a[data-bug-type]').tooltip({title: 'Report bug'});

        _.each(@$el.find('[data-original-title]'), (elem) ->
            $(elem).tooltip({title: elem.getAttribute('data-original-title')});
        )

        if @detailView
            @detailView.render()

        marketplacesHistoryDiv = @$el.find("#marketplaces-history")
        @marketPlacesHistory = new EzBob.Underwriter.MarketPlacesHistory()
        @marketPlacesHistory.customerId = @model.customerId
        @marketPlacesHistory.silent = true

        @marketPlaceHistoryView = new EzBob.Underwriter.MarketPlacesHistoryView(
            model: @marketPlacesHistory
            el: marketplacesHistoryDiv
            customerId: @model.customerId
        )

        return this
    # end of onRender

    events:
        "click .tryRecheckYodlee": "tryRecheckYodlee"
        "click .reCheckMP": "reCheckmarketplaces"
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

        @detailView = new EzBob.Underwriter.MarketPlaceDetailsView model: @model, currentId: id, customerId: @model.customerId, personalInfoModel: @options.personalInfoModel

        EzBob.App.jqmodal.show(@detailView)

        @detailView.on "reCheck", @reCheckmarketplaces, @
        @detailView.on "disable-shop", @disableShop, @
        @detailView.on "enable-shop", @enableShop, @
        @detailView.on("recheck-token", @renewToken)
        @detailView.customerId = @model.customerId
        @detailView.render()

    showMPError: -> false

    serializeData: ->
        isMarketplace = (x) ->
            unless (EzBob.CgVendors.all()[x.get('Name')])
                return not x.get 'IsPaymentAccount'
            cg = EzBob.CgVendors.all()[x.get('Name')]
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
                monthAnnualizedSales : 0

        for m in data.marketplaces
            data.summary.monthSales += m.monthSales if m.Disabled == false
            data.summary.anualSales += m.anualSales if m.Disabled == false
            data.summary.monthAnnualizedSales += m.monthAnnualizedSales if m.Disabled == false
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

    tryRecheckYodlee: (e) ->

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
            
    parseYodlee: () ->
        parseYodleeView = new EzBob.Underwriter.ParseYodleeView(
            el: @$el.find('#parse-yodlee-container')
            customerId: @model.customerId
            model: @model
        )

        parseYodleeView.render()
        @$el.find('#parse-yodlee-container').show()
        $(".mps-tables").hide()
        @