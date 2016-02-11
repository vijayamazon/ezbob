var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};
Backbone.Model.prototype.toJSON = function () {
    var json = _.clone(this.attributes);
    for (var attr in json) {
        if ((json[attr] instanceof Backbone.Model) || (json[attr] instanceof Backbone.Collection)) {
            json[attr] = json[attr].toJSON();
        }
    }
    return json;
};
EzBob.Underwriter.InvestorContactModel = Backbone.Model.extend({
    
    defaults: {
      
        Comment: '',
        ContactEmail: '',
        ContactLastName: '',
        ContactMobile: '',
        ContactOfficeNumber: '',
        ContactPersonalName: '',
        InvestorContactID: '',
        IsActive: '',
        IsPrimary: '',
        Role: '',
        TimeStamp: ''
    }, // defaults
  
    idAttribute: 'InvestorContactID'
});
EzBob.Underwriter.InvestorBankmodel = Backbone.Model.extend({
    defaults: {
        AccountType: '',
        AccountTypeStr: '',
        BankAccountName: '',
        BankAccountNumber: '',
        BankSortCode: '',
        InvestorBankAccountID: '',
       
        IsActive: ''
        
    }, // defaults

  
});
EzBob.Underwriter.InvestorContactModels = Backbone.Collection.extend({
    
    Contacts: EzBob.InvestorContactModel
    
}); // EzBob.AddressModels
EzBob.Underwriter.InvestorBankModels = Backbone.Collection.extend({
   
    Banks: EzBob.InvestorBankmodel
}); // EzBob.AddressModels
EzBob.Underwriter.InvestorModel = Backbone.Model.extend({
    idAttribute: 'InvestorID',
    
	urlRoot: '' + gRootPath + 'Underwriter/Investor/LoadInvestor'
});


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
		money: '.cashInput',
		sameBank: '#SameBank',
		secondBank: '.second-bank',
		fundsTransferDate: '#FundsTransferDate'
	},
	serializeData: function () {
		return {

		};
	},
	events: {
		'click .add-investor': 'addInvestorClicked',
		'change #SameBank': 'sameBankChanged',
        'click #CancelAddInvestor':'cancel'
	},

	onRender: function() {
		this.ui.fundsTransferDate.numericOnly(2);
		this.ui.phone.mask('0?9999999999', { placeholder: ' ' });
		this.ui.phone.numericOnly(11);
		this.ui.numeric.numericOnly(20);
		this.ui.money.moneyFormat();
		this.ui.form.validate({
			rules: {
				CompanyName: { required: true },
				InvestorType: { required: true },
				FundsTransferDate: { required: false, digits: true, min: 1, max: 31 },
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
		var data = this.ui.form.find(':not(:hidden)').serializeArray();
		var sameBankChecked = this.ui.sameBank.is(':checked');
		var sameBank = _.find(data, function (d) { return d.name === 'SameBank'; });
		if (sameBank) {
			sameBank.value = sameBankChecked;
		} else {
			data.push({ name: 'SameBank', value: sameBankChecked });
		}

		_.find(data, function (d) { return d.name === 'MonthlyFundingCapital'; }).value = this.$el.find('#MonthlyFundingCapital').autoNumeric('get');
		_.find(data, function (d) { return d.name === 'FundingLimitForNotification'; }).value = this.$el.find('#FundingLimitForNotification').autoNumeric('get');

		var self = this;
		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/AddInvestor', data);
		xhr.done(function(res) {
			if (res.success) {
			
			    self.router.navigate('manageInvestor');
			    self.router.handleRoute('manageInvestors',null,null,true);
			    self.render();
			    //navigate to manage
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
    cancel:function() {
        this.render();
        return false;
    }
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
