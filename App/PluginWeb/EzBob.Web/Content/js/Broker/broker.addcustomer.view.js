EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.AddCustomerView = EzBob.Broker.SubmitView.extend({
	initialize: function() {
		EzBob.Broker.AddCustomerView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-add-customer');

		this.initSubmitBtn('button[type=submit]');

		this.initValidatorCfg();
	}, // initialize

	events: function() {
		var evt = EzBob.Broker.AddCustomerView.__super__.events.apply(this, arguments);

		evt['click .back-to-list'] = 'backToList';

		return evt;
	}, // events

	clear: function() {
		EzBob.Broker.AddCustomerView.__super__.clear.apply(this, arguments);

		this.$el.find('#LeadFirstName, #LeadLastName, #LeadEmail').val('').blur();

		this.inputChanged();
	}, // clear

	setAuthOnRender: function() {
		return false;
	}, // setAuthOnRender

	onFocus: function() {
		EzBob.Broker.AddCustomerView.__super__.onFocus.apply(this, arguments);

		this.$el.find('#LeadFirstName').focus();
	}, // onFocus

	backToList: function() {
		location.assign('#dashboard');
	}, // backToList

	onSubmit: function(event) {
		var oData = this.$el.find('form').serializeArray();

		var sAddMode = $(event.currentTarget).attr('data-add-mode');

		var sEmail = this.$el.find('#LeadEmail').val();

		switch (sAddMode) {
		case 'invitation':
		case 'fill':
			break;

		default:
			UnBlockUi();
			return;
		} // switch

		oData.push({ name: 'LeadAddMode', value: sAddMode, });
		oData.push({ name: 'ContactEmail', value: this.router.getAuth(), });

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/AddLead', oData);

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				self.clear();

				if (sAddMode === 'fill') {
					BlockUi();
					location.assign(
						'' + window.gRootPath + 'Broker/BrokerHome/FillWizard' +
						'?sLeadEmail=' + encodeURIComponent(sEmail) +
						'&sContactEmail=' + encodeURIComponent(self.router.getAuth())
					);
				}
				else {
					EzBob.App.trigger('info', 'A lead has been added.');
					self.backToList();
				} // if

				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to add a lead. Please retry.');
		});
	}, // onSubmit

	initValidatorCfg: function() {
		this.validator = this.$el.find('form').validate({
			rules: {
				LeadFirstName: EzBob.Validation.NameValidationObject,
				LeadLastName: { required: true, maxlength: 255, },
				LeadEmail: { required: true, email: true, maxlength: 255, },
			},

			messages: {
				LedLastName: { required: 'Please enter person last name.', },
				LeadEmail: { required: 'This field is required.', email: 'Please enter lead email.', },
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		});
	}, // initValidatorCfg
}); // EzBob.Broker.AddCustomerView
