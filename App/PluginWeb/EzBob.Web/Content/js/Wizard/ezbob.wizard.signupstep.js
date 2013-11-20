﻿///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="../recaptcha_ajax.js" />
/// <reference path="../ezbob.design.js" />


var EzBob = EzBob || {};


EzBob.QuickSignUpStepView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#signup-template').html());

        if (typeof ordsu == 'undefined') { ordsu = Math.random() * 10000000000000000; }
        if (typeof ordpi == 'undefined') { ordpi = Math.random() * 10000000000000000; }
        if (typeof ordty == 'undefined') { ordty = Math.random() * 10000000000000000; }
        if (typeof ordla == 'undefined') { ordla = Math.random() * 10000000000000000; }

        this.on('ready', this.ready, this);
        this.model.on('change:loggedIn', this.render, this);

        this.showOfflineHelp = true;
    },
    events: {
        'click :submit': 'submit',
        'change input': 'inputChanged',
        'keyup input': 'inputChanged',
        'change select': 'inputChanged',
        'focus #amount': "amountFocused",
    },
    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        this.form = this.$el.find('.signup');
        this.validator = EzBob.validateSignUpForm(this.form);

        var isLoggedIn = this.model.get('loggedIn');

        if (isLoggedIn) {
            this.setReadOnly();
            this.trigger('ready');
            this.trigger('next');
        }

        EzBob.UiAction.register(this.$el.find('[ui-event-control-id]'));

        this.$el.find('img[rel]').setPopover("left");
        this.$el.find('li[rel]').setPopover("left");

        this.$el.find('#amount').moneyFormat();
        this.inputChanged();

        fixSelectValidate(this.$el.find('select'));

        if (this.showOfflineHelp && !isLoggedIn && (EzBob.getCookie('isoffline') == 'yes')) {
            this.showOfflineHelp = false;

            var oDialog = this.$el.find('#offline_help');
            if (oDialog.length > 0) {
                var x = $.colorbox({ inline: true, transition: 'none', open: true, href: oDialog });
            }
        } // if

        return this;
    },
    inputChanged: function (evt) {
        this.setFieldStatusNotRequired(evt, 'promoCode');
        this.setFieldStatusNotRequired(evt, 'amount');

        this.toggleOtherSelect(evt, 'customerReason', '#otherReasonDiv');
        this.toggleOtherSelect(evt, 'customerSourceOfRepayment', '#otherSourceDiv');

        var enabled = EzBob.Validation.checkForm(this.validator);
        $("#signupSubmitButton").toggleClass('disabled', !enabled);
    },
    amountFocused: function () {
        this.$el.find("#amount").change();
    },
    setFieldStatusNotRequired: function (evt, el) {
        if (evt && evt.target.id == el && evt.target.value == '') {
            var img = $(evt.target).closest('div').find('.field_status');
            img.field_status('set', 'empty', 2);
        }
    },
    toggleOtherSelect: function (evt, el, div) {
        if (evt && evt.target.id == el) {
            var other = false;
            if ($(evt.target).find('option:selected').text() == 'Other') {
                other = true;
            }
            $(div).toggleClass('hide', !other);
        }

        return false;
    },
    submit: function () {
        if (this.$el.find(':submit').hasClass("disabled")) {
            return false;
        }

        this.blockBtn(true);
        var that = this;
        if (this.model.get('loggedIn')) {
            this.trigger('ready');
            this.trigger('next');
            that.blockBtn(false);
            return false;
        };

        if (!EzBob.Validation.checkForm(that.validator)) {
            that.blockBtn(false);
            return false;
        }
        var data = that.form.serializeArray();
        var amount = _.find(data, function (d) { return d.name == "amount"; });
        if (amount) { amount.value = this.$el.find("#amount").autoNumericGet(); }

        var xhr = $.post(that.form.attr("action"), data);

        xhr.done(function (result) {
            if (result.success) {
                $('body').attr('auth', 'auth');
                that.$el.find('input[type="password"], input[type="text"]').tooltip('hide');
                EzBob.App.trigger('customerLoggedIn');
                EzBob.App.trigger('clear');
                //EzBob.App.trigger('info', "You have successfully registered. The message was sent to your email.");
                $.get(window.gRootPath + "Start/TopButton").done(function (dat) {
                    $('#pre_header').html(dat);
                });
                that.model.set('loggedIn', true); // triggers 'ready' and 'next'
            } else {
                if (result.errorMessage) EzBob.App.trigger("error", result.errorMessage);
                that.captcha.reload();
                that.$el.find(':submit').addClass("disabled");
            }
            that.blockBtn(false);
        });

        xhr.fail(function () {
            EzBob.App.trigger("error", "Something went wrong");
            that.captcha.reload();
            that.blockBtn(false);
        });

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
        BlockUi(isBlock ? "on" : "off");
    }
});