(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.GA = (function() {

    function GA() {}

    GA.prototype.trackPage = function(url) {
      return _gaq.push(['_trackPageview', url]);
    };

    return GA;

  })();

  EzBob.GATest = (function() {

    function GATest() {}

    GATest.prototype.trackPage = function(url) {
      return console.log('Track PageView: %s', url);
    };

    return GATest;

  })();

}).call(this);
