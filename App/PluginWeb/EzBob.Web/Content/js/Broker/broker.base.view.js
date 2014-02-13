EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.BaseView = Backbone.View.extend({
	initialize: function() {
		this.router = this.options.router;
	}, // initialize

	clear: function() {}, // clear

	onFocus: function() {}, // onFocus

	setSomethingEnabled: function(sSelector, bEnabled) {
		var oElm = this.$el.find(sSelector);

		if (bEnabled)
			oElm.removeClass('disabled').removeAttr('disabled').removeProp('disabled');
		else
			oElm.addClass('disabled').attr('disabled', 'disabled').prop('disabled', 'disabled');

		return oElm;
	}, // setSomethingEnabled

	adjustValidatorCfg: function(oCfg) {}, // adjustValidatorCfg
}); // EzBob.Broker.BaseView