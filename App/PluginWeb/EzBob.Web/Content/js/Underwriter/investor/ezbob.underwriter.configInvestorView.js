var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ConfigInvestorModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/ConfigInvestor'
});

EzBob.Underwriter.ConfigInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#config-investor-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.ConfigInvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},

	ui: {
		form: 'form#config-investor-form'

	},

	serializeData: function() {
		return {

		};
	},

	events: {

	},

	onRender: function() {

	},

	show: function() {
		return this.$el.show();
	},

	hide: function() {
		return this.$el.hide();
	},


});