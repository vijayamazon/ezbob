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
        @dialog.one 'dialogclose', =>
            @close()
        @dialog.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn')
        if view.onAfterShow
            view.onAfterShow.call(view)

    hideModal: ->
        @dialog.dialog('destroy')
