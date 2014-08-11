root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.FundingView extends Backbone.Marionette.ItemView
    template: "#funding-template"

    initialize: ->
        @model.on "change reset", @render, @

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
        unless $("body").hasClass("role-manager")
            @$el.find('#addFundsBtn, #cancelManuallyAddedFundsBtn').hide()

    hide: ->
        @$el.hide()

    show: ->
        @$el.show()

    serializeData: ->
        model: @model

class EzBob.Underwriter.FundingModel extends Backbone.Model
    urlRoot: -> "#{gRootPath}Underwriter/Funding/GetCurrentFundingStatus"

    notEnoughFunds: ->
        @get('RequiredFunds') > @get('AvailableFunds')
