root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.CreditLineEditDialog extends Backbone.Marionette.ItemView
    template: '#credit-line-edit-dialog-template'
    
    initialize: (options) ->
        @model = options.model
        @modelBinder = new Backbone.ModelBinder()
        
        @method = null
        @medal = null
        @value = null

    events: 
        'click .btnOk': 'save'
        'click .suggested-amount-link': 'suggestedAmountClicked'

    ui:
        form: "form"
        amount: "#edit-offer-amount"

    jqoptions: ->
        modal: true
        resizable: true
        title: "Offer credit line edit"
        position: "center"
        draggable: true
        dialogClass: "credit-line-edit-popup"
        width: 550

    save:-> 
        return unless @ui.form.valid()
        post = $.post "#{window.gRootPath}ApplicationInfo/ChangeCashRequestOpenCreditLine", @getPostData()
        post.done => 
            @model.fetch()
            @trigger('done');
        @close()

    suggestedAmountClicked: (el) ->
        $elem = $(el.currentTarget)
        @method = $elem.data('method')
        @medal = $elem.data('medal')
        @value = $elem.data('value')
        @ui.amount.val(@value).change()
        
        @save()
        false

    getPostData:->
        m = @model.toJSON()
        
        data =
            id     : m.CashRequestId
            amount : m.amount
            method : @method
            medal  : @medal
            value  : @value

        return data
            
    bindings:
        amount:
            selector: "#edit-offer-amount"
            converter: EzBob.BindingConverters.moneyFormat

    onRender: -> 
        @modelBinder.bind @model, @el, @bindings
        @$el.find("#edit-offer-amount").autoNumeric(EzBob.moneyFormat)
        if(@$el.find("#edit-offer-amount").val() == "-")
            @$el.find("#edit-offer-amount").val("")

        @setValidator()

    serializeData: ->
        m: @model.toJSON()

    setValidator: ->
        @ui.form.validate
            rules:
                offeredCreditLine:
                    required:true
                    autonumericMin: EzBob.Config.XMinLoan
                    autonumericMax: EzBob.Config.MaxLoan
             messages:
                offeredCreditLine:
                    autonumericMin: "Offer is below limit."
                    autonumericMax: "Offer is above limit."
            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight