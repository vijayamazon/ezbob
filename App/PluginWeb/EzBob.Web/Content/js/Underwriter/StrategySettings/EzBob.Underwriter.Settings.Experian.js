(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

  EzBob.Underwriter.Settings.ExperianModel = (function(_super) {

    __extends(ExperianModel, _super);

    function ExperianModel() {
      return ExperianModel.__super__.constructor.apply(this, arguments);
    }

    ExperianModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsExperian";

    return ExperianModel;

  })(Backbone.Model);

  EzBob.Underwriter.Settings.ExperianView = (function(_super) {

    __extends(ExperianView, _super);

    function ExperianView() {
      return ExperianView.__super__.constructor.apply(this, arguments);
    }

    ExperianView.prototype.template = "#experian-settings-template";

    ExperianView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    ExperianView.prototype.bindings = {
      FinancialAccounts_MainApplicant: "[name='FinancialAccounts_MainApplicant']",
      FinancialAccounts_AliasOfMainApplicant: "[name='FinancialAccounts_AliasOfMainApplicant']",
      FinancialAccounts_AssociationOfMainApplicant: "[name='FinancialAccounts_AssociationOfMainApplicant']",
      FinancialAccounts_JointApplicant: "[name='FinancialAccounts_JointApplicant']",
      FinancialAccounts_AliasOfJointApplicant: "[name='FinancialAccounts_AliasOfJointApplicant']",
      FinancialAccounts_AssociationOfJointApplicant: "[name='FinancialAccounts_AssociationOfJointApplicant']",
      FinancialAccounts_No_Match: "[name='FinancialAccounts_No_Match']"
    };

    ExperianView.prototype.events = {
      "click #SaveExperianSettings": "saveSettings",
      "click #CancelExperianSettings": "cancelSettings"
    };

    ExperianView.prototype.saveSettings = function() {
      BlockUi("on");
      this.model.save().done(function() {
        return EzBob.ShowMessage("Saved successfully", "Successful");
      });
      this.model.save().complete(function() {
        return BlockUi("off");
      });
      return false;
    };

    ExperianView.prototype.cancelSettings = function() {
      this.update();
      return false;
    };

    ExperianView.prototype.update = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    ExperianView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      if (!$("body").hasClass("role-manager")) {
        this.$el.find("select").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        return this.$el.find("button").hide();
      }
    };

    ExperianView.prototype.show = function(type) {
      return this.$el.show();
    };

    ExperianView.prototype.hide = function() {
      return this.$el.hide();
    };

    ExperianView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return ExperianView;

  })(Backbone.Marionette.ItemView);

}).call(this);
