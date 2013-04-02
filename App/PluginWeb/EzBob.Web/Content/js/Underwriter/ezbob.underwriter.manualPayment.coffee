root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {} 

class EzBob.Underwriter.ManualPaymentView extends Backbone.Marionette.ItemView
    template: "#manualPayment-template"

    onRender: ->
        @$el.find('.ezDateTime').splittedDateTime()
        @validator = EzBob.validatemanualPaymentForm @ui.form
        @minAmount = 0.1
        @maxAmount = 0
        @updatePaymentData()
        @

    events:
        "click .confirm" : "confirmClicked"
        "click .uploadFiles" : "uploadFilesClicked"
        "change [name='totalSumPaid']" : "updatePaymentData"
        "change [name='paymentDate']" : "updatePaymentData"

    ui: {
            form : '#payment-form'
            money: "[name='totalSumPaid']"
            date: "[name='paymentDate']"
            fees: "[name='fees']"
            interest: "[name='interest']"
            principal: "[name='principal']"
        }

    confirmClicked: ->        
        return false unless @validator.form()
        @trigger "addPayment", @ui.form.serialize()
        @close()

    uploadFilesClicked: ->
        $("#addNewDoc").click()
        false

    updatePaymentData: ->
        data = {
            date: @ui.date.val()
            money: ValueOrDefault @ui.money.val(), 0
            loanId: @model.get "loanId"
        }
        request = $.get window.gRootPath + "Underwriter/LoanHistory/GetPaymentInfo", data
        request.done (r) =>
            return if r.error
            @ui.fees.val r.Fee
            @ui.principal.val r.Principal
            @ui.interest.val r.Interest
            @ui.money.val r.Amount

            @minAmount = r.MinValue
            @maxAmount = r.Balance

            moneyTitle = "Minium value = #{@minAmount}, maximum value = #{@maxAmount}"
            @ui.money.attr 'data-original-title', moneyTitle
            @ui.money.tooltip
                'trigger': 'hover'
                'title': moneyTitle
            @ui.money.tooltip("enable").tooltip('fixTitle')
