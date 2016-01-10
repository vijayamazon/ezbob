var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.RepaymentsTransactionsView = Backbone.Marionette.ItemView.extend({
	template: '#repayments-transactions-template',
	initialize: function(options) {
		this.investorID = options.investorID;
		console.log('investor', this.investorID);
		//this.model.set('InvestorID', this.invetorID);
		this.isRangeSubmitted = false;
		this.model.on("change", this.render, this);
	},

	serializeData: function() {
		return {
			accountingData: this.applyRange()
		};
	},

	events: {
		"click #repayments-transactions-range-submit-btn": "submitRange"
	},

	onRender: function() {
		this.setUpView();
		$('#repayments-transactions-from').val(this.dateFrom);
		$('#repayments-transactions-to').val(this.dateTo);
		return this;
	},

	setUpView: function() {
		this.$el.find('#repayments-transactions-from,#repayments-transactions-to').datepicker({ format: 'dd/mm/yyyy' });
	},

	submitRange: function() {
		this.isRangeSubmitted = true;

		this.dateFrom = $('#repayments-transactions-from').val();
		this.dateTo = $('#repayments-transactions-to').val();

		this.render();
	},

	applyRange: function() {

		var transactionsFrom = $('#repayments-transactions-from').val();
		var transactionsTo = $('#repayments-transactions-to').val();

		if (!this.isRangeSubmitted) {
			return this.model.get("TransactionsList");
		} else {
			var temp = _.filter(this.model.get("TransactionsList"), function(a) {
				return ((moment(transactionsFrom, "DD/MM/YYYY") < moment(a.TransactionDate)) && (moment(a.TransactionDate) < moment(transactionsTo, "DD/MM/YYYY").add(1, 'days')));
			});
			return temp;
		}
	},

	show: function() {

		return this.$el.show();
	},
});