﻿var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};



EzBob.Underwriter.FundingTransactionsView = Backbone.Marionette.ItemView.extend({
	template: '#funding-transactions-template',
	initialize: function(options) {
		this.investorID = options.investorID;
		this.investorAccountID = options.investorAccountID;
		this.isRangeSubmitted = false;
		this.model.on("change", this.render, this);
	},

	serializeData: function() {
		return {
			accountingData: this.applyRange(),
			InvestorID: this.investorID
		};
	},

	events: {
		"click #funding-transactions-range-submit-btn": "submitRange",
		"click .add-funding-transaction-btn": "addTransaction"
	},

	onRender: function() {
		this.setUpView();
		$('#funding-transactions-from').val(this.dateFrom);
		$('#funding-transactions-to').val(this.dateTo);
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
			bankAccountType: 'Funding'
		});

		this.transctionView.render();
	},

	submitRange: function() {
		this.isRangeSubmitted = true;

		this.dateFrom = $('#funding-transactions-from').val();
		this.dateTo = $('#funding-transactions-to').val();

		this.render();
	},

	applyRange: function() {
		
		var transactionsFrom = $('#funding-transactions-from').val();
		var transactionsTo = $('#funding-transactions-to').val();

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