var EzBob = EzBob || {};

EzBob.QuickSignUpStepView = Backbone.View.extend({
    initialize: function () {
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
        this.twilioEnabled = false;

		var that = this;

		var xhr = $.post(window.gRootPath + "Account/GetTwilioConfig");

		xhr.done(function(res) {
			that.twilioEnabled = res.isSmsValidationActive;
			that.switchedToCaptcha = res.switchedToCaptcha;
			that.numberOfMobileCodeAttempts = res.numberOfMobileCodeAttempts + 1;
			return false;
		});

		xhr.always(function() {
			if (that.twilioEnabled && !that.switchedToCaptcha) {
				that.$el.find('#twilioDiv').show();
				$.post(window.gRootPath + "Account/DebugLog_Message", { message: 'The visible object is mobile code' });
			} else {
				that.$el.find('#captchaDiv').show();
				$.post(window.gRootPath + "Account/DebugLog_Message", { message: 'The visible object is captcha' });
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

        this.$el.find('.phonenumber').numericOnly(11);
        this.$el.find('.phonenumbercode').numericOnly(6);
        
        fixSelectValidate(this.$el.find('select'));

        if (this.showOfflineHelp && ($('body').attr('data-offline') === 'yes')) {
            this.showOfflineHelp = false;

            var oDialog = this.$el.find('#offline_help');
            if (oDialog.length > 0)
                var x = $.colorbox({ inline: true, open: true, href: oDialog });
        } // if

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        this.inputChanged();

		if (this.$el.find('.broker-for-customer').length)
			this.switchToCaptcha();

        this.$el.find('#Email').focus();

        this.readyToProceed = true;
        return this;
    },

    inputChanged: function (evt) {
        this.setFieldStatusNotRequired(evt, 'promoCode');

        var enabled = EzBob.Validation.checkForm(this.validator);
        enabled = enabled && (!this.twilioEnabled || this.activatedCode || this.switchedToCaptcha);
        $('#signupSubmitButton').toggleClass('disabled', !enabled);
    },

    amountFocused: function () {
        this.$el.find('#amount').change();
    },

    setFieldStatusNotRequired: function (evt, el) {
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

        this.activatedCode = true;

        this.mobileCodesSent++;
        if (this.mobileCodesSent === this.numberOfMobileCodeAttempts) {
            EzBob.App.trigger('warning', "Switching to authentication via captcha");
            this.$el.find('#twilioDiv').hide();
            this.$el.find('#captchaDiv').show();
            this.switchedToCaptcha = true;
            $.post(window.gRootPath + "Account/SwitchedToCaptcha");
            return false;
        }
        var that = this;
        var xhr = $.post(window.gRootPath + "Account/GenerateMobileCode", { mobilePhone: this.$el.find('.phonenumber').val() });
        xhr.done(function (isError) {
            if (isError !== 'False' && (!isError.success || isError.error === 'True')) {
                EzBob.App.trigger('error', "Error sending code, please authenticate using captcha");
                that.$el.find('#twilioDiv').hide();
                that.$el.find('#captchaDiv').show();
                that.switchedToCaptcha = true;
                $.post(window.gRootPath + "Account/SwitchedToCaptcha");
            } else {
                var codeSentObject = that.$el.find('#codeSentLabel');
                codeSentObject.animate({ opacity: 1 });
            }

            return false;
        });
        xhr.always(function () {
            that.$el.find('#mobileCodeDiv').show();
            that.$el.find('#generateMobileCode').val('Resend activation code');
            if (document.getElementById('generateMobileCode') === document.activeElement) {
                document.getElementById('mobileCode').focus();
            }
        });

        return false;
    },

    mobilePhoneChanged: function () {
        var isValidPhone = this.validator.check(this.$el.find('.phonenumber'));
        var generateCodeButton = $('#generateMobileCode');
        if (isValidPhone) {
            if (generateCodeButton.hasClass('disabled')) {
                generateCodeButton.removeClass('disabled');
            }
        }
        else {
            if (!generateCodeButton.hasClass('disabled')) {
                generateCodeButton.addClass('disabled');
            }
            this.$el.find('#mobileCodeDiv').hide();
            this.$el.find('#generateMobileCode').val('Send activation code');
            var codeSentObject = this.$el.find('#codeSentLabel');
            codeSentObject.animate({ opacity: 0 });
        }

        return false;
    },

    switchToCaptcha: function () {
        EzBob.App.trigger('clear');
        this.$el.find('#twilioDiv').hide();
        this.$el.find('#captchaDiv').show();
        this.switchedToCaptcha = true;
        $.post(window.gRootPath + "Account/SwitchedToCaptcha");
        return false;
    },

    submit: function () {
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
        var amount = _.find(data, function (d) { return d.name === 'amount'; });
        if (amount) { amount.value = this.$el.find('#amount').autoNumericGet(); }

        if (this.switchedToCaptcha) {
            data.push({ name: "switchedToCaptcha", value: "True" });
        } else {
            data.push({ name: "switchedToCaptcha", value: "False" });
        }

        var xhr = $.post(this.form.attr('action'), data);

        var that = this;

        xhr.done(function (result) {
            if (result.success) {
                $.post(window.gRootPath + "Account/DebugLog_Message", { message: 'Success' });
                $('body').attr('auth', 'auth');

                that.$el.find('input[type="password"], input[type="text"]').tooltip('hide');

                EzBob.App.trigger('customerLoggedIn');
                EzBob.App.trigger('clear');

                $('body').attr('data-user-name', $('#Email').val());

                ShowHideSignLogOnOff();

                that.model.set('loggedIn', true); // triggers 'ready' and 'next'
            } else {
                $.post(window.gRootPath + "Account/DebugLog_Message", { message: result.errorMessage });
                if (result.errorMessage) EzBob.App.trigger('error', result.errorMessage);
                if (!that.twilioEnabled || that.switchedToCaptcha) {
                    that.captcha.reload();
                }
                that.$el.find(':submit').addClass('disabled');
            }
            that.blockBtn(false);
        });

        xhr.fail(function () {
            $.post(window.gRootPath + "Account/DebugLog_Message", { message: 'Something went wrong' });
            EzBob.App.trigger('error', 'Something went wrong');
            if (!that.twilioEnabled || that.switchedToCaptcha) {
                that.captcha.reload();
            }
            that.blockBtn(false);
        });

        return false;
    }, // submit

    ready: function () {
        this.setReadOnly();
    }, // ready

    setReadOnly: function () {
        this.readOnly = true;
        this.$el.find(':input').not(':submit').attr('disabled', 'disabled').attr('readonly', 'readonly').css('disabled');
        var captchaElement = this.$el.find('#captcha');
        if (captchaElement)
            captchaElement.hide();

        captchaElement = this.$el.find('.captcha');
        if (captchaElement)
            captchaElement.hide();

        this.$el.find(':submit').val('Continue');
        this.$el.find('[name="securityQuestion"]').trigger('liszt:updated');
    },
    blockBtn: function (isBlock) {
        BlockUi(isBlock ? 'on' : 'off');
    }
});