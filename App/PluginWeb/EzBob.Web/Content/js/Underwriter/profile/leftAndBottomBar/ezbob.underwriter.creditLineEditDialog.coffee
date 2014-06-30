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
        "keydown" : "onEnterKeydown"
        'change .percent' : 'percentChanged'

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
        width: 800

    percentChanged: (event) ->
        $elem = $(event.target)
        percent = $elem.autoNumericGet() / 100
        method = $elem.data('method')
        value = $elem.data('value')
        link = @$el.find('a.Manual' + method)
        link.text("Manual offer " + EzBob.formatPoundsNoDecimals(value * percent) + " (" + EzBob.formatPercents(percent) + ")")
            .data('value', value * percent)
            .data('percent', percent)

    onEnterKeydown: (event) ->
        if event.keyCode is 13
            @ui.amount.change().blur()
            @save()
            return false
        true

    save:->
        return unless @validator.checkForm()
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
        @percent = $elem.data('percent')
        @ui.amount.val(@value).change().blur()
        
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
            percent : @percent

        return data
            
    bindings:
        amount:
            selector: "#edit-offer-amount"
            converter: EzBob.BindingConverters.moneyFormat

    onRender: -> 
        @modelBinder.bind @model, @el, @bindings
        @$el.find("#edit-offer-amount").autoNumeric(EzBob.moneyFormat)
        @$el.find(".percent").autoNumeric(EzBob.percentFormat).blur()
        if(@$el.find("#edit-offer-amount").val() == "-")
            @$el.find("#edit-offer-amount").val("")

        @validator = @setValidator()

    serializeData: ->
        m: @model.toJSON()

    setValidator: ->
        @ui.form.validate
            rules:
                editOfferAmount:
                    required:true
                    autonumericMin: EzBob.Config.XMinLoan
                    autonumericMax: EzBob.Config.MaxLoan
             messages:
                editOfferAmount:
                    autonumericMin: "Offer is below limit."
                    autonumericMax: "Offer is above limit."
            errorPlacement: EzBob.Validation.errorPlacement,
            unhighlight: EzBob.Validation.unhighlight