(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.GA = (function() {

    function GA() {}

    GA.prototype.trackPage = function(url) {
      return _gaq.push(['_trackPageview', url]);
    };

    GA.prototype.trackEvent = function(category, action, opt_label, opt_value, opt_noninteraction) {
      return _gaq.push(['_trackEvent', category, action, opt_label, opt_value, opt_noninteraction]);
    };

    GA.prototype.trackEventReditect = function(url, category, action, opt_label, opt_value, opt_noninteraction) {
      _gaq.push(['_trackEvent', category, action, opt_label, opt_value, opt_noninteraction]);
      return setTimeout((function() {
        return $.post(url);
      }), 500);
    };

    return GA;

  })();

  EzBob.GATest = (function() {

    function GATest() {}

    GATest.prototype.trackPage = function(url) {
      return console.log('Track PageView: %s', url);
    };

    GATest.prototype.trackEvent = function(category, action, opt_label, opt_value, opt_noninteraction) {
      return console.log("Track Event: " + category + ":" + action + ":" + opt_label);
    };

    GATest.prototype.trackEventReditect = function(url, category, action, opt_label, opt_value, opt_noninteraction) {
      console.log("Track Event: " + category + ":" + action + ":" + opt_label + " Url: %s", url);
      return setTimeout((function() {
        return window.location = url;
      }), 500);
    };

    return GATest;

  })();

}).call(this);
