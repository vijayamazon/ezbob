EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.BaseView = EzBob.View.extend({
	initialize: function() {
		this.router = this.options.router;
	}, // initialize

	events: function() {
		var evt = {};
		return evt;
	},
	
	clear: function () { }, // clear

	onFocus: function() {}, // onFocus

	adjustValidatorCfg: function(oCfg) {}, // adjustValidatorCfg

	onBlur: function () { EzBob.App.trigger('clear'); }, // onBlur
	
	onRender: function () { }, // onRender
}); // EzBob.Broker.BaseView