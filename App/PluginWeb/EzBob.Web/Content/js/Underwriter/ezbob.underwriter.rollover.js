(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.RolloverView = (function(_super) {

    __extends(RolloverView, _super);

    function RolloverView() {
      return RolloverView.__super__.constructor.apply(this, arguments);
    }

    RolloverView.prototype.initialize = function() {
      return this.template = _.template($('#payment-rollover-template').html());
    };

    RolloverView.prototype.events = {
      "click .confirm": "addRollover"
    };

    RolloverView.prototype.render = function() {
      var that;
      that = this;
      this.model.title = "add rollover";
      this.$el.html(this.template({
        model: this.model
      }));
      this.$el.find('.ezDateTime').splittedDateTime();
      this.validate = EzBob.validateRollover(this.$el.find("#rollover-dialog"));
      return this;
    };

    RolloverView.prototype.addRollover = function() {
      if (!this.validate.form()) {
        return false;
      }
      this.trigger("addRollover", this.$el.find('form').serialize());
      return true;
    };

    return RolloverView;

  })(Backbone.View);

}).call(this);
