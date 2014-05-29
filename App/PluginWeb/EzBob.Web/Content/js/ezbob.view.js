var EzBob = EzBob || {};

(function() {
	var areSameDay = function(oMomentA, oMomentB) {
		return (oMomentA.year() === oMomentB.year()) && (oMomentA.dayOfYear() === oMomentB.dayOfYear());
	}; // areSameDay

	var isJustBefore = function(oPrevious, oNext) {
		return this.areSameDay(moment(oPrevious).add('days', 1), moment(oNext));
	}; // isJustBefore

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

	var oExtObj = {
		isSomethingEnabled: checkIsEnabled,
		setSomethingEnabled: setEnabled,
		areSameDay: areSameDay,
		isJustBefore: isJustBefore,
	};

	EzBob.View = Backbone.View.extend(oExtObj);

	EzBob.MarionetteView = Backbone.Marionette.View.extend(oExtObj);

	EzBob.ItemView = Backbone.Marionette.ItemView.extend(oExtObj);

	EzBob.SimpleView = function() {};
	_.extend(EzBob.SimpleView.prototype, oExtObj);
})(); // scope
