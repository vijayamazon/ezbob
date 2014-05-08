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
        InitiationFee: {
            selector: "#initiationFee",
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
        ProfitBeforeTax: {
            selector: "#profitBeforeTax",
            converter: EzBob.BindingConverters.moneyFormat
        },

        DefaultRate: {
            selector: "#defaultRate",
            converter: EzBob.BindingConverters.percentsFormat
        },
        TenureAsPercentOfLoanTerm: {
            selector: "#tenureAsPercentOfLoanTerm",
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
            converter: EzBob.BindingConverters.numericOnlyFormat
        },
        InterestOnlyPeriod: {
            selector: "#interestOnlyPeriod",
            converter: EzBob.BindingConverters.numericOnlyFormat
        },
        TenureAsMonthsOfLoanTerm: {
            selector: "#tenureAsMonthsOfLoanTerm",
            converter: EzBob.BindingConverters.numericOnlyFormat
        }
    },

    events: {
        'click #pricingModelResetButton': 'resetClicked',
        'click #pricingModelCalculateButton': 'calculateClicked',
        'click #expandCollapseInputsButton': 'expandCollapseInputsClicked',
        'click #expandCollapseOutputsButton': 'expandCollapseOutputsClicked'
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
        return this;
    }
});


