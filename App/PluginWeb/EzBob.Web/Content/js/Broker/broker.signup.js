EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.SignupView = Backbone.View.extend({
	initialize: function() {
		this.$el = $('.section-signup');
		this.initValidatorCfg();

		this.passwordStrengthView = new EzBob.StrengthPasswordView({
			model: new EzBob.StrengthPassword(),
			el: $('#strength-password-view'),
			passwordSelector: '#Password',
		});
	}, // initialize

	events: {
		'click #generateMobileCode': 'generateMobileCode',
		'keyup #ContactMobile': 'mobilePhoneChanged',

		'change input': 'inputChanged',
		'keyup  input': 'inputChanged',
		'change select': 'inputChanged',
	}, // events

	render: function() {
		this.$el.find('#EstimatedMonthlyClientAmount').moneyFormat();

		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.phonenumbercode').numericOnly(6);

		this.passwordStrengthView.render();

		this.inputChanged();

		this.$el.find('#FirmName').focus();

		return this;
	}, // render

	inputChanged: function(evt) {
		var enabled = EzBob.Validation.checkForm(this.validator);
		this.$el.find('#SignupBrokerButton').toggleClass('disabled', !enabled);
	}, // inputChanged

	mobilePhoneChanged: function() {
		var isValidPhone = this.validator.check(this.$el.find('#ContactMobile'));
		$('#generateMobileCode').toggleClass('disabled', !isValidPhone);
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
			ignore: ':not(:visible)',
		});
	}, // initValidatorCfg
}); // EzBob.Broker.SignupView