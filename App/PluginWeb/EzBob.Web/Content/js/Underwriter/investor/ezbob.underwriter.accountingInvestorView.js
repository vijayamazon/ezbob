var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AccountingInvestorModel = Backbone.Model.extend({
	url: '' + gRootPath + 'Underwriter/Investor/GetAccountingData'
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
			data: this.model.get('AccountingList')
		};
	},

	events: {

	},

	onRender: function() {
		this.displayAccountingData();

	},


	displayAccountingData: function() {
		
		this.model.fetch();

	}, // displayAccountingData


	show: function() {
		

		return this.$el.show();
	},

	hide: function() {
		return this.$el.hide();
	},


});
