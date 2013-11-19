root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.SageAccountInfoView extends Backbone.View                
    initialize: (options) ->
        that = this;
        window.SageAccountAdded = (result) ->            
            if (result.error)
                EzBob.App.trigger('error', result.error);
            else
                EzBob.App.trigger('info', 'Congratulations. Sage account was added successfully.');
            
            that.trigger('completed');
            that.trigger('ready');

        return false

class EzBob.SageAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/SageMarketPlaces/Accounts"

class EzBob.SageAccounts extends Backbone.Collection
    model: EzBob.SageAccountModel
    url: "#{window.gRootPath}Customer/SageMarketPlaces/Accounts"
