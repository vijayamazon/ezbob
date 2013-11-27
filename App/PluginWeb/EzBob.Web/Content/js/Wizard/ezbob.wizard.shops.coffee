root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.StoreInfoStepModel extends EzBob.WizardStepModel
  url: "#{window.gRootPath}Customer/MarketPlaces/Accounts"

  initialize: (options) ->
    @set
      isOffline: options.isOffline
      isProfile: options.isProfile
      stores: options.mpAccounts

  getStores: ->
    stores = []
    
    mpAccounts = @get('mpAccounts')
    
    if(mpAccounts)
        for shop in mpAccounts
            if shop.MpName == "Pay Pal"
                shop.MpName = "paypal"
            stores.push {displayName: shop.displayName, type: shop.MpName}

    return stores


class EzBob.StoreInfoStepView extends Backbone.View
  initialize: ->
    @readyToProceed = false
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
    @readyToProceed = true
    this