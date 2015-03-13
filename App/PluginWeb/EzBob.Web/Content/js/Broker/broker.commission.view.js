EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.CommissionView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		//EzBob.Broker.InstantOfferView.__super__.initialize.apply(this, arguments);
	    this.$el = $('#commision-view');
	}, // initialize

	onRender: function () {

	}, //onRender

	clear: function () {

	}, // clear

	setAuthOnRender: function() {
		return false;
	}, // setAuthOnRender
	
}); // EzBob.Broker.CommissionView
