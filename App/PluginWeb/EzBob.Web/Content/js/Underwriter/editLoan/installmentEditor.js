var EzBob = EzBob || {};

EzBob.AddInstallmentTrack = (function() {
	function AddInstallmentTrack(installment, loan) {
		this.installment = installment;
		this.loan = loan;
		this.installment.on('change:Date', this.dateChanged, this);
	} // constructor

	AddInstallmentTrack.prototype.dateChanged = function() {
		var date = moment.utc(this.installment.get('Date'));
		var before = this.loan.getInstallmentBefore(date);
		var after = this.loan.getInstallmentAfter(date);
		var balance = 0;
		var balanceBefore = 0;

		if (before === null) {
			balance = this.loan.get('Amount');
			balanceBefore = balance;
		} // if

		if (after === null) {
			balance = 0;
			balanceBefore = before.get('Balance');
		} // if

		if (before !== null && after !== null) {
			balance = before.get('Balance') - after.get('Balance');
			balance = balance * (date.toDate() - moment.utc(before.get('Date')).toDate());
			balance = balance / (moment.utc(after.get('Date')).toDate() - moment.utc(before.get('Date')).toDate());
			balance = before.get('Balance') - balance;
			balance = Math.round(balance * 100) / 100;
			balanceBefore = before.get('Balance');
		} // if

		this.installment.set({ BalanceBeforeRepayment: balanceBefore });
		this.installment.set({ Balance: balance });
	}; // dateChanged

	return AddInstallmentTrack;
})(); // EzBob.AddInstallmentTrack

EzBob.InstallmentEditor = Backbone.Marionette.ItemView.extend({
	template: '#loan_editor_edit_installment_template',

	initialize: function() {
		this.oldValues = this.model.toJSON();
		this.modelBinder = new Backbone.ModelBinder();

		if (this.model.get('IsAdding'))
			new EzBob.AddInstallmentTrack(this.model, this.options.loan);
	}, // initialize

	events: {
		'click .cancel': 'cancelChanges',
		'click .apply': 'saveChanges',
	}, // events

	bindings: {
		Date: {
			selector: "input[name='date']",
			converter: EzBob.BindingConverters.dateTime
		},
		Balance: {
			selector: "input[name='balance']",
			converter: EzBob.BindingConverters.floatNumbers
		},
		Principal: {
			selector: "input[name='loanRepayment']",
			converter: EzBob.BindingConverters.floatNumbers
		},
		InterestRate: {
			selector: "input[name='interestRate']",
			converter: EzBob.BindingConverters.percents
		},
		Total: {
			selector: "input[name='totalRepayment']",
			converter: EzBob.BindingConverters.floatNumbers
		}
	}, // bindings

	ui: {
		form: 'form',
		shift: '.shift-installments :checkbox',
		shiftRates: '.shift-rates :checkbox'
	}, // ui

	onRender: function() {
		this.setValidation();
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.$el.find('input[name="date"]').datepicker({ format: 'dd/mm/yyyy' });
		this.$el.find('input[data-content], span[data-content]').setPopover();
	}, // onRender

	setValidation: function() {
		this.ui.form.validate({
			rules: {
				date: {
					required: true,
					minlength: 6,
					maxlength: 20,
				},
				interestRate: {
					positive: true,
					max: 100,
				},
				balance: {
					min: 0,
				}
			},
			messages: {
				'date': {
					required: 'Please, fill the installment date'
				},
				'interestRate': {
					positive: 'Interest rate cannot be less than zero',
					max: 'Interest rate cannot be greater than 100%'
				},
				'balance': 'Balance cannot be less than zero'
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight
		});
	}, // setValidation

	saveChanges: function() {
		if (!this.ui.form.valid())
			return false;

		if (this.ui.shift.prop('checked') && this.oldValues.Date !== this.model.get('Date'))
			this.options.loan.shiftDate(this.model, this.model.get('Date'), this.oldValues.Date);

		if (this.ui.shiftRates.prop('checked') && this.oldValues.InterestRate !== this.model.get('InterestRate'))
			this.options.loan.shiftInterestRate(this.model, this.model.get('InterestRate'));

		this.trigger('apply');
		this.close();
		return false;
	}, // saveChanges

	cancelChanges: function() {
		this.model.set('Principal', this.oldValues.Principal);
		this.close();
		return false;
	}, // cancelChanges

	onClose: function() {
		this.modelBinder.unbind();
	}, // onClose
}); // EzBob.InstallmentEditor
