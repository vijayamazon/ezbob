﻿var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.AccountActivityView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#account-activity-template').html());
		this.model.on('fetch', this.render, this);
	}, // initialize

	render: function() {
		this.$el.html(this.template({ loans: this.model.get('Loans').toJSON() }));
		EzBob.UiAction.registerView(this);
		return this;
	}, // render
});
