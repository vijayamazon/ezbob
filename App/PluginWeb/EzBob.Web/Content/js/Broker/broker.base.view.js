EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.BaseView = EzBob.View.extend({
	initialize: function() {
		this.router = this.options.router;
	}, // initialize

	clear: function() {}, // clear

	onFocus: function() {}, // onFocus

	adjustValidatorCfg: function(oCfg) {}, // adjustValidatorCfg
}); // EzBob.Broker.BaseView