root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.JqModalRegion extends  Backbone.Marionette.Region
    constructor: ->
        @on 'view:show', @showModal, @
        @dialog = $('<div/>')
        super()
    
    el: 'fake'

    getEl: (selector) ->
        @dialog

    showModal: (view)->
        view.on 'close', @.hideModal, @
        @dialog.dialog(view.jqoptions())
        @dialog.one 'dialogclose', =>
            @close()
        @dialog.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn')

    hideModal: ->
        @dialog.dialog('destroy')
###
class EzBob.TestView extends Backbone.Marionette.View
    constructor: ->
        _.bindAll @
        super()
    render: ->
        @$el.html('<h2>Hello!</h2>')

    jqoptions: ->
        buttons:
            'Ok': @onOk
            'Cancel': @onCancel
        modal: true
        title: 'A test modal window'

    onOk: ->
        console.log 'ok'
        @.close()

    onCancel: ->
        console.log 'cancel'
        @.close()

    close: ->
        console.log 'view closed'
        super()
###