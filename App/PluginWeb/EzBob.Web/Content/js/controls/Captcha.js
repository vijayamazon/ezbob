///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
/// <reference path="../lib/recaptcha_ajax.js" />
/// <reference path="../ezbob.design.js" />
/// <reference path="../App/ezbob.app.js" />

var EzBob = EzBob || {};


//-------------------------------------------------------------
EzBob.SimpleCaptcha = Backbone.View.extend({
    initialize: function (parameters) {
        this.setElement($('#' + parameters.elementId));
        this.tabindex = parameters.tabindex || 0;
        this.captchaUrl = window.gRootPath + 'Account/SimpleCaptcha';

        this.CaptchaMode = EzBob.Config.CaptchaMode;
    },
    render: function () {
        var that = this;
        $.ajax({ url: this.captchaUrl, cache: false })
            .done(function (response) {
                that.$el.html("<div class='simpleCaptcha'>" + response + "</div>");
                that.$el.find("br").remove();
                that.$el.find('a').html('Refresh');
                that.$el.find('input').attr('tabindex', that.tabindex);
                that.$el.find('input#CaptchaInputText').attr('tabindex', that.tabindex);

                //fix conflict with Backbone history and refresh button
                that.$el.find('.simpleCaptcha a').on("click", function () {
                    return false;
                });
            })
            .fail(function () {
                EzBob.App.trigger('error', "Captcha connection failed. Please try again later.");
            });
        return false;
    },
    reload: function () {
        this.render();
    }
});

//------------------------------------------------------

EzBob.ReCaptcha = Backbone.View.extend({
    initialize: function (parameters) {
        this.elementId = parameters.elementId;
        this.tabindex = parameters.tabindex || 0;
        this.captchaUrl = window.gRootPath + 'Account/SimpleCaptcha';
    },
    render: function () {
        Recaptcha.create("6Le8aM8SAAAAAFeTTn1qU2sNYROvowK9S1jyJCJd",
                this.elementId,
                { theme: "white", lang: 'en', tabindex: this.tabindex });
    },
    reload: function() {
        Recaptcha.reload();
    }
});

EzBob.EmptyCaptcha = Backbone.View.extend({
    initialize: function (parameters) {
        this.element = $(document.getElementById(parameters.elementId));
    },
    render: function () {
        this.element.parents('.captcha').css('display', 'none');
        return this;
    },
    reload: function () {
        
    }
});
