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

    save: ->
        @trigger 'save'
        if @onSave?
            @onSave()

    onClose: ->
        @modelBinder.unbind()