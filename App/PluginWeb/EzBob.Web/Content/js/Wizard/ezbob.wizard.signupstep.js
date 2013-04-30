﻿///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="../recaptcha_ajax.js" />
/// <reference path="../ezbob.design.js" />


var EzBob = EzBob || {};


EzBob.QuickSignUpStepView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#signup-template').html());
        this.on('ready', this.ready, this);
        this.model.on('change:loggedIn', this.render, this);
    },
    events: {
        'click :submit': 'submit',
        'keydown input[name="Email"]': 'inputChanged',
        'paste input[name="Email"]': 'inputChanged',
        'input input[name="Email"]': 'inputChanged',
        'change input[name="Email"]': 'emailChanged',
        'keydown input[name="signupPass1"]': 'inputChanged',
        'paste input[name="signupPass1"]': 'inputChanged',
        'input input[name="signupPass1"]': 'inputChanged',
        'change input[name="signupPass1"]': 'password1Changed',
        'keydown input[name="signupPass2"]': 'inputChanged',
        'paste input[name="signupPass2"]': 'inputChanged',
        'input input[name="signupPass2"]': 'inputChanged',
        'change input[name="signupPass2"]': 'password2Changed',
        'change select[name="securityQuestion"]': "securityQuestionChanged",
        'keydown input[name="SecurityAnswer"]': 'inputChanged',
        'paste input[name="SecurityAnswer"]': 'inputChanged',
        'input input[name="SecurityAnswer"]': 'inputChanged',
        'change input[name="SecurityAnswer"]': 'secretAnswerChanged'
    },
    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        this.form = this.$el.find('.signup');
        this.validator = EzBob.validateSignUpForm(this.form);

        if (this.model.get('loggedIn')) {
            this.setReadOnly();
            this.trigger('ready');
            this.trigger('next');
        }
        
        this.$el.find('img[rel]').setPopover("left");
        this.$el.find('li[rel]').setPopover("left");
        
        return this;
    },
    inputChanged: function () {
    	if (EzBob.Validation.checkForm(this.validator)) {
            $("#signupSubmitButton.disabled").removeClass('disabled');
        } else {
            $("#signupSubmitButton").addClass('disabled');
        }
    },
    emailChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "EmailImage", "#Email", "#RotateImage", "#OkImage", "#FailImage");
    },
    password1Changed: function () {
    	EzBob.Validation.displayIndication(this.validator, "Password1Image", "#signupPass1", "#RotateImage", "#OkImage", "#FailImage");
        if ($("#signupPass2").val() != "") {
        	EzBob.Validation.displayIndication(this.validator, "Password2Image", "#signupPass2", "#RotateImage", "#OkImage", "#FailImage");
        }
    },
    password2Changed: function () {
    	EzBob.Validation.displayIndication(this.validator, "Password2Image", "#signupPass2", "#RotateImage", "#OkImage", "#FailImage");
    },
    securityQuestionChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "SecurityQuestionImage", "#securityQuestion", "#RotateImage", "#OkImage", "#FailImage");
    },
    secretAnswerChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "SecretAnswerImage", "#SecurityAnswer", "#RotateImage", "#OkImage", "#FailImage");
    },
    submit: function () {
        if (this.$el.find(':submit').hasClass("disabled")) {
            return false;
        }

        this.blockBtn(true);

        var that = this;

        if (this.model.get('signedIn') || (this.model.get('loggedIn'))) {
            this.trigger('ready');
            this.trigger('next');
            that.blockBtn(false);
            return false;
        };

        if (!EzBob.Validation.validateAndNotify(that.validator)) {
            that.blockBtn(false);
            return false;
        }

        $.post(that.form.attr("action"), that.form.serialize(), function(result) {
            if (result.success) {
                that.$el.find('input[type="password"], input[type="text"]').tooltip('hide');
                EzBob.App.trigger("signedIn");
                EzBob.App.trigger("clear");
                //EzBob.App.trigger('info', "You have successfully registered. The message was sent to your email.");
                that.model.set('signedIn', true);
                that.trigger('ready');
                that.trigger('next');
                $.get(window.gRootPath + "Start/TopButton").done(function(dat) {
                    $('#pre_header').html(dat);
                });
            } else {
                if (result.errorMessage) EzBob.App.trigger("error", result.errorMessage);
                that.captcha.reload();
            }
            that.blockBtn(false);
        }, "json");

        return false;
    },
    ready: function () {
        this.setReadOnly();
    },
    setReadOnly: function () {
        this.readOnly = true;
        this.$el.find(':input').not(':submit').attr('disabled', 'disabled').attr('readonly', 'readonly').css('disabled');
        this.$el.find('#captcha').hide();
        this.$el.find('.captcha').hide();
        this.$el.find(':submit').val('Continue');
        this.$el.find('[name="securityQuestion"]').trigger("liszt:updated");
    },
    blockBtn: function (isBlock) {
        BlockUi(isBlock ? "on": "off");
        this.$el.find(':submit').toggleClass("disabled", isBlock);
    }
});