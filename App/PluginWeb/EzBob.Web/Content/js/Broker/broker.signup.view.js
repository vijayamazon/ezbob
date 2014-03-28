EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.SignupView = EzBob.Broker.MobilePhoneView.extend({
	initialize: function() {
		EzBob.Broker.SignupView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-signup');

		this.initSubmitBtn('#SignupBrokerButton');

		this.initMobilePhoneFields({
			PhoneFieldID: 'ContactMobile',
			MobileCodeFieldID: 'MobileCode',
			GenerateCodeBtnID: 'generateMobileCode',
			MobileCodeSectionID: 'mobileCodeDiv',
			CodeSentLabelID: 'codeSentLabel',
		});

		this.initValidatorCfg();

		this.passwordStrengthView = new EzBob.StrengthPasswordView({
			model: new EzBob.StrengthPassword(),
			el: $('#strength-password-view'),
			passwordSelector: '#Password',
		});
	}, // initialize

	clear: function() {
		EzBob.Broker.SignupView.__super__.clear.apply(this, arguments);

		this.$el.find(
			'#FirmName, #FirmRegNum, #FirmWebSite, ' +
			'#ContactName, #ContactEmail, #ContactOtherPhone, #EstimatedMonthlyAppCount, ' +
			'#EstimatedMonthlyClientAmount, #Password, #Password2, ' +
			'#AgreeToTerms, #AgreeToPrivacyPolicy'
		).val('').blur();

		this.inputChanged();
	}, // clear

	inputChanged: function() {
		EzBob.Broker.SignupView.__super__.inputChanged.apply(this, arguments);

		var sUrl = this.$el.find('#FirmWebSite').val();

		console.log('url', sUrl, 'matches', /^(https?:\/\/)?[^ \'"]+/.test(sUrl));
	}, // inputChanged

	onSubmit: function(event) {
		var sEmail = this.$el.find('#ContactEmail').val();

		var oData = this.$el.find('form').serializeArray();

		var amt = _.find(oData, function(d) { return d.name === 'EstimatedMonthlyClientAmount'; });
		if (amt)
			amt.value = this.$el.find('#EstimatedMonthlyClientAmount').autoNumericGet();

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/Signup', oData);

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				EzBob.App.trigger('clear');
				self.clear();
				self.router.setAuth(sEmail);
				self.router.dashboard();
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to sign up. Please retry.');
		});
	}, // onSubmit

	onRender: function() {
		this.$el.find('#EstimatedMonthlyClientAmount').moneyFormat();

		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.phonenumbercode, #EstimatedMonthlyAppCount').numericOnly(6);

		this.passwordStrengthView.render();
	}, // onRender

	onFocus: function() {
		this.$el.find('#FirmName').focus();
	}, // onFocus

	initValidatorCfg: function() {
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
				FirmWebSite: { required: false, maxlength: 255, optionalUrl: true, },
				ContactName: { required: true, maxlength: 255, },
				ContactEmail: { required: true, email: true, maxlength: 255, },
				ContactOtherPhone: { required: false, regex: '^0[0-9]{10}$', },
				EstimatedMonthlyClientAmount: { required: true, defaultInvalidPounds: true, regex: '^(?!£ 0.00$)', },
				Password: $.extend({}, passPolicy),
				Password2: passPolicy2,
				EstimatedMonthlyAppCount: { required: true, maxlength: 6, regex: '^[1-9]\d*', },
				AgreeToTerms: { required: true, },
				AgreeToPrivacyPolicy: { required: true, },
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
		};

		this.validator = this.$el.find('form').validate(
			this.setMobilePhoneValidatorCfg(oCfg)
		);
	}, // initValidatorCfg
}); // EzBob.Broker.SignupView