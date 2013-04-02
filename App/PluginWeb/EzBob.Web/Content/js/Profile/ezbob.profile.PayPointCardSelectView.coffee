root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.PayPointCardSelectView extends Backbone.Marionette.ItemView
    template: '#PayPointCardSelectViewTemplate'

    initialize: ->
        @cards = (card for card in @model.get('PayPointCards') when card.ExpireDate? && moment(card.ExpireDate).toDate() > moment(@options.date).toDate())

    events:
        'change input[name="cardOptions"]' : 'optionsChanged'
        'click .btn-continue' : 'continue'

    ui: cont: '.btn-continue'

    optionsChanged: ->
        @onRender()

    onRender: ->
        val = @getCardType()
        select = @$el.find 'select'
        if val == 'useExisting'
            select.removeAttr 'readonly disabled'
            @ui.cont.text 'Confirm'
        else
            @ui.cont.text 'Continue'
            select.attr 
                'readonly': 'readonly'
                'disabled': 'disabled'

    getCardType: -> @$el.find('input[name="cardOptions"]:checked').val()

    hasCards: ->
        @cards.length > 1

    serializeData: -> cards: @cards

    continue: ->
        val = @getCardType()
        if val == 'useExisting'
            @trigger 'select', @$el.find('option:selected').val()
        else
            @trigger 'existing'
        @close()
        return false