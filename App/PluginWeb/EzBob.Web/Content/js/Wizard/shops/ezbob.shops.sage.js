var EzBob = EzBob || {};

EzBob.SageAccountInfoView = Backbone.View.extend({
	initialize: function (options) {
		var that;
		that = this;
		window.SageAccountAdded = function (result) {
			if (result.error) {
				EzBob.App.trigger('error', result.error);
			} else {
				EzBob.App.trigger('info', 'Congratulations. Sage account was added successfully.');
			}
			$.colorbox.close();
			that.trigger('completed');
			return that.trigger('ready');
		};
		return false;
	}
});

EzBob.SageAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/SageMarketPlaces/Accounts"
});

EzBob.SageAccounts = Backbone.Collection.extend({
	model: EzBob.SageAccountModel,
	url: "" + window.gRootPath + "Customer/SageMarketPlaces/Accounts"
});
