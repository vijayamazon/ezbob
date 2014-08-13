EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.LoginView = EzBob.Broker.SubmitView.extend({
	initialize: function() {
		EzBob.Broker.LoginView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-login');

		this.initSubmitBtn('#LoginBrokerButton');

		this.initValidatorCfg();
	}, // initialize

	events: function() {
		var evt = EzBob.Broker.LoginView.__super__.events.apply(this, arguments);

		evt = $.extend({}, evt, {
			'click a.ForgottenPassword': 'forgotten',
			'click a.Signup': 'signup',
		});

		return evt;
	}, // events

	forgotten: function(event) {
		event.preventDefault();
		event.stopPropagation();
		this.router.forgotten();
	}, // forgotten

	signup: function(event) {
		event.preventDefault();
		event.stopPropagation();
		this.router.signup();
	}, // signup

	clear: function() {
		this.$el.find('#LoginEmail, #LoginPassword').val('').blur();
		this.inputChanged();
	}, // clear

	onSubmit: function(event) {
		var sEmail = this.$el.find('#LoginEmail').val();

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/Login', this.$el.find('form').serializeArray());

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				console.log('broker login result:', res);

				EzBob.Csrf.updateToken(res.antiforgery_token);
				EzBob.App.trigger('clear');
				self.clear();
				self.router.setAuth(sEmail, res.properties);

				if (res.properties.SignedTermsID === res.properties.CurrentTermsID)
					self.router.followReturnUrl();
				else
					self.router.requestAcceptTerms(res.properties.CurrentTermsID, res.properties.CurrentTerms);

				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to log in. Please retry.');
		});
	}, // onSubmit

	onFocus: function() {
		EzBob.Broker.LoginView.__super__.onFocus.apply(this, arguments);

		this.$el.find('#LoginEmail').focus();
	}, // onFocus

	initValidatorCfg: function() {
		this.validator = this.$el.find('form').validate({
			rules: {
				LoginEmail: { required: true, email: true, maxlength: 255, },
				LoginPassword: { required: true, minlength: 6, maxlength: 255, },
			},

			messages: {
				LoginEmail: { required: 'This field is required.', email: 'Please enter contact person email.', },
				LoginPassword: { regex: 'This field is required.', },
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		});
	}, // initValidatorCfg
}); // EzBob.Broker.LoginView
