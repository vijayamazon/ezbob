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
		'click .config-investor': 'submit'
	},

	submit: function() {
		return false;
	},

	onRender: function() {
		this.$el.find('#investorID').text(this.investorID);
	},

	show: function(id) {
		this.investorID = id;
		this.render();
		return this.$el.show();
	},

	hide: function() {
		return this.$el.hide();
	},


});