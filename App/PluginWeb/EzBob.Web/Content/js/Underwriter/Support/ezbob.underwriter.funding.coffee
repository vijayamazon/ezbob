root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.FundingView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.FundingModel()
        @model.on "change reset", @render, @
        @model.fetch()
        
        @requiredFunds = -1

        xhr1 = $.post "#{window.gRootPath}Underwriter/Funding/GetRequiredFunds"
        xhr1.done (res) =>
            @requiredFunds = res
            @render()

        xhr = $.post "#{window.gRootPath}Underwriter/Funding/GetAvailableFundsInterval"
        xhr.done (res) =>
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

        li = $(document.getElementById("liFunding"))
        tdHeader = $(document.getElementById("available-funds-td-header"))
        tdValue = $(document.getElementById("available-funds-td-value"))
        fundingDiv = $(document.getElementById("fundingDiv"))

        availableFundsNum = @model.get('AvailableFunds')
        availableFundsStr = 'Funding ' + EzBob.formatPoundsNoDecimals(availableFundsNum).replace(/\s+/g, '')
        fundingDiv.text(availableFundsStr)

        if (@requiredFunds > availableFundsNum)
            if !li.hasClass('available-funds-alert')
                li.addClass('available-funds-alert')
            if !tdHeader.hasClass('available-funds-alert-text-color')
                tdHeader.addClass('available-funds-alert-text-color')
            if !tdValue.hasClass('available-funds-alert-text-color')
                tdValue.addClass('available-funds-alert-text-color')
        else
            if li.hasClass('available-funds-alert')
                li.removeClass('available-funds-alert')
            if tdHeader.hasClass('available-funds-alert-text-color')
                tdHeader.removeClass('available-funds-alert-text-color')
            if tdValue.hasClass('available-funds-alert-text-color')
                tdValue.removeClass('available-funds-alert-text-color')

    hide: ->
        @$el.hide()

    show: ->
        @$el.show()

    serializeData: ->
        model: @model

class EzBob.Underwriter.FundingModel extends Backbone.Model
    urlRoot: -> "#{gRootPath}Underwriter/Funding/GetCurrentFundingStatus"