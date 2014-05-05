var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SettingsMainView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#settings-main-template').html());
	}, // initialize

	render: function() {
		this.$el.html(this.template({ 'settings': this.model.toJSON() }));
		EzBob.UiAction.registerView(this);
		return this;
	}, // render

	events: {
		'click .edit-password': 'editPassword',
		'click .edit-question': 'editQuestion'
	}, // events

	editPassword: function() {
		this.trigger('edit:password');
		return false;
	}, // editPassword

	editQuestion: function() {
		this.trigger('edit:question');
		return false;
	}, // editQuestion
}); // EzBob.Profile.SettingsMainView
