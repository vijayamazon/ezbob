var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.LoanDetailsModel = Backbone.Model.extend({
	url: function() {
		return window.gRootPath + "Customer/Loan/Details/" + this.id;
	}, // url
}); // EzBob.Profile.LoanDetailsModel

EzBob.Profile.LoanModel = Backbone.Model.extend({
	urlRoot: function() {
		return window.gRootPath + "Customer/Loan/Get/" + this.loanId;
	}, // urlRoot
}); // EzBob.Profile.LoanModel

EzBob.Profile.LoanDetailsView = Backbone.Marionette.ItemView.extend({
	template: "#loan-details-template",

	initialize: function(options) {
		this.details = new EzBob.Profile.LoanDetailsModel({ id: this.model.loanId });
		this.bindTo(this.details, 'change reset', this.render, this);
		this.bindTo(this.model, 'change', this.render, this);
		this.details.fetch();
		this.customer = options.customer;
	}, // initialize

	onRender: function() {
		EzBob.UiAction.registerView(this);
		return this;
	}, // onRender

	serializeData: function() {
		var details = this.details.toJSON();

		return {
			loan: this.model.toJSON(),
			transactions: details.Transactions,
			schedule: details.Schedule,
			customer: this.customer.toJSON(),
			pacnetTransactions: details.PacnetTransactions,
			charges: details.Charges,
			area: 'Customer',
			rollovers: details.Rollovers
		};
	}, // serializeData
}); // EzBob.Profile.LoanDetailsView
