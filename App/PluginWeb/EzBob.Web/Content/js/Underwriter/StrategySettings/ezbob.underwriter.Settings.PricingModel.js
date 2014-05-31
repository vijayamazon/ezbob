(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SettingsPricingModelModel = (function(_super) {
    __extends(SettingsPricingModelModel, _super);

    function SettingsPricingModelModel() {
      _ref = SettingsPricingModelModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    SettingsPricingModelModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsPricingModel";

    return SettingsPricingModelModel;

  })(Backbone.Model);

  EzBob.Underwriter.SettingsPricingModelView = (function(_super) {
    __extends(SettingsPricingModelView, _super);

    function SettingsPricingModelView() {
      _ref1 = SettingsPricingModelView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    SettingsPricingModelView.prototype.template = "#pricing-model-settings-template";

    SettingsPricingModelView.prototype.initialize = function(options) {
      this.scenarios = options.scenarios;
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsPricingModelView.prototype.bindings = {
      TenurePercents: {
        selector: "input[name='TenurePercents']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      DefaultRateCompanyShare: {
        selector: "input[name='DefaultRateCompanyShare']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      CollectionRate: {
        selector: "input[name='CollectionRate']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      EuCollectionRate: {
        selector: "input[name='EuCollectionRate']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      DebtPercentOfCapital: {
        selector: "input[name='DebtPercentOfCapital']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      CostOfDebt: {
        selector: "input[name='CostOfDebt']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      ProfitMarkup: {
        selector: "input[name='ProfitMarkup']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      SetupFeePercents: {
        selector: "input[name='SetupFeePercents']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      BrokerSetupFeePercents: {
        selector: "input[name='BrokerSetupFeePercents']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      OpexAndCapex: {
        selector: "input[name='OpexAndCapex']"
      },
      Cogs: {
        selector: "input[name='Cogs']"
      },
      InterestOnlyPeriod: {
        selector: "input[name='InterestOnlyPeriod']"
      }
    };

    SettingsPricingModelView.prototype.events = {
      "click button[name='SavePricingModelSettings']": "saveSettings",
      "click button[name='CancelPricingModelSettings']": "cancelSettings",
      "change #PricingModelScenarioSettings": "scenarioChanged"
    };

    SettingsPricingModelView.prototype.scenarioChanged = function() {
      var that, xhr,
        _this = this;

      this.selectedScenario = this.$el.find('#PricingModelScenarioSettings').val();
      BlockUi();
      that = this;
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SettingsPricingModelForScenario", {
        scenarioName: this.selectedScenario
      });
      return xhr.done(function(res) {
        that.model.set(res);
        UnBlockUi();
        return that.render();
      });
    };

    SettingsPricingModelView.prototype.saveSettings = function() {
      var xhr,
        _this = this;

      BlockUi("on");
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SettingsSavePricingModelScenario", {
        scenarioName: this.$el.find('#PricingModelScenarioSettings').val(),
        model: JSON.stringify(this.model.toJSON())
      });
      xhr.done(function() {
        return EzBob.ShowMessage("Saved successfully", "Successful");
      });
      xhr.complete(function() {
        return BlockUi("off");
      });
      return false;
    };

    SettingsPricingModelView.prototype.update = function() {
      var xhr,
        _this = this;

      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    SettingsPricingModelView.prototype.cancelSettings = function() {
      return this.update();
    };

    SettingsPricingModelView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      this.$el.find("input[name='TenurePercents']").percentFormat();
      this.$el.find("input[name='DefaultRateCompanyShare']").percentFormat();
      this.$el.find("input[name='CollectionRate']").percentFormat();
      this.$el.find("input[name='EuCollectionRate']").percentFormat();
      this.$el.find("input[name='DebtPercentOfCapital']").percentFormat();
      this.$el.find("input[name='CostOfDebt']").percentFormat();
      this.$el.find("input[name='ProfitMarkup']").percentFormat();
      this.$el.find("input[name='OpexAndCapex']").numericOnlyWithDecimal();
      this.$el.find("input[name='Cogs']").numericOnlyWithDecimal();
      this.$el.find("input[name='InterestOnlyPeriod']").numericOnly(2);
      if (!$("body").hasClass("role-manager")) {
        this.$el.find("input").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button").hide();
      }
      if (this.selectedScenario) {
        return this.$el.find('#PricingModelScenarioSettings').val(this.selectedScenario);
      }
    };

    SettingsPricingModelView.prototype.show = function(type) {
      return this.$el.show();
    };

    SettingsPricingModelView.prototype.hide = function() {
      return this.$el.hide();
    };

    SettingsPricingModelView.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    SettingsPricingModelView.prototype.serializeData = function() {
      return {
        model: this.model.toJSON(),
        scenarios: this.scenarios.toJSON()
      };
    };

    return SettingsPricingModelView;

  })(Backbone.Marionette.ItemView);

}).call(this);
