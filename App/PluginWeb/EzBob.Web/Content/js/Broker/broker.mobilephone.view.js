EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.MobilePhoneView = EzBob.Broker.SubmitView.extend({
	initialize: function() {
		EzBob.Broker.MobilePhoneView.__super__.initialize.apply(this, arguments);

		this.on('show-captcha', this.showCaptcha, this);

		this.hasCodeEverBeenSent = false;

		this.aryCurrentCount = {};

		this.sendAttemptsCount = 0;

		this.specialKeys = {
			8: 'Backspace',
			9: 'Tab',
			12: 'Num 5 without num lock',
			16: 'Shift',
			17: 'Ctrl',
			18: 'Alt',
			19: 'Pause/Break',
			20: 'Caps Lock',
			27: 'Escape',
			33: 'Page Up',
			34: 'Page Down',
			35: 'End',
			36: 'Home',
			37: 'Left',
			38: 'Up',
			39: 'Right',
			40: 'Down',
			45: 'Insert',
			46: 'Delete',
			91: 'Left Win',
			92: 'Right Win',
			93: 'Menu',
			112: 'F1',
			113: 'F2',
			114: 'F3',
			115: 'F4',
			116: 'F5',
			117: 'F6',
			118: 'F7',
			119: 'F8',
			120: 'F9',
			121: 'F10',
			122: 'F11',
			123: 'F12',
			144: 'Num Lock',
			145: 'Scroll Lock',
		};

		this.isCaptchaShown = false;

		SetCaptchaMode();
	}, // initialize

	initMobilePhoneFields: function(opts) {
		this.PhoneFieldID = opts.PhoneFieldID;
		this.MobileCodeFieldID = opts.MobileCodeFieldID;
		this.GenerateCodeBtnID = opts.GenerateCodeBtnID;
		this.MobileCodeSectionID = opts.MobileCodeSectionID;
		this.CodeSentLabelID = opts.CodeSentLabelID;
		this.CaptchaEnabledFieldID = opts.CaptchaEnabledFieldID;
		this.CaptchaSectionID = opts.CaptchaSectionID;
	}, // initMobilePhoneFields

	customValidationResult: function(evt) {
		var isMobileCodeVisible = !this.$el.find('#' + this.MobileCodeSectionID).hasClass('hide');

		if (isMobileCodeVisible && (this.$el.find('#' + this.MobileCodeFieldID).val().length === 6))
			return true; // mobile code is visible and entered

		if (!this.CaptchaEnabledFieldID)
			return false; // there is no captcha in this view, so cannot continue

		return this.$el.find('#' + this.CaptchaEnabledFieldID).val() === '1';
	}, // customValidationResult

	events: function() {
		var evt = EzBob.Broker.MobilePhoneView.__super__.events.apply(this, arguments);

		evt['click #' + this.GenerateCodeBtnID] = 'generateMobileCode';

		evt['change #' + this.PhoneFieldID] = 'mobilePhoneAccessed';
		evt['keyup #' + this.PhoneFieldID] = 'mobilePhoneAccessed';
		evt['paste #' + this.PhoneFieldID] = 'mobilePhoneAccessed';
		evt['cut #' + this.PhoneFieldID] = 'mobilePhoneAccessed';

		return evt;
	}, // events

	clear: function() {
		this.$el.find('#' + this.PhoneFieldID + ', #' + this.MobileCodeFieldID).val('').blur();
		this.mobilePhoneChanged();
	}, // clear

	mobilePhoneAccessed: function(event) {
		var bIsPhoneNumberValid = this.validator.check(this.$el.find('#' + this.PhoneFieldID));

		if (bIsPhoneNumberValid) {
			var sPhoneNumber = this.$el.find('#' + this.PhoneFieldID).val();

			if ((sPhoneNumber === this.getLastCodeTarget()) || this.currentCount(sPhoneNumber)) {
				var bEnabled = !this.isCaptchaShown &&
					(this.currentCount(sPhoneNumber) < this.maxSendToCurrentCount()) &&
					(this.sendAttemptsCount < this.maxSendAttemptsCount());

				this.setSomethingEnabled('#' + this.GenerateCodeBtnID, bEnabled).val('Resend authentication code');

				if (event && event.keyCode && this.specialKeys[event.keyCode])
					this.$el.find('#' + this.MobileCodeFieldID).val('');
				else
					this.$el.find('#' + this.MobileCodeFieldID).val('').focus();

				this.$el.find('#' + this.MobileCodeSectionID).removeClass('hide').show();
				this.setSentLabelVisible(true);
			}
			else
				this.mobilePhoneChanged();
		}
		else
			this.mobilePhoneChanged();

		return false;
	}, // mobilePhoneAccessed

	mobilePhoneChanged: function() {
		this.setSomethingEnabled(
			'#' + this.GenerateCodeBtnID, !this.isCaptchaShown && this.validator.check(this.$el.find('#' + this.PhoneFieldID))
		).val('Send activation code');

		this.$el.find('#' + this.MobileCodeFieldID).val('').blur();
		this.$el.find('#' + this.MobileCodeSectionID).addClass('hide').hide();
		this.setSentLabelVisible(false);
	}, // mobilePhoneChanged

	onRender: function() {
		EzBob.Broker.MobilePhoneView.__super__.onRender.apply(this, arguments);

		if (this.CaptchaEnabledFieldID) {
			this.captchaView = new EzBob.Captcha({
				elementId: 'broker-captcha',
				tabindex: this.$el.find('#' + this.CaptchaSectionID).attr('data-tab-index'),
			});
			this.captchaView.render();
		} // if
	}, // onRender

	reloadCaptcha: function() {
		if (this.captchaView)
			this.captchaView.reload();
	}, // reloadCaptcha

	setSentLabelVisible: function(bVisible) {
		if (bVisible)
			this.$el.find('#' + this.CodeSentLabelID).removeClass('hide').show().animate({ opacity: 1 });
		else
			this.$el.find('#' + this.CodeSentLabelID).animate({ opacity: 0 }).addClass('hide').hide();
	}, // setSentLabelVisible

	handleTooManyAttempts: function() {
		if ((this.currentCount() >= this.maxSendToCurrentCount()) || (this.sendAttemptsCount >= this.maxSendAttemptsCount())) {
			this.setSomethingEnabled('#' + this.GenerateCodeBtnID, false);

			EzBob.App.trigger('clear');

			if (this.sendAttemptsCount >= this.maxSendAttemptsCount()) {
				EzBob.App.trigger('error', 'Too many attempts to send verification code.');

				if (this.CaptchaEnabledFieldID)
					this.showCaptcha();
			} // if
			else
				EzBob.App.trigger('warning', 'Too many attempts to send verification code to number ' + this.$el.find('#' + this.PhoneFieldID).val() + '.');
		} // if
	}, // handleTooManyAttempts

	showCaptcha: function() {
		this.isCaptchaShown = true;

		this.setSomethingEnabled('#' + this.GenerateCodeBtnID, false).addClass('hide');
		this.$el.find('#' + this.PhoneFieldID).removeClass('form_field_left_side').addClass('form_field');

		this.$el.find('#' + this.MobileCodeFieldID).val('').blur();
		this.$el.find('#' + this.MobileCodeSectionID).addClass('hide').hide();
		this.setSentLabelVisible(false);

		this.$el.find('#' + this.CaptchaEnabledFieldID).val('1');
		this.$el.find('#' + this.CaptchaSectionID).removeClass('hide').show();

		this.inputChanged();
	}, // showCaptcha

	setLastCodeTarget: function(sPhoneNumber) {
		this.$el.find('#' + this.CodeSentLabelID).attr('data-sent-to', sPhoneNumber);
	}, // setLastCodeTarget

	getLastCodeTarget: function() {
		return this.$el.find('#' + this.CodeSentLabelID).attr('data-sent-to');
	}, // getLastCodeTarget

	incCurrentCount: function() {
		var sPhoneNumber = this.getLastCodeTarget();

		if (!sPhoneNumber)
			return;

		if (this.aryCurrentCount[sPhoneNumber])
			this.aryCurrentCount[sPhoneNumber]++;
		else
			this.aryCurrentCount[sPhoneNumber] = 1;
	}, // incCurrentCount

	currentCount: function(sPhoneNumber) {
		sPhoneNumber = sPhoneNumber || this.getLastCodeTarget();

		if (!sPhoneNumber)
			return 0;

		return this.aryCurrentCount[sPhoneNumber] || 0;
	}, // currentCount

	generateMobileCode: function() {
		if (!this.isSomethingEnabled('#' + this.GenerateCodeBtnID))
			return false;

		EzBob.App.trigger('clear');

		var sPhoneNumber = this.$el.find('#' + this.PhoneFieldID).val();

		this.setLastCodeTarget(sPhoneNumber);

		this.incCurrentCount();
		this.sendAttemptsCount++;

		this.handleTooManyAttempts();

		if ((this.currentCount() <= this.maxSendToCurrentCount()) && (this.sendAttemptsCount <= this.maxSendAttemptsCount())) {
			var self = this;

			var xhr = $.post(window.gRootPath + 'Account/GenerateMobileCode', { mobilePhone: sPhoneNumber, });

			xhr.done(function(isError) {
				if (isError !== 'False' && (!isError.success || isError.error === 'True'))
					EzBob.App.trigger('error', 'Error sending code.');
				else {
					self.setSentLabelVisible(true);
					self.hasCodeEverBeenSent = true;
				} // if

				return false;
			}); // on success

			xhr.fail(function() {
				EzBob.App.trigger('error', 'Error sending code.');
			}); // on failure

			xhr.always(function() {
				var bShowMobileCode = self.hasCodeEverBeenSent;

				if (bShowMobileCode && self.CaptchaEnabledFieldID)
					bShowMobileCode = self.$el.find('#' + self.CaptchaEnabledFieldID).val() !== '1';

				if (bShowMobileCode) {
					self.$el.find('#' + self.MobileCodeSectionID).removeClass('hide').show();
					self.$el.find('#' + self.GenerateCodeBtnID).val('Resend authentication code');

					if (document.activeElement && ($(document.activeElement).attr('id') === self.GenerateCodeBtnID))
						self.$el.find('#' + self.MobileCodeFieldID).focus();
				}
				else {
					self.$el.find('#' + self.MobileCodeFieldID).val('').blur();
					self.$el.find('#' + self.MobileCodeSectionID).addClass('hide').hide();
				} // if
			}); // always
		} // if

		return false;
	}, // generateMobileCode

	setMobilePhoneValidatorCfg: function(oCfg) {
		oCfg.rules[this.PhoneFieldID] = { required: true, regex: '^0[0-9]{10}$', };
		oCfg.rules[this.MobileCodeFieldID] =  { required: true, minlength: 6, maxlength: 6, };

		oCfg.messages[this.PhoneFieldID] = { regex: 'Please enter a valid UK number.', };
		oCfg.messages[this.MobileCodeFieldID] = { regex: 'Please enter the code you received.' };

		if (this.CaptchaEnabledFieldID)
			oCfg.rules.CaptchaInputText = { required: true, minlength: 6, maxlength: 6, };

		return oCfg;
	}, // setMobilePhoneValidatorCfg

	maxSendToCurrentCount: function() {
		return parseInt($('#broker-sms-count').attr('data-max-per-number') || 3, 10);
	}, // maxSendToCurrentCount

	maxSendAttemptsCount: function() {
		return parseInt($('#broker-sms-count').attr('data-max-per-page') || 10, 10);
	}, // maxSendAttemptsCount
}); // EzBob.Broker.SignupView