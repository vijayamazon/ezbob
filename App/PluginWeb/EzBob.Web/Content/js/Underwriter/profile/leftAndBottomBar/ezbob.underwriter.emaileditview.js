var EzBob = EzBob || {};

EzBob.EmailEditView = Backbone.Marionette.ItemView.extend({
	template: "#email-edit-template",

	events: {
		'click .email-confirm-manually': 'confirmManually',
		'click .email-send-new-request': 'sendNewRequest',
		'click .email-change-address': 'changeEmail',
		'keypress input[name="edit-email"]': 'editEmailEnterPressed'
	}, // events

	ui: {
		'email': 'input[name="edit-email"]'
	}, // ui

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: "Edit Email",
			position: "center",
			draggable: false,
			width: 530,
			dialogClass: "edit-email-popup"
		};
	}, // jqoptions

	confirmManually: function() {
		var xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/ManuallyConfirm", {
			id: this.model.id
		});

		var self = this;

		xhr.success(function() {
			self.model.fetch();
			self.close();
		});

		return false;
	}, // confirmManually

	sendNewRequest: function() {
		var xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/Resend", {
			id: this.model.id
		});

		var self = this;

		xhr.success(function() {
			self.model.fetch();
			self.close();
		});

		return false;
	}, // sendNewRequest

	changeEmail: function() {
		if (!this.validator.form())
			return false;

		var xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/ChangeEmail", {
			id: this.model.id,
			email: this.ui.email.val()
		});

		var self = this;

		xhr.success(function(response) {
			if (!response.success) {
				if (response.error)
					EzBob.ShowMessage(response.error);
			} // if

			self.model.fetch();
			self.close();
		});

		return false;
	}, // changeEmail

	onRender: function() {
		this.form = this.$el.find('#email-edit-form');
		this.validator = EzBob.validateChangeEmailForm(this.form);
		return this;
	}, // onRender

	editEmailEnterPressed: function(e) {
		if (e.keyCode === 13)
			return false;
	}, // editEmailEnterPressed
}); // EzBob.EmailEditView
