EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.BaseView = Backbone.View.extend({
	initialize: function() {
		this.router = this.options.router;
	}, // initialize

	clear: function() {}, // clear

	onFocus: function() {}, // onFocus

	isSomethingEnabled: function(oSomething) {
		var oElm = (typeof (oSomething) === 'string') ? this.$el.find(oSomething) : oSomething;

		if (oElm.hasClass('disabled') || oElm.attr('disabled') || oElm.prop('disabled'))
			return false;

		return true;
	}, // isSomethingEnabled

	setSomethingEnabled: function(oSomething, bEnabled) {
		var oElm = (typeof (oSomething) === 'string') ? this.$el.find(oSomething) : oSomething;

		if (bEnabled)
			oElm.removeClass('disabled').removeAttr('disabled').removeProp('disabled');
		else
			oElm.addClass('disabled').attr('disabled', 'disabled').prop('disabled', 'disabled');

		return oElm;
	}, // setSomethingEnabled

	adjustValidatorCfg: function(oCfg) {}, // adjustValidatorCfg
}); // EzBob.Broker.BaseView