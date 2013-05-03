(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.PayPointCardSelectView = (function(_super) {

    __extends(PayPointCardSelectView, _super);

    function PayPointCardSelectView() {
      return PayPointCardSelectView.__super__.constructor.apply(this, arguments);
    }

    PayPointCardSelectView.prototype.template = '#PayPointCardSelectViewTemplate';

    PayPointCardSelectView.prototype.events = {
      'change input[name="cardOptions"]': 'optionsChanged',
      'click .btn-continue': 'continue'
    };

    PayPointCardSelectView.prototype.optionsChanged = function() {
      return this.onRender();
    };

    PayPointCardSelectView.prototype.onRender = function() {
      var select, val;
      val = this.getCardType();
      select = this.$el.find('select');
      if (val === 'useExisting') {
        return select.removeAttr('readonly disabled');
      } else {
        return select.attr({
          'readonly': 'readonly',
          'disabled': 'disabled'
        });
      }
    };

    PayPointCardSelectView.prototype.getCardType = function() {
      return this.$el.find('input[name="cardOptions"]:checked').val();
    };

    PayPointCardSelectView.prototype["continue"] = function() {
      var val;
      val = this.getCardType();
      if (val === 'useExisting') {
        this.trigger('existing');
      } else {
        this.trigger('select', this.$el.find('option:selected').val());
      }
      this.close();
      return false;
    };

    return PayPointCardSelectView;

  })(Backbone.Marionette.ItemView);

}).call(this);
