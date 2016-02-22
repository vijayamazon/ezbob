var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PricingModelCalculationsModel = Backbone.Model.extend({
	idAttribute: "Id",
	urlRoot: window.gRootPath + "Underwriter/PricingModelCalculations/Index/"
});

EzBob.Underwriter.PricingModelScenarios = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/PricingModelCalculations/GetScenarios/"
});

EzBob.Underwriter.PricingModelCalculationsView = Backbone.Marionette.ItemView.extend({
	template: '#pricing-model-calculation-template',

	initialize: function () {
		this.scenarios = new EzBob.Underwriter.PricingModelScenarios();
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on('reset fetch sync', this.makeInitialCalculation, this);
		this.inputsExpanded = false;
		this.outputsExpanded = false;
	},

	bindings: {
		LoanAmount: {
			selector: '#loanAmount',
			converter: EzBob.BindingConverters.moneyFormat
		},
		SetupFeePounds: {
			selector: '#setupFeePounds',
			converter: EzBob.BindingConverters.moneyFormat
		},
		BrokerSetupFeePounds: {
			selector: '#brokerSetupFeePounds',
			converter: EzBob.BindingConverters.moneyFormat
		},
		Cogs: {
			selector: '#cogs',
			converter: EzBob.BindingConverters.moneyFormat
		},
		OpexAndCapex: {
			selector: '#opexAndCapex',
			converter: EzBob.BindingConverters.moneyFormat
		},

		ProfitMarkup: {
			selector: '#profitMarkup',
			converter: EzBob.BindingConverters.percentsFormat
		},
		SetupFeePercents: {
			selector: '#setupFeePercents',
			converter: EzBob.BindingConverters.percentsFormat
		},
		BrokerSetupFeePercents: {
			selector: '#brokerSetupFeePercents',
			converter: EzBob.BindingConverters.percentsFormat
		},
		DefaultRateCompanyShare: {
			selector: '#defaultRateCompanyShare',
			converter: EzBob.BindingConverters.percentsFormat
		},
		DefaultRateCustomerShare: {
			selector: '#defaultRateCustomerShare',
			converter: EzBob.BindingConverters.percentsFormat
		},
		TenurePercents: {
			selector: '#tenurePercents',
			converter: EzBob.BindingConverters.percentsFormat
		},
		CollectionRate: {
			selector: '#collectionRate',
			converter: EzBob.BindingConverters.percentsFormat
		},
		DebtPercentOfCapital: {
			selector: '#debtPercentOfCapital',
			converter: EzBob.BindingConverters.percentsFormat
		},
		CostOfDebt: {
			selector: '#costOfDebt',
			converter: EzBob.BindingConverters.percentsFormat
		},

		LoanTerm: {
			selector: '#loanTerm',
			converter: EzBob.BindingConverters.monthsFormatNoDecimals
		},
		InterestOnlyPeriod: {
			selector: '#interestOnlyPeriod',
			converter: EzBob.BindingConverters.monthsFormatNoDecimals
		},
		TenureMonths: {
			selector: '#tenureMonths',
			converter: EzBob.BindingConverters.monthsFormat
		}
	},

	events: {
		'focusout #loanAmount': 'loanAmountChanged',
		'focusout #loanTerm': 'loanTermChanged',
		'focusout #tenurePercents': 'tenurePercentsChanged',
		'focusout #tenureMonths': 'tenureMonthsChanged',
		'focusout #setupFeePounds': 'setupFeePoundsChanged',
		'focusout #setupFeePercents': 'setupFeePercentsChanged',
		'focusout #brokerSetupFeePounds': 'brokerSetupFeePoundsChanged',
		'focusout #brokerSetupFeePercents': 'brokerSetupFeePercentsChanged',
		'focusout #defaultRateCompanyShare': 'defaultRateCompanyShareChanged',
		'focusout #defaultRateCustomerShare': 'defaultRateCustomerShareChanged',
		'click #pricingModelResetButton': 'resetClicked',
		'click #pricingModelCalculateButton': 'calculateClicked',
		'click #expandCollapseInputsButton': 'expandCollapseInputsClicked',
		'change #PricingModelScenario': 'scenarioChanged',
		'click .pricing-model-flow-type': 'flowTypeChanged',
	},

	flowTypeChanged: function() {
		var newFlowType = parseInt(this.$el.find('.pricing-model-flow-type:checked').data('flow-type'), 10);

		this.model.set('FlowType', newFlowType);

		this.$el.find('.default-rate-customer-company').toggleClass('hide', newFlowType !== 1);
	}, // flowTypeChanged

	scenarioChanged: function () {
		this.selectedScenario = this.$el.find('#PricingModelScenario').val();

		var that = this;

		var request = $.post(
			window.gRootPath + 'Underwriter/PricingModelCalculations/GetScenarioConfigs', {
				customerId: this.model.get('Id'),
				scenarioName: this.$el.find('#PricingModelScenario').val()
			}
		);

		request.success(function (res) {
			res.LoanAmount = that.model.get('LoanAmount');
			that.model.set(res);
			that.renderAndRememberExpanded();
			BlockUi('off', that.$el);
			that.calculateClicked();
		});
	},

	loanAmountChanged: function () {
		var setupFeePounds = this.model.get('LoanAmount') * this.model.get('SetupFeePercents');
		this.model.set('SetupFeePounds', setupFeePounds);

		var brokerSetupFeePounds = this.model.get('LoanAmount') * this.model.get('BrokerSetupFeePercents');
		this.model.set('BrokerSetupFeePounds', brokerSetupFeePounds);

		var totalSetupFeePounds = setupFeePounds + brokerSetupFeePounds;
		var totalSetupFeePercents = Math.round((this.model.get('SetupFeePercents') + this.model.get('BrokerSetupFeePercents')) * 10000) / 100;
		this.setTotalSetupFee(totalSetupFeePercents, totalSetupFeePounds);
	},

	loanTermChanged: function () {
		var tenureMonths = this.model.get('LoanTerm') * this.model.get('TenurePercents');
		this.model.set('TenureMonths', tenureMonths);
	},

	getDefaultRateFromServer: function () {
		var that = this;
		var request = $.post(
			window.gRootPath + 'Underwriter/PricingModelCalculations/GetDefaultRate',
			{
				customerId: this.model.get('Id'),
				companyShare: this.model.get('DefaultRateCompanyShare'),
				flowTypeID: this.model.get('FlowType'),
			}
		);

		request.success(function (res) {
			that.model.set('DefaultRate', res);
			that.renderAndRememberExpanded();
			BlockUi('off', that.$el);
		});
	},

	defaultRateCompanyShareChanged: function () {
		var customerShare = 1 - this.model.get('DefaultRateCompanyShare');
		this.model.set('DefaultRateCustomerShare', customerShare);
		this.getDefaultRateFromServer();
	},

	defaultRateCustomerShareChanged: function () {
		var companyShare = 1 - this.model.get('DefaultRateCustomerShare');
		this.model.set('DefaultRateCompanyShare', companyShare);
		this.getDefaultRateFromServer();
	},

	setTotalSetupFee: function (percents, pounds) {
		this.$el.find('#totalSetupFeePercents').text(percents + '%');
		this.$el.find('#totalSetupFeePounds').text('£' + pounds);
	},

	setupFeePoundsChanged: function () {
		var setupFeePercents = 0;
		var loanAmount = this.model.get('LoanAmount');
		if (loanAmount !== 0) {
			setupFeePercents = this.model.get('SetupFeePounds') / loanAmount;
		}
		this.model.set('SetupFeePercents', setupFeePercents);

		var totalSetupFeePercents = Math.round((this.model.get('BrokerSetupFeePercents') + setupFeePercents) * 10000) / 100;
		var totalSetupFeePounds = parseFloat(this.model.get('SetupFeePounds')) + parseFloat(this.model.get('BrokerSetupFeePounds'));
		this.setTotalSetupFee(totalSetupFeePercents, totalSetupFeePounds);
	},

	setupFeePercentsChanged: function () {
		var setupFeePounds = this.model.get('SetupFeePercents') * this.model.get('LoanAmount');
		this.model.set('SetupFeePounds', setupFeePounds);

		var totalSetupFeePercents = Math.round((this.model.get('BrokerSetupFeePercents') + this.model.get('SetupFeePercents')) * 10000) / 100;
		var totalSetupFeePounds = setupFeePounds + this.model.get('BrokerSetupFeePounds');
		this.setTotalSetupFee(totalSetupFeePercents, totalSetupFeePounds);
	},

	brokerSetupFeePoundsChanged: function () {
		var brokerSetupFeePercents = 0;
		var loanAmount = this.model.get('LoanAmount');
		if (loanAmount !== 0)
			brokerSetupFeePercents = this.model.get('BrokerSetupFeePounds') / loanAmount;

		this.model.set('BrokerSetupFeePercents', brokerSetupFeePercents);

		var totalSetupFeePercents = Math.round((this.model.get('SetupFeePercents') + brokerSetupFeePercents) * 10000) / 100;
		var totalSetupFeePounds = parseFloat(this.model.get('SetupFeePounds')) + parseFloat(this.model.get('BrokerSetupFeePounds'));
		this.setTotalSetupFee(totalSetupFeePercents, totalSetupFeePounds);
	},

	brokerSetupFeePercentsChanged: function () {
		var brokerSetupFeePounds = this.model.get('BrokerSetupFeePercents') * this.model.get('LoanAmount');
		this.model.set('BrokerSetupFeePounds', brokerSetupFeePounds);

		var totalSetupFeePercents = Math.round((this.model.get('BrokerSetupFeePercents') + this.model.get('SetupFeePercents')) * 10000) / 100;
		var totalSetupFeePounds = brokerSetupFeePounds + this.model.get('SetupFeePounds');
		this.setTotalSetupFee(totalSetupFeePercents, totalSetupFeePounds);
	},

	tenurePercentsChanged: function () {
		var tenureMonths = this.model.get('TenurePercents') * this.model.get('LoanTerm');
		this.model.set('TenureMonths', tenureMonths);
	},

	tenureMonthsChanged: function () {
		var tenurePercents = 0;
		var loanTerm = this.model.get('LoanTerm');
		if (loanTerm !== 0)
			tenurePercents = this.model.get('TenureMonths') / loanTerm;

		this.model.set('TenurePercents', tenurePercents);
	},

	expandCollapseInputsClicked: function () {
		if (this.inputsExpanded) {
			this.inputsExpanded = false;
			this.$el.find('.minor-input-row').addClass('hide');
			this.$el.find('#expandInputsSign').removeClass('hide');
			this.$el.find('#collapseInputsSign').addClass('hide');
		} else {
			this.inputsExpanded = true;
			this.$el.find('.minor-input-row').removeClass('hide');
			this.$el.find('#expandInputsSign').addClass('hide');
			this.$el.find('#collapseInputsSign').removeClass('hide');
		}
	},

	resetClicked: function () {
		var that = this;
		this.model.fetch({ data: { customerId: this.model.get('Id') } }).done(function () {
			that.renderAndRememberExpanded();
		});
	},

	makeInitialCalculation: function () {
		this.refillScenarioNames();

		if (this.model.get('LoanAmount') > 0 && this.model.get('TenureMonths') > 0)
			this.calculateClicked();
		else
			this.renderAndRememberExpanded();
	}, // makeInitialCalculation 

	calculateClicked: function () {
		if (this.model.get('LoanAmount') <= 0) {
			EzBob.ShowMessage("Loan amount must be positive", "Wrong loan amount");
			return;
		}

		BlockUi('on', this.$el);
		var that = this;
		var request = $.post(
			window.gRootPath + 'Underwriter/PricingModelCalculations/Calculate',
			{
				customerId: this.model.get('Id'),
				pricingModelModel: JSON.stringify(this.model.toJSON())
			}
		);

		request.success(function (res) {
			that.model.set(res);
			that.renderAndRememberExpanded();
			BlockUi('off', that.$el);
		});
	},

	serializeData: function () { return { model: this.model.toJSON(), }; },

	renderAndRememberExpanded: function () {
		this.render();
		if (this.inputsExpanded) {
			this.inputsExpanded = false;
			this.expandCollapseInputsClicked();
		}
		if (this.outputsExpanded) {
			this.outputsExpanded = false;
			this.expandCollapseOutputsClicked();
		}
	},

	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);

		this.$el.find('#interestRate').percentFormat();
		this.$el.find('#defaultRateCompanyShare').percentFormat();
		this.$el.find('#defaultRateCustomerShare').percentFormat();
		this.$el.find('#tenurePercents').percentFormat();
		this.$el.find('#collectionRate').percentFormat();
		this.$el.find('#debtPercentOfCapital').percentFormat();
		this.$el.find('#costOfDebt').percentFormat();
		this.$el.find('#setupFeePercents').percentFormat();
		this.$el.find('#brokerSetupFeePercents').percentFormat();
		this.$el.find('#profitMarkup').percentFormat();

		this.$el.find('#loanAmount').moneyFormat();
		this.$el.find('#setupFeePounds').moneyFormat();
		this.$el.find('#brokerSetupFeePounds').moneyFormat();
		this.$el.find('#cogs').moneyFormat();
		this.$el.find('#opexAndCapex').moneyFormat();

		this.$el.find('#loanTerm').monthFormatNoDecimals();
		this.$el.find('#interestOnlyPeriod').monthFormatNoDecimals();
		this.$el.find('#tenureMonths').monthFormat();

		var setupFeePounds = this.model.get('LoanAmount') * this.model.get('SetupFeePercents');
		var brokerSetupFeePounds = this.model.get('LoanAmount') * this.model.get('BrokerSetupFeePercents');
		var totalSetupFeePounds = setupFeePounds + brokerSetupFeePounds;
		var totalSetupFeePercents = (this.model.get('SetupFeePercents') + this.model.get('BrokerSetupFeePercents')) * 100;
		this.setTotalSetupFee(totalSetupFeePercents, totalSetupFeePounds);

		EzBob.handleUserLayoutSetting();

		this.renderFlowType();

		var self = this;
		this.scenarios.fetch().done(function() { self.refillScenarioNames(); });

		return this;
	}, // onRender

	renderFlowType: function() {
		var currentFlowType = this.model.get('FlowType');

		if (!currentFlowType) {
			this.flowTypeChanged(); // Read value from UI into model.
			currentFlowType = this.model.get('FlowType');
		} // if

		this.$el.find('.pricing-model-flow-type').each(function() {
			var chk = $(this);

			if (parseInt(chk.data('flow-type'), 10) === currentFlowType)
				chk.attr('checked', 'checked');
			else
				chk.removeAttr('checked');
		});

		this.flowTypeChanged(); // Adjust UI with current flow type.

		// this.getDefaultRateFromServer();
	}, // renderFlowType

	refillScenarioNames: function() {
		var originID = this.model.get('OriginID');

		if (!originID)
			return;

		var scenarios = this.scenarios.get('scenarios');

		if (!scenarios)
			return;

		var selectBox = this.$el.find('#PricingModelScenario').empty();

		var self = this;

		_.each(scenarios, function(scenario) {
			if (scenario.OriginID !== originID)
				return;

			var opt = $('<option />');

			selectBox.append(opt);

			opt.attr('value', scenario.ScenarioName);
			opt.text(scenario.ScenarioName);

			if (self.selectedScenario && (scenario.ScenarioName === self.selectedScenario))
				opt.attr('selected', 'selected');
		});
	}, // refillScenarioNames
});


