var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PricingModelCalculationsModel = Backbone.Model.extend({
    url: window.gRootPath + "Underwriter/PricingModelCalculations/Index/"
});

EzBob.Underwriter.PricingModelCalculationsView = Backbone.Marionette.ItemView.extend({
    template: "#pricing-model-calculation-template",
    
    initialize: function () {
        this.modelBinder = new Backbone.ModelBinder();
        this.model.on('reset fetch sync', this.render, this);
        this.inputsExpanded = false;
        this.outputsExpanded = false;
    },
    
    bindings: {
        LoanAmount: {
            selector: "#loanAmount",
            converter: EzBob.BindingConverters.moneyFormat
        },
        SetupFeePounds: {
            selector: "#setupFeePounds",
            converter: EzBob.BindingConverters.moneyFormat
        },
        Cogs: {
            selector: "#cogs",
            converter: EzBob.BindingConverters.moneyFormat
        },
        OpexAndCapex: {
            selector: "#opexAndCapex",
            converter: EzBob.BindingConverters.moneyFormat
        },
        
        ProfitMarkup: {
            selector: "#profitMarkup",
            converter: EzBob.BindingConverters.percentsFormat
        },
        SetupFeePercents: {
            selector: "#setupFeePercents",
            converter: EzBob.BindingConverters.percentsFormat
        },
        DefaultRateCompanyShare: {
            selector: "#defaultRateCompanyShare",
            converter: EzBob.BindingConverters.percentsFormat
        },
        DefaultRateCustomerShare: {
            selector: "#defaultRateCustomerShare",
            converter: EzBob.BindingConverters.percentsFormat
        },
        TenurePercents: {
            selector: "#tenurePercents",
            converter: EzBob.BindingConverters.percentsFormat
        },
        CollectionRate: {
            selector: "#collectionRate",
            converter: EzBob.BindingConverters.percentsFormat
        },
        DebtPercentOfCapital: {
            selector: "#debtPercentOfCapital",
            converter: EzBob.BindingConverters.percentsFormat
        },
        CostOfDebt: {
            selector: "#costOfDebt",
            converter: EzBob.BindingConverters.percentsFormat
        },

        LoanTerm: {
            selector: "#loanTerm",
            converter: EzBob.BindingConverters.monthsFormatNoDecimals
        },
        InterestOnlyPeriod: {
            selector: "#interestOnlyPeriod",
            converter: EzBob.BindingConverters.monthsFormatNoDecimals
        },
        TenureMonths: {
            selector: "#tenureMonths",
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
        'focusout #defaultRateCompanyShare': 'defaultRateCompanyShareChanged',
        'focusout #defaultRateCustomerShare': 'defaultRateCustomerShareChanged',
        'click #pricingModelResetButton': 'resetClicked',
        'click #pricingModelCalculateButton': 'calculateClicked',
        'click #expandCollapseInputsButton': 'expandCollapseInputsClicked',
        'click #expandCollapseOutputsButton': 'expandCollapseOutputsClicked'
    },

    loanAmountChanged: function () {
        var setupFeePounds = this.model.get('LoanAmount') * this.model.get('SetupFeePercents');
        this.model.set('SetupFeePounds', setupFeePounds);
    },

    loanTermChanged: function () {
        var tenureMonths = this.model.get('LoanTerm') * this.model.get('TenurePercents');
        this.model.set('TenureMonths', tenureMonths);
    },
    
    getDefaultRateFromServer: function() {
        var that = this;
        var request = $.post(
			window.gRootPath + 'Underwriter/PricingModelCalculations/GetDefaultRate',
			{
			    customerId: this.model.get('Id'),
			    companyShare: this.model.get('DefaultRateCompanyShare')
			}
		);

        request.success(function (res) {
            that.model.set('DefaultRate', res);
            that.renderAndRememberExpanded();
            UnBlockUi();
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

    setupFeePoundsChanged: function () {
        var setupFeePercents = 0;
        var loanAmount = this.model.get('LoanAmount');
        if (loanAmount != 0) {
            setupFeePercents = this.model.get('SetupFeePounds') / loanAmount;
        }
        this.model.set('SetupFeePercents', setupFeePercents);
    },

    setupFeePercentsChanged: function () {
        var setupFeePounds = this.model.get('SetupFeePercents') * this.model.get('LoanAmount');
        this.model.set('SetupFeePounds', setupFeePounds);
    },

    tenurePercentsChanged: function () {
        var tenureMonths = this.model.get('TenurePercents') * this.model.get('LoanTerm');
        this.model.set('TenureMonths', tenureMonths);
    },

    tenureMonthsChanged: function () {
        var tenurePercents = 0;
        var loanTerm = this.model.get('LoanTerm');
        if (loanTerm != 0) {
            tenurePercents = this.model.get('TenureMonths') / loanTerm;
        }
        this.model.set('TenurePercents', tenurePercents);
    },
    
    expandCollapseOutputsClicked: function () {
        if (this.outputsExpanded) {
            this.outputsExpanded = false;
            this.$el.find('.minor-output-row').addClass('hide');
            this.$el.find('#expandOutputsSign').removeClass('hide');
            this.$el.find('#collapseOutputsSign').addClass('hide');
        } else {
            this.outputsExpanded = true;
            this.$el.find('.minor-output-row').removeClass('hide');
            this.$el.find('#expandOutputsSign').addClass('hide');
            this.$el.find('#collapseOutputsSign').removeClass('hide');
        }
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

    calculateClicked: function () {
        BlockUi();
        var that = this;

        var request = $.post(
			window.gRootPath + 'Underwriter/PricingModelCalculations/Calculate',
			{
				customerId: this.model.get('Id'),
				pricingModelModel: JSON.stringify(this.model.toJSON()),
			}
		);

        request.success(function (res) {
            that.model.set(res);
            if (that.model.get('EuLoanPercentages') == 0.02) {
                that.model.set('SetupFee2Class', '');
                that.model.set('SetupFee175Class', 'pricing-model-gray');
            } else {
                that.model.set('SetupFee2Class', 'pricing-model-gray');
                that.model.set('SetupFee175Class', '');
            }

            that.renderAndRememberExpanded();
            UnBlockUi();
        });
    },

    serializeData: function() {
        var data = this.model.toJSON();
        return { model: data };
    },

    renderAndRememberExpanded: function() {
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
        this.$el.find('#profitMarkup').percentFormat();

        this.$el.find('#loanAmount').moneyFormat();
        this.$el.find('#setupFeePounds').moneyFormat();
        this.$el.find('#cogs').moneyFormat();
        this.$el.find('#opexAndCapex').moneyFormat();
        
        this.$el.find('#loanTerm').monthFormatNoDecimals();
        this.$el.find('#interestOnlyPeriod').monthFormatNoDecimals();
        this.$el.find('#tenureMonths').monthFormat();
        
        return this;
    }
});


