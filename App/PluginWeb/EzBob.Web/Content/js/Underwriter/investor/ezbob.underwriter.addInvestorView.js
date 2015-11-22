var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.InvestorModel = Backbone.Model.extend({
	url: "" + gRootPath + "Underwriter/Investor/Index"
});

EzBob.Underwriter.AddInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#add-investor-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},
	ui: {
		form: 'form#add-investor-form',
		phone: '.phone',
		numeric: '.numeric'
	},
	serializeData: function () {
		return {

		};
	},
	events: {
		'click .add-investor': 'addInvestorClicked'
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
				ContactMobile: { required: true, regex: "^0[0-9]{10}$" },
				ContactOfficeNumber: { required: true, regex: "^0[0-9]{10}$" },
				Comment: { required: false, },
				InvestorBankAccountType: { required: false, },
				BankSortCode: { required: true, number: true },
				BankAccountNumber: { required: true, number: true },
				BankAccountName: { required: true, },
			},
			messages: {
				"ContactMobile": { regex: "Please enter a valid UK number" },
				"ContactOfficeNumber": { regex: "Please enter a valid UK number" }
				},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
		});
		return this;
	},
	addInvestorClicked: function(){
		if (!this.ui.form.valid()) {
			console.log('invalid');
			return false;
		}

		console.log('valid');
		return false;
	},
	show: function () {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});
