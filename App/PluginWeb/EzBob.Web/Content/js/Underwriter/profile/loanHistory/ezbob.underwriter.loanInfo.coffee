root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.LoanInfoView extends Backbone.Marionette.ItemView
    template: "#profile-loan-info-template"

    initialize: (options) ->
        @bindTo @model, "change reset sync", @render, this
        @personalInfo = options.personalInfo
        @bindTo @personalInfo, "change", @UpdateNewCreditLineState, this
        @bindTo @personalInfo, "change:CreditResult", @changeCreditResult, this
        EzBob.App.vent.on 'newCreditLine:done', @showCreditLineDialog, this
        EzBob.App.vent.on 'newCreditLine:error', @showErrorDialog, this
        EzBob.App.vent.on 'newCreditLine:pass', @showNothing, this
        @parentView = options.parentView

    events:
        "click [name='startingDateChangeButton']"           : "editStartingDate"
        "click [name='offerValidUntilDateChangeButton']"    : "editOfferValidUntilDate"
        "click [name='repaymentPeriodChangeButton']"        : "editRepaymentPeriod"
        "click [name='interestRateChangeButton']"           : "editInterestRate"
        "click [name='openCreditLineChangeButton']"         : "editOfferedCreditLine"
        "click [name='openPacnetManualButton']"             : "openPacnetManual"
        "click [name='clearPacnetManualButton']"             : "clearPacnetManual"
        "click [name='editDetails']"                        : "editDetails"
        "click [name='setupFeeEditButton']"                 : "editSetupFee"
        "click [name='newCreditLineBtn']"                   : "runNewCreditLine"
        'click [name="allowSendingEmail"]'                  : 'allowSendingEmail'
        'click [name="loanType"]'                           : 'loanType'
        'click [name="isLoanTypeSelectionAllowed"]'         : 'isLoanTypeSelectionAllowed'
        'click [name="discountPlan"]'                       : 'discountPlan'
        'click [name="loanSource"]'                         : 'loanSource'

    editOfferValidUntilDate: ->
        d = new EzBob.Dialogs.DateEdit(
            model: @model
            propertyName: "OfferValidateUntil"
            title: "Offer valid until edit"
            width: 370
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
            width: 350
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
            width : 350
            postValueName: "period"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestRepaymentPeriod"
            data:
                id: @model.get("CashRequestId")
        )
        d.render()
        return

    editOfferedCreditLine: ->
        that = this
        d = new EzBob.Dialogs.OfferedCreditLineEdit(
            model: @model
            propertyName: "OfferedCreditLine"
            title: "Offer credit line edit"
            width: 350
            postValueName: "amount"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestOpenCreditLine"
            data:
                id: @model.get("CashRequestId")

            min: EzBob.Config.XMinLoan
            max: EzBob.Config.MaxLoan
        )
        d.render()
        d.on "done", ->
            that.model.fetch()

        return

    openPacnetManual: ->
        that = this
        d = new EzBob.Dialogs.PacentManual(
            model: @model
            title: "Pacnet Balance - Add Manual Funds"
            width: 350
            postValueName: "amount"
            url: "Underwriter/ApplicationInfo/SavePacnetManual"
            data:
                limit: EzBob.Config.PacnetBalanceMaxManualChange

            min: EzBob.Config.PacnetBalanceMaxManualChange * -1
            max: EzBob.Config.PacnetBalanceMaxManualChange
        )
        d.render()
        d.on "done", ->
            that.model.fetch()

        return

    clearPacnetManual: ->
        that = this

        d = new EzBob.Dialogs.CheckBoxEdit(
            model: @model
            propertyName: "UseSetupFee"
            title: "Pacnet Balance - Clear Manual Funds"
            width: 350
            checkboxName: "I am sure"
            postValueName: "isSure"
            url: "Underwriter/ApplicationInfo/DisableTodaysPacnetManual"
            data:
                isSure: @model.get("IsSure")
        )

        d.render()
        d.on "done", ->
            that.model.fetch()
        return

    editInterestRate: ->
        d = new EzBob.Dialogs.PercentsEdit(
            model: @model
            propertyName: "InterestRate"
            title: "Interest rate edit"
            width: 350
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
            width: 350
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
            width: 350
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
        .css("height", "30px").css("width", "270px")
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
            width: 350
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
            width: 350
            postValueName: "loanTypeSelection"
            comboValues: [{ value: 0, text: 'Disabled' }, { value: 1, text: 'Enabled' }]
            url: "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( 'done', => @LoanTypeSelectionAllowedChanged() )
        return

    LoanTypeSelectionAllowedChanged: =>
        isCustomerRepaymentPeriodSelectionAllowed = @model.get('LoanSource').IsCustomerRepaymentPeriodSelectionAllowed

        if !isCustomerRepaymentPeriodSelectionAllowed || @model.get('IsLoanTypeSelectionAllowed') in [ 1, '1' ]
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
            width: 350
            comboValues: @model.get('LoanTypes')
            postValueName: "LoanType"
            url: "Underwriter/ApplicationInfo/LoanType"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( "done", => @model.fetch())
        return

    loanSource: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: "LoanSource.LoanSourceID"
            title: "Loan source"
            width: 350
            comboValues: _.map(@model.get('AllLoanSources'), (ls) -> { value: ls.Id, text: ls.Name })
            postValueName: "LoanSourceID"
            url: "Underwriter/ApplicationInfo/LoanSource"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( "done", => @model.fetch() )
        return

    validateLoanSourceRelated: ->
        loanSourceModel = @model.get 'LoanSource'

        @validateInterestVsSource loanSourceModel.MaxInterest

        if loanSourceModel.DefaultRepaymentPeriod == -1
            @$el.find('button[name=repaymentPeriodChangeButton]').removeAttr 'disabled'
        else
            @$el.find('button[name=repaymentPeriodChangeButton]').attr 'disabled', 'disabled'

        @parentView.clearDecisionNotes()

        if loanSourceModel.MaxEmployeeCount != -1
            nEmployeeCount = @model.get 'EmployeeCount'

            if nEmployeeCount >= 0 and nEmployeeCount > loanSourceModel.MaxEmployeeCount
                @parentView.appendDecisionNote '<div class=red>Employee count (' + nEmployeeCount + ') is greater than max employee count (' + loanSourceModel.MaxEmployeeCount + ') for this loan source.</div>'
        # end if max employee count

        if loanSourceModel.MaxAnnualTurnover != -1
            nAnnualTurnover = @model.get 'AnnualTurnover'

            if nAnnualTurnover >= 0 and nAnnualTurnover > loanSourceModel.MaxAnnualTurnover
                @parentView.appendDecisionNote '<div class=red>Annual turnover (' + EzBob.formatPoundsNoDecimals(nAnnualTurnover) + ') is greater than max annual turnover (' + EzBob.formatPoundsNoDecimals(loanSourceModel.MaxAnnualTurnover) + ') for this loan source.</div>'
        # end if max employee count

        if loanSourceModel.AlertOnCustomerReasonType != -1
            nCustomerReasonType = @model.get 'CustomerReasonType'

            if loanSourceModel.AlertOnCustomerReasonType == nCustomerReasonType
                @parentView.appendDecisionNote '<div class=red>Please note customer reason: "' + @model.get('CustomerReason') + '".</div>'
        # end if alert on customer reason type
    # end of validateSourceRelated

    validateInterestVsSource: (nMaxInterest) ->
        if nMaxInterest == -1
            return

        @$el.find('.interest-exceeds-max-by-loan-source').toggleClass 'hide', @model.get('InterestRate') <= nMaxInterest

        @$el.find('.discount-exceeds-max-by-loan-source').addClass 'hide'

        sPercentList = @model.get 'DiscountPlanPercents'

        if sPercentList == ''
            return

        nBaseRate = @model.get 'InterestRate'

        aryPercentList = sPercentList.split ','

        for pct in aryPercentList
            if pct[0] == '('
                pct = pct.substr 1

            nPct = parseFloat pct

            nChange = 100.0 + nPct

            nRate = nBaseRate * nChange / 100.0

            if nRate > nMaxInterest
                @$el.find('.discount-exceeds-max-by-loan-source').removeClass 'hide'
                break
            # end if
        # end for
    # end of validateInterestVsSource

    discountPlan: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: "DiscountPlanId"
            title: "Discount Plan"
            width: 350
            comboValues: _.map(@model.get('DiscountPlans'), (v) -> {value: v.Id, text: v.Name})
            postValueName: "DiscountPlanId"
            url: "Underwriter/ApplicationInfo/DiscountPlan"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( "done", => @model.fetch())
        return

    UpdateNewCreditLineState: ->
        waiting = @personalInfo.get("CreditResult") is "WaitingForDecision"
        currentStatus = @personalInfo.get("Disabled")
        return if currentStatus == undefined
        xhr = @_getIsStatusEnabled currentStatus
        xhr.done (result) =>
            isStatusEnabled = result
            disabled =  waiting or !isStatusEnabled
            $("input[name='newCreditLineBtn']").toggleClass "disabled", disabled
            $("#newCreditLineLnkId").toggleClass "disabled", disabled

    statuses: {}

    _getIsStatusEnabled: (status) ->

        if EzBob.Underwriter.LoanInfoView::statuses[status]?
            d = $.Deferred()
            d.resolve(EzBob.Underwriter.LoanInfoView::statuses[status])
            return d.promise()

        xhr = $.ajax "#{window.gRootPath}Underwriter/ApplicationInfo/GetIsStatusEnabled", {cache : true, data: {status: status}}
        xhr.done (result) ->
            EzBob.Underwriter.LoanInfoView::statuses[status] = result

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

        @validateLoanSourceRelated()

    changeCreditResult: ->
        @model.fetch()
        @personalInfo.fetch()

    showCreditLineDialog: ->
        xhr = @model.fetch()
        xhr.done =>
            dialog = new EzBob.Underwriter.CreditLineDialog (model: @model)
            EzBob.App.jqmodal.show dialog 

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