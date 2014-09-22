var EzBob = EzBob || {};

EzBob.FreeAgentAccountInfoView = Backbone.View.extend({
	initialize: function (options) {
		var that;
		that = this;
		window.FreeAgentAccountAdded = function (result) {
			if (result.error) {
				EzBob.App.trigger('error', result.error);
			} else {
				EzBob.App.trigger('info', 'Congratulations. Free Agent account was added successfully.');
			}
			$.colorbox.close();
			that.trigger('completed');
			return that.trigger('ready');
		};
		return false;
	}
});

EzBob.FreeAgentAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/FreeAgentMarketPlaces/Accounts"
});

EzBob.FreeAgentAccounts = Backbone.Collection.extend({
	model: EzBob.FreeAgentAccountModel,
	url: "" + window.gRootPath + "Customer/FreeAgentMarketPlaces/Accounts"
});
