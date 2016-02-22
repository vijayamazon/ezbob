var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddTransactionView = Backbone.Marionette.ItemView.extend({
	template: '#add-transaction-template',
	initialize: function(options) {
		this.investorAccountingModel = options.investorAccountingModel;
		this.transactionsModel = options.transactionsModel;
		this.investorID = options.investorID;
		this.bankAccountType = options.bankAccountType;
		this.investorAccountID = options.investorAccountID;
	},

	ui: {
		'form': '#addTransactionForm',
		'addTransactionAmount': '#AddTransactionAmount',
		'addTransactionDate': '#AddTransactionDate',
		'addBankTransactionRef': '#AddBankTransactionRef',
		'addTransactionComment': '#AddTransactionComment',
		'money': '.cashInput'
	},//ui   

	events: {
		"click #add-transaction-cancel-btn": "cancelAddingTransaction",
		"click #add-transaction-submit-btn": "submitAddingTransaction"
	},

	onRender: function() {
		this.ui.addTransactionDate.datepicker({ format: 'dd/mm/yyyy' });
		this.ui.money.moneyFormat();

		this.ui.form.validate({
			rules: {
				AddTransactionDate: { required: true, requiredDate: true, dateCheck: true },
				AddTransactionAmount: { required: true, autonumericMin: 0, autonumericMax: 100000000 },
				AddBankTransactionRef: { required: false },
				AddTransactionComment: { required: false }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
			ignore: ':not(:visible)'
		});


		return this;
	},

	cancelAddingTransaction: function() {
		this.$el.empty();
		this.$el.parent().find('.add-transaction-btn').show();
	},

	submitAddingTransaction: function() {

		if (!this.ui.form.valid()) {
			return false;
		}
		
		var amount = this.ui.addTransactionAmount.autoNumeric('get');

		BlockUi();
		var submitParam = {
			investorID: this.investorID,
			investorAccountID: this.investorAccountID,
			transactionAmount: amount,
			transactionDate: this.ui.addTransactionDate.val(),
			bankAccountType: this.bankAccountType,
			transactionComment: this.ui.addTransactionComment.val(),
			bankTransactionRef: this.ui.addBankTransactionRef.val()
		};

		var self = this;
		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/AddTransaction', submitParam);
		

		xhr.done(function(res) {
			UnBlockUi();
			if (res.success) {
				self.investorAccountingModel.fetch();
				EzBob.App.vent.trigger('investorTransactionAdded', self.investorID, self.bankAccountType);
				EzBob.ShowMessage('Transfer made successfully', 'Done', null, 'Ok');
			} else {
				EzBob.ShowMessage("Failed making transfer", 'Failed', null, 'Ok');
			}
		});

		xhr.always(function() {
			UnBlockUi();
		});

		return false;
	}

}); 