root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoStepModel extends EzBob.WizardStepModel
  initialize: (options) ->
    @set
      ebayStores: new EzBob.EbayStoreModels(options.ebayMarketPlaces)
      amazonMarketplaces: new EzBob.AmazonStoreModels(options.amazonMarketPlaces)


  getStores: ->
    stores = []
    ebays = @get("ebayStores").toJSON()
    amazons = @get("amazonMarketplaces").toJSON()
    ekms = @get("ekmShops")
    volusions = @get("volusionShops")
    payPoints = @get("payPointAccounts")

    for shop in ebays
        stores.push {displayName: shop.displayName, type: "Ebay"}

    for shop in amazons
        stores.push {displayName: shop.displayName, type: "Amazon"}

    for shop in ekms
        stores.push {displayName: shop.displayName, type: "EKM"}

    for shop in volusions
        stores.push {displayName: shop.displayName, type: "Volusion"}

    for shop in payPoints
        stores.push {displayName: shop.displayName, type: "PayPoint"}

    return stores

class EzBob.StoreInfoStepView extends Backbone.View
  initialize: ->
    @StoreInfoView = new EzBob.StoreInfoView(model: @model)
    @StoreInfoView.on "completed", @completed, this
    @StoreInfoView.on "ready", @ready, this
    @StoreInfoView.on "next", @next, this
    @StoreInfoView.on "previous", @previous, this

  completed: ->
    @trigger "completed"

  ready: ->
    @trigger "ready"

  next: ->
    @trigger "next"

  previous: ->
    @trigger "previous"

  render: ->
    @StoreInfoView.render().$el.appendTo @$el
    this