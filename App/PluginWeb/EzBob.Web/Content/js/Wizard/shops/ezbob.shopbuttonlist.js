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
      this.listView.model.on("change reset", this.listChanged, this);
      return StoreButtonWithListView.__super__.initialize.call(this, options);
    };

    StoreButtonWithListView.prototype.listChanged = function() {
      if (this.listView.model.length > 0) {
        return this.trigger("ready", this.name);
      }
    };

    StoreButtonWithListView.prototype.render = function() {
      StoreButtonWithListView.__super__.render.call(this);
      this.$el.append(this.listView.render().$el);
      return this;
    };

    StoreButtonWithListView.prototype.update = function() {
      return this.listView.update();
    };

    return StoreButtonWithListView;

  })(EzBob.StoreButtonView);

}).call(this);
