var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SettingsQuestionView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#settings-question-template').html());
	}, // initialize

	render: function() {
		this.$el.html(this.template({ settings: this.model.toJSON() }));
		this.form = this.$el.find('#change-question');
		this.validator = EzBob.validateChangeSecurityQuestion(this.form);

		this.$el.find('#securityQuestion').val(this.model.get('SecurityQuestionId')).change().attardi_labels('toggle');
		
		//this.inputKeyuped();
		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	events: {
		'click .back': 'back',
		'click .submit': 'submit',
		'keyup input': 'inputKeyuped',
		'change input': 'inputKeyuped',
		'change select': 'inputKeyuped'
	}, // events

	back: function() {
		this.trigger('back');
		return false;
	}, // back

	submit: function(e) {
		var isDisable = $(e.currentTarget).hasClass("disabled");

		if (isDisable)
			return false;

		if (!this.validator.checkForm())
			return false;

		var questionId = this.$el.find('select option:selected').val();
		var question = this.$el.find('select option:selected').text();
		var answer = this.$el.find('input[name="answer"]').val();
		var password = this.$el.find('input[name="password"]').val();
		var that = this;

		$.post(window.gRootPath + "Customer/AccountSettings/UpdateSecurityQuestion", {
			Question: questionId,
			Answer: answer,
			Password: password
		}).done(function(r) {
			if (!r.success) {
				if (r.error)
					EzBob.App.trigger('error', r.error);
				else
					EzBob.App.trigger('error', 'Failed to update security question.');

				return false;
			} // if

			that.model.set({
				SecurityQuestionId: questionId,
				Answer: answer
			});

			EzBob.App.trigger('info', 'Security question has been updated.');
			that.trigger("SecurityQuestionUpdated", question);
			that.$el.find('input[name="answer"]').val("");
			that.back();
			return false;
		});

		this.$el.find('input[name="password"]').val("");

		return false;
	}, // submit

	inputKeyuped: function() {
		var enabled = this.validator.checkForm();
		this.$el.find('.submit').toggleClass("disabled", !enabled);
	}, // inputKeyuped
});