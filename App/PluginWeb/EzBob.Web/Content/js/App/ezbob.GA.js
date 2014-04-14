(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.GA = (function() {

    function GA() {}

    GA.prototype.trackPage = function(url) {
      if ((typeof _gaq !== "undefined" && _gaq !== null)) {
        return _gaq.push(['_trackPageview', url]);
      } else {
        return console.log('Track PageView: %s', url);
      }
    };

    return GA;

  })();

}).call(this);
