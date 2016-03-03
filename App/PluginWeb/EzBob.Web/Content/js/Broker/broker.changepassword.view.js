var EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.ChangePasswordView = EzBob.Broker.SubmitView.extend({
	initialize: function(options) {
		EzBob.Broker.ChangePasswordView.__super__.initialize.apply(this, arguments);
		this.router = options.router;
		this.theTable = null;
		this.leadTable = null;

		this.$el = $('#section-dashboard-account-info');

		this.initSubmitBtn('#UpdatePassword');
		
		this.passwordStrengthView = new EzBob.StrengthPasswordView({
			model: new EzBob.StrengthPassword(),
			el: $('#strength-update-password-view'),
			passwordSelector: '#NewPassword',
		});
		
		this.initValidatorCfg();
	}, // initialize

	events: function() {
		var evt = EzBob.Broker.ChangePasswordView.__super__.events.apply(this, arguments);
		return evt;
	}, // events

	clear: function() {
		EzBob.Broker.ChangePasswordView.__super__.clear.apply(this, arguments);
		EzBob.App.trigger('clear');
		this.$el.find('form').find('.form_field').val('').blur();
		this.inputChanged();
	}, // clear

	setAuthOnRender: function() {
		return false;
	}, // setAuthOnRender

	onSubmit: function() {
		var oData = this.$el.find('form').serializeArray();

		oData.push({
			name: "ContactEmail",
			value: this.router.getAuth(),
		});

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerAccount/UpdatePassword', oData);

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				self.clear();
				self.$el.find('#OldPassword,#NewPassword,#NewPassword2').removeAttr("style");
				self.passwordStrengthView.show();
				self.setSubmitEnabled(false);
				self.$el.find('.error-wrap label').hide();

				self.$el.find('IMG.field_status').field_status({ required: true });

				EzBob.App.trigger('info', 'Your password has been updated.');
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);
			else
				EzBob.App.trigger('error', 'Failed to update your password. Please retry.');

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to update your password. Please retry.');
		});
	}, // onSubmit

	initValidatorCfg: function() {
		var passPolicy = { required: true, minlength: 6, maxlength: 255 };

		var passPolicyText = EzBob.dbStrings.PasswordPolicyCheck;

		if (EzBob.Config.PasswordPolicyType !== 'simple') {
			passPolicy.regex = '^.*([a-z]+.*[A-Z]+) |([a-z]+.*[^A-Za-z0-9]+)|([a-z]+.*[0-9]+)|([A-Z]+.*[a-z]+)|([A-Z]+.*[^A-Za-z0-9]+)|([A-Z]+.*[0-9]+)|([^A-Za-z0-9]+.*[a-z]+.)|([^A-Za-z0-9]+.*[A-Z]+)|([^A-Za-z0-9]+.*[0-9]+.)|([0-9]+.*[a-z]+)|([0-9]+.*[A-Z]+)|([0-9]+.*[^A-Za-z0-9]+).*$';
			passPolicy.minlength = 7;
			passPolicyText = 'Password has to have 2 types of characters out of 4 (letters, caps, digits, special chars).';
		} // if

		var passPolicy2 = $.extend({}, passPolicy);
		passPolicy2.equalTo = '#NewPassword';

		var oCfg = {
			rules: {
				OldPassword: $.extend({}, passPolicy),
				NewPassword: $.extend({}, passPolicy),
				NewPassword2: passPolicy2,
			},

			messages: {
				OldPassword: { required: passPolicyText, regex: passPolicyText },
				NewPassword: { required: passPolicyText, regex: passPolicyText },
				NewPassword2: { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		};

		this.validator = this.$el.find('form').validate(oCfg);
	}, // initValidatorCfg
}); // EzBob.Broker.DashboardView
