var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SettingsPasswordView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#settings-password-template').html());
	}, // initialize

	render: function() {
		this.$el.html(this.template({ settings: this.model.toJSON() }));

		this.form = this.$el.find('#change-password');

		this.validator = EzBob.validateChangePassword(this.form);

		this.$el.find('.submit').toggleClass('disabled', true);

		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	events: {
		'click .back': 'back',
		'click .submit': 'submit',
		'change input[type="password"]': 'changed',
		'keyup input[type="password"]': 'changed'
	}, // events

	back: function() {
		this.clear();
		this.trigger('back');
		return false;
	}, // back

	submit: function() {
		var that = this;

		if (!this.validator.form())
			return false;

		BlockUi();

		var oRequest = $.post(window.gRootPath + "Customer/AccountSettings/ChangePassword", {
			oldPassword: this.$el.find('input[name="password"]').val(),
			newPassword: this.$el.find('input[name="new_password"]').val(),
		});
		
		oRequest.done(function(result) {
			if (result.success) {
				EzBob.App.trigger('info', 'Password has been changed.');
				that.back();
			}
			else
				EzBob.App.trigger('error', 'Error occurred while changing password: ' + (result.error || 'invalid old password.'));
		});

		oRequest.always(function() {
			UnBlockUi();
		});

		return false;
	}, // submit

	changed: function() {
		var enabled = this.validator.form();
		this.$el.find('.submit').toggleClass('disabled', !enabled);
	}, // changed

	clear: function() {
		this.$el.find('input[name="password"]').val('');
		this.$el.find('input[name="new_password"]').val('');
		this.$el.find('input[name="new_password2"]').val('');
	}, // clear
}); // EzBob.Profile.SettingsPasswordView
