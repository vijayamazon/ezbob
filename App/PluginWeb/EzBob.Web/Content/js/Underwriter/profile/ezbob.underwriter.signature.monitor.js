var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SignatureMonitorView = Backbone.View.extend({
	initialize: function(options) {
		console.log('el =', options.el, 'model = ', options.model);
	}, // initialize

	reload: function(nCustomerID) {
		console.log('reload for customer', nCustomerID);
	}, // reload
}); // EzBob.Underwriter.SignatureMonitorView
