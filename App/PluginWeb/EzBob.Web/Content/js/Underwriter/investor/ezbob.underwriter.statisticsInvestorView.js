var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.StatisticsInvestorModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/StatisticsInvestor'
});

EzBob.Underwriter.StatisticsInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#statistics-investor-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.StatisticsInvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},

	ui: {
		form: 'form#statistics-investor-form'

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