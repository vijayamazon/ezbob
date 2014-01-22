var EzBob = EzBob || {};

EzBob.QuickSignUpStepView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#signup-template').html());

		if (typeof ordsu === 'undefined') { ordsu = Math.random() * 10000000000000000; }
		if (typeof ordpi === 'undefined') { ordpi = Math.random() * 10000000000000000; }
		if (typeof ordty === 'undefined') { ordty = Math.random() * 10000000000000000; }
		if (typeof ordla === 'undefined') { ordla = Math.random() * 10000000000000000; }

		this.on('ready', this.ready, this);
		this.model.on('change:loggedIn', this.render, this);

	    this.switchedToCaptcha = false;
		this.showOfflineHelp = true;
		this.readyToProceed = false;
	    this.activatedCode = false;
		this.mobileCodesSent = 0;

		var that = this;
	    
		var xhr = $.post(window.gRootPath + "Home/GetTwilioConfig");
		xhr.done(function (res) {
		    that.twilioEnabled = res.isSmsValidationActive;
		    that.numberOfMobileCodeAttempts = res.numberOfMobileCodeAttempts + 1;

		    return false;
		});
		xhr.always(function () {

		    if (that.twilioEnabled) {
		        that.$el.find('#twilioDiv').show();
		    } else {
		        that.$el.find('#captchaDiv').show();
		    }

		    return false;
		});
	},

	events: {
	    'click :submit': 'submit',
	    'click #generateMobileCode': 'generateMobileCode',
	    'click #switchToCaptcha': 'switchToCaptcha',

		'change input': 'inputChanged',
		'keyup  input': 'inputChanged',
        'change select': 'inputChanged',

		'focus #amount': 'amountFocused',
	    
	    'keyup #mobilePhone': 'mobilePhoneChanged'
	},

	render: function() {
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
	    
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.phonenumbercode').numericOnly(6);

		fixSelectValidate(this.$el.find('select'));

		if (this.showOfflineHelp && ($('body').attr('data-offline') === 'yes')) {
			this.showOfflineHelp = false;

			var oDialog = this.$el.find('#offline_help');
			if (oDialog.length > 0)
				var x = $.colorbox({ inline: true, transition: 'none', open: true, href: oDialog });
		} // if

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.inputChanged();

		this.$el.find('#Email').focus();

		this.readyToProceed = true;
		return this;
	},

	inputChanged: function(evt) {
		this.setFieldStatusNotRequired(evt, 'promoCode');
		var enabled = EzBob.Validation.checkForm(this.validator);
	    enabled = enabled && (!this.twilioEnabled || this.activatedCode);
		$('#signupSubmitButton').toggleClass('disabled', !enabled);
	},

	amountFocused: function() {
		this.$el.find('#amount').change();
	},

	setFieldStatusNotRequired: function(evt, el) {
		if (evt && evt.target.id === el && evt.target.value === '') {
			var img = $(evt.target).closest('div').find('.field_status');
			img.field_status('set', 'empty', 2);
		} // if
	},

	generateMobileCode: function () {
	    if ($('#generateMobileCode').hasClass('disabled')) {
	        return false;
	    }
	    EzBob.App.trigger('clear');
	    $('#mobileCodeDiv').show();
	    $('#generateMobileCode').val('Resend activation code');

	    this.activatedCode = true;

	    this.mobileCodesSent++;
	    if (this.mobileCodesSent == this.numberOfMobileCodeAttempts) {
	        EzBob.App.trigger('warning', "Switching to authentication via captcha");
	        this.$el.find('#twilioDiv').hide();
	        this.$el.find('#captchaDiv').show();
	        this.switchToCaptcha = true;
	        return false;
	    }
	    var that = this;
	    var xhr = $.post(window.gRootPath + "Account/GenerateMobileCode", { mobilePhone: this.$el.find('.phonenumber').val() });
	    xhr.done(function (isError) {
	        if (isError == "True") {
	            EzBob.App.trigger('error', "Error sending code, please authenticate using captcha");
	            that.$el.find('#twilioDiv').hide();
	            that.$el.find('#captchaDiv').show();
	            that.switchToCaptcha = true;
	        } else {
	            EzBob.App.trigger('info', "Code was sent to mobile");
	        }

	        return false;
	    });

	    return false;
	},

	mobilePhoneChanged: function () {
	    var isValidPhone = this.validator.check(this.$el.find('.phonenumber'));
	    var generateCodeButton = $('#generateMobileCode');
	    if (isValidPhone && generateCodeButton.hasClass('disabled')) {
	        generateCodeButton.removeClass('disabled');
	    }
	    else if (!isValidPhone && !generateCodeButton.hasClass('disabled')) {
	        generateCodeButton.addClass('disabled');
	    }

	    return false;
	},

	switchToCaptcha: function () {
	    EzBob.App.trigger('clear');
	    this.$el.find('#twilioDiv').hide();
	    this.$el.find('#captchaDiv').show();
	    this.switchedToCaptcha = true;
	    return false;
	},
	
	submit: function() {
		if (this.$el.find(':submit').hasClass('disabled'))
			return false;

		this.blockBtn(true);

	    var mobilePhone = '', mobileCode = '';
	    
	    if (this.twilioEnabled && !this.switchedToCaptcha) {
	        mobilePhone = $('#mobilePhone').val();
	        mobileCode = $('#mobileCode').val();
	        this.model.set('twilioPhone', mobilePhone);
        }

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
		var amount = _.find(data, function(d) { return d.name === 'amount'; });
		if (amount) { amount.value = this.$el.find('#amount').autoNumericGet(); }

	    var xhr = $.post(this.form.attr('action'), {
	        email: $('#Email').val(),
	        signupPass1: $('#signupPass1').val(),
	        signupPass2: $('#signupPass2').val(),
	        securityQuestion: $('#securityQuestion').val(),
	        securityAnswer: $('#SecurityAnswer').val(),
	        promoCode: $('#promoCode').val(),
	        amount: $('#amount').val(),
	        mobilePhone: mobilePhone,
	        mobileCode: mobileCode,
	        switchedToCaptcha: this.switchedToCaptcha
	    });

		var that = this;

		xhr.done(function(result) {
			if (result.success) {
				$('body').attr('auth', 'auth');

				that.$el.find('input[type="password"], input[type="text"]').tooltip('hide');

				EzBob.App.trigger('customerLoggedIn');
				EzBob.App.trigger('clear');

				$.get(window.gRootPath + 'Start/TopButton').done(function(dat) {
					$('#pre_header').html(dat);
				});

				that.model.set('loggedIn', true); // triggers 'ready' and 'next'
			} else {
			    if (result.errorMessage) EzBob.App.trigger('error', result.errorMessage);
			    if (!that.twilioEnabled || that.switchedToCaptcha) {
			        that.captcha.reload();
			    }
			    that.$el.find(':submit').addClass('disabled');
			}
			that.blockBtn(false);
		});

		xhr.fail(function() {
		    EzBob.App.trigger('error', 'Something went wrong');
		    if (!that.twilioEnabled || that.switchedToCaptcha) {
		        that.captcha.reload();
		    }
		    that.blockBtn(false);
		});

		return false;
	}, // submit

	ready: function() {
		this.setReadOnly();
	}, // ready

	setReadOnly: function() {
		this.readOnly = true;
		this.$el.find(':input').not(':submit').attr('disabled', 'disabled').attr('readonly', 'readonly').css('disabled');
		var captchaElement = this.$el.find('#captcha');
	    if (captchaElement != undefined) {
	        captchaElement.hide();
	    }
	    captchaElement = this.$el.find('.captcha');
	    if (captchaElement != undefined) {
	        captchaElement.hide();
	    }
		this.$el.find(':submit').val('Continue');
		this.$el.find('[name="securityQuestion"]').trigger('liszt:updated');
	},
	blockBtn: function(isBlock) {
		BlockUi(isBlock ? 'on' : 'off');
	}
});