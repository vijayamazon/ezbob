var EzBob = EzBob || {};

EzBob.CustomerLoginView = Backbone.View.extend({
	initialize: function() {
		return this.template = _.template($('#customerlogin-template').html());
	}, // initialize

	events: {
		'click :submit': 'submit',
		'keyup input': 'inputChanged',
		'change input': 'inputChanged',
		'click.checks #RememberMe': 'rememberMeChanged'
	}, // events

	render: function() {
		this.$el.html(this.template());

		this.form = this.$el.find('.simple-login');

		this.validator = EzBob.validateLoginForm(this.form);

		this.$el.find('img[rel]').setPopover('left');
		this.$el.find('li[rel]').setPopover('left');

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		$('#Password').focus();
		$('#UserName').focus();

		EzBob.UiAction.registerView(this);
		return this;
	}, // render

	rememberMeChanged: function() {
		var rememberMe = this.$el.find('#RememberMe');
		return rememberMe.val(rememberMe.is(':checked'));
	}, // rememberMeChanged

	inputChanged: function() {
		var enabled = EzBob.Validation.checkForm(this.validator);
		return $('#loginSubmit').toggleClass('disabled', !enabled);
	}, // inputChanged

	submit: function() {
		if (this.$el.find(':submit').hasClass('disabled'))
			return false;

		if (!EzBob.Validation.checkForm(this.validator))
			return false;

		this.blockBtn(true);

		if (!EzBob.Validation.checkForm(this.validator)) {
			this.blockBtn(false);
			return false;
		} // if

		var self = this;
		var data = this.form.serialize();
		var xhr = $.post(this.form.attr('action'), data);

		xhr.done(function(result, status) {
			EzBob.ServerLog.debug('login request completed with status', status);

			if (status === 'success') {
				if (result.success) {
					if (result.broker) {
						document.location.href = '' + window.gRootPath + 'Broker#login';
					} else if (result.everlineWizard) {
						document.location.href = '' + window.gRootPath + 'Customer/Wizard?isEverline=true';
					} else if (result.everlineAccount) {
						document.location.href = 'https://accounts.everline.com/login';
					} else {
						document.location.href = '' + window.gRootPath + 'Customer/Profile';
					}
				}
				else {
					EzBob.App.trigger('error', result.errorMessage);
					self.blockBtn(false);
				}
			}
			else {
				if (result.errorMessage)
					EzBob.App.trigger('error', result.errorMessage);

				self.blockBtn(false);
			}
		});

		return false;
	}, // submit

	blockBtn: function(isBlock) {
		BlockUi((isBlock ? 'on' : 'off'));
		this.$el.find(':submit').toggleClass('disabled', isBlock);
	}, // blockBtn
}); // EzBob.CustomerLoginView
