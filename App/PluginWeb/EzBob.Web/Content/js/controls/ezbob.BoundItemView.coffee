root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.BoundItemView  extends Backbone.Marionette.ItemView
    events: {}
    initialize: ->
        @events['click .btn-primary'] = 'save'
        @modelBinder = new Backbone.ModelBinder()
        super()

    onRender: () ->
        @modelBinder.bind @model, @el, @bindings

    jqoptions: ->
        modal: true
        resizable: false
        title: "Bug Reporter"
        position: "center"
        draggable: false
        dialogClass: "bugs-popup"
        width: 500

    save: ->
        @trigger 'save'
        if @onSave?
            @onSave()

    onClose: ->
        @modelBinder.unbind()