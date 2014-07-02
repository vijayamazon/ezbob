root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.FundingView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.FundingModel()
        @model.on "change reset", @render, @
        @requiredFunds = -1
        @model.fetch()
        
        xhr = $.post "#{window.gRootPath}Underwriter/Funding/GetAvailableFundsInterval"
        xhr.done (res) =>
            xhr1 = $.post "#{window.gRootPath}Underwriter/Funding/GetRequiredFunds"
            xhr1.done (res) =>
                @requiredFunds = res
                @render()
                @modelUpdater = setInterval(=> 
                    @model.fetch()
                , res )

    template: "#funding-template"

    events:
        "click #addFundsBtn": "addFunds"
        "click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds"

    addFunds: (e) ->
        that = this
        d = new EzBob.Dialogs.PacentManual(
            model: @model
            title: "Pacnet Balance - Add Manual Funds"
            width: 400
            postValueName: "amount"
            url: "Underwriter/Funding/SavePacnetManual"
            data:
                limit: EzBob.Config.PacnetBalanceMaxManualChange

            min: EzBob.Config.PacnetBalanceMaxManualChange * -1
            max: EzBob.Config.PacnetBalanceMaxManualChange
            required: true
        )
        d.render()
        d.on "done", ->
            that.model.fetch()

    cancelManuallyAddedFunds: (e) ->
        that = this
        d = new EzBob.Dialogs.CheckBoxEdit(
            model: @model
            propertyName: "UseSetupFee"
            title: "Pacnet Balance - Clear Manual Funds"
            width: 400
            checkboxName: "I am sure"
            postValueName: "isSure"
            url: "Underwriter/Funding/DisableCurrentManualPacnetDeposits"
            data:
                isSure: @model.get("IsSure")
        )

        d.render()
        d.on "done", ->
            that.model.fetch()

    onRender: ->
        if !$("body").hasClass("role-manager")
            @$el.find('#addFundsBtn').hide()
            @$el.find('#cancelManuallyAddedFundsBtn').hide()

        fundingAlert = $(".fundingAlert")

        availableFundsNum = @model.get('AvailableFunds')
        availableFundsStr = 'Funding ' + EzBob.formatPoundsNoDecimals(availableFundsNum).replace(/\s+/g, '')
        fundingAlert.html(availableFundsStr)
        
        li = $("[id='liFunding']")
        tdHeader = $("[id='available-funds-td-header']")
        tdValue = $("[id='available-funds-td-value']")

        if (@requiredFunds > availableFundsNum)
            fundingAlert.addClass('red_cell')
            li.addClass('available-funds-alert')
            tdHeader.addClass('available-funds-alert-text-color')
            tdValue.addClass('available-funds-alert-text-color')
        else
            fundingAlert.removeClass('red_cell')
            li.removeClass('available-funds-alert')
            tdHeader.removeClass('available-funds-alert-text-color')
            tdValue.removeClass('available-funds-alert-text-color')

    hide: ->
        @$el.hide()

    show: ->
        @$el.show()

    serializeData: ->
        model: @model

class EzBob.Underwriter.FundingModel extends Backbone.Model
    urlRoot: -> "#{gRootPath}Underwriter/Funding/GetCurrentFundingStatus"