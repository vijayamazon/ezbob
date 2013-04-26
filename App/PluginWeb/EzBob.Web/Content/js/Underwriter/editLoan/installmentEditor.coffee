root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.AddInstallmentTrack
    constructor: (@installment, @loan) ->
        @installment.on "change:Date", @dateChanged, this

    dateChanged: ->
        date = moment.utc @installment.get("Date")
        before = @loan.getInstallmentBefore date
        after = @loan.getInstallmentAfter date

        balance = 0
        balanceBefore = 0

        if before is null
            balance = @loan.get('Amount')
            balanceBefore = balance

        if after is null
            balance = 0
            balanceBefore = before.get('Balance')

        if before isnt null and after isnt null
            balance = before.get('Balance') - after.get('Balance')
            balance = balance * (date.toDate() - moment.utc(before.get("Date")).toDate())
            balance = balance / (moment.utc(after.get("Date")).toDate() - moment.utc(before.get("Date")).toDate())
            balance = before.get('Balance') - balance
            balance = Math.round(balance * 100) / 100
            balanceBefore = before.get('Balance')

        @installment.set("BalanceBeforeRepayment": balanceBefore)
        @installment.set("Balance": balance)



class EzBob.InstallmentEditor extends Backbone.Marionette.ItemView
    template: "#loan_editor_edit_installment_template"

    initialize: ->
        console.log @model.toJSON()
        @oldValues = @model.toJSON()
        @modelBinder = new Backbone.ModelBinder()

        if @model.get("IsAdding")
            new EzBob.AddInstallmentTrack(@model, @options.loan)

    events:
        "click .cancel": "cancelChanges"
        "click .apply" : "saveChanges"

    bindings:
        Date:
            selector: "input[name='date']"
            converter: EzBob.BidingConverters.dateTime
        Balance:
            selector: "input[name='balance']"
            converter: EzBob.BidingConverters.floatNumbers
        Principal:
            selector: "input[name='loanRepayment']"
            converter: EzBob.BidingConverters.floatNumbers
        InterestRate:
            selector: "input[name='interestRate']"
            converter: EzBob.BidingConverters.percents
        Total:
            selector: "input[name='totalRepayment']"
            converter: EzBob.BidingConverters.floatNumbers
    ui:
        form: "form"
        shift: ".shift-installments :checkbox"
        shiftRates: ".shift-rates :checkbox"

    onRender: ->
        @setValidation()
        @modelBinder.bind @model, @el, @bindings
        @$el.find('input[name="date"]').datepicker(format: 'dd/mm/yyyy')
        @$el.find('input[data-content], span[data-content]').setPopover()

    setValidation: ->
        @ui.form.validate
            rules:
                date:
                    required:true
                    minlength: 6
                    maxlength: 20
                interestRate:
                    positive: true
                    max: 100
                balance:
                    min: 0
            messages:
                "date":
                    required: "Please, fill the installment date"
                "interestRate":
                    positive: "Interest rate cannot be less than zero"
                    max: "Interest rate cannot be greater than 100%"
                "balance": "Balance cannot be less than zero"
            errorPlacement: EzBob.Validation.errorPlacement
            unhighlight: EzBob.Validation.unhighlight

    saveChanges: ->
        return unless @ui.form.valid()

        if @ui.shift.prop('checked') && @oldValues.Date != @model.get("Date")
            @options.loan.shiftDate @model, @model.get("Date"), @oldValues.Date

        if @ui.shiftRates.prop('checked') && @oldValues.InterestRate != @model.get("InterestRate")
            @options.loan.shiftInterestRate @model, @model.get("InterestRate")

        @trigger "apply"
        @close()
        false

    cancelChanges: ->
        @model.set @oldValues
        @close()
        false

    onClose: ->
        @modelBinder.unbind()