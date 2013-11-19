(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsChargesModel = (function(_super) {

    __extends(SettingsChargesModel, _super);

    function SettingsChargesModel() {
      return SettingsChargesModel.__super__.constructor.apply(this, arguments);
    }

    SettingsChargesModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsCharges";

    return SettingsChargesModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsChargesView = (function(_super) {

    __extends(SettingsChargesView, _super);

    function SettingsChargesView() {
      return SettingsChargesView.__super__.constructor.apply(this, arguments);
    }

    SettingsChargesView.prototype.template = "#charges-settings-template";

    SettingsChargesView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsChargesView.prototype.bindings = {
      LatePaymentCharge: "input[name='latePaymentCharge']",
      RolloverCharge: "input[name='rolloverCharge']",
      PartialPaymentCharge: "input[name='partialPaymentCharge']",
      AdministrationCharge: "input[name='administrationCharge']",
      OtherCharge: "input[name='otherCharge']",
      AmountToChargeFrom: "input[name='amountToChargeFrom']"
    };

    SettingsChargesView.prototype.events = {
      "click button[name='SaveChargesSettings']": "saveSettings",
      "click button[name='CancelChargesSettings']": "cancelSettings"
    };

    SettingsChargesView.prototype.saveSettings = function() {
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

    SettingsChargesView.prototype.cancelSettings = function() {
      this.update();
      return false;
    };

    SettingsChargesView.prototype.update = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    SettingsChargesView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      if (!$("body").hasClass("role-manager")) {
        this.$el.find("input").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button[name='SaveChargesSettings'], button[name='CancelChargesSettings']").hide();
      }
      return this.setValidator();
    };

    SettingsChargesView.prototype.show = function(type) {
      return this.$el.show();
    };

    SettingsChargesView.prototype.hide = function() {
      return this.$el.hide();
    };

    SettingsChargesView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    SettingsChargesView.prototype.setValidator = function() {
      return this.validator = this.$el.find('form').validate({
        rules: {
          latePaymentCharge: {
            required: true,
            min: 0
          },
          rolloverCharge: {
            required: true,
            min: 0
          },
          partialPaymentCharge: {
            required: true,
            min: 0
          },
          administrationCharge: {
            required: true,
            min: 0
          },
          otherCharge: {
            required: true,
            min: 0
          },
          amountToChargeFrom: {
            required: true,
            min: 0
          }
        }
      });
    };

    return SettingsChargesView;

  })(Backbone.Marionette.ItemView);

}).call(this);
