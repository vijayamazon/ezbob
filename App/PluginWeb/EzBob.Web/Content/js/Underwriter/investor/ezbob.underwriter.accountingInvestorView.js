var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AccountingInvestorModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/AccountingInvestor'
});

EzBob.Underwriter.AccountingInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#accounting-investor-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.AccountingInvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},

	ui: {
		form: 'form#accounting-investor-form'

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
