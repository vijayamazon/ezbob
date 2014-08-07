var EzBob = EzBob || {};

EzBob.CustomerCreatePasswordView = EzBob.View.extend({
	initialize: function() {
		this.template = _.template($('#customer-create-password-template').html());
	}, // initialize

	events: {
		'click :submit': 'submit',
		'keyup input': 'inputChanged',
		'change input': 'inputChanged',
		'click.checks #RememberMe': 'rememberMeChanged'
	}, // events

	render: function() {
		this.$el.html(this.template());

		this.form = this.$el.find('.simple-create-password');

		this.validator = this.buildValidator();

		this.$el.find('img[rel]').setPopover('left');
		this.$el.find('li[rel]').setPopover('left');

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		$('#Password').focus();

		EzBob.UiAction.registerView(this);
		return this;
	}, // render

	rememberMeChanged: function() {
		var rememberMe = this.$el.find('#RememberMe');
		rememberMe.val(rememberMe.is(':checked'));
	}, // rememberMeChanged

	inputChanged: function() {
		this.setSomethingEnabled(':submit', EzBob.Validation.checkForm(this.validator));
	}, // inputChanged

	submit: function() {
		if (!this.isSomethingEnabled(':submit'))
			return false;

		if (!EzBob.Validation.checkForm(this.validator))
			return false;

		var data = this.form.serialize();

		BlockUi();

		$.post(this.form.attr('action'), data).done(function(result, status) {
			EzBob.ServerLog.debug('create password request completed with status', status);

			if (status === 'success') {
				if (result.success) {
					document.location.href = window.gRootPath + (result.broker ? 'Broker#login' : 'Customer/Profile');
					return;
				} // if
			} // if

			if (result.errorMessage)
				EzBob.App.trigger('error', result.errorMessage);
			else
				EzBob.App.trigger('error', 'Could not to set account password.');

			UnBlockUi();
		}).fail(function() {
			EzBob.App.trigger('error', 'Failed to set account password.');
			UnBlockUi();
		});

		return false;
	}, // submit

	buildValidator: function () {
		var oPolicy = EzBob.createPasswordValidationPolicy();

		var passPolicy = oPolicy.policy;
		var passPolicyText = oPolicy.text;

		var passPolicy2 = $.extend({}, passPolicy);
		passPolicy2.equalTo = '#Password';

		return this.form.validate({
			rules: {
				Password: $.extend({}, passPolicy),
				signupPass2: passPolicy2,
			},
			messages: {
				Password: { required: passPolicyText, regex: passPolicyText },
				signupPass2: { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
			ignore: ':not(:visible)'
		});
	}, // buildValidator
}); // EzBob.CustomerCreatePasswordView
