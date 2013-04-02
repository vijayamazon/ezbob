(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.BidingConverters = EzBob.BidingConverters || {};

  EzBob.BidingConverters.floatNumbers = function(direction, value) {
    return parseFloat(value);
  };

  EzBob.BidingConverters.dateTime = function(direction, value) {
    if (direction === 'ModelToView') {
      return moment.utc(value).format('DD/MM/YYYY');
    } else {
      return moment.utc(value, "DD/MM/YYYY").toDate();
    }
  };

}).call(this);
