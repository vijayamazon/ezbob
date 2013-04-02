root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.LoanInfoView extends Backbone.Marionette.ItemView
    template: "#profile-loan-info-template"

    initialize: (options) ->
        @bindTo @model, "change reset", @render, this
        @personalInfo = options.personalInfo
        @bindTo @personalInfo, "change", @UpdateNewCreditLineState, this
        @bindTo @personalInfo, "change:CreditResult", @changeCreditResult, this

    events:
        "click [name='startingDateChangeButton']"           : "editStartingDate"
        "click [name='offerValidUntilDateChangeButton']"    : "editOfferValidUntilDate"
        "click [name='repaymentPeriodChangeButton']"        : "editRepaymentPeriod"
        "click [name='interestRateChangeButton']"           : "editInterestRate"
        "click [name='openCreditLineChangeButton']"         : "editOfferedCreditLine"
        "click [name='editDetails']"                        : "editDetails"
        "click [name='setupFeeEditButton']"                 : "editSetupFee"
        "click [name='newCreditLineBtn']"                   : "runNewCreditLine"
        'click [name="allowSendingEmail"]'                  : 'allowSendingEmail'
        'click [name="loanType"]'                           : 'loanType'

    editOfferValidUntilDate: ->
        d = new EzBob.Dialogs.DateEdit(
            model: @model
            propertyName: "OfferValidateUntil"
            title: "Offer valid until edit"
            postValueName: "date"
            url: "Underwriter/ApplicationInfo/ChangeOferValid"
            data:
                id: @model.get("CustomerId")
        )
        d.render()
        return

    editStartingDate: ->
        that = this
        d = new EzBob.Dialogs.DateEdit(
            model: @model
            propertyName: "StartingFromDate"
            title: "Starting date edit"
            postValueName: "date"
            url: "Underwriter/ApplicationInfo/ChangeStartingDate"
            data:
                id: @model.get("CustomerId")
        )
        d.render()
        d.on "done", ->
            that.model.fetch()

        return

    editRepaymentPeriod: ->
        d = new EzBob.Dialogs.IntegerEdit(
            model: @model
            propertyName: "RepaymentPerion"
            title: "Repayment period edit"
            postValueName: "period"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestRepaymentPeriod"
            data:
                id: @model.get("CashRequestId")
        )
        d.render()
        return

    editOfferedCreditLine: ->
        d = new EzBob.Dialogs.OfferedCreditLineEdit(
            model: @model
            propertyName: "OfferedCreditLine"
            title: "Offer credit line edit"
            postValueName: "amount"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestOpenCreditLine"
            data:
                id: @model.get("CashRequestId")

            min: EzBob.Config.XMinLoan
            max: EzBob.Config.MaxLoan
        )
        d.render()
        return

    editInterestRate: ->
        d = new EzBob.Dialogs.PercentsEdit(
            model: @model
            propertyName: "InterestRate"
            title: "Interest rate edit"
            postValueName: "interestRate"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestInterestRate"
            data:
                id: @model.get("CashRequestId")
        )
        d.render()
        return

    editDetails: ->
        d = new EzBob.Dialogs.TextEdit(
            model: @model
            propertyName: "Details"
            title: "Details edit"
            postValueName: "details"
            url: "Underwriter/ApplicationInfo/SaveDetails"
            data:
                id: @model.get("CustomerId")
        )
        d.render()
        return

    editSetupFee: ->
        d = new EzBob.Dialogs.CheckBoxEdit(
            model: @model
            propertyName: "UseSetupFee"
            title: "Setup Fee"
            postValueName: "enbaled"
            checkboxName: "Enable Setup Fee"
            url: "Underwriter/ApplicationInfo/ChangeSetupFee"
            data:
                id: @model.get("CashRequestId")
        )
        d.render()
        return

    runNewCreditLine: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        @RunCustomerCheck()
        false

    RunCustomerCheck: ->
        $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLine",
            Id: @model.get("CustomerId")
        ).done((response) =>
            updater = new ModelUpdater(@personalInfo, 'CreditResult', 'WaitingForDecision')
            updater.start()
        ).fail (data) ->
            console.error data.responseText
        false

    allowSendingEmail: ->
        d = new EzBob.Dialogs.CheckBoxEdit
            model: @model
            propertyName: "AllowSendingEmail"
            title: "Allow sending emails"
            postValueName: "enbaled"
            checkboxName: "Allow"
            url: "Underwriter/ApplicationInfo/AllowSendingEmails"
            data: {id: @model.get("CashRequestId")}
        d.render()
        return

    loanType: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: "LoanTypeId"
            title: "Loan type"
            comboValues: @model.get('LoanTypes')
            postValueName: "LoanType"
            url: "Underwriter/ApplicationInfo/LoanType"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( "done", => @model.fetch())
        return

    UpdateNewCreditLineState: ->
        waiting = @personalInfo.get("CreditResult") is "WaitingForDecision"
        collection = @personalInfo.get("Disabled") != 0
        disabled =  waiting or collection
        @$el.find("input[name='newCreditLineBtn']").toggleClass "disabled", disabled

    serializeData: ->
        m: @model.toJSON()
    
    onRender: ->
        @UpdateNewCreditLineState()

    changeCreditResult: ->
        @model.fetch()
        @personalInfo.fetch()

class ModelUpdater
    constructor: (@model, @property, @value) ->
        BlockUi 'on'

    start: =>
        xhr = @model.fetch()
        xhr.done =>
            @check()
    
    check: ->
        if @model.get(@property) is @value
            BlockUi 'off'
            return
        else
            setTimeout @start, 1000
        this



class EzBob.Underwriter.LoanInfoModel extends Backbone.Model
    idAttribute: "Id"
    urlRoot: "#{window.gRootPath}Underwriter/ApplicationInfo/Index"
    initialize: ->
        @on "change:OfferValidateUntil", @offerChanged, this
        @on "change:LoanTypeId", @loanTypeChanged, this

    offerChanged: ->
        until_ = moment(@get("OfferValidateUntil"), "DD/MM/YYYY")
        now = moment()
        @set OfferExpired: until_ < now

    loanTypeChanged: ->
        types = @get 'LoanTypes'
        id = parseInt @get('LoanTypeId'), 10
        type = _.find(types, (t) -> t.value is id)
        @set "LoanType", type.text