root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.CreditLineDialog extends Backbone.Marionette.ItemView
    template: '#credit-line-dialog-template'
    
    initialize: () ->
        @cloneModel = @model.clone()
        @modelBinder = new Backbone.ModelBinder()
    
    events: 
        'click .btnOk': 'save'
    
    ui:
        form: "form"

    save:-> 
        return unless @ui.form.valid()
        postData = @serializeData()
        action = "#{window.gRootPath}Underwriter/ApplicationInfo/ChangeCreditLine"
        post = $.post action, postData 
        post.done => @model.fetch()
        @close()
    
    serializeData:->
        m = @cloneModel.toJSON()
        data=
            id:m.CashRequestId
            loanType:m.LoanTypeId
            amount:m.OfferedCreditLine
            interestRate:m.InterestRate
            repaymentPeriod:m.RepaymentPerion
            offerStart:m.StartingFromDate
            offerValidUntil:m.OfferValidateUntil
            useSetupFee:m.UseSetupFee
        return data
            
    bindings:
        OfferedCreditLine:
            selector: "input[name='offeredCreditLine']"
        InterestRate:
            selector:"input[name='interestRate']"
            converter:EzBob.BidingConverters.percents
        RepaymentPerion:
            selector: "input[name='repaymentPeriod']"
        StartingFromDate:
            selector:"input[name='startingFromDate']"
        OfferValidateUntil:
            selector: "input[name='offerValidUntil']"
        UseSetupFee:
            selector:"input[name='enableSetupFee']"

    onRender: -> 
        @modelBinder.bind @cloneModel, @el, @bindings
        @$el.find("#startingFromDate, #offerValidUntil").datepicker({ autoclose: true, format: 'dd/mm/yyyy' }).datepicker('show');
        @$el.find("#offeredCreditLine").autoNumeric (EzBob.moneyFormat)
        @$el.find("#interestRate").autoNumeric (EzBob.percentFormat)
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
                    min: 1
                    max: 100

                startingFromDate:
                    required:true
                    dateCheck: true
                offerValidUntil:
                    required:true
                    dateCheck: true

            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight
