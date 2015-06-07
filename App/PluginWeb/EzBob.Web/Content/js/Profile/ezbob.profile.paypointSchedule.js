var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.PaypointLoanScheduleView = Backbone.View.extend({
	initialize: function(options) {
		this.template = _.template($('#paypoint-schedule-template').html());
		this.schedule = options.schedule;
	}, // initialize

	render: function() {
		this.$el.html(this.template({ schedule: this.schedule }));
	}, // render
}); // EzBob.Profile.PaypointLoanScheduleView
