(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.InstallmentEditor = (function(_super) {

    __extends(InstallmentEditor, _super);

    function InstallmentEditor() {
      return InstallmentEditor.__super__.constructor.apply(this, arguments);
    }

    return InstallmentEditor;

  })(Backbone.Marionette.ItemView);

}).call(this);
