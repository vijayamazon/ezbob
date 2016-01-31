var EzBob = EzBob || {};

EzBob.FeeEditor = Backbone.Marionette.ItemView.extend({
	template: '#loan_editor_edit_fee_template',

	initialize: function() {
		this.oldValues = this.model.toJSON();
		this.modelBinder = new Backbone.ModelBinder();
	}, // initialize

	events: {
		'click .cancel': 'cancelChanges',
		'click .apply': 'saveChanges'
	}, // events

	bindings: {
		Date: {
			selector: "input[name='date']",
			converter: EzBob.BindingConverters.dateTime,
		},
		Fees: {
			selector: "input[name='fees']",
			converter: EzBob.BindingConverters.floatNumbers,
		},
		Description: {
			selector: "textarea[name='description']",
		}
	}, // bindings

	ui: {
		form: 'form'
	}, // ui

	onRender: function() {
		this.setValidation();

		this.modelBinder.bind(this.model, this.el, this.bindings);

		this.$el.find('input[name="date"]').datepicker({ format: 'dd/mm/yyyy' });
	}, // onRender

	setValidation: function() {
		var minDate = this.options.loan.get('Date');

		this.options.loan.get('Items').forEach(function(installment) {
			var status = installment.get('Status');

			if (installment.get('Type') !== 'Installment')
				return;

			if (!(status === 'Paid' || status === 'PaidEarly'))
				return;

			minDate = installment.get('Date');
		});

		this.ui.form.validate({
			rules: {
				date: {
					required: true,
					minlength: 6,
					maxlength: 20,
					minDate: minDate,
				},
				fees: {
					min: 0,
				},
			},
			messages: {
				'date': {
					required: 'Please, fill the installment date',
					minDate: 'Fee cannot be added before loan starts or before paid installment',
				},
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
		});
	}, // setValidation

	saveChanges: function() {
		if (!this.ui.form.valid())
			return false;

		this.trigger('apply');
		this.close();
		return false;
	}, // saveChanges

	cancelChanges: function() {
		this.model.set(this.oldValues);
		this.close();
		return false;
	}, // cancelChanges

	onClose: function() {
		this.modelBinder.unbind();
	}, // onClose
}); // EzBob.FeeEditor
