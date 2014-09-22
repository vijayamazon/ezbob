var EzBob = EzBob || {};

EzBob.StoreInfoStepModel = EzBob.WizardStepModel.extend({
	url: "" + window.gRootPath + "Customer/MarketPlaces/Accounts",
	initialize: function (options) {
		return this.set({
			customer: options,
			isOffline: options.IsOffline,
			isProfile: options.IsProfile,
			stores: options.mpAccounts
		});
	},
	getStores: function () {
		var mpAccounts, shop, stores, _i, _len;
		stores = [];
		mpAccounts = this.get('mpAccounts');
		if (mpAccounts) {
			for (_i = 0, _len = mpAccounts.length; _i < _len; _i++) {
				shop = mpAccounts[_i];
				if (shop.MpName === "Pay Pal") {
					shop.MpName = "paypal";
				}
				stores.push({
					displayName: shop.displayName,
					type: shop.MpName
				});
			}
		}
		return stores;
	}
});

EzBob.StoreInfoStepView = Backbone.View.extend({
	initialize: function () {
		this.readyToProceed = false;
		this.StoreInfoView = new EzBob.StoreInfoView({
			model: this.model
		});
		this.StoreInfoView.on("completed", this.completed, this);
		this.StoreInfoView.on("ready", this.ready, this);
		this.StoreInfoView.on("next", this.next, this);
		return this.StoreInfoView.on("previous", this.previous, this);
	},
	completed: function () {
		return this.trigger("completed");
	},
	ready: function () {
		return this.trigger("ready");
	},
	next: function () {
		this.ready();
		return this.trigger("next");
	},
	previous: function () {
		return this.trigger("previous");
	},
	render: function () {
		this.StoreInfoView.render().$el.appendTo(this.$el);
		this.readyToProceed = true;
		return this;
	}
});