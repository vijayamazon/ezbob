(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.StrategyAutomationView = (function(_super) {

    __extends(StrategyAutomationView, _super);

    function StrategyAutomationView() {
      return StrategyAutomationView.__super__.constructor.apply(this, arguments);
    }

    StrategyAutomationView.prototype.initialize = function() {
      return this.template = _.template($("#automation-detail-settings").html());
    };

    StrategyAutomationView.prototype.render = function() {
      var approval, automation, rejection;
      this.$el.html(this.template());
      automation = this.$el.find("#automation-settings");
      approval = this.$el.find("#approvals-settings");
      rejection = this.$el.find("#rejections-settings");
      this.automationModel = new EzBob.Underwriter.SettingsAutomationModel();
      this.automationView = new EzBob.Underwriter.SettingsAutomationView({
        el: automation,
        model: this.automationModel
      });
      this.approvalModel = new EzBob.Underwriter.SettingsApprovalModel();
      this.approvalView = new EzBob.Underwriter.SettingsApprovalView({
        el: approval,
        model: this.approvalModel
      });
      this.rejectionModel = new EzBob.Underwriter.SettingsRejectionModel();
      return this.rejectionView = new EzBob.Underwriter.SettingsRejectionView({
        el: rejection,
        model: this.rejectionModel
      });
    };

    StrategyAutomationView.prototype.show = function(type) {
      return this.$el.show();
    };

    StrategyAutomationView.prototype.hide = function() {
      return this.$el.hide();
    };

    StrategyAutomationView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return StrategyAutomationView;

  })(Backbone.View);

}).call(this);
