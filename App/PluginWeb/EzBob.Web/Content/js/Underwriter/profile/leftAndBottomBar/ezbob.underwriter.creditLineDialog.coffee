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

    onChangeLoanTypeSelectionAllowed: ->
        if @cloneModel.get('IsLoanTypeSelectionAllowed')
            @$el.find('#loan-type, #repaymentPeriod').attr('disabled', 'disabled')
        else
            @$el.find('#loan-type, #repaymentPeriod').removeAttr('disabled')

    onChangeStartingDate:->
        startingDate =  moment.utc(@cloneModel.get("StartingFromDate"), "DD/MM/YYYY")
        if  startingDate isnt null
            endDate = startingDate.add('days', 1)
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
        data=
            id:m.CashRequestId
            loanType:$("#loan-type option:selected").val()
            amount:parseFloat(@$el.find("#offeredCreditLine").autoNumericGet())
            interestRate:m.InterestRate
            repaymentPeriod:m.RepaymentPerion
            offerStart:m.StartingFromDate
            offerValidUntil:m.OfferValidateUntil
            useSetupFee:m.UseSetupFee
            allowSendingEmail:m.AllowSendingEmail
            isLoanTypeSelectionAllowed:m.IsLoanTypeSelectionAllowed
        return data
            
    bindings:
       # OfferedCreditLine:
        #    selector: "input[name='offeredCreditLine']"
        InterestRate:
            selector:"input[name='interestRate']"
            converter:EzBob.BidingConverters.percentsFormat
        RepaymentPerion:
            selector: "input[name='repaymentPeriod']"
            converter:EzBob.BidingConverters.notNull            
        StartingFromDate:
            selector:"input[name='startingFromDate']"
        OfferValidateUntil:
            selector: "input[name='offerValidUntil']"
        UseSetupFee:
            selector:"input[name='enableSetupFee']"
        AllowSendingEmail:
            selector: "input[name='allowSendingEmail']"
        IsLoanTypeSelectionAllowed:
            selector: "input[name='isLoanTypeSelectionAllowed']"

    onRender: -> 
        @modelBinder.bind @cloneModel, @el, @bindings
        @$el.find("#startingFromDate, #offerValidUntil").mask("99/99/9999").datepicker({ autoclose: true, format: 'dd/mm/yyyy' })
        @$el.find("#offeredCreditLine").autoNumeric (EzBob.moneyFormat)
        @$el.find("#interestRate").autoNumeric (EzBob.percentFormat)
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

            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight