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
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    SettingsPricingModelView.prototype.bindings = {
      PricingModelTenurePercents: {
        selector: "input[name='PricingModelTenurePercents']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelDefaultRateCompanyShare: {
        selector: "input[name='PricingModelDefaultRateCompanyShare']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelCollectionRate: {
        selector: "input[name='PricingModelCollectionRate']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelEuCollectionRate: {
        selector: "input[name='PricingModelEuCollectionRate']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelDebtOutOfTotalCapital: {
        selector: "input[name='PricingModelDebtOutOfTotalCapital']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelCostOfDebtPA: {
        selector: "input[name='PricingModelCostOfDebtPA']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelProfitMarkupPercentsOfRevenue: {
        selector: "input[name='PricingModelProfitMarkupPercentsOfRevenue']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelSetupFee: {
        selector: "input[name='PricingModelSetupFee']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      PricingModelOpexAndCapex: {
        selector: "input[name='PricingModelOpexAndCapex']"
      },
      PricingModelCogs: {
        selector: "input[name='PricingModelCogs']"
      },
      PricingModelInterestOnlyPeriod: {
        selector: "input[name='PricingModelInterestOnlyPeriod']"
      }
    };

    SettingsPricingModelView.prototype.events = {
      "click button[name='SavePricingModelSettings']": "saveSettings",
      "click button[name='CancelPricingModelSettings']": "cancelSettings"
    };

    SettingsPricingModelView.prototype.saveSettings = function() {
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
      this.$el.find("input[name='PricingModelTenurePercents']").percentFormat();
      this.$el.find("input[name='PricingModelDefaultRateCompanyShare']").percentFormat();
      this.$el.find("input[name='PricingModelCollectionRate']").percentFormat();
      this.$el.find("input[name='PricingModelEuCollectionRate']").percentFormat();
      this.$el.find("input[name='PricingModelDebtOutOfTotalCapital']").percentFormat();
      this.$el.find("input[name='PricingModelCostOfDebtPA']").percentFormat();
      this.$el.find("input[name='PricingModelProfitMarkupPercentsOfRevenue']").percentFormat();
      this.$el.find("input[name='PricingModelOpexAndCapex']").numericOnlyWithDecimal();
      this.$el.find("input[name='PricingModelCogs']").numericOnlyWithDecimal();
      return this.$el.find("input[name='PricingModelInterestOnlyPeriod']").numericOnly(2);
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

    return SettingsPricingModelView;

  })(Backbone.Marionette.ItemView);

}).call(this);
