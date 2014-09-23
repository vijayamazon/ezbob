root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.AddBankAccount extends EzBob.BoundItemView
    template: '#add-bank-account-template'

    jqoptions: ->
        modal: true
        resizable: false
        title: "Add Bank Account"
        position: "center"
        draggable: false
        dialogClass: "add-bank-account-popup"
        width: 600

    events:
        "click .check-account": "check"

    bindings:
        BankAccount:
            selector: "input[name='number']"
        SortCode:
            selector: "input[name='sortcode']"
        AccountType:
            selector: "select[name='accountType']"

    initialize: (options) ->
        @model = new Backbone.Model(customerId: options.customerId, SortCode: "", BankAccount: "", AccountType:"")
        super()

    onRender: ->
        @validator = @$el.find('form').validate
            rules:
                number:
                    required: true
                    number: true
                    minlength: 8
                    maxlength: 8
                sortcode:
                    required: true
                    number: true
                    minlength: 6
                    maxlength: 6
                accountType:
                    required: true
            errorPlacement: EzBob.Validation.errorPlacement
            unhighlight: EzBob.Validation.unhighlight
        super()

    onSave: ->
        return false unless @validator.form()
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/TryAddBankAccount", @model.toJSON()
        xhr.done (r) =>
            if r.error?
                EzBob.ShowMessage(r.error, "Error")
                return 
            @trigger 'saved'
            @close()
        xhr.complete -> BlockUi "off"
        false

    check: ->
        return false unless @validator.form()
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/CheckBankAccount", @model.toJSON()
        xhr.done (r) ->
            return if r.error?
            view = new EzBob.Underwriter.BankAccountDetailsView model: new Backbone.Model(r)
            EzBob.App.jqmodal.show view
        xhr.complete -> BlockUi "off"