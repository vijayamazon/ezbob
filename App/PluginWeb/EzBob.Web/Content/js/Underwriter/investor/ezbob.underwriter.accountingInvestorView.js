var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AccountingInvestorModel = Backbone.Model.extend({
	url: '' + gRootPath + 'Underwriter/Investor/GetAccountingData'
});

EzBob.Underwriter.AccountingInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#accounting-investor-template",
	initialize: function() {
		this.model.on("change reset", this.render, this);
		this.includeNonActiveInvestors = false;
		EzBob.App.vent.on('investorTransactionAdded', this.transactionAdded, this);
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
		"click .repayments-transactions": "openRepaymentsTransactions",
		"click .investor-scheduler": "openConfigScheduler"
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

		if (this.currentInvestorID && this.currentBankAccountType) {
			if (this.currentBankAccountType === 'Funding') {
				this.$el.find('.funding-transactions[data-investorid="' + this.currentInvestorID + '"]').click();
			}
			if (this.currentBankAccountType === 'Repayments') {
				this.$el.find('.repayments-transactions[data-investorid="' + this.currentInvestorID + '"]').click();
			}
		}

		this.$el.find('[data-toggle="tooltip"]').tooltip({
			placement: 'right', viewport: 'body'
		});
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
	    var tr = $(el.currentTarget).closest('tr');
		var investorIDTransactionsOpenNow = this.$el.find('tr.funding-acc-transactions').data('id');
		$('.investor-data-row').removeClass("active");
		this.$el.find('tr.repayments-acc-transactions,tr.funding-acc-transactions,tr.tr-investor-scheduler').remove();
		if (investorIDTransactionsOpenNow !== investorID) {
		    var newRow = $('<tr class="funding-acc-transactions investor-sub-content-box" data-id=' + investorID + '><td colspan=8></td></tr>');
		    tr.after(newRow);
		    var transactionsEl = this.$el.find('tr.funding-acc-transactions td');

		    var investorData = _.find(this.model.get("AccountingList"), function(a) {
		        return a.InvestorID === investorID;
		    });

		    if (investorData)
		        this.investorAccountID = investorData.FundingBankAccountID;

		    this.fundingTransactionsModel = new EzBob.Underwriter.TransactionsModel({ InvestorID: investorID });
		    this.transctionView = new EzBob.Underwriter.FundingTransactionsView({
		        el: transactionsEl,
		        investorID: investorID,
		        investorAccountID: this.investorAccountID,
		        investorAccountingModel: this.model,
		        model: this.fundingTransactionsModel
		    });
		    this.fundingTransactionsModel.fetch({ data: { bankAccountType: 'Funding' } });
		    $(tr).addClass("active");
		} else {
		    $(tr).removeClass("active");
		}

		return false;

	},

	openRepaymentsTransactions: function(el) {
	    var investorID = $(el.currentTarget).data('investorid');
	    var tr = $(el.currentTarget).closest('tr');
	    var investorIDTransactionsOpenNow = this.$el.find('tr.repayments-acc-transactions').data('id');
	    $('.investor-data-row').removeClass("active");
		this.$el.find('tr.repayments-acc-transactions,tr.funding-acc-transactions,tr.tr-investor-scheduler').remove();
		if (investorIDTransactionsOpenNow !== investorID) {
		    var newRow = $('<tr class="repayments-acc-transactions investor-sub-content-box" data-id=' + investorID + '><td colspan=8></td></tr>');
		  
		    tr.after(newRow);
		    var transactionsEl = this.$el.find('tr.repayments-acc-transactions > td');

		    var investorData = _.find(this.model.get("AccountingList"), function(a) {
		        return a.InvestorID === investorID;
		    });

		    if (investorData)
		        this.investorAccountID = investorData.RepaymentsBankAccountID;

		    this.repaymentsTransactionsModel = new EzBob.Underwriter.TransactionsModel({ InvestorID: investorID });
		    this.transctionView = new EzBob.Underwriter.RepaymentsTransactionsView({
		        el: transactionsEl,
		        investorID: investorID,
		        investorAccountID: this.investorAccountID,
		        investorAccountingModel: this.model,
		        model: this.repaymentsTransactionsModel
		    });

		    this.repaymentsTransactionsModel.fetch({ data: { bankAccountType: 'Repayments' } });
		    $(tr).addClass("active");
		} else {
		    $(tr).removeClass("active");
		}
		
		return false;
	},

	transactionAdded: function(investorID, banckAccountType) {
		this.currentInvestorID = investorID;
		this.currentBankAccountType = banckAccountType;
	},


	openConfigScheduler: function(el) {
	    var investorID = $(el.currentTarget).data('investorid');
	    var tr = $(el.currentTarget).closest('tr');
		var investorIDSchedulerOpenNow = this.$el.find('tr.tr-investor-scheduler').data('id');
		$('.investor-data-row').removeClass("active");
		this.$el.find('tr.repayments-acc-transactions,tr.funding-acc-transactions,tr.tr-investor-scheduler').remove();
		if (investorIDSchedulerOpenNow !== investorID) {
		    var newRow = $('<tr class="tr-investor-scheduler investor-sub-content-box" data-id=' + investorID + '><td colspan=8></td></tr>');

		    tr.after(newRow);
		    var schedulerEl = this.$el.find('tr.tr-investor-scheduler td');

		    this.configSchedulerModel = new EzBob.Underwriter.ConfigSchedulerModel({ InvestorID: investorID });
		    this.transctionView = new EzBob.Underwriter.ConfigSchedulerView({
		        el: schedulerEl,
		        investorID: investorID,
		        model: this.configSchedulerModel
		    });
		    $(tr).addClass("active");
		    this.configSchedulerModel.fetch();
		} else {
		    $(tr).removeClass("active");
		}

		return false;

	},

	show: function() {
		this.model.fetch();
		return this.$el.show();
	},

	hide: function() {
		return this.$el.hide();
	},


});
