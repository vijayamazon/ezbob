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
	render: function (callback) {
		var that = this;
		$.ajax({ url: this.captchaUrl, cache: false })
			.done(function(response) {
				that.$el.html("<div class='simpleCaptcha'>" + response + "</div>");
				that.$el.find("br").remove();
				that.$el.find('a').html('Refresh');
				that.$el.find('input').attr('tabindex', that.tabindex);

				that.$el.find('input#CaptchaInputText').replaceWith(
					$('<div />').append(
						$('<label />')
							.addClass('attardi-input')
							.append(
								$('<span />').text('Please enter above characters here')
							)
							.append(
								$('<input />')
									.attr({
										tabindex: that.tabindex,
										id: "CaptchaInputText",
										name: "CaptchaInputText",
										type: "text"
									}) // attr
									.addClass('form_field')
							) // append to label
					) // append to div
						.append('<span>&nbsp;</span>')
						.append(
							$('<img />').addClass('field_status').attr('id', 'CaptchaInputTextImage')
						)
				); // replaceWith

				that.$el.find('img.field_status').field_status({ required: true });

				//fix conflict with Backbone history and refresh button
				that.$el.find('.simpleCaptcha a').on("click", function() {
					return false;
				});
			})
			.fail(function() {
				EzBob.App.trigger('error', "Captcha connection failed. Please try again later.");
			})
			.always(function() {
				if (callback)
					callback.apply();
			});
		return false;
	},
	reload: function (callback) {
		this.render(callback);
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
	reload: function () {
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
