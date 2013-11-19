(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsAutomationModel = (function(_super) {

    __extends(SettingsAutomationModel, _super);

    function SettingsAutomationModel() {
      return SettingsAutomationModel.__super__.constructor.apply(this, arguments);
    }

    SettingsAutomationModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/AutomationGeneral";

    return SettingsAutomationModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsAutomationView = (function(_super) {

    __extends(SettingsAutomationView, _super);

    function SettingsAutomationView() {
      return SettingsAutomationView.__super__.constructor.apply(this, arguments);
    }

    SettingsAutomationView.prototype.template = "#automation-settings-template";

    SettingsAutomationView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsAutomationView.prototype.events = {
      "click button[name='SaveAutomationSettings']": "saveSettings",
      "click button[name='CancelAutomationSettings']": "cancelSettings"
    };

    SettingsAutomationView.prototype.saveSettings = function() {
      BlockUi("on");
      this.model.save().done(function() {
        return EzBob.ShowMessage("Saved successfully", "Successful");
      });
      this.model.save().complete(function() {
        return BlockUi("off");
      });
      this.model.save();
      return false;
    };

    SettingsAutomationView.prototype.update = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    SettingsAutomationView.prototype.cancelSettings = function() {
      this.update();
      return false;
    };

    SettingsAutomationView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      if (!$("body").hasClass("role-manager")) {
        return this.$el.find("button[name='SaveAutomationSettings'], button[name='CancelAutomationSettings']").hide();
      }
    };

    SettingsAutomationView.prototype.show = function(type) {
      return this.$el.show();
    };

    SettingsAutomationView.prototype.hide = function() {
      return this.$el.hide();
    };

    SettingsAutomationView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return SettingsAutomationView;

  })(Backbone.Marionette.ItemView);

}).call(this);
