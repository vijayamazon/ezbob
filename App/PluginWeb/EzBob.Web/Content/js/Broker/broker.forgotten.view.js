EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.ForgottenView = EzBob.Broker.MobilePhoneView.extend({
	initialize: function() {
		EzBob.Broker.ForgottenView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-forgotten');

		this.initSubmitBtn('#RestorePassBrokerButton');

		this.initMobilePhoneFields({
			PhoneFieldID: 'ForgottenMobile',
			MobileCodeFieldID: 'ForgottenMobileCode',
			GenerateCodeBtnID: 'forgottenGenerateMobileCode',
			MobileCodeSectionID: 'forgottenMobileCodeDiv',
			CodeSentLabelID: 'forgottenSentLabel',
			CaptchaEnabledFieldID: '',
		});

		this.initValidatorCfg();
	}, // initialize

	clear: function() {
		EzBob.Broker.ForgottenView.__super__.clear.apply(this, arguments);
		this.inputChanged();
	}, // clear

	onSubmit: function(event) {
		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/RestorePassword', this.$el.find('form').serializeArray());

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				EzBob.App.trigger('clear');
				self.clear();
				self.router.setAuth();
				self.router.login();
				EzBob.App.trigger('info', 'New password has been sent to your email.');
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to restore password. Please retry.');
		});
	}, // onSubmit

	onRender: function() {
		EzBob.Broker.ForgottenView.__super__.onRender.apply(this, arguments);
		$('body').removeClass('broker-dashboard');
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.phonenumbercode').numericOnly(6);
	}, // onRender

	onFocus: function() {
		EzBob.Broker.ForgottenView.__super__.onFocus.apply(this, arguments);

		this.$el.find('#ForgottenMobile').focus();
	}, // onFocus

	initValidatorCfg: function() {
		var oCfg = {
			rules: {},

			messages: {},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		};

		this.validator = this.$el.find('form').validate(
			this.setMobilePhoneValidatorCfg(oCfg)
		);
	}, // initValidatorCfg
}); // EzBob.Broker.ForgottenView