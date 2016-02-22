var EzBob = EzBob || {};

EzBob.QuickSignUpStepView = Backbone.View.extend({
	template: _.template($('#signup-template').html()),
	initialize: function () {
		// This post is for Nir's A\B testing
		//$.post("http://www.ezbob.com/thank-page/", { action: "sptAjaxRecordConversion", sptID: 5765 });

		this.on('ready', this.ready, this);
		this.model.on('change:loggedIn', this.render, this);

		this.showOfflineHelp = true;
		this.readyToProceed = false;
		this.activatedCode = false;
		this.mobileCodesSent = 0;
		this.twilioEnabled = false;
		this.switchedToCaptcha = false;
		this.alreadyRendered = false;

		var that = this;

		var xhr = $.post(window.gRootPath + "Account/GetTwilioConfig");

		xhr.done(function (res) {
			if (that.switchedToCaptcha)
				that.twilioEnabled = false;
			else {
				that.twilioEnabled = res.isSmsValidationActive;
				that.numberOfMobileCodeAttempts = res.numberOfMobileCodeAttempts + 1;
			}

			return false;
		});

		xhr.always(function () {
			if (that.twilioEnabled) {
				that.$el.find('#twilioDiv').removeClass('hide');
				EzBob.ServerLog.debug('The visible object is mobile code');
			}
			else {
				that.$el.find('#captchaDiv').removeClass('hide');
				EzBob.ServerLog.debug('The visible object is captcha');
			} // if

			return false;
		});
	},

	events: {
		'click #signupSubmitButton': 'submit',
		'click #generateMobileCode': 'generateMobileCode',
		'click #switchToCaptcha': 'switchToCaptcha',

		'change input': 'inputChanged',
		'keyup  input': 'inputChanged',
		'change select': 'inputChanged',

		'focus #amount': 'amountFocused',

		'keyup #mobilePhone': 'mobilePhoneChanged',
		'change #mobilePhone': 'mobilePhoneChanged',
	},

	render: function () {
		if (this.model.get('loggedIn')) {
			this.readyToProceed = false;

			this.trigger('ready');

			var sLastSavedStep = this.model.get('LastSavedWizardStep');
			if (sLastSavedStep)
				this.trigger('jump-to', sLastSavedStep);

			this.trigger('next');
			return this;
		} // if

		this.$el.html(this.template(this.model.toJSON()));
		this.form = this.$el.find('.signup');
		this.validator = EzBob.validateSignUpForm(this.form);

		this.$el.find('img[rel]').setPopover('left');
		this.$el.find('li[rel]').setPopover('left');

		this.$el.find('#amount').moneyFormat();

		this.$el.find('.phonenumber').mask('0?9999999999', { placeholder: ' ' });
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.phonenumbercode').numericOnly(6);

		fixSelectValidate(this.$el.find('select'));

		if (this.showOfflineHelp && ($('body').attr('data-offline') === 'yes')) {
			this.showOfflineHelp = false;

			var oDialog = this.$el.find('#offline_help');
			if (oDialog.length > 0)
				$.colorbox({
					inline: true,
					close: '<i class="pe-7s-close"></i>',
					open: true,
					href: oDialog,
					maxWidth: '100%',
					maxHeight: '100%',
					onOpen: function() {
						$('body').addClass('stop-scroll');
					},
					onClosed: function() {
						$('body').removeClass('stop-scroll');
					}
				});
		} // if

		var oEverlineDialog = this.$el.find('#everline_help');
		var email = this.model.get('Email');

		if (email)
			this.$el.find('#Email').val(email);

		if (this.model.get('IsEverline')) {
			oEverlineDialog.val(email);
			//currently not showing the explanation popup but saving the email 
			EzBob.UiAction.saveOne(EzBob.UiAction.evtLinked(), oEverlineDialog, true);

			this.showEverlineHelp = true;
			if (this.showEverlineHelp) {
				this.showEverlineHelp = false;

				if (oEverlineDialog.length > 0)
					$.colorbox({
						inline: true,
						open: true,
						href: oEverlineDialog,
						close: '<i class="pe-7s-close"></i>',
						maxWidth: '100%',
						maxHeight: '100%',
						onOpen: function() {
							$('body').addClass('stop-scroll');
						},
						onClosed: function() {
							$('body').removeClass('stop-scroll');
						}
					});
			}
		} // if

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.inputChanged();

		var brokerFillsForCustomer = !!this.$el.find('.broker-for-customer').length;

		var emailObj = this.$el.find('#Email');

		if (brokerFillsForCustomer) {
			this.switchToCaptcha();
			emailObj.attr('readonly', 'readonly').addClass('disabled');
		} // if

		if (this.alreadyRendered)
			EzBob.Validation.element(this.validator, emailObj);
		else {
			if (emailObj.val() !== '')
				EzBob.Validation.element(this.validator, emailObj);

			this.alreadyRendered = true;
		} // if

		emailObj.change().attardi_labels('toggle');
		if (emailObj.val() === '')
			setTimeout(this.focusOnEmail, 50);
		else
			setTimeout(this.focusOnPassword, 50);

		this.readyToProceed = true;
		return this;
	}, // render

	focusOnEmail: function () {
		document.getElementById('Email').focus();
	},

	focusOnPassword: function () {
		document.getElementById('signupPass1').focus();
	},

	inputChanged: function () {
		var enabled = EzBob.Validation.checkForm(this.validator);

		var isInCaptchaMode = !this.$el.find('#captchaDiv').hasClass('hide');
		enabled = enabled && (isInCaptchaMode || this.activatedCode);
		this.$el.find('#signupSubmitButton').toggleClass('disabled', !enabled);
	},

	amountFocused: function () {
		this.$el.find('#amount').change();
	},

	generateMobileCode: function () {
		if (this.$el.find('#generateMobileCode').hasClass('disabled'))
			return false;

		EzBob.App.trigger('clear');

		this.activatedCode = true;

		this.mobileCodesSent++;
		if (this.mobileCodesSent === this.numberOfMobileCodeAttempts) {
			EzBob.App.trigger('warning', "Switching to authentication via captcha");
			this.$el.find('#twilioDiv').addClass('hide');
			this.$el.find('#captchaDiv').removeClass('hide');
			return false;
		}
		var that = this;
		var xhr = $.post(window.gRootPath + "Account/GenerateMobileCode", { mobilePhone: this.$el.find('.phonenumber').val() });
		xhr.done(function (isError) {
			if (isError !== 'False' && (!isError.success || isError.error === 'True')) {
				EzBob.App.trigger('error', "Error sending code, please authenticate using captcha");
				that.$el.find('#twilioDiv').addClass('hide');
				that.$el.find('#captchaDiv').removeClass('hide');
			} else {
				var codeSentObject = that.$el.find('#codeSentLabel');
				codeSentObject.animate({ opacity: 1 });
			}


		});
		xhr.always(function () {
			that.$el.find('#mobileCodeDiv').show();
			that.$el.find('#generateMobileCode').text('Resend code');
			if (document.getElementById('generateMobileCode') === document.activeElement) {
				document.getElementById('mobileCode').focus();
			}
		});

		return false;
	},

	mobilePhoneChanged: function () {
		var isValidPhone = this.validator.check(this.$el.find('.phonenumber'));

		var generateCodeButton = this.$el.find('#generateMobileCode');

		if (isValidPhone) {
			generateCodeButton.removeClass('disabled');
		}
		else {
			generateCodeButton.addClass('disabled');
			this.$el.find('#mobileCodeDiv').hide();
			this.$el.find('#generateMobileCode').val('Send authentication code');
			var codeSentObject = this.$el.find('#codeSentLabel');
			codeSentObject.animate({ opacity: 0 });
		}

		return false;
	},

	switchToCaptcha: function () {
		this.switchedToCaptcha = true;
		EzBob.App.trigger('clear');
		this.$el.find('#twilioDiv').addClass('hide');
		this.$el.find('#captchaDiv').removeClass('hide');
		return false;
	},

	submit: function () {
		if (this.$el.find('#signupSubmitButton').hasClass('disabled'))
			return false;

		this.blockBtn(true);

		var mobilePhone = '', mobileCode = '';

		var isInCaptchaMode = !this.$el.find('#captchaDiv').hasClass('hide');
		if (!isInCaptchaMode) {
			mobilePhone = this.$el.find('#mobilePhone').val();
			mobileCode = this.$el.find('#mobileCode').val();
			this.model.set('twilioPhone', mobilePhone);
		} // if

		if (this.model.get('loggedIn')) {
			this.trigger('ready');
			this.trigger('next');
			this.blockBtn(false);
			return false;
		} // if

		if (!EzBob.Validation.checkForm(this.validator)) {
			this.blockBtn(false);
			return false;
		} // if

		var data = this.form.serializeArray();

		var amount = _.find(data, function (d) { return d.name === 'amount'; });
		if (amount)
			amount.value = this.$el.find('#amount').autoNumeric('get');

		if (isInCaptchaMode)
			data.push({ name: "isInCaptchaMode", value: "True" });
		else
			data.push({ name: "isInCaptchaMode", value: "False" });

		data.push({ name: "whiteLabelId", value: this.model.get('WhiteLabelId') });

		var xhr = $.post(this.form.attr('action'), data);

		var that = this;

		xhr.done(function (result) {
			var sEmail = that.$el.find('#Email').val();

			if (result.success) {
				EzBob.Csrf.updateToken(result.antiforgery_token);
				EzBob.ServerLog.debug('Customer', sEmail, 'signed up successfully.');
				$('body').attr('auth', 'auth');

				that.$el.find('input[type="password"], input[type="text"]').tooltip('hide');

				EzBob.App.Iovation.callIovation('signup');
				EzBob.App.trigger('customerLoggedIn');
				EzBob.App.trigger('clear');

				$('body').attr('data-user-name', sEmail);
				$('body').attr('data-ref', result.refNumber);
				window.ShowHideSignLogOnOff();

				that.model.set('loggedIn', true); // triggers 'ready' and 'next'


			}
			else {

				EzBob.ServerLog.warn('Customer', sEmail, 'failed to sign up with error message:', result.errorMessage);

				if (result.errorMessage)
					EzBob.App.trigger('error', result.errorMessage);

				if (isInCaptchaMode)
					that.captcha.reload();

				that.$el.find('#signupSubmitButton').addClass('disabled');
				that.blockBtn(false);
			}
		});

		xhr.fail(function () {
			var sEmail = that.$el.find('#Email').val();

			EzBob.ServerLog.alert('Something went wrong while customer', sEmail, 'tried to sign up.');

			EzBob.App.trigger('error', 'Something went wrong');

			if (isInCaptchaMode)
				that.captcha.reload();

			that.blockBtn(false);
		});

		return false;
	}, // submit

	ready: function () {
		this.setReadOnly();
	}, // ready

	setReadOnly: function () {
		this.readOnly = true;
		this.$el.find(':input').not('#signupSubmitButton').attr('disabled', 'disabled').attr('readonly', 'readonly').css('disabled');
		var captchaElement = this.$el.find('#captcha');
		if (captchaElement)
			captchaElement.hide();

		captchaElement = this.$el.find('.captcha');
		if (captchaElement)
			captchaElement.hide();

		this.$el.find('#signupSubmitButton').val('Next');
		this.$el.find('[name="securityQuestion"]').trigger('liszt:updated');
	},
	blockBtn: function (isBlock) {
		BlockUi(isBlock ? 'on' : 'off');
	},//blockBtn
});