var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.TransactionsModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/GetTransactionsData'
});

EzBob.Underwriter.FundingTransactionsView = Backbone.Marionette.ItemView.extend({
	template: '#funding-transactions-template',
	initialize: function(options) {
		this.investorAccountingModel = options.investorAccountingModel;
		this.investorID = options.investorID;
		this.investorAccountID = options.investorAccountID;
		this.isRangeSubmitted = false;
		this.model.on("change", this.render, this);
	},

	ui: {
		repaymentsTransactionsFrom: '#repayments-transactions-from',
		repaymentsTransactionsTo: '#repayments-transactions-to'
	},

	serializeData: function() {
		return {
			accountingData: this.applyRange(),
			InvestorID: this.investorID
		};
	},

	events: {
		"click #funding-transactions-range-submit-btn": "submitRange",
		"click .add-transaction-btn": "addTransaction"
	},

	onRender: function() {
		this.setUpView();
		this.ui.repaymentsTransactionsFrom.val(this.dateFrom);
		this.ui.repaymentsTransactionsTo.val(this.dateTo);
		this.$el.find('[data-toggle="tooltip"]').tooltip({
			placement: 'left', viewport: 'body'
		});
		return this;
	},
	
	setUpView: function() {
		this.$el.find('#funding-transactions-from,#funding-transactions-to').datepicker({ format: 'dd/mm/yyyy' });
	},

	addTransaction: function(el) {
		var button = $(el.currentTarget);
		var investorID = button.hide().data('id');
		var addTransactionEl = button.parent().find('.add-transaction-wrapper');		
		this.transctionView = new EzBob.Underwriter.AddTransactionView({
			el: addTransactionEl,
			investorID: investorID,
			investorAccountID: this.investorAccountID,
			investorAccountingModel: this.investorAccountingModel,
			transactionsModel: this.model,
			bankAccountType: 'Funding'
		});

		this.transctionView.render();

		return false;
	},

	submitRange: function() {
		this.isRangeSubmitted = true;

		this.dateFrom = this.ui.repaymentsTransactionsFrom.val();
		this.dateTo = this.ui.repaymentsTransactionsTo.val();

		this.render();
	},

	applyRange: function() {
		
		var transactionsFrom = this.$el.find('#repayments-transactions-from').val();
		var transactionsTo = this.$el.find('#repayments-transactions-to').val();

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