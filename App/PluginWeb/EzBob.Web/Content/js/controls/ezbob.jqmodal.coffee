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
