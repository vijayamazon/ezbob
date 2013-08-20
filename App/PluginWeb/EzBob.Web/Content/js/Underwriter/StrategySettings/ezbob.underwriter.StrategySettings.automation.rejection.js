(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsRejectionModel = (function(_super) {
    __extends(SettingsRejectionModel, _super);

    function SettingsRejectionModel() {
      _ref = SettingsRejectionModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    SettingsRejectionModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/AutomationRejection";

    return SettingsRejectionModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsRejectionView = (function(_super) {
    __extends(SettingsRejectionView, _super);

    function SettingsRejectionView() {
      _ref1 = SettingsRejectionView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    SettingsRejectionView.prototype.template = "#rejection-settings-template";

    SettingsRejectionView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsRejectionView.prototype.bindings = {
      EnableAutomaticRejection: "select[name='enableAutomaticRejection']",
      LowCreditScore: "input[name='lowCreditScore']",
      TotalAnnualTurnover: "input[name='totalAnnualTurnover']",
      TotalThreeMonthTurnover: "input[name='totalThreeMonthTurnover']",
      Reject_Defaults_CreditScore: "input[name='reject_Defaults_CreditScore']",
      Reject_Defaults_AccountsNum: "input[name='reject_Defaults_AccountsNum']",
      Reject_Defaults_Amount: "input[name='reject_Defaults_Amount']",
      Reject_Defaults_MonthsNum: "input[name='reject_Defaults_MonthsNum']",
      Reject_Minimal_Seniority: "input[name='reject_Minimal_Seniority']",
      EnableAutomaticReRejection: "select[name='enableAutomaticReRejection']",
      AutoRejectionException_CreditScore: "input[name='autoRejectionException_CreditScore']",
      AutoRejectionException_AnualTurnover: "input[name='autoRejectionException_AnualTurnover']"
    };

    SettingsRejectionView.prototype.events = {
      "click button[name='SaveRejectionSettings']": "saveSettings",
      "click button[name='CancelRejectionSettings']": "cancelSettings"
    };

    SettingsRejectionView.prototype.saveSettings = function() {
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

    SettingsRejectionView.prototype.update = function() {
      var xhr,
        _this = this;

      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    SettingsRejectionView.prototype.cancelSettings = function() {
      this.update();
      return false;
    };

    SettingsRejectionView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      if (!$("body").hasClass("role-manager")) {
        this.$el.find(" select[name='enableAutomaticRejection'],                         input[name='lowCreditScore'],                         input[name='totalAnnualTurnover'],                         input[name='totalThreeMonthTurnover'],                         input[name='reject_Defaults_CreditScore'],                         input[name='reject_Defaults_AccountsNum'],                         input[name='reject_Defaults_Amount'],                         input[name='reject_Minimal_Seniority'],                        select[name='enableAutomaticReRejection'],                         input[name='autoRejectionException_CreditScore'],                         input[name='autoRejectionException_AnualTurnover'],                        input[name='reject_Defaults_MonthsNum']").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button[name='SaveRejectionSettings'], button[name='CancelRejectionSettings']").hide();
      }
      return this.setValidator();
    };

    SettingsRejectionView.prototype.show = function(type) {
      return this.$el.show();
    };

    SettingsRejectionView.prototype.hide = function() {
      return this.$el.hide();
    };

    SettingsRejectionView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    SettingsRejectionView.prototype.setValidator = function() {
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
          lowCreditScore: {
            required: true,
            min: 0
          },
          totalAnnualTurnover: {
            required: true,
            min: 0
          },
          totalThreeMonthTurnover: {
            required: true,
            min: 0
          },
          reject_Defaults_CreditScore: {
            required: true,
            min: 0
          },
          reject_Defaults_AccountsNum: {
            required: true,
            min: 0
          },
          reject_Defaults_Amount: {
            required: true,
            min: 0
          },
          reject_Defaults_MonthsNum: {
            required: true,
            min: 0
          },
          reject_Minimal_Seniority: {
            required: true,
            min: 0
          },
          autoRejectionException_CreditScore: {
            required: true,
            min: 0
          },
          autoRejectionException_AnualTurnover: {
            required: true,
            min: 0
          }
        }
      });
    };

    return SettingsRejectionView;

  })(Backbone.Marionette.ItemView);

}).call(this);
