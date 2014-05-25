var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SettingsQuestionView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#settings-question-template').html());
	}, // initialize

	render: function() {
		this.$el.html(this.template({ settings: this.model.toJSON() }));

		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	events: {
		'click .back': 'back',
		'click .submit': 'submit',
		'keyup input': 'inputKeyuped'
	}, // events

	back: function() {
		this.trigger('back');
		return false;
	}, // back

	submit: function(e) {
		var isDisable = $(e.currentTarget).hasClass("disabled");

		if (isDisable)
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
		var $inputs = this.$el.find('input');
		var isEmpty = false;
		var $submitButton = this.$el.find('.submit');

		_.each($inputs, function(value) {
			isEmpty = $(value).val().toString().isNullOrEmpty();

			if (isEmpty)
				return;
		});

		$submitButton.toggleClass("disabled", isEmpty);
	}, // inputKeyuped
});