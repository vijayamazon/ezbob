EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.LoginView = Backbone.View.extend({
	initialize: function() {
		this.$el = $('.section-login');
		this.initValidatorCfg();

		this.router = this.options.router;
	}, // initialize

	events: {
		'change input': 'inputChanged',
		'keyup  input': 'inputChanged',

		'click a.ForgottenPassword': 'forgotten',
		'click a.Signup': 'signup',

		'click  #LoginBrokerButton': 'performLogin',
	}, // events

	forgotten: function() {
		event.preventDefault();
		event.stopPropagation();
		this.router.forgotten();
	}, // forgotten

	signup: function() {
		event.preventDefault();
		event.stopPropagation();
		this.router.signup();
	}, // signup

	clear: function() {
		this.$el.find('#LoginEmail, #LoginPassword').val('').blur();
		this.inputChanged();
	}, // clear

	performLogin: function() {
		event.preventDefault();
		event.stopPropagation();

		var oBtn = this.$el.find('#LoginBrokerButton');

		if (oBtn.hasClass('disabled') || oBtn.attr('disabled') || oBtn.prop('disabled'))
			return;

		this.setLoginEnabled(false);
		BlockUi();

		var sEmail = this.$el.find('#LoginEmail').val();

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/Login', this.$el.find('form').serializeArray());

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				EzBob.App.trigger('clear');
				self.clear();
				self.router.setAuth(sEmail);
				self.router.dashboard();
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSignupEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSignupEnabled(true);
			EzBob.App.trigger('error', 'Failed to log in. Please retry.');
		});
	}, // performLogin

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			this.setLoginEnabled(false);
			return this;
		} // if

		this.router.setAuth();

		this.$el.find('.customer-sidebar').append($('.common-customer-sidebar'));

		this.inputChanged();

		this.$el.find('#LoginEmail').focus();

		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	setLoginEnabled: function(bEnabled) {
		return this.setSomethingEnabled('#LoginBrokerButton', bEnabled);
	}, // setLoginEnabled

	setSomethingEnabled: function(sSelector, bEnabled) {
		var oElm = this.$el.find(sSelector);

		if (bEnabled)
			oElm.removeClass('disabled').removeAttr('disabled').removeProp('disabled');
		else
			oElm.addClass('disabled').attr('disabled', 'disabled').prop('disabled', 'disabled');

		return oElm;
	}, // setSomethingEnabled

	inputChanged: function(evt) {
		this.setLoginEnabled(EzBob.Validation.checkForm(this.validator));
	}, // inputChanged

	initValidatorCfg: function() {
		this.validator = this.$el.find('.login-form').validate({
			rules: {
				LoginEmail: { required: true, email: true, maxlength: 255, },
				LoginPassword: { required: true, minlength: 6, maxlength: 255, },
			},

			messages: {
				LoginEmail: { required: 'This field is required.', email: 'Please enter contact person email.', },
				LoginPassword: { regex: 'This field is required.', },
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		});
	}, // initValidatorCfg
}); // EzBob.Broker.LoginView
