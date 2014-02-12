EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.SignupView = Backbone.View.extend({
	initialize: function() {
		this.$el = $('.section-signup');
		this.initValidatorCfg();

		this.router = this.options.router;

		this.passwordStrengthView = new EzBob.StrengthPasswordView({
			model: new EzBob.StrengthPassword(),
			el: $('#strength-password-view'),
			passwordSelector: '#Password',
		});
	}, // initialize

	events: {
		'click #generateMobileCode': 'generateMobileCode',

		'keyup #ContactMobile': 'mobilePhoneChanged',
		'paste #ContactMobile': 'mobilePhoneChanged',
		'cut #ContactMobile': 'mobilePhoneChanged',

		'click #SignupBrokerButton': 'performSignup',

		'change input': 'inputChanged',
		'keyup  input': 'inputChanged',
		'change select': 'inputChanged',
	}, // events

	clear: function() {
		this.$el.find(
			'#FirmName, #FirmRegNum, ' +
			'#ContactName, #ContactEmail, #ContactMobile, #ContactOtherPhone, ' +
			'#MobileCode, #EstimatedMonthlyClientAmount, #Password, #Password2'
		).val('').blur();

		this.inputChanged();
	}, // clear

	performSignup: function() {
		event.preventDefault();
		event.stopPropagation();

		var oBtn = this.$el.find('#SignupBrokerButton');

		if (oBtn.hasClass('disabled') || oBtn.attr('disabled') || oBtn.prop('disabled'))
			return;

		this.setSignupEnabled(false);
		BlockUi();

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

			self.setSignupEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSignupEnabled(true);
			EzBob.App.trigger('error', 'Failed to sign up. Please retry.');
		});
	}, // performSignup

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			this.setSignupEnabled(false);
			return this;
		} // if

		this.router.setAuth();

		this.$el.find('#EstimatedMonthlyClientAmount').moneyFormat();

		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.phonenumbercode').numericOnly(6);

		this.passwordStrengthView.render();

		this.inputChanged();

		this.$el.find('#FirmName').focus();

		return this;
	}, // render

	setSignupEnabled: function(bEnabled) {
		return this.setSomethingEnabled('#SignupBrokerButton', bEnabled);
	}, // setSignupEnabled

	setSomethingEnabled: function(sSelector, bEnabled) {
		var oElm = this.$el.find(sSelector);

		if (bEnabled)
			oElm.removeClass('disabled').removeAttr('disabled').removeProp('disabled');
		else
			oElm.addClass('disabled').attr('disabled', 'disabled').prop('disabled', 'disabled');

		return oElm;
	}, // setSomethingEnabled

	inputChanged: function(evt) {
		this.setSignupEnabled(EzBob.Validation.checkForm(this.validator));
	}, // inputChanged

	mobilePhoneChanged: function() {
		this.setSomethingEnabled(
			'#generateMobileCode', this.validator.check(this.$el.find('#ContactMobile'))
		).val('Send activation code');

		this.$el.find('#MobileCode').val('').blur();
		this.$el.find('#mobileCodeDiv').hide();
		this.$el.find('#codeSentLabel').animate({ opacity: 0 }).hide();

		return false;
	}, // mobilePhoneChanged

	generateMobileCode: function() {
		if ($('#generateMobileCode').hasClass('disabled'))
			return false;

		EzBob.App.trigger('clear');

		var that = this;

		var xhr = $.post(window.gRootPath + 'Account/GenerateMobileCode', { mobilePhone: this.$el.find('#ContactMobile').val() });

		xhr.done(function(isError) {
			if (isError !== 'False' && (!isError.success || isError.error === 'True'))
				EzBob.App.trigger('error', 'Error sending code.');
			else
				that.$el.find('#codeSentLabel').show().animate({ opacity: 1 });

			return false;
		});

		xhr.always(function() {
			that.$el.find('#mobileCodeDiv').show();
			that.$el.find('#generateMobileCode').val('Resend activation code');

			if (document.activeElement && ($(document.activeElement).attr('id') === 'generateMobileCode'))
				that.$el.find('#MobileCode').focus();
		});

		return false;
	}, // generateMobileCode

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

		this.validator = this.$el.find('.signup-form').validate({
			rules: {
				FirmName: { required: true, maxlength: 255, },
				FirmRegNum: { required: false, maxlength: 255, regex: '^[a-zA-Z0-9]+$', },
				ContactName: { required: true, maxlength: 255, },
				ContactEmail: { required: true, email: true, maxlength: 255, },
				ContactMobile: { required: true, regex: '^0[0-9]{10}$', },
				MobileCode: { required: true, minlength: 6, maxlength: 6, },
				ContactOtherPhone: { required: false, regex: '^0[0-9]{10}$', },
				EstimatedMonthlyClientAmount: { required: true, defaultInvalidPounds: true, regex: '^(?!£ 0.00$)', },
				Password: $.extend({}, passPolicy),
				Password2: passPolicy2,
			},

			messages: {
				FirmName: { required: 'Please enter your firm name.', },
				FirmRegNum: { regex: 'Please enter a valid company number.', },
				ContactName: { required: 'Please enter contact person name.', },
				ContactEmail: { required: 'Please enter contact person email.', email: 'Please enter contact person email.', },
				ContactMobile: { regex: 'Please enter a valid UK number.', },
				MobileCode: { regex: 'Please enter the code you received.' },
				Password: { required: passPolicyText, regex: passPolicyText },
				Password2: { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		});
	}, // initValidatorCfg
}); // EzBob.Broker.SignupView