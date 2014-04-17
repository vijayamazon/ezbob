// Uses:
// momentjs: 2.0.0, http://momentjs.com
// underscorejs: 1.4.2, http://underscorejs.org

var EzBob = EzBob || {};

(function() {
	EzBob.DateInterval = function(sYmdFrom, sYmdTo) {
		this.start = moment(sYmdFrom, 'YYYY-MM-DD');
		this.end = moment(sYmdTo, 'YYYY-MM-DD');

		if (this.areSameDay(this.start, this.end))
			throw 'Interval start date and end date point to the same day.';

		if (this.start.unix() > this.end.unix()) {
			var x = this.start;
			this.start = this.end;
			this.end = x;
		} // if
	}; // DateInterval constructor

	_.extend(EzBob.DateInterval.prototype, {
		intersects: function(oOtherInterval) {
			return this.contains(oOtherInterval.start) || this.contains(oOtherInterval.end) ||
				oOtherInterval.contains(this.start) || oOtherInterval.contains(this.end);
		}, // intersects

		contains: function(oMoment) {
			return (this.start.unix() <= oMoment.unix()) && (oMoment.unix() <= this.end.unix());
		}, // contains

		areSameDay: function(oMomentA, oMomentB) {
			return (oMomentA.year() === oMomentB.year()) && (oMomentA.dayOfYear() === oMomentB.dayOfYear());
		}, // areSameDay

		isJustBefore: function(oOtherInterval) {
			return this.areSameDay(moment(this.end).add('days', 1), oOtherInterval.start);
		}, // isJustBefore

		compareTo: function(oOtherInterval) {
			return this.start.unix() - oOtherInterval.start.unix();
		}, // compareTo

		toString: function() {
			return this.start.format('YYYY-MM-DD') + '...' + this.end.format('YYYY-MM-DD');
		}, // toString
	}); // DateInterval

	EzBob.DateIntervalSet = function() {
		this.intervals = [];
	}; // DateIntervalSet constructor

	_.extend(EzBob.DateIntervalSet.prototype, {
		add: function(sYmdFrom, sYmdTo) {
			var oInterval = null;

			try {
				oInterval = new EzBob.DateInterval(sYmdFrom, sYmdTo);
			}
			catch (e) {
				console.warn('Could not create an interval from', sYmdFrom, 'and', sYmdTo);
				return false;
			} // try

			var oExisting = _.find(this.intervals, oInterval.intersects, oInterval);

			if (oExisting) {
				console.warn('New interval', oInterval.toString(), 'intersects with existing interval', oExisting.toString());
				return false;
			} // if

			this.intervals.push(oInterval);
			return true;
		}, // add

		isConsequent: function() {
			if (this.intervals.length < 2)
				return true;

			this.intervals.sort(function(a, b) { return a.compareTo(b); });

			for (var i = 0; i < this.intervals.length - 1; i++) {
				if (!this.intervals[i].isJustBefore(this.intervals[i + 1])) {
					console.warn(this.intervals[i].toString(), 'is not followed by', this.intervals[i + 1].toString());
					return false;
				} // if
			} // for

			return true;
		}, // isConsequent
	}); // DateIntervalSet
})();
