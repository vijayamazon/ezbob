var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsPricingModelModel = Backbone.Model.extend({
	url: window.gRootPath + 'Underwriter/StrategySettings/SettingsPricingModel'
});

EzBob.Underwriter.SettingsPricingModelView = Backbone.Marionette.ItemView.extend({
	template: '#pricing-model-settings-template',

	initialize: function() {
		this.scenarios = new EzBob.Underwriter.PricingModelScenarios();
		this.model = new EzBob.Underwriter.SettingsPricingModelModel();
		this.modelBinder = new Backbone.ModelBinder();

		var self = this;

		this.scenarios.fetch().done(function() {
			self.render();
		});
	}, // initialize

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
		},
	}, // bindings

	events: {
		'click button[name="SavePricingModelSettings"]': 'saveSettings',
		'click button[name="CancelPricingModelSettings"]': 'cancelSettings',
		'change #PricingModelScenarioSettings': 'scenarioChanged'
	}, // events

	scenarioChanged: function () {
		this.selectedScenario = parseInt(this.$el.find('#PricingModelScenarioSettings').val(), 10);
		this.reload();
	}, // scenarioChanged

	reload: function() {
		BlockUi();

		var that = this;

		var xhr = $.post('' + window.gRootPath + 'Underwriter/StrategySettings/SettingsPricingModelForScenario', {
			scenarioID: this.selectedScenario
		});

		xhr.done(function (res) {
			console.log('new model', res);
			that.model.set(res);
			that.render();
			UnBlockUi();
		});
	}, // reload

	saveSettings: function () {
		BlockUi('on');

		var xhr = $.post('' + window.gRootPath + 'Underwriter/StrategySettings/SettingsSavePricingModelScenario', {
			scenarioID: this.$el.find('#PricingModelScenarioSettings').val(),
			model: JSON.stringify(this.model.toJSON())
		});

		xhr.done(function () {
			EzBob.ShowMessage('Saved successfully', 'Successful');
		});

		xhr.complete(function () {
			UnBlockUi();
		});

		return false;
	}, // saveSettings

	cancelSettings: function () {
		this.reload();
	}, // cancelSettings

	onRender: function () {
		var firstTime = !!!this.selectedScenario;

		this.refillScenarioNames();

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

		if (!$('body').hasClass('role-manager')) {
			this.$el.find('input').addClass('disabled').attr({
				readonly: 'readonly',
				disabled: 'disabled'
			});

			this.$el.find('button').hide();
		} // if

		if (firstTime)
			this.reload();
	}, // onRender

	refillScenarioNames: function() {
		var namesBox = this.$el.find('#PricingModelScenarioSettings').empty();

		var lst = this.scenarios.get('scenarios');

		var self = this;

		_.each(lst, function(scenario) {
			var opt = $('<option />');

			namesBox.append(opt);

			opt.attr('value', scenario.ScenarioID);
			opt.text(scenario.Origin + ': ' + scenario.ScenarioName);

			if (self.selectedScenario) {
				if (self.selectedScenario === scenario.ScenarioID)
					opt.attr('selected', 'selected');
			} else {
				self.selectedScenario = scenario.ScenarioID;
				opt.attr('selected', 'selected');
			} // if
		});
	}, // refillScenarioNames

	show: function () {
		this.$el.show();
	}, // show

	hide: function () {
		this.$el.hide();
	}, // hide

	onClose: function () {
		this.modelBinder.unbind();
	}, // onClose

	serializeData: function () {
		return { model: this.model.toJSON(), };
	}, // serializeData
}); // EzBob.Underwriter.SettingsPricingModelView
