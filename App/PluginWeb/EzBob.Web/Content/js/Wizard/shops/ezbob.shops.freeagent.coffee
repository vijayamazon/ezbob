root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.FreeAgentAccountInfoView extends Backbone.View                
    initialize: (options) ->
        that = this;
        window.FreeAgentAccountAdded = (result) ->            
            if (result.error)
                EzBob.App.trigger('error', result.error);
            else
                EzBob.App.trigger('info', 'Congratulations. Free Agent account was added successfully.');
            
            that.trigger('completed');
            that.trigger('ready');

        return false

class EzBob.FreeAgentAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/FreeAgentMarketPlaces/Accounts"

class EzBob.FreeAgentAccounts extends Backbone.Collection
    model: EzBob.FreeAgentAccountModel
    url: "#{window.gRootPath}Customer/FreeAgentMarketPlaces/Accounts"
