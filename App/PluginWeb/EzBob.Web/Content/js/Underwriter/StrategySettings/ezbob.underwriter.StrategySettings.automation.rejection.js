(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsRejectionModel = (function(_super) {

    __extends(SettingsRejectionModel, _super);

    function SettingsRejectionModel() {
      return SettingsRejectionModel.__super__.constructor.apply(this, arguments);
    }

    SettingsRejectionModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/AutomationRejection";

    return SettingsRejectionModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsRejectionView = (function(_super) {

    __extends(SettingsRejectionView, _super);

    function SettingsRejectionView() {
      return SettingsRejectionView.__super__.constructor.apply(this, arguments);
    }

    SettingsRejectionView.prototype.template = "#rejection-settings-template";

    SettingsRejectionView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsRejectionView.prototype.bindings = {
      EnableAutomaticRejection: "select[name='EnableAutomaticRejection']",
      LowCreditScore: "input[name='LowCreditScore']",
      TotalAnnualTurnover: "input[name='TotalAnnualTurnover']",
      TotalThreeMonthTurnover: "input[name='TotalThreeMonthTurnover']",
      Reject_Defaults_CreditScore: "input[name='Reject_Defaults_CreditScore']",
      Reject_Defaults_AccountsNum: "input[name='Reject_Defaults_AccountsNum']",
      Reject_Defaults_Amount: "input[name='Reject_Defaults_Amount']",
      Reject_Defaults_MonthsNum: "input[name='Reject_Defaults_MonthsNum']",
      Reject_Minimal_Seniority: "input[name='Reject_Minimal_Seniority']",
      EnableAutomaticReRejection: "select[name='EnableAutomaticReRejection']",
      AutoRejectionException_CreditScore: "input[name='AutoRejectionException_CreditScore']",
      AutoRejectionException_AnualTurnover: "input[name='AutoRejectionException_AnualTurnover']",
      Reject_LowOfflineAnnualRevenue: "input[name='Reject_LowOfflineAnnualRevenue']",
      Reject_LowOfflineQuarterRevenue: "input[name='Reject_LowOfflineQuarterRevenue']",
      Reject_LateLastMonthsNum: "input[name='Reject_LateLastMonthsNum']",
      Reject_NumOfLateAccounts: "input[name='Reject_NumOfLateAccounts']"
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
        this.$el.find("select, input").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button").hide();
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
          LowCreditScore: {
            required: true,
            min: 0
          },
          TotalAnnualTurnover: {
            required: true,
            min: 0
          },
          TotalThreeMonthTurnover: {
            required: true,
            min: 0
          },
          Reject_Defaults_CreditScore: {
            required: true,
            min: 0
          },
          Reject_Defaults_AccountsNum: {
            required: true,
            min: 0
          },
          Reject_Defaults_Amount: {
            required: true,
            min: 0
          },
          Reject_Defaults_MonthsNum: {
            required: true,
            min: 0
          },
          Reject_Minimal_Seniority: {
            required: true,
            min: 0
          },
          AutoRejectionException_CreditScore: {
            required: true,
            min: 0
          },
          AutoRejectionException_AnualTurnover: {
            required: true,
            min: 0
          },
          Reject_LowOfflineAnnualRevenue: {
            required: true,
            min: 0
          },
          Reject_LowOfflineQuarterRevenue: {
            required: true,
            min: 0
          },
          Reject_LateLastMonthsNum: {
            required: true,
            min: 0
          },
          Reject_NumOfLateAccounts: {
            required: true,
            min: 0
          }
        }
      });
    };

    return SettingsRejectionView;

  })(Backbone.Marionette.ItemView);

}).call(this);
