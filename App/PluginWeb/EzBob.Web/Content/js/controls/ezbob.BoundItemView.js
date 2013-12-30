(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.BoundItemView = (function(_super) {

    __extends(BoundItemView, _super);

    function BoundItemView() {
      return BoundItemView.__super__.constructor.apply(this, arguments);
    }

    BoundItemView.prototype.events = {};

    BoundItemView.prototype.initialize = function() {
      this.events['click .btn-primary'] = 'save';
      this.modelBinder = new Backbone.ModelBinder();
      return BoundItemView.__super__.initialize.call(this);
    };

    BoundItemView.prototype.onRender = function() {
      return this.modelBinder.bind(this.model, this.el, this.bindings);
    };

    BoundItemView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Bug Reporter",
        position: "center",
        draggable: false,
        dialogClass: "bugs-popup",
        width: 500
      };
    };

    BoundItemView.prototype.save = function() {
      this.trigger('save');
      if (this.onSave != null) {
        return this.onSave();
      }
    };

    BoundItemView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return BoundItemView;

  })(Backbone.Marionette.ItemView);

}).call(this);
