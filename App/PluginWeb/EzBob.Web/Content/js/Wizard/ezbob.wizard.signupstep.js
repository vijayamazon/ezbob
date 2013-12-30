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

		this.showOfflineHelp = true;
		this.readyToProceed = false;
		this.mobileCodesSent = 0;

		var that = this;
		var xhr = $.post(window.gRootPath + "Home/GetTwilioConfig");
		xhr.done(function (isSmsValidationActive, numberOfMobileCodeAttempts) {
		    that.twilioEnabled = isSmsValidationActive;
		    that.numberOfMobileCodeAttempts = numberOfMobileCodeAttempts + 1;

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

		if (this.showOfflineHelp && (EzBob.getCookie('isoffline') === 'yes')) {
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
		this.setFieldStatusNotRequired(evt, 'amount');

		this.toggleOtherSelect(evt, 'customerReason', '#otherReasonDiv');
		this.toggleOtherSelect(evt, 'customerSourceOfRepayment', '#otherSourceDiv');

		var enabled = EzBob.Validation.checkForm(this.validator);
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

	toggleOtherSelect: function(evt, el, div) {
		if (evt && evt.target.id === el) {
			var other = false;
			if ($(evt.target).find('option:selected').text() === 'Other') {
				other = true;
			}
			$(div).toggleClass('hide', !other);
		}

		return false;
	},
	
	generateMobileCode: function () {
	    EzBob.App.trigger('clear');
	    
	    var isValidPhone = this.validator.check(this.$el.find('.phonenumber'));
	    if (!isValidPhone) {
	        EzBob.App.trigger('error', "Please enter a valid phone first");
	        return false;
	    }

	    this.mobileCodesSent++;
	    if (this.mobileCodesSent == this.numberOfMobileCodeAttempts) {
	        EzBob.App.trigger('warning', "Switching to authentication via captcha");
	        this.$el.find('#twilioDiv').hide();
	        this.$el.find('#captchaDiv').show();
	        this.twilioEnabled = false;
	        return false;
	    }
	    
	    var xhr = $.post(window.gRootPath + "Account/GenerateMobileCode", { mobilePhone: this.$el.find('.phonenumber').val() });
	    xhr.done(function () {
	        EzBob.App.trigger('info', "Code was sent to mobile");

	        return false;
	    });

	    return false;
	},

	switchToCaptcha: function () {
	    EzBob.App.trigger('clear');
	    this.$el.find('#twilioDiv').hide();
	    this.$el.find('#captchaDiv').show();
	    this.twilioEnabled = false;
	    return false;
	},
	
	submit: function() {
		if (this.$el.find(':submit').hasClass('disabled'))
			return false;

		this.blockBtn(true);

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

	    

		var xhr = $.post(this.form.attr('action'), data);

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
			    if (!that.twilioEnabled) {
			        that.captcha.reload();
			    }
			    that.$el.find(':submit').addClass('disabled');
			}
			that.blockBtn(false);
		});

		xhr.fail(function() {
		    EzBob.App.trigger('error', 'Something went wrong');
		    if (!that.twilioEnabled) {
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