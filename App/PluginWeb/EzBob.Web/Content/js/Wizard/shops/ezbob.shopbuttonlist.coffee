#
#    base class for store button with a list.
#    inherit from this view and define this.listView that will be added after store button
#
class EzBob.StoreButtonWithListView extends EzBob.StoreButtonView
    initialize: (options) ->
        #@listView.model.on "change reset", @listChanged, this
        super(options)

    #listChanged: -> 
        #@trigger("ready", @name)  if @listView.model.length > 0

    render: ->        
        super()
        #@$el.append @listView.render().$el
        this

    #update: ->
        #@listView.update()