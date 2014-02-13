EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.MobilePhoneView = EzBob.Broker.SubmitView.extend({
	initialize: function() {
		EzBob.Broker.MobilePhoneView.__super__.initialize.apply(this, arguments);
	}, // initialize

	initMobilePhoneFields: function(opts) {
		this.PhoneFieldID = opts.PhoneFieldID;
		this.MobileCodeFieldID = opts.MobileCodeFieldID;
		this.GenerateCodeBtnID = opts.GenerateCodeBtnID;
		this.MobileCodeSectionID = opts.MobileCodeSectionID;
		this.CodeSentLabelID = opts.CodeSentLabelID;
	}, // initMobilePhoneFields

	events: function() {
		var evt = EzBob.Broker.MobilePhoneView.__super__.events.apply(this, arguments);

		evt['click #' + this.GenerateCodeBtnID] = 'generateMobileCode';

		evt['keyup #' + this.PhoneFieldID] = 'mobilePhoneChanged';
		evt['paste #' + this.PhoneFieldID] = 'mobilePhoneChanged';
		evt['cut #' + this.PhoneFieldID] = 'mobilePhoneChanged';

		return evt;
	}, // events

	clear: function() {
		this.$el.find('#' + this.PhoneFieldID + ', #' + this.MobileCodeFieldID).val('').blur();
		this.mobilePhoneChanged();
	}, // clear

	mobilePhoneChanged: function() {
		this.setSomethingEnabled(
			'#' + this.GenerateCodeBtnID, this.validator.check(this.$el.find('#' + this.PhoneFieldID))
		).val('Send activation code');

		this.$el.find('#' + this.MobileCodeFieldID).val('').blur();
		this.$el.find('#' + this.MobileCodeSectionID).hide();
		this.$el.find('#' + this.CodeSentLabelID).animate({ opacity: 0 }).hide();

		return false;
	}, // mobilePhoneChanged

	generateMobileCode: function() {
		if ($('#' + this.GenerateCodeBtnID).hasClass('disabled'))
			return false;

		EzBob.App.trigger('clear');

		var self = this;

		var xhr = $.post(window.gRootPath + 'Account/GenerateMobileCode', { mobilePhone: this.$el.find('#' + this.PhoneFieldID).val() });

		xhr.done(function(isError) {
			if (isError !== 'False' && (!isError.success || isError.error === 'True'))
				EzBob.App.trigger('error', 'Error sending code.');
			else
				self.$el.find('#' + self.CodeSentLabelID).show().animate({ opacity: 1 });

			return false;
		});

		xhr.always(function() {
			self.$el.find('#' + self.MobileCodeSectionID).show();
			self.$el.find('#' + self.GenerateCodeBtnID).val('Resend activation code');

			if (document.activeElement && ($(document.activeElement).attr('id') === self.GenerateCodeBtnID))
				self.$el.find('#' + self.MobileCodeFieldID).focus();
		});

		return false;
	}, // generateMobileCode

	setMobilePhoneValidatorCfg: function(oCfg) {
		oCfg.rules[this.PhoneFieldID] = { required: true, regex: '^0[0-9]{10}$', };
		oCfg.rules[this.MobileCodeFieldID] =  { required: true, minlength: 6, maxlength: 6, };

		oCfg.messages[this.PhoneFieldID] = { regex: 'Please enter a valid UK number.', };
		oCfg.messages[this.MobileCodeFieldID] = { regex: 'Please enter the code you received.' };

		return oCfg;
	}, // setMobilePhoneValidatorCfg
}); // EzBob.Broker.SignupView