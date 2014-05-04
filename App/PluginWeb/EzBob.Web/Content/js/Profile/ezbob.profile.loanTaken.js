var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.LoanTakenView = Backbone.View.extend({
	initialize: function() {
		this.$el = $('.loan-taken');
	}, // initialize

	render: function() {
		EzBob.UiAction.registerView(this);
	}, // render
}); // EzBob.LoanTakenView
