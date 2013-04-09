﻿root = exports ? this
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
    ui:
        form: "form"

    onChangeStartingDate:->
        endDate = moment.utc(@cloneModel.get("StartingFromDate"), "DD/MM/YYYY").add('days', 1)
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
        return data
            
    bindings:
       # OfferedCreditLine:
        #    selector: "input[name='offeredCreditLine']"
        InterestRate:
            selector:"input[name='interestRate']"
            converter:EzBob.BidingConverters.percentsFormat
        RepaymentPerion:
            selector: "input[name='repaymentPeriod']"
            
        StartingFromDate:
            selector:"input[name='startingFromDate']"
        OfferValidateUntil:
            selector: "input[name='offerValidUntil']"
        UseSetupFee:
            selector:"input[name='enableSetupFee']"
        AllowSendingEmail:
            selector: "input[name='allowSendingEmail']"

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
                    autonumericMax: 100
                
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
                    
            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight
