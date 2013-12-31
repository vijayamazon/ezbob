root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.PayPointCardModel extends Backbone.Model


class EzBob.Underwriter.AddPayPointCardView extends Backbone.Marionette.ItemView
    template: '#add-paypoint-card-template'
    
    jqoptions: ->
        modal: true
        resizable: false
        title: "Add Paypoint"
        position: "center"
        draggable: false
        dialogClass: "add-paypoint-popup"
        width: 620

    events:
        'click .btn-primary'  : 'save'
    ui:
        'transactionid' : 'input[name="transactionid"]'
        'cardno' : 'input[name="cardno"]'
        'expiredate' : 'input[name="expiredate"]'

    onRender: -> 
        @setValidator()
        @ui.expiredate.mask '99/99'

    save: ->
        return false unless @validator.form()
        @model.set
            'transactionid' : @ui.transactionid.val()
            'cardno' : @ui.cardno.val()
            'expiredate' :moment(@ui.expiredate.val(), 'MM/YY').toDate().toJSON()
        @trigger 'save'
        @close()

    setValidator: ->
        @validator = @$el.find('form').validate
            rules:
                transactionid:
                    required: true
                cardno:
                    required: true
                    number: true
                    minlength: 4
                expiredate:
                    required: true, regex: "^(0[1-9]|1[012])/\([0-9]{2})"
