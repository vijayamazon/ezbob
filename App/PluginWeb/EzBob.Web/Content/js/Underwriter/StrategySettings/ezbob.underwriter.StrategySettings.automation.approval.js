(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsApprovalModel = (function(_super) {

    __extends(SettingsApprovalModel, _super);

    function SettingsApprovalModel() {
      return SettingsApprovalModel.__super__.constructor.apply(this, arguments);
    }

    SettingsApprovalModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/AutomationApproval";

    return SettingsApprovalModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsApprovalView = (function(_super) {

    __extends(SettingsApprovalView, _super);

    function SettingsApprovalView() {
      return SettingsApprovalView.__super__.constructor.apply(this, arguments);
    }

    SettingsApprovalView.prototype.template = "#approval-settings-template";

    SettingsApprovalView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsApprovalView.prototype.bindings = {
      EnableAutomaticApproval: "select[name='enableAutomaticApproval']",
      EnableAutomaticReApproval: "select[name='enableAutomaticReApproval']",
      MaxCapHomeOwner: "input[name='maxCapHomeOwner']",
      MaxCapNotHomeOwner: "input[name='maxCapNotHomeOwner']"
    };

    SettingsApprovalView.prototype.events = {
      "click button[name='SaveApprovalSettings']": "saveSettings",
      "click button[name='CancelApprovalSettings']": "cancelSettings"
    };

    SettingsApprovalView.prototype.saveSettings = function() {
      if (!this.validator.form()) {
        return;
      }
      BlockUi("on");
      this.model.save().done(function() {
        return EzBob.ShowMessage("Saved successfully", "Successful");
      });
      this.model.save().complete(function() {
        return BlockUi("off");
      });
      return false;
    };

    SettingsApprovalView.prototype.update = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    SettingsApprovalView.prototype.cancelSettings = function() {
      this.update();
      return false;
    };

    SettingsApprovalView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      if (!$("body").hasClass("role-manager")) {
        this.$el.find(" select[name='enableAutomaticApproval'],                        select[name='enableAutomaticReApproval'],                        input[name='maxCapHomeOwner'],                        input[name='maxCapNotHomeOwner']").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button[name='SaveApprovalSettings'], button[name='CancelApprovalSettings']").hide();
      }
      return this.setValidator();
    };

    SettingsApprovalView.prototype.show = function(type) {
      return this.$el.show();
    };

    SettingsApprovalView.prototype.hide = function() {
      return this.$el.hide();
    };

    SettingsApprovalView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    SettingsApprovalView.prototype.setValidator = function() {
      return this.validator = this.$el.find('form').validate({
        onfocusout: function() {
          return true;
        },
        onkeyup: function() {
          return false;
        },
        onclick: function() {
          return false;
        },
        rules: {
          maxCapHomeOwner: {
            required: true,
            min: 0
          },
          maxCapNotHomeOwner: {
            required: true,
            min: 0
          }
        }
      });
    };

    return SettingsApprovalView;

  })(Backbone.Marionette.ItemView);

}).call(this);
