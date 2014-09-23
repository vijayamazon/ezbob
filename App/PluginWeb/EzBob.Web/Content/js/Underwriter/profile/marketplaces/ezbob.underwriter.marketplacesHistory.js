var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarketPlacesHistoryModel = Backbone.Model.extend({ idAttribute: "Id" });

EzBob.Underwriter.MarketPlacesHistory = Backbone.Collection.extend({
	model: EzBob.Underwriter.MarketPlacesHistoryModel,
	url: function() {
		return "" + window.gRootPath + "Underwriter/MarketPlaces/GetCustomerMarketplacesHistory/?customerId=" + this.customerId;
	}
});

EzBob.Underwriter.MarketPlacesHistoryView = Backbone.Marionette.ItemView.extend({
	template: "#marketplace-history-template",

	initialize: function() {
		this.model.on("reset change sync", this.render, this);
		this.loadMarketPlacesHistory();
	},

	loadMarketPlacesHistory: function() {
		var that = this;

		this.model.fetch().done(function() {
			if (that.model.length > 0)
				that.render();
		});
	},

	events: {
		"click .showHistoryMarketPlaces": "showHistoryMarketPlacesClicked",
		"click .showCurrentMarketPlaces": "showCurrentMarketPlacesClicked",
		"click .parseYodleeMp": "parseYodleeClicked",
		"click .uploadHmrcMp": "uploadHmrcClicked",
		"click .enterHmrcMp": "enterHmrcClicked"
	},

	serializeData: function() {
		return {
			MarketPlacesHistory: this.model
		};
	},

	showHistoryMarketPlacesClicked: function() {
		var date = this.$el.find("#mpHistoryDdl :selected").val();
		EzBob.App.vent.trigger('ct:marketplaces.history', date);
	},

	showCurrentMarketPlacesClicked: function() {
		EzBob.App.vent.trigger('ct:marketplaces.history', null);
	},

	parseYodleeClicked: function(event) {
		event.preventDefault();
		event.stopPropagation();
		EzBob.App.vent.trigger('ct:marketplaces.parseYodlee');
	},

	uploadHmrcClicked: function(event) {
		event.preventDefault();
		event.stopPropagation();
		EzBob.App.vent.trigger('ct:marketplaces.uploadHmrc');
	},

	enterHmrcClicked: function(event) {
		event.preventDefault();
		event.stopPropagation();
		EzBob.App.vent.trigger('ct:marketplaces.enterHmrc');
	}
});
