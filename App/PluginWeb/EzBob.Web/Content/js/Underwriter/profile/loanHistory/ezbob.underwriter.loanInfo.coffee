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
        "click [name='clearPacnetManualButton']"            : "clearPacnetManual"
        "click [name='editDetails']"                        : "editDetails"
        "click [name='manualSetupFeeEditAmountButton']"     : "editManualSetupFeeAmount"
        "click [name='manualSetupFeeEditPercentButton']"    : "editManualSetupFeePercent"
        "click [name='newCreditLineBtn']"                   : "runNewCreditLine"
        'click [name="loanType"]'                           : 'loanType'
        'click [name="isLoanTypeSelectionAllowed"]'         : 'isLoanTypeSelectionAllowed'
        'click [name="discountPlan"]'                       : 'discountPlan'
        'click [name="loanSource"]'                         : 'loanSource'
        'click .create-loan-hidden-toggle'                  : 'toggleCreateLoanHidden'
        'click #create-loan-hidden-btn'                     : 'createLoanHidden'

    toggleCreateLoanHidden: (event) ->
        return unless event.ctrlKey
        @$el.find('#create-loan-hidden').toggleClass 'hide'

    createLoanHidden: ->
        nCustomerID = @model.get 'CustomerId'
        nAmount = parseInt(@$el.find('#create-loan-hidden-amount').val(), 10) || 0
        sDate = @$el.find('#create-loan-hidden-date').val()

        if nAmount <= 0
            EzBob.ShowMessageTimeout 'Amount not specified.', 'Cannot create loan', 2
            return

        unless /^\d\d\d\d-\d\d-\d\d$/.test sDate
            EzBob.ShowMessageTimeout 'Date not specified.', 'Cannot create loan', 2
            return

        oXhr = $.post(
            window.gRootPath + 'Underwriter/ApplicationInfo/CreateLoanHidden',
            { nCustomerID: nCustomerID, nAmount: nAmount, sDate: sDate }
        )

        oXhr.done( (res) =>
            if res.success 
                @$el.find('#create-loan-hidden-amount').val('')
                @$el.find('#create-loan-hidden-date').val('')
                @$el.find('#create-loan-hidden').addClass 'hide'

            if res.error
                EzBob.ShowMessage res.error, 'Cannot create loan'
            else
                EzBob.ShowMessage 'Loan created successfully', 'Loan created successfully'
        )

        oXhr.fail( =>
            EzBob.ShowMessage 'Failed to create loan.', 'Cannot create loan'
        )
    # end of createLoanHidden

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
            width: 400
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
            width : 400
            postValueName: "period"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestRepaymentPeriod"
            data:
                id: @model.get("CashRequestId")
            required: true
        )
        d.render()
        return

    editOfferedCreditLine: ->
        that = this
        view = new EzBob.Underwriter.CreditLineEditDialog(
            model: @model
        )
        EzBob.App.jqmodal.show view
        view.on "showed", ->
            view.$el.find("input").focus()
        view.on "done", ->
            that.model.fetch()

        return

    openPacnetManual: ->
        that = this
        d = new EzBob.Dialogs.PacentManual(
            model: @model
            title: "Pacnet Balance - Add Manual Funds"
            width: 400
            postValueName: "amount"
            url: "Underwriter/ApplicationInfo/SavePacnetManual"
            data:
                limit: EzBob.Config.PacnetBalanceMaxManualChange

            min: EzBob.Config.PacnetBalanceMaxManualChange * -1
            max: EzBob.Config.PacnetBalanceMaxManualChange
            required: true
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
            width: 400
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
            width: 400
            postValueName: "interestRate"
            url: "Underwriter/ApplicationInfo/ChangeCashRequestInterestRate"
            data:
                id: @model.get("CashRequestId")
            required: true
        )
        d.render()
        return

    editDetails: ->
        d = new EzBob.Dialogs.TextEdit(
            model: @model
            propertyName: "Details"
            title: "Details edit"
            width: 400
            postValueName: "details"
            url: "Underwriter/ApplicationInfo/SaveDetails"
            data:
                id: @model.get("CustomerId")
        )
        d.render()
        return

    editManualSetupFeeAmount: ->
        d = new EzBob.Dialogs.PoundsNoDecimalsEdit(
            model: @model
            propertyName: "ManualSetupFeeAmount"
            title: "Manual setup fee amount edit"
            width: 400
            postValueName: "manualAmount"
            url: "Underwriter/ApplicationInfo/ChangeManualSetupFeeAmount"
            data:
                id: @model.get("CashRequestId")
            required: false
        )
        d.render()
        return

    editManualSetupFeePercent: ->
        d = new EzBob.Dialogs.PercentsEdit(
            model: @model
            propertyName: "ManualSetupFeePercent"
            title: "Manual setup fee percent edit"
            width: 400
            postValueName: "manualPercent"
            url: "Underwriter/ApplicationInfo/ChangeManualSetupFeePercent"
            data:
                id: @model.get("CashRequestId")
            required: false
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
        that = this
        BlockUi "on"
        $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLine",
            Id: @model.get("CustomerId")
            NewCreditLineOption : newCreditLineOption
        ).done((response) =>
            if (response.Message == "Go to new mode")
                $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLineNewMode1",
                    Id: that.model.get("CustomerId")
                    NewCreditLineOption : newCreditLineOption
                )
                .done((innerResponse) =>
                    $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLineNewMode2",
                        Id: that.model.get("CustomerId")
                        NewCreditLineOption : newCreditLineOption
                    ).done((innerResponse2) =>
                        that.personalInfo.fetch().done(() =>
                            BlockUi 'off'
                            if that.personalInfo.get('CreditResult') != "WaitingForDecision"
                                EzBob.App.vent.trigger('newCreditLine:pass')
                                return
                            if that.personalInfo.get('StrategyError') != null and that.personalInfo.get('StrategyError') != ''
                                EzBob.App.vent.trigger('newCreditLine:error', that.personalInfo.get('StrategyError'))
                            else
                                EzBob.App.vent.trigger('newCreditLine:done')
                        )
                    )
                )
            else
                updater = new ModelUpdater(@personalInfo, 'IsMainStratFinished')
                updater.start()
        ).fail (data) ->
            console.error data.responseText

    isLoanTypeSelectionAllowed: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: "IsLoanTypeSelectionAllowed"
            title: "Customer selection"
            width: 400
            postValueName: "loanTypeSelection"
            comboValues: [{ value: 0, text: 'Disabled' }, { value: 1, text: 'Enabled' }]
            url: "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( 'done', => @LoanTypeSelectionAllowedChanged() )
        return

    LoanTypeSelectionAllowedChanged: =>
        loanSource = @model.get('LoanSource') or {}
        isCustomerRepaymentPeriodSelectionAllowed = loanSource.IsCustomerRepaymentPeriodSelectionAllowed

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
            width: 400
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
            width: 400
            comboValues: _.map(@model.get('AllLoanSources'), (ls) -> { value: ls.Id, text: ls.Name })
            postValueName: "LoanSourceID"
            url: "Underwriter/ApplicationInfo/LoanSource"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( "done", => @model.fetch() )
        return

    validateLoanSourceRelated: ->
        loanSourceModel = @model.get('LoanSource') or {}

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

        if not sPercentList?
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
            width: 400
            comboValues: _.map(@model.get('DiscountPlans'), (v) -> {value: v.Id, text: v.Name})
            postValueName: "DiscountPlanId"
            url: "Underwriter/ApplicationInfo/DiscountPlan"
            data: {id: @model.get("CashRequestId")}
        d.render()
        d.on( "done", => @model.fetch())
        return

    UpdateNewCreditLineState: ->
        waiting = @personalInfo.get("CreditResult") is "WaitingForDecision"
        disabled = waiting or !@personalInfo.get("IsCustomerInEnabledStatus")
        $("input[name='newCreditLineBtn']").toggleClass "disabled", disabled
        $("#newCreditLineLnkId").toggleClass "disabled", disabled

    statuses: {}

    serializeData: ->
        m: @model.toJSON()

    onRender: ->
        @$el.find(".tltp").tooltip()
        @$el.find(".tltp-left").tooltip({placement: "left"})
        @UpdateNewCreditLineState()
        @LoanTypeSelectionAllowedChanged()

        @initSwitch(".brokerCommisionSwitch", 'UseBrokerSetupFee', @toggleValue, 'ChangeBrokerSetupFee')
        @initSwitch(".setupFeeSwitch", 'UseSetupFee', @toggleValue, 'ChangeSetupFee')
        @initSwitch(".sendEmailsSwitch", 'AllowSendingEmail', @toggleValue, 'AllowSendingEmails')

        if @model.get('IsLoanTypeSelectionAllowed') in [ 2, '2' ]
            @$el.find('button[name=isLoanTypeSelectionAllowed]').attr('disabled', 'disabled')
        else
            @$el.find('button[name=isLoanTypeSelectionAllowed]').removeAttr('disabled')

        @validateLoanSourceRelated()

    initSwitch: (elemClass, param, func, method) ->
        that = this
        state = @model.get(param)
        @$el.find(elemClass).bootstrapSwitch()
        @$el.find(elemClass).bootstrapSwitch('setState', state)
        @$el.find(elemClass).on('switch-change', (event, state) ->
            func.call(that, event, state, method, param)
        )

    toggleValue: (event, state, method, param) ->
        id = @model.get 'CashRequestId'
        BlockUi()

        $.post(window.gRootPath + 'Underwriter/ApplicationInfo/' + method,
            id: id
            enabled: state.value
        ).done( (result) =>
            if result.error
                EzBob.App.trigger 'error', result.error
            else
                @model.set(param, result.status)
                
            if (!isNaN(result.setupFee))
                @model.set("SetupFee", result.setupFee)
            
        ).always( ->
            UnBlockUi()
        )

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
        if(type)
            @set "LoanType", type.text