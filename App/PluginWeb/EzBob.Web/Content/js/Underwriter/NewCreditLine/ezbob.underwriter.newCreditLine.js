(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  $('body').on('click', 'a[name="newCreditLineLnk"]', function(e) {
    return $('#newCreditLineButtonId').click();
  });

}).call(this);
