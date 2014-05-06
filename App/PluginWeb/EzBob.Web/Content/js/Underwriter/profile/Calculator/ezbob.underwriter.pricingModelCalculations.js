var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PricingModelCalculationsModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/PricingModelCalculations/Index/"
});

EzBob.Underwriter.PricingModelCalculationsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#pricing-model-calculation-template').html());
        this.model.on('change reset fetch sync', this.render, this);
    },

    events: {
        'focusout #loanAmount': 'loanAmountChanged',
        'focusout #loanTerm': 'loanTermChanged',
        'focusout #interestRate': 'interestRateChanged',
        'focusout #defaultRate': 'defaultRateChanged',
        'focusout #tenureAsPercentOfLoanTerm': 'tenureAsPercentOfLoanTermChanged',
        'focusout #collectionRate': 'collectionRateChanged',
        'focusout #debtPercentOfCapital': 'debtPercentOfCapitalChanged',
        'focusout #costOfDebt': 'costOfDebtChanged',
        'focusout #initiationFee': 'initiationFeeChanged',
        'focusout #cogs': 'cogsChanged',
        'focusout #opexAndCapex': 'opexAndCapexChanged',
        'focusout #profitBeforeTax': 'profitBeforeTaxChanged',
        'focusout #interestOnlyPeriod': 'interestOnlyPeriodChanged',
        'focusout #tenureAsMonthsOfLoanTerm': 'tenureAsMonthsOfLoanTermChanged',
        'click #pricingModelResetButton': 'resetClicked',
        'click #pricingModelCalculateButton': 'calculateClicked'
    },

    resetClicked: function () {
        this.model.fetch();
        this.render();
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
            that.render();
            UnBlockUi();
        });
    },

    interestOnlyPeriodChanged: function () {
        this.model.set('InterestOnlyPeriod', this.$el.find('#interestOnlyPeriod').autoNumericGet());
    },

    tenureAsMonthsOfLoanTermChanged: function () {
        this.model.set('TenureAsMonthsOfLoanTerm', this.$el.find('#tenureAsMonthsOfLoanTerm').autoNumericGet());
    },

    costOfDebtChanged: function () {
        this.model.set('CostOfDebt', this.$el.find('#costOfDebt').autoNumericGet());
    },

    initiationFeeChanged: function () {
        this.model.set('InitiationFee', this.$el.find('#initiationFee').autoNumericGet());
    },

    cogsChanged: function () {
        this.model.set('Cogs', this.$el.find('#cogs').autoNumericGet());
    },

    opexAndCapexChanged: function () {
        this.model.set('OpexAndCapex', this.$el.find('#opexAndCapex').autoNumericGet());
    },

    profitBeforeTaxChanged: function () {
        this.model.set('ProfitBeforeTax', this.$el.find('#profitBeforeTax').autoNumericGet());
    },

    defaultRateChanged: function () {
        this.model.set('DefaultRate', this.$el.find('#defaultRate').autoNumericGet());
    },

    tenureAsPercentOfLoanTermChanged: function () {
        this.model.set('TenureAsPercentOfLoanTerm', this.$el.find('#tenureAsPercentOfLoanTerm').autoNumericGet());
    },

    collectionRateChanged: function () {
        // TODO: figure percents issue
        var value = this.$el.find('#collectionRate').val();
        this.model.set('CollectionRate', this.$el.find('#collectionRate').autoNumericGet());
        this.$el.find('#collectionRate').val(value);
    },

    debtPercentOfCapitalChanged: function () {
        this.model.set('DebtPercentOfCapital', this.$el.find('#debtPercentOfCapital').autoNumericGet());
    },

    loanAmountChanged: function () {
        this.model.set('LoanAmount', this.$el.find('#loanAmount').autoNumericGet());
    },
    
    loanTermChanged: function () {
        this.model.set('LoanTerm', this.$el.find('#loanTerm').autoNumericGet());
    },
    
    interestRateChanged: function () {
        this.model.set('InterestRate', this.$el.find('#interestRate').autoNumericGet());
    },

    render: function () {
        this.$el.html(this.template({ model: this.model.toJSON() }));
        
        this.$el.find('#interestRate').percentFormat();
        this.$el.find('#defaultRate').percentFormat();
        this.$el.find('#tenureAsPercentOfLoanTerm').percentFormat();
        this.$el.find('#collectionRate').percentFormat();
        this.$el.find('#debtPercentOfCapital').percentFormat();
        this.$el.find('#costOfDebt').percentFormat();

        this.$el.find('#loanAmount').moneyFormat();
        this.$el.find('#initiationFee').moneyFormat();
        this.$el.find('#cogs').moneyFormat();
        this.$el.find('#opexAndCapex').moneyFormat();
        this.$el.find('#profitBeforeTax').moneyFormat();

        this.$el.find('#loanTerm').numericOnly(2);
        this.$el.find('#interestOnlyPeriod').numericOnly(2);
        this.$el.find('#tenureAsMonthsOfLoanTerm').numericOnly(2);
        return this;
    }
});


