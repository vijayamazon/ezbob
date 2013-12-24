(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.PerksView = (function(_super) {

    __extends(PerksView, _super);

    function PerksView() {
      return PerksView.__super__.constructor.apply(this, arguments);
    }

    PerksView.prototype.template = "#perks-template";

    return PerksView;

  })(Backbone.Marionette.Layout);

}).call(this);
