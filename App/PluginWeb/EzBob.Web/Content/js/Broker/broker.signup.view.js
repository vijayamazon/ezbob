var EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.SignupView = EzBob.Broker.MobilePhoneView.extend({
	initialize: function () {
		EzBob.Broker.SignupView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-signup');

		this.initSubmitBtn('#SignupBrokerButton');

		this.initMobilePhoneFields({
			PhoneFieldID: 'ContactMobile',
			MobileCodeFieldID: 'MobileCode',
			GenerateCodeBtnID: 'generateMobileCode',
			MobileCodeSectionID: 'mobileCodeDiv',
			CodeSentLabelID: 'codeSentLabel',
			CaptchaEnabledFieldID: 'SignupIsCaptchaEnabled',
			CaptchaSectionID: 'CaptchaSection',
		});

		this.initValidatorCfg();

		this.passwordStrengthView = new EzBob.StrengthPasswordView({
			model: new EzBob.StrengthPassword(),
			el: $('#strength-password-view'),
			passwordSelector: '#Password',
		});

		EzBob.App.on('brkr:signup-with-captcha', _.bind(this.switchToCaptcha, this));
	}, // initialize

	events: function () {
		var evt = EzBob.Broker.SignupView.__super__.events.apply(this, arguments);

		evt['click .show-terms-and-conditions'] = 'showConsent';
		evt['click #FCARegistered'] = 'changeFCARegisered';
		evt['click #NonFCARegistered'] = 'changeFCARegisered';
		evt['click #switchToCaptcha'] = 'switchToCaptcha';

		return evt;
	}, // events

	switchToCaptcha: function () {
		this.trigger('show-captcha');
	}, // switchToCaptcha

	changeFCARegisered: function(e) {
		var isFcaRegistered = ($(e.currentTarget).val() === 'true');
		if (!isFcaRegistered)
			this.$el.find('#LicenseNumber').val('');
		this.$el.find('.license_number_field_container').toggle(isFcaRegistered);
		this.inputChanged();
	}, // changeFCARegisered

	clear: function () {
		EzBob.Broker.SignupView.__super__.clear.apply(this, arguments);

		this.$el.find(
			'#FirmName, #FirmRegNum, #FirmWebSite, ' +
			'#ContactName, #ContactEmail, #ContactOtherPhone, ' +
			'#Password, #Password2, ' +
			'#AgreeToTerms, #AgreeToPrivacyPolicy,#LicenseNumber'
		).val('').blur();

		this.inputChanged();
	}, // clear

	onSubmit: function (event) {
		var sEmail = this.$el.find('#ContactEmail').val();

		var oData = this.$el.find('form').serializeArray();

		var fws = _.find(oData, function (d) { return d.name === 'FirmWebSite'; });
		if (fws && !/^https?:\/\//.test(fws.value))
			fws.value = 'http://' + fws.value;

		oData.push({
			name: 'TermsID',
			value: $('#broker-terms-and-conditions').attr('data-terms-version'),
		});

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerAccount/Signup', oData);

		var self = this;

		oRequest.success(function (res) {
			UnBlockUi();

			if (res.success) {
				EzBob.Csrf.updateToken(res.antiforgery_token);
				EzBob.App.trigger('clear');
				self.clear();
				self.router.setAuth(sEmail, res.properties);
				self.router.dashboard();
				return;
			} // if

			self.reloadCaptcha();

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function () {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to sign up. Please retry.');
		});
	}, // onSubmit

	onRender: function () {
		EzBob.Broker.SignupView.__super__.onRender.apply(this, arguments);
		$('body').removeClass('broker-dashboard');

		this.$el.find('.phonenumber').numericOnly(11);

		this.passwordStrengthView.render();

		if ($('#broker-sms-count').attr('data-force-captcha') === 'yes')
			EzBob.App.trigger('brkr:signup-with-captcha');
	}, // onRender

	onFocus: function () {
		EzBob.Broker.SignupView.__super__.onFocus.apply(this, arguments);

		this.$el.find('#FirmName').focus();
	}, // onFocus

	showConsent: function (event) {
		event.preventDefault();
		event.stopPropagation();

		$('#broker-terms-and-conditions').dialog({
			modal: true,
			open: function() {
				$('body').addClass('stop-scroll');
			},
			close: function() {
				$('body').removeClass('stop-scroll');
			},
			width: 800,
			height: 600,

			buttons: {
				Close: {
					text: 'Close',
					'class': 'button btn-green ev-btn-org',
					click: function () {
						$(this).dialog("close");
					},
				}, // close button
			}, // buttons
		});

		return false;
	}, // showConsent

	initValidatorCfg: function () {
		var passPolicy = { required: true, minlength: 6, maxlength: 255 };

		var passPolicyText = EzBob.dbStrings.PasswordPolicyCheck;

		if (EzBob.Config.PasswordPolicyType !== 'simple') {
			passPolicy.regex = '^.*([a-z]+.*[A-Z]+) |([a-z]+.*[^A-Za-z0-9]+)|([a-z]+.*[0-9]+)|([A-Z]+.*[a-z]+)|([A-Z]+.*[^A-Za-z0-9]+)|([A-Z]+.*[0-9]+)|([^A-Za-z0-9]+.*[a-z]+.)|([^A-Za-z0-9]+.*[A-Z]+)|([^A-Za-z0-9]+.*[0-9]+.)|([0-9]+.*[a-z]+)|([0-9]+.*[A-Z]+)|([0-9]+.*[^A-Za-z0-9]+).*$';
			passPolicy.minlength = 7;
			passPolicyText = 'Password has to have 2 types of characters out of 4 (letters, caps, digits, special chars).';
		} // if

		var passPolicy2 = $.extend({}, passPolicy);
		passPolicy2.equalTo = '#Password';

		var oCfg = {
			rules: {
				FirmName: { required: true, maxlength: 255, },
				FirmRegNum: { required: false, maxlength: 255, regex: '^[a-zA-Z0-9]+$', },
				FirmWebSite: { required: false, url: false, optionalUrl: true },
				ContactName: { required: true, maxlength: 255, },
				ContactEmail: { required: true, email: true, maxlength: 255, },
				ContactOtherPhone: { required: false, regex: '^0[0-9]{10}$', },
				Password: $.extend({}, passPolicy),
				Password2: passPolicy2,
				AgreeToTerms: { required: true, },
				AgreeToPrivacyPolicy: { required: true, },
				LicenseNumber: { required: true, maxlength: 50 }
			},

			messages: {
				FirmName: { required: 'Please enter your broker name.', },
				FirmRegNum: { regex: 'Please enter a valid company number.', },
				ContactName: { required: 'Please enter contact person full name.', },
				ContactEmail: { required: 'Please enter contact person email.', email: 'Please enter contact person email.', },
				Password: { required: passPolicyText, regex: passPolicyText },
				Password2: { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
			ignore: ':not(:visible)',
		};

		this.validator = this.$el.find('form').validate(
			this.setMobilePhoneValidatorCfg(oCfg)
		);
	}, // initValidatorCfg
}); // EzBob.Broker.SignupView