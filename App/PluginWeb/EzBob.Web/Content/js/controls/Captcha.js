var EzBob = EzBob || {};

EzBob.SimpleCaptcha = Backbone.View.extend({
	initialize: function (parameters) {
		this.setElement($('#' + parameters.elementId));
		this.tabindex = parameters.tabindex || 0;
		this.captchaUrl = window.gRootPath + 'Account/SimpleCaptcha';

		this.CaptchaMode = EzBob.Config.CaptchaMode;
	}, // initialize

	render: function (callback) {
		var that = this;

		var oXhr = $.ajax({ url: this.captchaUrl, cache: false });

		oXhr.done(function(response) {
			that.$el.html('<div class=simpleCaptcha>' + response + '</div>');
			that.$el.find('br').remove();
			that.$el.find('a').html('Refresh');
			that.$el.find('input').attr('tabindex', that.tabindex);

			that.$el.find('input#CaptchaInputText').replaceWith(
				$('<div />').append(
					$('<label />')
						.addClass('attardi-input')
						.append(
							$('<span />').text('Enter characters shown above')
						)
						.append(
							$('<input />')
								.attr({
									tabindex: that.tabindex,
									id: 'CaptchaInputText',
									name: 'CaptchaInputText',
									type: 'text',
									'ui-event-control-id': 'signup:captcha',
									maxlength: 6,
								}) // attr
								.addClass('form_field')
						) // append to label
						.append(
							$('<img />').addClass('field_status required').attr('id', 'CaptchaInputTextImage')
						)// append img
				) // append to div
					.append('<span>&nbsp;</span>')
			); // replaceWith

		    that.$el.find('img.field_status').field_status({ required: true });
		    that.$el.find('#CaptchaImage').addClass('captcha-image-margin');

			EzBob.UiAction.registerView(that);

			// fix conflict with Backbone history and refresh button
			that.$el.find('.simpleCaptcha a').on('click', function(evt) {
				evt.preventDefault();
				return false;
			});
		}); // done

		oXhr.fail(function() {
			EzBob.App.trigger('error', 'Captcha connection failed. Please try again later.');
		}); // fail

		oXhr.always(function() {
			if (callback)
				callback.apply();
		}); // always

		return false;
	}, // render

	reload: function (callback) {
		this.render(callback);
	} // reload
}); // EzBob.SimpleCaptcha

EzBob.ReCaptcha = Backbone.View.extend({
	initialize: function (parameters) {
		this.elementId = parameters.elementId;
		this.tabindex = parameters.tabindex || 0;
		this.captchaUrl = window.gRootPath + 'Account/SimpleCaptcha';
	}, // initialize

	render: function () {
		Recaptcha.create('6Le8aM8SAAAAAFeTTn1qU2sNYROvowK9S1jyJCJd',
			this.elementId,
			{ theme: 'white', lang: 'en', tabindex: this.tabindex }
		);
	}, // render

	reload: function () {
		Recaptcha.reload();
	} // reload
}); // EzBob.ReCaptcha

EzBob.EmptyCaptcha = Backbone.View.extend({
	initialize: function (parameters) {
		this.element = $(document.getElementById(parameters.elementId));
	}, // initialize

	render: function () {
		console.log('empty captcha')
		this.element.parents('.captcha').css('display', 'none');
		return this;
	}, // render

	reload: function () {} // reload
}); // EzBob.EmptyCaptcha
