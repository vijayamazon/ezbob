(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.StrategySettingsView = (function(_super) {

    __extends(StrategySettingsView, _super);

    function StrategySettingsView() {
      return StrategySettingsView.__super__.constructor.apply(this, arguments);
    }

    StrategySettingsView.prototype.initialize = function() {
      return this.template = _.template($("#strategy-detail-settings").html());
    };

    StrategySettingsView.prototype.render = function() {
      var charges, experian, general;
      console.clear();
      this.$el.html(this.template());
      general = this.$el.find("#general-settings");
      charges = this.$el.find("#charges-settings");
      experian = this.$el.find("#experian-settings");
      this.generalModel = new EzBob.Underwriter.SettingsGeneralModel();
      this.generalView = new EzBob.Underwriter.SettingsGeneralView({
        el: general,
        model: this.generalModel
      });
      this.chargesModel = new EzBob.Underwriter.SettingsChargesModel();
      this.chargesView = new EzBob.Underwriter.SettingsChargesView({
        el: charges,
        model: this.chargesModel
      });
      this.experianModel = new EzBob.Underwriter.Settings.ExperianModel();
      return this.experianView = new EzBob.Underwriter.Settings.ExperianView({
        el: experian,
        model: this.experianModel
      });
    };

    StrategySettingsView.prototype.show = function(type) {
      return this.$el.show();
    };

    StrategySettingsView.prototype.hide = function() {
      return this.$el.hide();
    };

    StrategySettingsView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return StrategySettingsView;

  })(Backbone.View);

}).call(this);
