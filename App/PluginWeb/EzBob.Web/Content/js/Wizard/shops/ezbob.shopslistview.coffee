root = exports ? this
root.EzBob = root.EzBob or {}
#
#    renders list of stores.
#
class EzBob.StoreListView extends Backbone.Marionette.ItemView
  template: "#ebay-store-list"
  serializeData: ->
    stores: @model.toJSON()
  initialize: ->
    @model.on "all", @render, this