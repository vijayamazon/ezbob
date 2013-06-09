root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.FreeAgentAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'FreeAgent', logoText: '', shops: @model})

    update: ->
        @model.fetch().done -> EzBob.App.trigger 'ct:storebase.shop.connected'

class EzBob.FreeAgentAccountInfoView extends Backbone.Marionette.ItemView
    template: '#FreeAgentAccoutInfoTemplate'
    
    events: 
        'click #freeagentContinueBtn': 'continueClicked'
        "click a.back": "back"
                
    initialize: (options) ->
        that = this;
        window.FreeAgentAccountAdded = (result) ->            
            if (result.error)
                EzBob.App.trigger('error', result.error);
            else
                EzBob.App.trigger('info', 'Congratulations. Free Agent account was added successfully.');
            
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');

        return false
        
    continueClicked: (e) ->  
        return false if @$el.find('#freeagentContinueBtn').hasClass('disabled')

    render: ->
        super()
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link FreeAgent Account"

class EzBob.FreeAgentAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/FreeAgentMarketPlaces/Accounts"

class EzBob.FreeAgentAccounts extends Backbone.Collection
    model: EzBob.FreeAgentAccountModel
    url: "#{window.gRootPath}Customer/FreeAgentMarketPlaces/Accounts"
