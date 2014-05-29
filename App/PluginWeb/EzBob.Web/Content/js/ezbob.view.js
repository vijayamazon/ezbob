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

	EzBob.View = Backbone.View.extend({
		isSomethingEnabled: checkIsEnabled,
		setSomethingEnabled: setEnabled,
		areSameDay: areSameDay,
		isJustBefore: isJustBefore,
	}); // EzBob.View

	EzBob.MarionetteView = Backbone.Marionette.View.extend({
		isSomethingEnabled: checkIsEnabled,
		setSomethingEnabled: setEnabled,
		areSameDay: areSameDay,
		isJustBefore: isJustBefore,
	}); // EzBob.MarionetteView

	EzBob.ItemView = Backbone.Marionette.ItemView.extend({
		isSomethingEnabled: checkIsEnabled,
		setSomethingEnabled: setEnabled,
		areSameDay: areSameDay,
		isJustBefore: isJustBefore,
	}); // EzBob.ItemView

	EzBob.SimpleView = function() {};
	EzBob.SimpleView.prototype.setSomethingEnabled = setEnabled;
	EzBob.SimpleView.prototype.isSomethingEnabled = checkIsEnabled;
	EzBob.SimpleView.prototype.isJustBefore = isJustBefore;
	EzBob.SimpleView.prototype.areSameDay = areSameDay;
})(); // scope
