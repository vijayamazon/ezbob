var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorBankView = Backbone.Marionette.ItemView.extend({
	template: '#manage-investor-bank-template',
	initialize: function (options) {

	    this.model.on('change reset', this.render, this);
	    this.model.set('InvestorID', options.InvestorID, { silent: true });

	    this.model.set('Bank', options.Bank, { silent: true });
	    this.model.set('EditID', options.EditID, { silent: true });
	
	},//initialize

	events: {
		'click #investorBankSubmit': 'submit',
		'click #investorBankBack': 'cancel'
	},

	ui: {
		'form': '#addEditInvestorBankAccountForm',
		'phone': '.phone',
		'numeric': '.numeric',
		'AccountType': '#AccountType',
		'BankSortCode': '#BankSortCode',
		'BankAccountNumber': '#BankAccountNumber',
		'BankAccountName': '#BankAccountName',
		'IsActive': '#IsActive',
		'InvestorBankAccountID': '#InvestorBankAccountID'
	},//ui

	onRender: function () {
		this.ui.numeric.numericOnly(20);

		this.editID = this.model.get('editID');

		var self = this;
		if (this.editID) {
		    var bank=this.model.get('Bank');
		    if (bank) {
				this.ui.AccountType.val(bank.AccountType).change();
				this.ui.BankSortCode.val(bank.BankSortCode).change();
				this.ui.BankAccountNumber.val(bank.BankAccountNumber).change();
				this.ui.BankAccountName.val(bank.BankAccountName).change();
				this.ui.IsActive.prop('checked', bank.IsActive).change();
				this.ui.InvestorBankAccountID.val(bank.InvestorBankAccountID).change();
			}
		}

		this.ui.form.validate({
			rules: {
				'AccountType': { required: false },
				'BankSortCode': { required: true, digits: true },
				'BankAccountNumber': { required: true, digits: true },
				'BankAccountName': { required: true, },
			},
			messages: {
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
			ignore: ':not(:visible)'
		});
	},//onRender

	submit: function(){
		if (!this.ui.form.valid()) {
			return false;
		}
		BlockUi();
		var data = this.ui.form.serializeArray();
		data.push({ name: 'InvestorID', value: this.model.get('InvestorID') });

		var self = this;

		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/ManageInvestorBankAccount', data);
		xhr.done(function(res) {
			if (res.success) {
				EzBob.ShowMessage('Bank added/updated successfully', 'Done', null, 'Ok');
			
				self.trigger('cancel');

				$("tr[data-id='" + investorID + "']").click();
		
			} else {
				EzBob.ShowMessage(res.error, 'Failed saving investor bank', null, 'Ok');
			}
		});

		xhr.fail(function (res) {
			EzBob.ShowMessage(res, 'Failed saving investor bank', null, 'Ok');
		});
		xhr.always(function() {
			UnBlockUi();
		});

		return false;
	},

	cancel: function () {
	    this.remove();
	    this.unbind();
	    this.model.unbind("change reset", this.modelChanged);
	 //   $('.add-contact-row').show();
	    return false;
		
	}
});//EzBob.Underwriter.ManageInvestorBankView