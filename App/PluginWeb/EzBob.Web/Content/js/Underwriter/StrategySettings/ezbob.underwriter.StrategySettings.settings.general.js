(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsGeneralModel = (function(_super) {

    __extends(SettingsGeneralModel, _super);

    function SettingsGeneralModel() {
      return SettingsGeneralModel.__super__.constructor.apply(this, arguments);
    }

    SettingsGeneralModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsGeneral";

    return SettingsGeneralModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsGeneralView = (function(_super) {

    __extends(SettingsGeneralView, _super);

    function SettingsGeneralView() {
      return SettingsGeneralView.__super__.constructor.apply(this, arguments);
    }

    SettingsGeneralView.prototype.template = "#general-settings-template";

    SettingsGeneralView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsGeneralView.prototype.bindings = {
      BWABusinessCheck: "select[name='bwaBusinessCheck']",
      HmrcSalariesMultiplier: {
        selector: "input[name='HmrcSalariesMultiplier']",
        converter: EzBob.BindingConverters.percentsFormat
      }
    };

    SettingsGeneralView.prototype.events = {
      "click button[name='SaveGeneralSettings']": "saveSettings",
      "click button[name='CancelGeneralSettings']": "cancelSettings"
    };

    SettingsGeneralView.prototype.saveSettings = function() {
      BlockUi("on");
      console.log;
      this.model.save().done(function() {
        return EzBob.ShowMessage("Saved successfully", "Successful");
      });
      this.model.save().complete(function() {
        return BlockUi("off");
      });
      this.model.save();
      return false;
    };

    SettingsGeneralView.prototype.update = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    SettingsGeneralView.prototype.cancelSettings = function() {
      return this.update();
    };

    SettingsGeneralView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      if (!$("body").hasClass("role-manager")) {
        this.$el.find("button[name='SaveGeneralSettings'], button[name='CancelGeneralSettings']").hide();
      }
      return this.$el.find("input[name='HmrcSalariesMultiplier']").percentFormat();
    };

    SettingsGeneralView.prototype.show = function(type) {
      return this.$el.show();
    };

    SettingsGeneralView.prototype.hide = function() {
      return this.$el.hide();
    };

    SettingsGeneralView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return SettingsGeneralView;

  })(Backbone.Marionette.ItemView);

}).call(this);
