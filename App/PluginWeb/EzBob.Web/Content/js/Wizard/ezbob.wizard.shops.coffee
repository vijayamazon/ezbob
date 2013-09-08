root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoStepModel extends EzBob.WizardStepModel
  initialize: (options) ->
    @set
      ebayStores: new EzBob.EbayStoreModels(options.ebayMarketPlaces)
      amazonMarketplaces: new EzBob.AmazonStoreModels(options.amazonMarketPlaces)
      isOffline: options.isOffline

  getStores: ->
    stores = []

    ebays = @get("ebayStores").toJSON()
    amazons = @get("amazonMarketplaces").toJSON()
    ekms = @get("ekmShops")
    freeagents = @get("freeAgentAccounts")
    sageAccounts = @get("sageAccounts")
    payPoints = @get("payPointAccounts")
    yodlees = @get("yodleeAccounts")
    paypals = @get("paypalAccounts")

    for shop in ebays
        stores.push {displayName: shop.displayName, type: "eBay"}

    for shop in amazons
        stores.push {displayName: shop.displayName, type: "Amazon"}

    for shop in ekms
        stores.push {displayName: shop.displayName, type: "EKM"}

    for shop in payPoints
        stores.push {displayName: shop.displayName, type: "PayPoint"}

    for shop in yodlees
        stores.push {displayName: shop.displayName, type: "Yodlee"}

    for shop in paypals
        stores.push {displayName: shop.displayName, type: "paypal"}
        
    for shop in freeagents
        stores.push {displayName: shop.displayName, type: "FreeAgent"}

    for shop in sageAccounts
        stores.push {displayName: shop.displayName, type: "Sage"}

    aryCGAccounts = $.parseJSON $('div#cg-account-list').text()

    for accountTypeName, accountTypeData of aryCGAccounts
      listOfShops = @get accountTypeData.ClientSide.StoreInfoStepModelShops
      if listOfShops != undefined
        for shop in listOfShops
          stores.push { displayName: shop.displayName, type: accountTypeName }

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
    @ready()
    @trigger "next"

  previous: ->
    @trigger "previous"

  render: ->
    @StoreInfoView.render().$el.appendTo @$el
    this