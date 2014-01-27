root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.CreditLineDialog extends Backbone.Marionette.ItemView
    template: '#credit-line-dialog-template'
    
    initialize: () ->
        @cloneModel = @model.clone()
        @modelBinder = new Backbone.ModelBinder()
        @bindTo @cloneModel, "change:StartingFromDate", @onChangeStartingDate, this
    
    events: 
        'click .btnOk': 'save'
        'change #loan-type ' : 'onChangeLoanType'
        'click #isLoanTypeSelectionAllowed': 'onChangeLoanTypeSelectionAllowed'
        'change #isLoanTypeSelectionAllowed': 'onChangeLoanTypeSelectionAllowed'
    ui:
        form: "form"
    
    jqoptions: ->
        modal: true
        resizable: false
        title: "Credit Line"
        position: "center"
        draggable: false
        dialogClass: "creditline-popup"
        width: 840

    onChangeLoanTypeSelectionAllowed: ->
        controlledElements = '#loan-type, #repaymentPeriod'

        if @cloneModel.get('IsLoanTypeSelectionAllowed') in [ 1, '1' ]
            @$el.find(controlledElements).attr('disabled', 'disabled')
            if @cloneModel.get('LoanTypeId') != 1
                @cloneModel.set 'LoanTypeId', 1
        else
            @$el.find(controlledElements).removeAttr('disabled')

    onChangeStartingDate:->
        startingDate =  moment.utc(@cloneModel.get("StartingFromDate"), "DD/MM/YYYY")
        if  startingDate isnt null
            endDate = startingDate.add('hours', @cloneModel.get('OfferValidForHours'))
            @cloneModel.set("OfferValidateUntil", endDate.format('DD/MM/YYYY'))

    onChangeLoanType:->
        loanTypeId =+ @$el.find("#loan-type option:selected").val()
        currentLoanType = _.find(@cloneModel.get("LoanTypes"), (l) ->
            l.Id is loanTypeId
        )
        return unless loanTypeId?
        @cloneModel.set("RepaymentPerion", currentLoanType.RepaymentPeriod ) 
        @

    save:-> 
        return unless @ui.form.valid()
        
        postData = @getPostData()
        action = "#{window.gRootPath}Underwriter/ApplicationInfo/ChangeCreditLine"
        post = $.post action, postData
        post.done => @model.fetch()
        @close()
    
    getPostData:->
        m = @cloneModel.toJSON()
        
        data =
            id                          : m.CashRequestId
            loanType                    : m.LoanTypeId
            discountPlan                : m.DiscountPlanId
            amount                      : m.amount
            interestRate                : m.InterestRate
            repaymentPeriod             : m.RepaymentPerion
            offerStart                  : m.StartingFromDate
            offerValidUntil             : m.OfferValidateUntil
            useSetupFee                 : m.UseSetupFee
            useBrokerSetupFee           : m.UseBrokerSetupFee
            manualSetupFeeAmount        : m.ManualSetupFeeAmount
            manualSetupFeePercent       : m.ManualSetupFeePercent
            allowSendingEmail           : m.AllowSendingEmail
            isLoanTypeSelectionAllowed  : m.IsLoanTypeSelectionAllowed
            
        console.log("m,data",m, data)
        debugger
        return data
            
    bindings:
        InterestRate:
            selector: "input[name='interestRate']"
            converter: EzBob.BindingConverters.percentsFormat
        RepaymentPerion:
            selector: "input[name='repaymentPeriod']"
            converter:EzBob.BindingConverters.notNull
        StartingFromDate:
            selector:"input[name='startingFromDate']"
        OfferValidateUntil:
            selector: "input[name='offerValidUntil']"
        UseSetupFee:
            selector:"input[name='enableSetupFee']"
        UseBrokerSetupFee:
            selector:"input[name='enableBrokerSetupFee']"
        AllowSendingEmail:
            selector: "input[name='allowSendingEmail']"
        IsLoanTypeSelectionAllowed:
            selector: "select[name='isLoanTypeSelectionAllowed']"
        DiscountPlanId: "select[name='discount-plan']"
        LoanTypeId: "select[name='loan-type']"
        amount:
            selector: "#offeredCreditLine"
            converter: EzBob.BindingConverters.moneyFormat
        ManualSetupFeePercent:
            selector: "input[name='manualSetupFeePercent']"
            converter: EzBob.BindingConverters.percentsFormat
        ManualSetupFeeAmount:
            selector: "input[name='manualSetupFeeAmount']"
            converter: EzBob.BindingConverters.moneyFormat


    onRender: -> 
        @modelBinder.bind @cloneModel, @el, @bindings
        @$el.find("#startingFromDate, #offerValidUntil").mask("99/99/9999").datepicker({ autoclose: true, format: 'dd/mm/yyyy' })
        @$el.find("#offeredCreditLine").autoNumeric(EzBob.moneyFormat)
        if(@$el.find("#offeredCreditLine").val() == "-") 
            @$el.find("#offeredCreditLine").val("")

        @$el.find("#interestRate").autoNumeric (EzBob.percentFormat)
        @$el.find("#manualSetupFeePercent").autoNumeric(EzBob.percentFormat)
        @$el.find("#manualSetupFeeAmount").autoNumeric(EzBob.moneyFormat)
        @$el.find("#repaymentPeriod").numericOnly()
        @setValidator()
        
    setValidator: ->
        @ui.form.validate
            rules:
                offeredCreditLine:
                    required:true
                    autonumericMin: EzBob.Config.XMinLoan
                    autonumericMax: EzBob.Config.MaxLoan

                repaymentPeriod:
                    required:true
                    autonumericMin: 1
                
                interestRate:
                    required:true
                    autonumericMin: 1
                    autonumericMax: 100

                startingFromDate:
                    required:true
                    dateCheck: true
                    
                offerValidUntil:
                    required:true
                    dateCheck: true
                
                manualSetupFeeAmount:
                    autonumericMin: 0
                    required: false
                manualSetupFeePercent:
                    autonumericMin: 0
                    required: false


             messages:
                interestRate:
                    autonumericMin: "Interest Rate is below limit."
                    autonumericMax: "Interest Rate is above limit."
                repaymentPeriod:
                    autonumericMin: "Repayment Period is below limit."
                startingFromDate:
                    dateCheck:"Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012"
                offerValidUntil:
                    dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012"
                manualSetupFeeAmount:
                    autonumericMin: "Can't be negative."
                manualSetupFeePercent:
                    autonumericMin: "Can't be negative."

            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight