var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsPricingModelModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsPricingModel"
});

EzBob.Underwriter.SettingsPricingModelView = Backbone.Marionette.ItemView.extend({
	template: "#pricing-model-settings-template",
	initialize: function (options) {
		this.scenarios = options.scenarios;
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	bindings: {
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
		CosmeCollectionRate: {
			selector: "input[name='CosmeCollectionRate']",
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
	},
	events: {
		"click button[name='SavePricingModelSettings']": "saveSettings",
		"click button[name='CancelPricingModelSettings']": "cancelSettings",
		"change #PricingModelScenarioSettings": "scenarioChanged"
	},
	scenarioChanged: function () {
		var that, xhr;
		this.selectedScenario = this.$el.find('#PricingModelScenarioSettings').val();
		BlockUi();
		that = this;
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SettingsPricingModelForScenario", {
			scenarioName: this.selectedScenario
		});
		return xhr.done(function (res) {
			that.model.set(res);
			UnBlockUi();
			return that.render();
		});
	},
	saveSettings: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SettingsSavePricingModelScenario", {
			scenarioName: this.$el.find('#PricingModelScenarioSettings').val(),
			model: JSON.stringify(this.model.toJSON())
		});
		xhr.done(function () {
			return EzBob.ShowMessage("Saved successfully", "Successful");
		});
		xhr.complete(function () {
			return BlockUi("off");
		});
		return false;
	},
	update: function () {
		this.model.fetch({ reset: true });
	},
	cancelSettings: function () {
		this.update();
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.$el.find("input[name='TenurePercents']").percentFormat();
		this.$el.find("input[name='DefaultRateCompanyShare']").percentFormat();
		this.$el.find("input[name='CollectionRate']").percentFormat();
		this.$el.find("input[name='EuCollectionRate']").percentFormat();
		this.$el.find("input[name='CosmeCollectionRate']").percentFormat();
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
			this.$el.find('#PricingModelScenarioSettings').val(this.selectedScenario);
		}
	},
	show: function (type) {
		this.$el.show();
	},
	hide: function () {
		this.$el.hide();
	},
	onClose: function () {
		this.modelBinder.unbind();
	},
	serializeData: function () {
		return {
			model: this.model.toJSON(),
			scenarios: this.scenarios.get('scenarios')
		};
	}
});
