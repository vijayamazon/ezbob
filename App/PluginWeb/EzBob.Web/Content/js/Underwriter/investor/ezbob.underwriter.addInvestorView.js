var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.InvestorModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/GetInvestor'
});

EzBob.Underwriter.InvestorBankModel = Backbone.Model.extend({
	defaults: {
		InvestorBankAccountType: '',
		BankSortCode: '',
		BankAccountNumber: '',
		BankAccountName: ''
	}, // defaults
}); // EzBob.InvestorBankModel

EzBob.Underwriter.InvestorBanksModels = Backbone.Collection.extend({
	model: EzBob.Underwriter.InvestorBanksModels
});

EzBob.Underwriter.AddInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#add-investor-template",
	initialize: function () {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},
	ui: {
		form: 'form#add-investor-form',
		phone: '.phone',
		numeric: '.numeric',
		sameBank: '#SameBank',
		secondBank: '.second-bank'
	},
	serializeData: function () {
		return {

		};
	},
	events: {
		'click .add-investor': 'addInvestorClicked',
		'change #SameBank': 'sameBankChanged'
	},

	onRender: function () {
		this.ui.phone.mask('0?9999999999', { placeholder: ' ' });
		this.ui.phone.numericOnly(11);
		this.ui.numeric.numericOnly(20);
		this.ui.form.validate({
			rules: {
				CompanyName: { required: true },
				InvestorType: { required: true },
				ContactPersonalName: { required: true },
				ContactLastName: { required: true },
				ContactEmail: { required: true, email: true, },
				Role: { required: false, },
				ContactMobile: { required: true, regex: '^0[0-9]{10}$' },
				ContactOfficeNumber: { required: true, regex: '^0[0-9]{10}$' },
				Comment: { required: false, },
				'InvestorBank[0].AccountType': { required: false, notEqual: '[id="InvestorBank[1].AccountType"]' },
				'InvestorBank[0].BankSortCode': { required: true, digits: true },
				'InvestorBank[0].BankAccountNumber': { required: true, digits: true },
				'InvestorBank[0].BankAccountName': { required: true, },
				'InvestorBank[1].AccountType': { required: false, notEqual: '[id="InvestorBank[0].AccountType"]' },
				'InvestorBank[1].BankSortCode': { required: true, digits: true },
				'InvestorBank[1].BankAccountNumber': { required: true, digits: true },
				'InvestorBank[1].BankAccountName': { required: true, },
			},
			messages: {
				"ContactMobile": { regex: 'Please enter a valid UK number' },
				"ContactOfficeNumber": { regex: 'Please enter a valid UK number' }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
			ignore: ":not(:visible)"
		});
		return this;
	},
	addInvestorClicked: function () {
		if (!this.ui.form.valid()) {
			return false;
		}
		BlockUi();
		var data = this.ui.form.serializeArray();
		var sameBankChecked = this.ui.secondBank.is(':checked');
		var sameBank = _.find(data, function (d) { return d.name === 'SameBank'; });
		if (sameBank) {
			sameBank.value = sameBankChecked;
		} else {
			data.push({ name: 'SameBank', value: sameBankChecked });
		}

		var self = this;
		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/AddInvestor', data);
		xhr.done(function(res) {
			if (res.success) {
				self.router.navigate('manageInvestor/' + res.InvestorID);
				self.router.handleRoute('manageInvestor', res.InvestorID);
			} else {
				EzBob.ShowMessage(res.error, "Failed adding investor", null, 'Ok');
			}
		});

		xhr.fail(function (res) {
			EzBob.ShowMessage(res, "Failed adding investor", null, 'Ok');
		});

		xhr.always(function() {
			UnBlockUi();
		});
		return false;
	},

	sameBankChanged: function(){
		this.ui.secondBank.toggle(!this.ui.sameBank.is(':checked'));
	},

	show: function () {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	},

	//bindings: {
	//	CompanyName: {
	//		selector: 'input[name="CompanyName"]',
	//	},
	//	InvestorType: {
	//		selector: 'input[name="InvestorType"]',
	//	},
	//	ContactPersonalName: {
	//		selector: 'input[name="ContactPersonalName"]',
	//	},
	//	ContactLastName: {
	//		selector: 'input[name="ContactLastName"]',
	//	},
	//	ContactEmail: {
	//		selector: 'input[name="ContactEmail"]',
	//	},
	//	Role: {
	//		selector: 'input[name="Role"]',
	//	},
	//	ContactMobile: {
	//		selector: 'input[name="ContactMobile"]',
	//	},
	//	ContactOfficeNumber: {
	//		selector: 'input[name="ContactOfficeNumber"]',
	//	},
	//	Comment: {
	//		selector: 'input[name="Comment"]',
	//	},
	//	InvestorBankAccountType: {
	//		selector: 'input[name="InvestorBankAccountType"]',
	//	},
	//	BankSortCode: {
	//		selector: 'input[name="BankSortCode"]',
	//	},
	//	BankAccountName: {
	//		selector: 'input[name="BankAccountName"]',
	//	},
	//}
});
