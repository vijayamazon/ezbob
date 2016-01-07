var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AccountingInvestorModel = Backbone.Model.extend({
	url: '' + gRootPath + 'Underwriter/Investor/GetAccountingData'
});

EzBob.Underwriter.TransactionsModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/GetTransactionsData'
});

EzBob.Underwriter.AccountingInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#accounting-investor-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.AccountingInvestorModel();
		this.model.on("change reset", this.render, this);
		this.includeNonActiveInvestors = false;
		return this;
	},

	ui: {
		form: 'form#accounting-investor-form',
		includeNonActiveInvestorsCheckbox: '.includeNonActiveInvestors'
	},

	serializeData: function() {
		return {
			data: this.filterInvestors()
		};
	},

	events: {
		"click .includeNonActiveInvestors": "showInvestorsData",
		"click .funding-transactions": "openFundingTransactions",
		"click .repayments-transactions": "openRpaymentsTransactions"
		
	},

	showInvestorsData: function() {
		this.includeNonActiveInvestors = this.ui.includeNonActiveInvestorsCheckbox.is(':checked');
		this.render();
	},

	onRender: function() {
		this.displayAccountingData();
		if (this.includeNonActiveInvestors) {
			this.ui.includeNonActiveInvestorsCheckbox.attr('checked', 'checked');
		}
	},


	displayAccountingData: function() {
		this.model.fetch(); 

	}, // displayAccountingData

	filterInvestors: function() {
		if (this.includeNonActiveInvestors) {
			return this.model.get("AccountingList");
		} else {
			var temp = _.filter(this.model.get("AccountingList"), function(a) {
				return a.IsInvestorActive;
			});
			return temp;
		}
	},

	openFundingTransactions: function(el) {
		var investorID = $(el.currentTarget).data('investorid');
		var investorIDTransactionsOpenNow = this.$el.find('tr.funding-acc-transactions').data('id');
		this.$el.find('tr.repayments-acc-transactions,tr.funding-acc-transactions').remove();
		if (investorIDTransactionsOpenNow !== investorID) {
			var newRow = $('<tr class="funding-acc-transactions" data-id=' + investorID + '><td colspan=8></td></tr>');
			var tr = $(el.currentTarget).closest('tr');
			tr.after(newRow);
			var transactionsEl = this.$el.find('tr.funding-acc-transactions td');
			this.fundingTransactionsModel = new EzBob.Underwriter.TransactionsModel({ InvestorID: investorID });
			this.transctionView = new EzBob.Underwriter.FundingTransactionsView({
				el: transactionsEl,
				investorID: investorID,
				model: this.fundingTransactionsModel
			});
		}
		
		this.fundingTransactionsModel.fetch({ data: { bankAccountType: 'Funding'} });
		
	},

	openRpaymentsTransactions: function(el) {
		var investorID = $(el.currentTarget).data('investorid');
		var investorIDTransactionsOpenNow = this.$el.find('tr.repayments-acc-transactions').data('id');
		this.$el.find('tr.repayments-acc-transactions,tr.funding-acc-transactions').remove();
		if (investorIDTransactionsOpenNow !== investorID) {
			var newRow = $('<tr class="repayments-acc-transactions" data-id=' + investorID + '><td colspan=8></td></tr>');
			var tr = $(el.currentTarget).closest('tr');
			tr.after(newRow);
			var transactionsEl = this.$el.find('tr.repayments-acc-transactions td');
			this.repaymentsTransactionsModel = new EzBob.Underwriter.TransactionsModel({ InvestorID: investorID });
			this.transctionView = new EzBob.Underwriter.RepaymentsTransactionsView({
				el: transactionsEl,
				investorID: investorID,
				model: this.repaymentsTransactionsModel
			});
		}

		this.repaymentsTransactionsModel.fetch({ data: { bankAccountType: 'Repayments' } });

	},

	show: function() {
		

		return this.$el.show();
	},

	hide: function() {
		return this.$el.hide();
	},


});
