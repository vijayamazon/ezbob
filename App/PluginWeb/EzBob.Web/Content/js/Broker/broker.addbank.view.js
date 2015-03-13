EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.AddBankView = EzBob.Broker.SubmitView.extend({
	initialize: function() {
		EzBob.Broker.AddBankView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-add-bank');

		this.initSubmitBtn('button[type=submit]');

		this.initValidatorCfg();
	}, // initialize

	events: function() {
		var evt = EzBob.Broker.AddBankView.__super__.events.apply(this, arguments);

		evt['click .back-to-list'] = 'backToList';

		return evt;
	}, // events

	clear: function() {
		EzBob.Broker.AddBankView.__super__.clear.apply(this, arguments);

		this.$el.find('.form_field').val('').blur();

		this.inputChanged();
	}, // clear

	setAuthOnRender: function() {
		return false;
	}, // setAuthOnRender

	onFocus: function() {
		EzBob.Broker.AddBankView.__super__.onFocus.apply(this, arguments);

		this.$el.find('#AccountNumber').focus();
	}, // onFocus

	backToList: function() {
		location.assign('#dashboard');
	}, // backToList

	onSubmit: function(event) {
		var oData = this.$el.find('form').serializeArray();

		oData.push({ name: 'ContactEmail', value: this.router.getAuth(), });

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/AddBank', oData);

		var self = this;

		oRequest.success(function(res) {
			if (res.success) {
				self.clear();
                EzBob.App.trigger('info', 'A bank account has been added.');
				self.backToList();
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
		
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to add a lead. Please retry.');
		});

	    oRequest.always(function() {
	        UnBlockUi();
	    });
	}, // onSubmit

	initValidatorCfg: function() {
		this.validator = this.$el.find('form').validate({
			rules: {
			    SortCode: { required: true, minlength: 6, maxlength: 6, digits: true },
			    AccountNumber: { required: true, minlength: 8, maxlength: 8, digits: true }
			},
            messages: {
			    SortCode: { minlength: "Please enter a valid Sort Code", maxlength: "Please enter a valid SortCode" }
			},
            errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		});
	}, // initValidatorCfg
}); // EzBob.Broker.AddCustomerView
