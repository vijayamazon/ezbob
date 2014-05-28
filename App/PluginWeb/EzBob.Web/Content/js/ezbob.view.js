var EzBob = EzBob || {};

(function() {
	var checkIsEnabled = function(oSomething) {
		var oElm = (typeof(oSomething) === 'string') ? this.$el.find(oSomething) : oSomething;

		if (oElm.hasClass('disabled') || oElm.attr('disabled') || oElm.prop('disabled'))
			return false;

		return true;
	}; // checkIsEnabled

	var setEnabled = function(oSomething, bEnabled) {
		var oElm = (typeof(oSomething) === 'string') ? this.$el.find(oSomething) : oSomething;

		if (bEnabled)
			return oElm.removeClass('disabled').removeAttr('disabled').removeProp('disabled');
		else
			return oElm.addClass('disabled').attr('disabled', 'disabled').prop('disabled', 'disabled');
	}; // setEnabled

	EzBob.View = Backbone.View.extend({
		isSomethingEnabled: checkIsEnabled,
		setSomethingEnabled: setEnabled,
	}); // EzBob.View

	EzBob.ItemView = Backbone.Marionette.ItemView.extend({
		isSomethingEnabled: checkIsEnabled,
		setSomethingEnabled: setEnabled,
	}); // EzBob.View

	EzBob.SimpleView = function() {};
	EzBob.SimpleView.prototype.setSomethingEnabled = setEnabled;
	EzBob.SimpleView.prototype.isSomethingEnabled = checkIsEnabled;
})(); // scope
