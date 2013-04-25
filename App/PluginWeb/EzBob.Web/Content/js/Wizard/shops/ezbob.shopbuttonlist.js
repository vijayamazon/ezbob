(function() {
  var _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  EzBob.StoreButtonWithListView = (function(_super) {
    __extends(StoreButtonWithListView, _super);

    function StoreButtonWithListView() {
      _ref = StoreButtonWithListView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StoreButtonWithListView.prototype.initialize = function(options) {
      return StoreButtonWithListView.__super__.initialize.call(this, options);
    };

    StoreButtonWithListView.prototype.render = function() {
      StoreButtonWithListView.__super__.render.call(this);
      $('.tooltipdiv').tooltip();
      return this;
    };

    return StoreButtonWithListView;

  })(EzBob.StoreButtonView);

}).call(this);
