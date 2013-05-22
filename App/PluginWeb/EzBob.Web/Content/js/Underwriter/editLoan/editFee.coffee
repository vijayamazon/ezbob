root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.FeeEditor extends Backbone.Marionette.ItemView
    template: "#loan_editor_edit_fee_template"

    initialize: ->
        @oldValues = @model.toJSON()
        @modelBinder = new Backbone.ModelBinder()

    events:
        "click .cancel": "cancelChanges"
        "click .apply" : "saveChanges"

    bindings:
        Date:
            selector: "input[name='date']"
            converter: EzBob.BindingConverters.dateTime
        Fees:
            selector: "input[name='fees']"
            converter: EzBob.BindingConverters.floatNumbers
        Description:
            selector: "textarea[name='description']"
    ui:
        form: "form"

    onRender: ->
        @setValidation()
        @modelBinder.bind @model, @el, @bindings
        @$el.find('input[name="date"]').datepicker(format: 'dd/mm/yyyy')

    setValidation: ->
        minDate = @options.loan.get("Date")
    
        #ajdust min date to last paid/paid early installment
        @options.loan.get("Items").forEach (installment) ->
            status = installment.get("Status")
            return unless installment.get("Type") is "Installment"
            return unless status is "Paid" or status is "PaidEarly"
            minDate = installment.get("Date")

        @ui.form.validate
            rules:
                date:
                    required:true
                    minlength: 6
                    maxlength: 20
                    minDate: minDate
                fees:
                    min: 0
            messages:
                "date":
                    required: "Please, fill the installment date"
                    minDate: "Fee cannot be added before loan starts or before paid installment"
            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight

    saveChanges: ->
        return unless @ui.form.valid()
        @trigger "apply"
        @close()
        false

    cancelChanges: ->
        @model.set @oldValues
        @close()
        false

    onClose: ->
        @modelBinder.unbind()