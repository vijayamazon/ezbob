(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.BindingConverters = EzBob.BindingConverters || {};

  EzBob.BindingConverters.floatNumbers = function(direction, value) {
    if (value === "" || value === null) {
      value = 0;
    }
    return parseFloat(value);
  };

  EzBob.BindingConverters.percents = function(direction, value) {
    if (value === "" || value === null) {
      value = 0;
    }
    value = parseFloat(value);
    if (direction === 'ModelToView') {
      return Math.round(value * 100 * 100) / 100;
    } else {
      return value / 100;
    }
  };

  EzBob.BindingConverters.notNull = function(direction, value) {
    if (value === "" || value === null) {
      value = 0;
    }
    return value;
  };

  EzBob.BindingConverters.dateTime = function(direction, value) {
    if (direction === 'ModelToView') {
      return moment.utc(value).format('DD/MM/YYYY');
    } else {
      return moment.utc(value, "DD/MM/YYYY").toDate();
    }
  };

  EzBob.BindingConverters.autonumericFormat = function(format) {
    return function(direction, value) {
      if (direction === 'ModelToView') {
        return EzBob.formatPoundsFormat(value, format);
      } else {
        return $.autoNumeric.Strip($("<input/>").val(value), format);
      }
    };
  };

  EzBob.BindingConverters.percentsFormat = function(direction, value) {
    var result;

    if (direction === 'ModelToView') {
      value = EzBob.BindingConverters.percents(direction, value);
      result = EzBob.BindingConverters.autonumericFormat(EzBob.percentFormat)(direction, value);
      return result;
    } else {
      value = EzBob.BindingConverters.autonumericFormat(EzBob.percentFormat)(direction, value);
      result = EzBob.BindingConverters.percents(direction, value);
      return result;
    }
  };

  EzBob.BindingConverters.numericOnlyFormat = function (direction, value) {
      var result;
      debugger;
      if (direction === 'ModelToView') {
          value = EzBob.BindingConverters.floatNumbers(direction, value);
          result = EzBob.BindingConverters.autonumericFormat(EzBob.numericOnlyFormat(2))(direction, value);
          return result;
      } else {
          value = EzBob.BindingConverters.autonumericFormat(EzBob.numericOnlyFormat(2))(direction, value);
          result = EzBob.BindingConverters.floatNumbers(direction, value);
          return result;
      }
  };

  EzBob.BindingConverters.moneyFormat = EzBob.BindingConverters.autonumericFormat(EzBob.moneyFormat);

  Backbone.Collection.prototype.safeFetch = function() {
    if (document.body.getAttribute('auth') === 'auth') {
      return this.fetch();
    } else {
      return {
        done: function() {}
      };
    }
  };

  Backbone.Model.prototype.safeFetch = function() {
    if (document.body.getAttribute('auth') === 'auth') {
      return this.fetch();
    } else {
      return {
        done: function() {}
      };
    }
  };

}).call(this);
