var EzBob = EzBob || {};

EzBob.ResetPasswordView = Backbone.Marionette.ItemView.extend({
	template: "#restore-pass-template",

	initialize: function() {
		this.focusCaptcha = _.bind(this.setFocusToCaptcha, this);
		this.focusAnswer = _.bind(this.setFocusToAnswer, this);
		this.focusEmail = _.bind(this.setFocusToEmail, this);

		this.mail = void 0;
		this.answerEnabled = true;
		this.emailEnabled = false;
		return this.captchaEnabled = EzBob.Config.CaptchaMode === 'off';
	}, // initialize

	focus: null,

	ui: {
		'questionArea': '#questionArea',
		'questionField': '#questionField',
		'email': '#email',
		'form': 'form',
		'getQuestionBtn': '#getQuestion',
		'restoreBtn': '#restore',
		'passwordRestoredArea': '.passwordRestoredArea',
		'restorePasswordArea': '.restorePasswordArea',
		'answer': '#Answer',
		'captcha': '#CaptchaInputText'
	}, // ui

	onRender: function() {
		$('footer.location-customer-everline .privacy-and-cookie-policy').hide();

		this.captcha = new EzBob.Captcha({
			elementId: 'captcha',
			tabindex: 11,
		});
		this.captcha.render();

		this.ui.email.data('changed', true);

		this.validator = EzBob.validateRestorePasswordForm(this.ui.form);

		this.initStatusIcons();

		$('#email').focus();

		EzBob.UiAction.registerView(this);

		var notifications = new EzBob.NotificationsView({ el: this.$el.find('.errorArea') });
		notifications.render();

		return this;
	}, // onRender

	events: {
		'click #getQuestion': 'getQuestionBtnClicked',
		'click #restore': 'restoreClicked',
		'keyup #email': 'inputEmailChanged',
		'change #email': 'inputEmailChanged',
		'keyup #Answer': 'inputAnswerChanged',
		'change #Answer': 'inputAnswerChanged',
		'keyup #CaptchaInputText': 'inputCaptchaChanged',
		'change #CaptchaInputText': 'inputCaptchaChanged',
	}, // events

	restoreClicked: function(e) {
		if (this.ui.restoreBtn.hasClass('disabled'))
			return false;

		var $el = $(e.currentTarget);

		if ($el.hasClass('disabled'))
			return false;

		$el.addClass('disabled');

		this.focus = null;

		var self = this;
		BlockUi();
		$.post('RestorePassword', this.ui.form.serializeArray()).done(function(data) {
			if (!EzBob.isNullOrEmpty(data.errorMessage) || !EzBob.isNullOrEmpty(data.error)) {
				EzBob.App.trigger('error', data.errorMessage || data.error);
				self.ui.questionArea.hide();
				self.ui.email.closest('div.control-group').show();
				$('.captcha').show();
				self.focus = self.focusCaptcha;
				self.ui.answer.val('');
				self.ui.getQuestionBtn.addClass('disabled');
				return false;
			} // if

			self.ui.passwordRestoredArea.show();
			self.ui.restorePasswordArea.hide();
			scrollTop();
			return false;
		}).fail(function(data) {
			EzBob.App.trigger('error', data.responceText);
			self.initStatusIcons();
			self.focus = self.focusCaptcha;
		}).always(function() {
			UnBlockUi();
			self.ui.email.data('changed', false);
			self.emailKeyuped();
			self.captcha.reload(self.focus);
		});

		return false;
	}, // restoreClicked

	setFocusToEmail: function() {
		$('#email').focus();
	}, // setFocusToEmail

	setFocusToAnswer: function() {
		$('#Answer').focus();
	}, // setFocusToAnswer

	setFocusToCaptcha: function() {
		$('#CaptchaInputText').focus();
	}, // setFocusToCaptcha

	inputCaptchaChanged: function() {
		this.captchaEnabled = this.validator.check($(this.ui.captcha.selector));
		var enabled = this.answerEnabled && this.emailEnabled && this.captchaEnabled;
		this.ui.getQuestionBtn.toggleClass('disabled', !enabled);
	}, //inputCaptchaChanged 

	inputEmailChanged: function() {
		this.emailEnabled = this.validator.check(this.ui.email);
		var enabled = this.answerEnabled && this.emailEnabled && this.captchaEnabled;
		this.ui.getQuestionBtn.toggleClass('disabled', !enabled);
	}, // inputEmailChanged

	inputAnswerChanged: function() {
		this.answerEnabled = this.validator.check(this.ui.answer);
		var enabled = this.answerEnabled && this.emailEnabled && this.captchaEnabled;
		return this.ui.restoreBtn.toggleClass('disabled', !enabled);
	}, // inputAnswerChanged

	emailKeyuped: function() {
		if (this.ui.email.data('changed'))
			return false;

		this.ui.email.data('changed', true);
		this.ui.questionArea.hide();
		this.ui.getQuestionBtn.show();
		this.captcha.$el.closest('.control-group').insertAfter(this.ui.email.closest('.control-group'));

		return false;
	}, // emailKeyuped 

	getQuestionBtnClicked: function() {
		if (this.ui.getQuestionBtn.hasClass('disabled'))
			return false;

		this.mail = this.ui.email.val();
		EzBob.App.trigger('clear');
		this.ui.questionArea.hide();
		this.focus = null;

		var self = this;
		BlockUi();
		$.post('QuestionForEmail', this.ui.form.serialize())
		 .done(function(response) {
			if (response.broker) {
				document.location.href = '' + window.gRootPath + 'Broker#ForgotPassword';
				return true;
			}

			if (!EzBob.isNullOrEmpty(response.errorMessage) || !EzBob.isNullOrEmpty(response.error)) {
				EzBob.App.trigger('error', response.errorMessage || response.error);
				self.ui.questionArea.hide();
				self.focus = self.focusCaptcha;
				self.ui.getQuestionBtn.addClass('disabled');
				return true;
			} // if

			if (EzBob.isNullOrEmpty(response.question)) {
				EzBob.App.trigger('warning', 'To recover your password security question fields must be completely filled in the account settings.');
				self.ui.questionArea.hide();
				self.focus = self.focusEmail;
				return true;
			} // if

			self.ui.questionField.text(response.question);
			self.ui.questionArea.show();
			self.initStatusIcons('email');
			self.ui.getQuestionBtn.hide();
			self.captcha.$el.closest('.control-group').insertAfter(self.ui.answer.closest('.control-group'));
			self.ui.email.data('changed', false);
			self.answerEnabled = false;
			self.ui.email.closest('div.control-group').hide();

			$('.captcha').hide();

			$('#Answer').focus();

			self.focus = self.focusAnswer;

			return true;
		 }).always(function() {
		 	UnBlockUi();
			self.captcha.reload(self.focus);
		});
		return false;
	}, // getQuestionBtnClicked

	initStatusIcons: function(e) {
		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		if (e === 'email')
			this.ui.email.change();
	}, // initStatusIcons
}); // EzBob.ResetPasswordView
