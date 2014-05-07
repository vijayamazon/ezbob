﻿var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PricingModelCalculationsModel = Backbone.Model.extend({
    url: window.gRootPath + "Underwriter/PricingModelCalculations/Index/"
});

EzBob.Underwriter.PricingModelCalculationsView = Backbone.Marionette.ItemView.extend({
    template: "#pricing-model-calculation-template",
    
    initialize: function () {
        this.modelBinder = new Backbone.ModelBinder();
        this.model.on('change reset fetch sync', this.render, this);
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

        InterestRate: {
            selector: "#interestRate",
            converter: EzBob.BindingConverters.percentsFormat
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
            converter: EzBob.BindingConverters.autonumericFormat
        },
        InterestOnlyPeriod: {
            selector: "#interestOnlyPeriod",
            converter: EzBob.BindingConverters.autonumericFormat
        },
        TenureAsMonthsOfLoanTerm: {
            selector: "#tenureAsMonthsOfLoanTerm",
            converter: EzBob.BindingConverters.autonumericFormat
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
        //this.model.fetch(); // TODO: make it work
    },

    calculateClicked: function () {
        BlockUi();
        var that = this;
        var request = $.post(window.gRootPath + 'Underwriter/PricingModelCalculations/Calculate', { loanAmount: this.model.get('LoanAmount') }); // TODO: pass the entire model
        request.success(function (res) {
            that.model.set('MonthlyInterestToCharge', res.MonthlyInterestToCharge);
            that.model.set('SetupFeeForEuLoan', res.SetupFeeForEuLoan);
            that.model.set('EuLoanPercentages', res.EuLoanPercentages);
            that.model.set('AverageLoanAmount', res.AverageLoanAmount);
            that.model.set('AverageRevenuePerLoan', res.AverageRevenuePerLoan);
            that.model.set('CogsOutput', res.CogsOutput);
            that.model.set('GrossProfit', res.GrossProfit);
            that.model.set('OpexAndCapexOutput', res.OpexAndCapexOutput);
            that.model.set('Ebitda', res.Ebitda);
            that.model.set('NetLossFromDefaults', res.NetLossFromDefaults);
            that.model.set('CostOfDebtOutput', res.CostOfDebtOutput);
            that.model.set('TotalCost', res.TotalCost);
            that.model.set('ProfitBeforeTaxOutput', res.ProfitBeforeTaxOutput);
            that.model.set('Balance', res.Balance);
            UnBlockUi();
        });
    },

    serializeData: function() {
        var data = this.model.toJSON();
        return { model: data };
    },

    onRender: function () {
        this.modelBinder.bind(this.model, this.el, this.bindings);
        return this;
    }
});


