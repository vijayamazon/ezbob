var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddTransactionView = Backbone.Marionette.ItemView.extend({
	template: '#add-transaction-template',
	initialize: function(options) {
		this.investorID = options.investorID;
		this.bankAccountType = options.bankAccountType;
		this.investorAccountID = options.investorAccountID;
	},

	events: {
		"click #add-transaction-cancel-btn": "cancelAddingTransaction",
		"click #add-transaction-submit-btn": "submitAddingTransaction"
	},

	onRender: function() {
		this.setUpView();
		return this;
	},

	setUpView: function() {
		this.$el.find('#add-transaction-date').datepicker({ format: 'dd/mm/yyyy' });
	},

	cancelAddingTransaction: function() {
		this.$el.empty();
		this.$el.parent().find('.add-funding-transaction-btn').show();
	},

	submitAddingTransaction: function() {
		var submitParam = {
			investorAccountID: this.investorAccountID,
			transactionAmount: $('#add-transaction-amount').val(),
			transactionDate: $('#add-transaction-date').val(),
			bankAccountType: this.bankAccountType,
			transactionComment: $('#add-transaction-comment').val()
		};
		var submit = $.post('' + window.gRootPath + 'Underwriter/Investor/AddTransaction', submitParam);
		var self = this;
		BlockUi('on');
		submit.done(function() {
			self.cancelAddingTransaction();
			this.model = new EzBob.Underwriter.AccountingInvestorModel();
			this.model.on("change reset", this.render, this);
		});
	},

	show: function() {

		return this.$el.show();
	},
}); 