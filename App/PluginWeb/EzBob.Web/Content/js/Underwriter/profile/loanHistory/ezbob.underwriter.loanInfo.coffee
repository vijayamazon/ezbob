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
        EzBob.App.vent.on 'newCreditLine:done', @showCreditLineDialog, this
        EzBob.App.vent.on 'newCreditLine:error', @showErrorDialog, this
        EzBob.App.vent.on 'newCreditLine:pass', @showNothing, this

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
        'click [name="isLoanTypeSelectionAllowed"]'         : 'isLoanTypeSelectionAllowed'
        'click [name="discountPlan"]'                       : 'discountPlan'

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

        el = ($ "<select/>")
        .css("height", "30px").css("width", "280px")
        .append( "<option value='1'>Skip everything, go to manual decision</option>")
        .append( "<option value='2'>Update everything except of MP's and go to manual decisions</option>")
        .append( "<option value='3'>Update everything and apply auto rules</option>")
        .append( "<option value='4'>Update everything and go to manual decision</option>")
        
        EzBob.ShowMessage el, "New Credit Line Option", (=>@RunCustomerCheck(el.val())), "OK", null, "Cancel"

        false

    RunCustomerCheck: (newCreditLineOption )->
        BlockUi "on"
        $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLine",
            Id: @model.get("CustomerId")
            NewCreditLineOption : newCreditLineOption
        ).done((response) =>
            updater = new ModelUpdater(@personalInfo, 'IsMainStratFinished')
            updater.start()
        ).fail (data) ->
            console.error data.responseText

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

    isLoanTypeSelectionAllowed: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: "IsLoanTypeSelectionAllowed"
            title: "Customer selection"
            postValueName: "loanTypeSelection"
            comboValues: [{ value: 0, text: 'Disabled' }, { value: 1, text: 'Enabled' }]
            url: "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( 'done', => @LoanTypeSelectionAllowedChanged() )
        return

    LoanTypeSelectionAllowedChanged: =>
        if @model.get('IsLoanTypeSelectionAllowed') in [ 1, '1' ]
            @$el.find('button[name=loanType], button[name=repaymentPeriodChangeButton]').attr('disabled', 'disabled')
            if @model.get('LoanTypeId') != 1
                @model.set 'LoanTypeId', 1
        else
            @$el.find('button[name=loanType], button[name=repaymentPeriodChangeButton]').removeAttr('disabled')

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

    discountPlan: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: "DiscountPlanId"
            title: "Discount Plan"
            comboValues: _.map(@model.get('DiscountPlans'), (v) -> {value: v.Id, text: v.Name})
            postValueName: "DiscountPlanId"
            url: "Underwriter/ApplicationInfo/DiscountPlan"
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
        @$el.find(".tltp").tooltip()
        @$el.find(".tltp-left").tooltip({placement: "left"})
        @UpdateNewCreditLineState()
        @LoanTypeSelectionAllowedChanged()

        if @model.get('IsLoanTypeSelectionAllowed') in [ 2, '2' ]
            @$el.find('button[name=isLoanTypeSelectionAllowed]').attr('disabled', 'disabled')
        else
            @$el.find('button[name=isLoanTypeSelectionAllowed]').removeAttr('disabled')

    changeCreditResult: ->
        @model.fetch()
        @personalInfo.fetch()

    showCreditLineDialog: ->
        xhr = @model.fetch()
        xhr.done =>
            dialog = new EzBob.Underwriter.CreditLineDialog (model: @model)
            EzBob.App.modal.show dialog 

    showErrorDialog: (errorMsg)->
        EzBob.ShowMessage errorMsg, "Something went wrong"

    showNothing: (errorMsg)->
        @

class ModelUpdater
    constructor: (@model, @property) ->

    start: =>
        xhr = @model.fetch()
        xhr.done =>
            @check()
    
    check: ->
        if Convert.toBool(@model.get(@property))
            BlockUi 'off'
            if @model.get('CreditResult') != "WaitingForDecision"
                EzBob.App.vent.trigger('newCreditLine:pass')
                return
            if @model.get('StrategyError') != null
                EzBob.App.vent.trigger('newCreditLine:error', @model.get('StrategyError'))
            else
                EzBob.App.vent.trigger('newCreditLine:done')
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