root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.FundingView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.FundingModel()
        @model.on "change reset", @render, @
        @requiredFunds = -1
        that = this
        @model.fetch().done(=>
            setInterval (->
                that.model.fetch()
            ), that.model.get('RefreshInterval')
        )

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

        fundsMenuText = $("[id='liFunding'] span,[id='liFunding'] i")
        
        availableFundsNum = @model.get('AvailableFunds')
        if (@model.get('RequiredFunds') > availableFundsNum)
            fundsMenuText.addClass('red_cell')
        else
            fundsMenuText.removeClass('red_cell')

    hide: ->
        @$el.hide()

    show: ->
        @$el.show()

    serializeData: ->
        model: @model

class EzBob.Underwriter.FundingModel extends Backbone.Model
    urlRoot: -> "#{gRootPath}Underwriter/Funding/GetCurrentFundingStatus"