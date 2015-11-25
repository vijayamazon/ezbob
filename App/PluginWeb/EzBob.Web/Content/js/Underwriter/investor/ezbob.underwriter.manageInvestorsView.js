var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorsView = Backbone.Marionette.ItemView.extend({
	template: "#manage-investors-template",
	initialize: function () {
		this.model = new EzBob.Underwriter.InvestorsModels();
		this.model.on("change reset", this.render, this);
		return this;
	},
	ui: {
		
	},
	serializeData: function () {
		return {

		};
	},
	events: {
		
	},

	onRender: function () {
		console.log('this.models', this.model);
		return this;
	},

	show: function () {
		var self = this;
		this.model.fetch().done(function() {
			self.$el.show();
		});
	},
	hide: function () {
		return this.$el.hide();
	},
});
