root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.JqModalRegion extends  Backbone.Marionette.Region
    constructor: ->
        @on 'view:show', @showModal, @
        @dialog = $('<div/>')
        super()
    
    isUnderwriter: document.location.href.indexOf("Underwriter") > -1

    el: 'fake'

    getEl: (selector) ->
        @dialog

    showModal: (view)->
        view.on 'close', @.hideModal, @
        option = view.jqoptions()
        if @isUnderwriter
            option['resizable'] = true
            option['draggable'] = true
        @dialog.dialog(option)
        @dialog.on 'dialogclose', =>
            @close()

        @dialog.find('.ui-dialog').addClass('box')
        @dialog.find('.ui-dialog-titlebar').addClass('box-title')

        @dialog.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn btn-primary')
        if view.onAfterShow
            view.onAfterShow.call(view)

    hideModal: ->
        @dialog.dialog('destroy')
