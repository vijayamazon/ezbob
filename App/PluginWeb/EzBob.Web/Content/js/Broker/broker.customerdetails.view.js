EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.CustomerDetailsView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.CustomerDetailsView.__super__.initialize.apply(this, arguments);

		this.CustomerID = this.options.customerid;

		this.$el = $('.section-customer-details');
	}, // initialize

	events: function() {
		var evt = {};

		evt['click .back-to-list'] = 'backToList';

		return evt;
	}, // events

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

		EzBob.App.trigger('clear');

		return this;
	}, // render

	backToList: function() {
		location.assign('#dashboard');
	}, // backToList
}); // EzBob.Broker.SubmitView