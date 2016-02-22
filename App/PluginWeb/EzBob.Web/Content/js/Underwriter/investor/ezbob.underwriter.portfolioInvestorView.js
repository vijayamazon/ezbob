var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PortfolioInvestorModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/PortfolioInvestor'
});

EzBob.Underwriter.PortfolioInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#portfolio-investor-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.PortfolioInvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},

	ui: {
		form: 'form#portfolio-investor-form'

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